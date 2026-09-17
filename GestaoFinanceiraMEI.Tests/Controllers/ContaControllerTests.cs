using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class ContaControllerTests
{
    private GestaoFinanceiraMEI.Data.AppDbContext _context = null!;
    private Mock<IAuthenticationService> _authServiceMock = null!;
    private ContaController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();

        _authServiceMock = new Mock<IAuthenticationService>();
        _authServiceMock
            .Setup(a => a.SignInAsync(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        _authServiceMock
            .Setup(a => a.SignOutAsync(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // ContaController usa RedirectToAction (Registro/Login/Logout), cujo
        // acesso à propriedade Url resolve IUrlHelperFactory via
        // HttpContext.RequestServices — como aqui esse RequestServices é um
        // ServiceProvider "de bancada" (não o pipeline real do ASP.NET Core),
        // é preciso registrar um IUrlHelperFactory de mentira, senão a
        // resolução do serviço lança InvalidOperationException.
        var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        urlHelperFactoryMock
            .Setup(f => f.GetUrlHelper(It.IsAny<Microsoft.AspNetCore.Mvc.ActionContext>()))
            .Returns(Mock.Of<IUrlHelper>());

        var servicos = ControllerTestExtensions.ServiceProviderCom(
            (typeof(IAuthenticationService), _authServiceMock.Object),
            (typeof(IUrlHelperFactory), urlHelperFactoryMock.Object));

        _controller = new ContaController(_context).ComHttpContext(servicos);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    private static RegistroViewModel RegistroValido() => new()
    {
        Nome = "Maria Silva",
        Email = "maria@example.com",
        NomeNegocio = "Ateliê da Maria",
        Senha = "senha123",
        ConfirmarSenha = "senha123"
    };

    // ---------------- Registro ----------------

    [Test]
    public void Registro_Get_RetornaViewComModeloVazio()
    {
        var resultado = _controller.Registro() as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.InstanceOf<RegistroViewModel>());
    }

    [Test]
    public async Task Registro_Post_ModelStateInvalido_RetornaViewComOMesmoModelo()
    {
        var model = RegistroValido();
        _controller.ModelState.AddModelError("Nome", "Informe o nome completo.");

        var resultado = await _controller.Registro(model) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(model));
        _authServiceMock.Verify(a => a.SignInAsync(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);
    }

    [Test]
    public async Task Registro_Post_EmailJaCadastrado_AdicionaErroDeModelState_ERetornaView()
    {
        _context.Usuarios.Add(new Usuario { Nome = "Já existe", Email = "maria@example.com", NomeNegocio = "X", SenhaHash = "h", SenhaSalt = "s" });
        await _context.SaveChangesAsync();

        var model = RegistroValido();

        var resultado = await _controller.Registro(model) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ModelState[nameof(RegistroViewModel.Email)]!.Errors, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task Registro_Post_DadosValidos_CriaUsuariaComSenhaHasheada_EAutentica_ERedirecionaParaDashboard()
    {
        var model = RegistroValido();

        var resultado = await _controller.Registro(model) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));
        Assert.That(resultado.ControllerName, Is.EqualTo("Dashboard"));

        var usuarioSalvo = await _context.Usuarios.SingleAsync(u => u.Email == model.Email);
        Assert.That(usuarioSalvo.SenhaHash, Is.Not.EqualTo(model.Senha));
        Assert.That(PasswordHasher.Verificar(model.Senha, usuarioSalvo.SenhaHash, usuarioSalvo.SenhaSalt), Is.True);

        _authServiceMock.Verify(a => a.SignInAsync(
            It.IsAny<Microsoft.AspNetCore.Http.HttpContext>(),
            It.IsAny<string>(),
            It.Is<System.Security.Claims.ClaimsPrincipal>(p => p.Identity!.IsAuthenticated),
            It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    [Test]
    public async Task Registro_Post_DadosValidos_CriaAsOitoCategoriasPadraoComANaturezaCorreta()
    {
        var model = RegistroValido();

        await _controller.Registro(model);

        var usuario = await _context.Usuarios.SingleAsync(u => u.Email == model.Email);
        var categorias = await _context.Categorias.Where(c => c.UsuarioId == usuario.Id).ToListAsync();

        Assert.That(categorias, Has.Count.EqualTo(8));
        Assert.That(categorias.Count(c => c.Tipo == TipoTransacao.Receita), Is.EqualTo(3));
        Assert.That(categorias.Single(c => c.Nome == "Fornecedores/Insumos").NaturezaDespesa, Is.EqualTo(NaturezaDespesa.CustoMercadoriaVendida));
        Assert.That(categorias.Single(c => c.Nome == "Aluguel").NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaFixa));
        Assert.That(categorias.Single(c => c.Nome == "Impostos (DAS)").NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DeducaoOuImposto));
        Assert.That(categorias.Single(c => c.Nome == "Transporte").NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaVariavel));
        Assert.That(categorias.Single(c => c.Nome == "Outras despesas").NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaVariavel));
    }

    // ---------------- Login ----------------

    [Test]
    public void Login_Get_RetornaViewComModeloVazio_EDefineReturnUrlNoViewData()
    {
        var resultado = _controller.Login("/dashboard") as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.InstanceOf<LoginViewModel>());
        Assert.That(_controller.ViewData["ReturnUrl"], Is.EqualTo("/dashboard"));
    }

    [Test]
    public async Task Login_Post_ModelStateInvalido_RetornaView()
    {
        var model = new LoginViewModel { Email = "x@x.com", Senha = "123456" };
        _controller.ModelState.AddModelError("Email", "obrigatório");

        var resultado = await _controller.Login(model) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(model));
    }

    [Test]
    public async Task Login_Post_UsuariaNaoEncontrada_AdicionaErroGenerico_ERetornaView()
    {
        var model = new LoginViewModel { Email = "naoexiste@x.com", Senha = "qualquer" };

        var resultado = await _controller.Login(model) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ModelState[string.Empty]!.Errors, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task Login_Post_SenhaIncorreta_AdicionaErroGenerico_ERetornaView()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");
        _context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@x.com", NomeNegocio = "X", SenhaHash = hash, SenhaSalt = salt });
        await _context.SaveChangesAsync();

        var model = new LoginViewModel { Email = "maria@x.com", Senha = "senhaErrada" };

        var resultado = await _controller.Login(model) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    [Test]
    public async Task Login_Post_CredenciaisValidas_SemReturnUrl_RedirecionaParaDashboard()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");
        _context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@x.com", NomeNegocio = "X", SenhaHash = hash, SenhaSalt = salt });
        await _context.SaveChangesAsync();

        var model = new LoginViewModel { Email = "maria@x.com", Senha = "senhaCorreta" };

        var resultado = await _controller.Login(model) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));
        Assert.That(resultado.ControllerName, Is.EqualTo("Dashboard"));
    }

    [Test]
    public async Task Login_Post_CredenciaisValidas_ComReturnUrlLocal_RedirecionaParaOReturnUrl()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");
        _context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@x.com", NomeNegocio = "X", SenhaHash = hash, SenhaSalt = salt });
        await _context.SaveChangesAsync();

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.IsLocalUrl("/Transacoes")).Returns(true);
        _controller.Url = urlHelperMock.Object;

        var model = new LoginViewModel { Email = "maria@x.com", Senha = "senhaCorreta" };

        var resultado = await _controller.Login(model, returnUrl: "/Transacoes") as RedirectResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Url, Is.EqualTo("/Transacoes"));
    }

    [Test]
    public async Task Login_Post_CredenciaisValidas_ComReturnUrlExterno_IgnoraOReturnUrl_RedirecionaParaDashboard()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");
        _context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@x.com", NomeNegocio = "X", SenhaHash = hash, SenhaSalt = salt });
        await _context.SaveChangesAsync();

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.IsLocalUrl("https://malicioso.com")).Returns(false);
        _controller.Url = urlHelperMock.Object;

        var model = new LoginViewModel { Email = "maria@x.com", Senha = "senhaCorreta" };

        var resultado = await _controller.Login(model, returnUrl: "https://malicioso.com") as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ControllerName, Is.EqualTo("Dashboard"));
    }

    // ---------------- Logout ----------------

    [Test]
    public async Task Logout_ChamaSignOutAsync_ERedirecionaParaHome()
    {
        var resultado = await _controller.Logout() as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));
        Assert.That(resultado.ControllerName, Is.EqualTo("Home"));
        _authServiceMock.Verify(a => a.SignOutAsync(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }
}

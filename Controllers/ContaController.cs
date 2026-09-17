using System.Security.Claims;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Responsável pelo cadastro, login e logout das usuárias do sistema.
/// </summary>
[AllowAnonymous]
public class ContaController : Controller
{
    private readonly AppDbContext _context;

    public ContaController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var emailJaExiste = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
        if (emailJaExiste)
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View(model);
        }

        var (hash, salt) = PasswordHasher.CriarHash(model.Senha);

        var usuario = new Usuario
        {
            Nome = model.Nome,
            Email = model.Email,
            NomeNegocio = model.NomeNegocio,
            SenhaHash = hash,
            SenhaSalt = salt,
            DataCadastro = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Categorias padrão para facilitar o primeiro uso do sistema.
        var categoriasPadrao = new List<Categoria>
        {
            new() { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = usuario.Id },
            new() { Nome = "Prestação de serviços", Tipo = TipoTransacao.Receita, UsuarioId = usuario.Id },
            new() { Nome = "Outras receitas", Tipo = TipoTransacao.Receita, UsuarioId = usuario.Id },
            new() { Nome = "Fornecedores/Insumos", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.CustoMercadoriaVendida, UsuarioId = usuario.Id },
            new() { Nome = "Aluguel", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = usuario.Id },
            new() { Nome = "Impostos (DAS)", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DeducaoOuImposto, UsuarioId = usuario.Id },
            new() { Nome = "Transporte", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = usuario.Id },
            new() { Nome = "Outras despesas", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = usuario.Id }
        };
        _context.Categorias.AddRange(categoriasPadrao);
        await _context.SaveChangesAsync();

        await AutenticarAsync(usuario);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (usuario is null || !PasswordHasher.Verificar(model.Senha, usuario.SenhaHash, usuario.SenhaSalt))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        await AutenticarAsync(usuario);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task AutenticarAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new("NomeNegocio", usuario.NomeNegocio)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}

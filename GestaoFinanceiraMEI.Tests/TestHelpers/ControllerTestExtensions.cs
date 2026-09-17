using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GestaoFinanceiraMEI.Tests.TestHelpers;

/// <summary>
/// Extensões para preparar um Controller para ser testado isoladamente
/// (fora do pipeline HTTP real do ASP.NET Core): simula a usuária
/// autenticada (claims do cookie), o TempData e, quando necessário, os
/// serviços resolvidos via HttpContext.RequestServices.
/// </summary>
public static class ControllerTestExtensions
{
    public const int UsuarioIdPadrao = 1;
    public const string NomePadrao = "Usuária de Teste";
    public const string EmailPadrao = "teste@example.com";
    public const string NomeNegocioPadrao = "Negócio de Teste";

    /// <summary>
    /// Configura o ControllerContext/HttpContext do controller como se a
    /// requisição viesse de uma usuária autenticada com o Id informado,
    /// reproduzindo os claims gravados pelo ContaController ao fazer login
    /// (ver AutenticadoController.UsuarioId).
    /// </summary>
    public static T ComoUsuarioAutenticado<T>(this T controller, int usuarioId = UsuarioIdPadrao, IServiceProvider? servicos = null)
        where T : Controller
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, NomePadrao),
            new(ClaimTypes.Email, EmailPadrao),
            new("NomeNegocio", NomeNegocioPadrao)
        };

        var identity = new ClaimsIdentity(claims, "TesteAutenticacao");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        if (servicos is not null)
            httpContext.RequestServices = servicos;

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    /// <summary>
    /// Configura o ControllerContext do controller com um HttpContext "cru"
    /// (sem usuária autenticada), com RequestServices resolvendo os serviços
    /// informados — usado para o ContaController, que é [AllowAnonymous] e
    /// usa HttpContext.SignInAsync/SignOutAsync (que buscam
    /// IAuthenticationService via RequestServices).
    /// </summary>
    public static T ComHttpContext<T>(this T controller, IServiceProvider servicos, ClaimsPrincipal? usuario = null)
        where T : Controller
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = servicos,
            User = usuario ?? new ClaimsPrincipal(new ClaimsIdentity())
        };

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    /// <summary>
    /// Monta um IServiceProvider mínimo contendo apenas o(s) serviço(s)
    /// mockado(s) informado(s) — usado para popular HttpContext.RequestServices
    /// nos testes do ContaController.
    /// </summary>
    public static IServiceProvider ServiceProviderCom(params (Type Tipo, object Instancia)[] servicos)
    {
        var colecao = new ServiceCollection();
        foreach (var (tipo, instancia) in servicos)
            colecao.AddSingleton(tipo, instancia);

        return colecao.BuildServiceProvider();
    }
}

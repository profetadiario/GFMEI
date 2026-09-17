using System.Security.Claims;
using GestaoFinanceiraMEI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class HomeControllerTests
{
    private static HomeController CriarController(bool autenticado)
    {
        var identity = autenticado
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TesteAutenticacao")
            : new ClaimsIdentity(); // sem authenticationType => IsAuthenticated == false

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return new HomeController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Test]
    public void Index_UsuariaAutenticada_RedirecionaParaDashboard()
    {
        var controller = CriarController(autenticado: true);

        var resultado = controller.Index() as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));
        Assert.That(resultado.ControllerName, Is.EqualTo("Dashboard"));
    }

    [Test]
    public void Index_UsuariaNaoAutenticada_RetornaView()
    {
        var controller = CriarController(autenticado: false);

        var resultado = controller.Index() as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public void Erro_RetornaView()
    {
        var controller = CriarController(autenticado: false);

        var resultado = controller.Erro() as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }
}

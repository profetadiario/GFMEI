using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class AutenticadoControllerTests
{
    /// <summary>
    /// AutenticadoController é abstrato: essa subclasse mínima só expõe a
    /// propriedade protegida UsuarioId publicamente, para poder testar a
    /// leitura do claim compartilhada por todos os controllers autenticados.
    /// </summary>
    private class ControllerDeTeste : AutenticadoController
    {
        public int ObterUsuarioId() => UsuarioId;
    }

    [Test]
    public void UsuarioId_ComClaimNameIdentifierValido_RetornaOIdConvertido()
    {
        var controller = new ControllerDeTeste().ComoUsuarioAutenticado(usuarioId: 42);

        Assert.That(controller.ObterUsuarioId(), Is.EqualTo(42));
    }

    [Test]
    public void UsuarioId_SemUsuariaAutenticada_LancaExcecao()
    {
        var controller = new ControllerDeTeste
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // Sem claim NameIdentifier, User.FindFirstValue retorna null e o
        // int.Parse(...)! do controller lança — comportamento atual e
        // esperado, já que [Authorize] nunca deixaria chegar aqui sem login.
        Assert.Catch(() => controller.ObterUsuarioId());
    }
}

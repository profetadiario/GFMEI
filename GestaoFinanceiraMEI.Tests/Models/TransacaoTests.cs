using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class TransacaoTests
{
    private static Transacao CriarTransacaoValida() => new()
    {
        Descricao = "Venda de bolo",
        Valor = 50m,
        Tipo = TipoTransacao.Receita,
        Data = DateTime.Today,
        CategoriaId = 1,
        UsuarioId = 1
    };

    [Test]
    public void Transacao_Valida_NaoGeraErrosDeValidacao()
    {
        var transacao = CriarTransacaoValida();

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Descricao_Vazia_GeraErroDeRequired()
    {
        var transacao = CriarTransacaoValida();
        transacao.Descricao = string.Empty;

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros.TemErroEm(nameof(Transacao.Descricao)), Is.True);
    }

    [Test]
    public void Descricao_MaiorQueLimite_GeraErroDeStringLength()
    {
        var transacao = CriarTransacaoValida();
        transacao.Descricao = new string('a', 151);

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros.TemErroEm(nameof(Transacao.Descricao)), Is.True);
    }

    [TestCase(0)]
    [TestCase(-10)]
    [TestCase(1_000_000.01)]
    public void Valor_ForaDoIntervaloPermitido_GeraErroDeRange(double valorInvalido)
    {
        var transacao = CriarTransacaoValida();
        transacao.Valor = (decimal)valorInvalido;

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros.TemErroEm(nameof(Transacao.Valor)), Is.True);
    }

    [TestCase(0.01)]
    [TestCase(1)]
    [TestCase(1_000_000)]
    public void Valor_DentroDoIntervaloPermitido_NaoGeraErro(double valorValido)
    {
        var transacao = CriarTransacaoValida();
        transacao.Valor = (decimal)valorValido;

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros.TemErroEm(nameof(Transacao.Valor)), Is.False);
    }

    [Test]
    public void CategoriaId_ZeroOuNegativo_NaoTemAtributoRangeENaoGeraErroPorSi()
    {
        // CategoriaId não tem [Range]; a validação "categoria pertence à
        // usuária e é compatível com o tipo" é feita no controller
        // (CategoriaValidaAsync), não por DataAnnotations.
        var transacao = CriarTransacaoValida();
        transacao.CategoriaId = 0;

        var erros = ValidationTestHelper.Validar(transacao);

        Assert.That(erros.TemErroEm(nameof(Transacao.CategoriaId)), Is.False);
    }

    [Test]
    public void Data_TemValorPadraoIgualAHoje()
    {
        var transacao = new Transacao();

        Assert.That(transacao.Data, Is.EqualTo(DateTime.Today));
    }

    [Test]
    public void NavegacoesDeCategoriaEUsuario_NaoInicializadas_SaoNulas()
    {
        var transacao = new Transacao();

        Assert.That(transacao.Categoria, Is.Null);
        Assert.That(transacao.Usuario, Is.Null);
    }
}

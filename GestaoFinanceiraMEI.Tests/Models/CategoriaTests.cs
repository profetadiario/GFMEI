using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class CategoriaTests
{
    private static Categoria CriarCategoriaValida() => new()
    {
        Nome = "Vendas",
        Tipo = TipoTransacao.Receita,
        UsuarioId = 1
    };

    [Test]
    public void Categoria_Valida_NaoGeraErrosDeValidacao()
    {
        var categoria = CriarCategoriaValida();

        var erros = ValidationTestHelper.Validar(categoria);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Nome_Vazio_GeraErroDeRequired()
    {
        var categoria = CriarCategoriaValida();
        categoria.Nome = string.Empty;

        var erros = ValidationTestHelper.Validar(categoria);

        Assert.That(erros.TemErroEm(nameof(Categoria.Nome)), Is.True);
    }

    [Test]
    public void Nome_MaiorQueLimite_GeraErroDeStringLength()
    {
        var categoria = CriarCategoriaValida();
        categoria.Nome = new string('a', 61);

        var erros = ValidationTestHelper.Validar(categoria);

        Assert.That(erros.TemErroEm(nameof(Categoria.Nome)), Is.True);
    }

    [Test]
    public void NaturezaDespesa_ValorPadrao_EDespesaVariavel()
    {
        var categoria = new Categoria();

        Assert.That(categoria.NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaVariavel));
    }

    [TestCase(NaturezaDespesa.DespesaVariavel)]
    [TestCase(NaturezaDespesa.DeducaoOuImposto)]
    [TestCase(NaturezaDespesa.CustoMercadoriaVendida)]
    [TestCase(NaturezaDespesa.DespesaFixa)]
    public void NaturezaDespesa_AceitaTodosOsValoresDoEnum(NaturezaDespesa natureza)
    {
        var categoria = CriarCategoriaValida();
        categoria.NaturezaDespesa = natureza;

        var erros = ValidationTestHelper.Validar(categoria);

        Assert.That(erros, Is.Empty);
        Assert.That(categoria.NaturezaDespesa, Is.EqualTo(natureza));
    }

    [Test]
    public void Transacoes_ComecaVazia_ENuncaENula()
    {
        var categoria = new Categoria();

        Assert.That(categoria.Transacoes, Is.Not.Null.And.Empty);
    }

    [Test]
    public void Usuario_NaoInicializado_EhNulo()
    {
        var categoria = new Categoria();

        Assert.That(categoria.Usuario, Is.Null);
    }
}

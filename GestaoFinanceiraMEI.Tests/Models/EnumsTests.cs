using System.ComponentModel.DataAnnotations;
using GestaoFinanceiraMEI.Models;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class TipoTransacaoTests
{
    [Test]
    public void Receita_TemValorInteiroUm()
    {
        Assert.That((int)TipoTransacao.Receita, Is.EqualTo(1));
    }

    [Test]
    public void Despesa_TemValorInteiroDois()
    {
        Assert.That((int)TipoTransacao.Despesa, Is.EqualTo(2));
    }
}

[TestFixture]
public class NaturezaDespesaTests
{
    [Test]
    public void DespesaVariavel_EhOValorZeroPadrao()
    {
        // Deliberado: categorias antigas (criadas antes dessa funcionalidade
        // existir, inclusive em produção) precisam cair automaticamente
        // nesse valor ao ganharem a nova coluna via ALTER TABLE ... DEFAULT 0.
        Assert.That((int)NaturezaDespesa.DespesaVariavel, Is.EqualTo(0));
    }

    [TestCase(NaturezaDespesa.DespesaVariavel, "Despesa variável")]
    [TestCase(NaturezaDespesa.DeducaoOuImposto, "Dedução ou imposto (ex.: DAS)")]
    [TestCase(NaturezaDespesa.CustoMercadoriaVendida, "Custo da mercadoria vendida (CMV)")]
    [TestCase(NaturezaDespesa.DespesaFixa, "Despesa fixa")]
    public void CadaValor_TemODisplayNameEsperado(NaturezaDespesa valor, string nomeEsperado)
    {
        var membro = typeof(NaturezaDespesa).GetMember(valor.ToString())[0];
        var atributo = (DisplayAttribute)Attribute.GetCustomAttribute(membro, typeof(DisplayAttribute))!;

        Assert.That(atributo.Name, Is.EqualTo(nomeEsperado));
    }

    [Test]
    public void Enum_TemExatamenteQuatroValores()
    {
        var valores = Enum.GetValues<NaturezaDespesa>();

        Assert.That(valores, Has.Length.EqualTo(4));
    }
}

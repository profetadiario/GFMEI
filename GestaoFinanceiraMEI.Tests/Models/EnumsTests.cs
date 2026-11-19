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
    [TestCase(NaturezaDespesa.RetiradaPessoal, "Retirada pessoal")]
    public void CadaValor_TemODisplayNameEsperado(NaturezaDespesa valor, string nomeEsperado)
    {
        var membro = typeof(NaturezaDespesa).GetMember(valor.ToString())[0];
        var atributo = (DisplayAttribute)Attribute.GetCustomAttribute(membro, typeof(DisplayAttribute))!;

        Assert.That(atributo.Name, Is.EqualTo(nomeEsperado));
    }

    [Test]
    public void RetiradaPessoal_TemValorInteiroQuatro_SemAlterarOsValoresJaGravadosNoBanco()
    {
        // O enum é gravado como inteiro: o novo valor entra no fim (4) e os
        // quatro anteriores mantêm seus números, então nenhuma categoria já
        // cadastrada muda de significado e não há migração de banco.
        Assert.That((int)NaturezaDespesa.RetiradaPessoal, Is.EqualTo(4));
        Assert.That((int)NaturezaDespesa.DespesaVariavel, Is.EqualTo(0));
        Assert.That((int)NaturezaDespesa.DeducaoOuImposto, Is.EqualTo(1));
        Assert.That((int)NaturezaDespesa.CustoMercadoriaVendida, Is.EqualTo(2));
        Assert.That((int)NaturezaDespesa.DespesaFixa, Is.EqualTo(3));
    }

    [Test]
    public void Enum_TemExatamenteCincoValores()
    {
        var valores = Enum.GetValues<NaturezaDespesa>();

        Assert.That(valores, Has.Length.EqualTo(5));
    }
}

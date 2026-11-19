using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class NecessidadeCapitalServiceTests
{
    private Mock<IRelatorioCreditoService> _relatorio = null!;
    private NecessidadeCapitalService _service = null!;
    private static readonly DateTime Hoje = new(2026, 11, 19);

    [SetUp]
    public void SetUp()
    {
        _relatorio = new Mock<IRelatorioCreditoService>();
        _service = new NecessidadeCapitalService(_relatorio.Object);
    }

    /// <summary>Relatório com um único mês considerado (o atual).</summary>
    private void ComMesAtual(decimal receitas, decimal despesasFixas, decimal retiradas = 0m)
    {
        var r = new RelatorioCredito { MesesConsiderados = 1 };
        for (var i = 0; i < 11; i++) r.Meses.Add(new MesRelatorio());
        var saldo = receitas - despesasFixas - retiradas;
        r.Meses.Add(new MesRelatorio { Receitas = receitas, DespesasFixas = despesasFixas, RetiradasPessoais = retiradas, SaldoFinal = saldo, TeveMovimento = true });
        _relatorio.Setup(s => s.ObterRelatorioAsync(1, Hoje)).ReturnsAsync(r);
    }

    [Test]
    public async Task CenarioDaDemo_CalculaReservaFaltaFolegoEPrazo()
    {
        ComMesAtual(receitas: 4050m, despesasFixas: 2535m);

        var n = await _service.CalcularAsync(1, Hoje, 3, null);

        Assert.That(n.CustoMensalMedio, Is.EqualTo(2535m));
        Assert.That(n.ReservaNecessaria, Is.EqualTo(7605m));
        Assert.That(n.SaldoAtual, Is.EqualTo(1515m));
        Assert.That(n.Falta, Is.EqualTo(6090m));
        Assert.That(n.MesesDeFolego, Is.EqualTo(0.6m));
        Assert.That(n.GuardarPorMes, Is.EqualTo(454.50m)); // 30% da sobra média
        Assert.That(n.MesesParaFormarReserva, Is.EqualTo(14));
    }

    [Test]
    public async Task RetiradasPessoaisEntramNasSaidasMensais()
    {
        ComMesAtual(receitas: 3000m, despesasFixas: 1000m, retiradas: 500m);

        var n = await _service.CalcularAsync(1, Hoje, 1, null);

        Assert.That(n.CustoMensalMedio, Is.EqualTo(1500m));
        Assert.That(n.ReservaCompleta, Is.True); // saldo 1.500 cobre 1 mês
        Assert.That(n.MesesParaFormarReserva, Is.EqualTo(0));
    }

    [Test]
    public async Task ValorParaGuardarInformado_SubstituiOPadrao()
    {
        ComMesAtual(receitas: 4050m, despesasFixas: 2535m);

        var n = await _service.CalcularAsync(1, Hoje, 3, 1000m);

        Assert.That(n.GuardarPorMes, Is.EqualTo(1000m));
        Assert.That(n.MesesParaFormarReserva, Is.EqualTo(7)); // 6.090 / 1.000
    }

    [TestCase(0, 1)]
    [TestCase(-5, 1)]
    [TestCase(30, 12)]
    public async Task MesesDeCobertura_FicamEntre1E12(int pedido, int esperado)
    {
        ComMesAtual(receitas: 100m, despesasFixas: 50m);

        var n = await _service.CalcularAsync(1, Hoje, pedido, null);

        Assert.That(n.MesesCobertura, Is.EqualTo(esperado));
    }

    [Test]
    public async Task SemSobra_NaoHaPrazoParaFormarReserva()
    {
        ComMesAtual(receitas: 1000m, despesasFixas: 1500m);

        var n = await _service.CalcularAsync(1, Hoje, 3, null);

        Assert.That(n.SaldoAtual, Is.EqualTo(-500m));
        Assert.That(n.Falta, Is.EqualTo(4500m)); // saldo negativo não "desconta" da reserva
        Assert.That(n.MesesDeFolego, Is.EqualTo(0m));
        Assert.That(n.GuardarPorMes, Is.EqualTo(0m));
        Assert.That(n.MesesParaFormarReserva, Is.Null);
    }

    [Test]
    public async Task SemLancamentos_SemDados()
    {
        var vazio = new RelatorioCredito();
        for (var i = 0; i < 12; i++) vazio.Meses.Add(new MesRelatorio());
        _relatorio.Setup(s => s.ObterRelatorioAsync(1, Hoje)).ReturnsAsync(vazio);

        var n = await _service.CalcularAsync(1, Hoje, 3, null);

        Assert.That(n.TemDados, Is.False);
        Assert.That(n.ReservaCompleta, Is.False);
        Assert.That(n.MesesDeFolego, Is.Null);
    }

    [Test]
    public async Task Controller_SemParametros_UsaTresMesesDeCobertura()
    {
        var servico = new Mock<INecessidadeCapitalService>();
        servico.Setup(s => s.CalcularAsync(ControllerTestExtensions.UsuarioIdPadrao, It.IsAny<DateTime>(), 3, null))
            .ReturnsAsync(new NecessidadeCapital { MesesCobertura = 3 });
        using var controller = new NecessidadeCapitalController(servico.Object).ComoUsuarioAutenticado();

        var view = await controller.Index(null, null) as ViewResult;

        Assert.That(((NecessidadeCapital)view!.Model!).MesesCobertura, Is.EqualTo(3));
    }
}

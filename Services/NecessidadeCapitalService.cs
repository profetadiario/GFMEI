namespace GestaoFinanceiraMEI.Services;

/// <inheritdoc cref="INecessidadeCapitalService"/>
public class NecessidadeCapitalService : INecessidadeCapitalService
{
    private readonly IRelatorioCreditoService _relatorioService;

    public NecessidadeCapitalService(IRelatorioCreditoService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    public async Task<NecessidadeCapital> CalcularAsync(int usuarioId, DateTime hoje, int mesesCobertura, decimal? guardarPorMes)
    {
        var relatorio = await _relatorioService.ObterRelatorioAsync(usuarioId, hoje);

        var cobertura = Math.Clamp(mesesCobertura, NecessidadeCapital.MesesCoberturaMinimo, NecessidadeCapital.MesesCoberturaMaximo);
        var saidas = relatorio.Meses.Sum(m => m.DespesasDoNegocio + m.RetiradasPessoais);

        return new NecessidadeCapital
        {
            TemDados = relatorio.TemDados,
            MesesConsiderados = relatorio.MesesConsiderados,
            MesesCobertura = cobertura,
            CustoMensalMedio = relatorio.MesesConsiderados == 0 ? 0m : Math.Round(saidas / relatorio.MesesConsiderados, 2),
            SaldoAtual = relatorio.SaldoAtual,
            SobraMedia = relatorio.SobraMedia,
            GuardarPorMes = guardarPorMes is > 0 ? guardarPorMes.Value : relatorio.ParcelaConfortavel
        };
    }
}

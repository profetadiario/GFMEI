using GestaoFinanceiraMEI.Services;

namespace GestaoFinanceiraMEI.ViewModels;

public class RelatorioCreditoViewModel
{
    public RelatorioCredito Relatorio { get; set; } = new();
    public string NomeEmpreendedora { get; set; } = string.Empty;
    public string NomeNegocio { get; set; } = string.Empty;

    /// <summary>Parcela que a usuária quer simular (null = ainda não simulou).</summary>
    public decimal? ParcelaSimulada { get; set; }

    public SituacaoParcela? Situacao => ParcelaSimulada is decimal p ? Relatorio.AvaliarParcela(p) : null;

    public decimal? PercentualDaSobra => ParcelaSimulada is decimal p ? Relatorio.PercentualDaSobra(p) : null;
}

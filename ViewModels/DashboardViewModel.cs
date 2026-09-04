using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;

namespace GestaoFinanceiraMEI.ViewModels;

public class DashboardViewModel
{
    public string NomeNegocio { get; set; } = string.Empty;
    public ResumoFinanceiro ResumoDoMes { get; set; } = new();
    public decimal SaldoGeral { get; set; }
    public List<ResumoMensal> HistoricoMensal { get; set; } = new();
    public MetaFinanceira? MetaDoMes { get; set; }
    public decimal ValorAlcancadoMeta { get; set; }
    public decimal TotalCaptado { get; set; }

    public decimal PercentualMeta =>
        MetaDoMes is null || MetaDoMes.ValorMeta == 0
            ? 0
            : Math.Min(100, Math.Round((ValorAlcancadoMeta / MetaDoMes.ValorMeta) * 100, 1));
}

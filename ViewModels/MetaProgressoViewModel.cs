using GestaoFinanceiraMEI.Models;

namespace GestaoFinanceiraMEI.ViewModels;

public class MetaProgressoViewModel
{
    public MetaFinanceira Meta { get; set; } = null!;
    public decimal ValorAlcancado { get; set; }

    public decimal PercentualAtingido =>
        Meta.ValorMeta == 0 ? 0 : Math.Clamp(Math.Round((ValorAlcancado / Meta.ValorMeta) * 100, 1), 0, 100);
}

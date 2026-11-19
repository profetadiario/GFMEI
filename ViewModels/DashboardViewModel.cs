using System.Globalization;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;

namespace GestaoFinanceiraMEI.ViewModels;

public class DashboardViewModel
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>A partir de quantos dias sem lançar o Painel mostra o lembrete.</summary>
    public const int DiasParaLembrete = 3;

    public string NomeNegocio { get; set; } = string.Empty;
    public string PeriodoResumo { get; set; } = string.Empty;
    public ResumoFinanceiro ResumoDoMes { get; set; } = new();
    public decimal SaldoGeral { get; set; }
    public List<ResumoMensal> HistoricoMensal { get; set; } = new();
    public MetaFinanceira? MetaDoMes { get; set; }
    public decimal ValorAlcancadoMeta { get; set; }
    public decimal TotalCaptado { get; set; }

    /// <summary>Despesas do negócio no mês corrente por categoria (para o gráfico).</summary>
    public List<GastoPorCategoria> GastosPorCategoria { get; set; } = new();

    /// <summary>Categoria de maior gasto do mês (null se não houve despesas no mês).</summary>
    public GastoPorCategoria? MaiorGastoDoMes { get; set; }

    /// <summary>Data do último lançamento até hoje (null se nunca lançou nada).</summary>
    public DateTime? UltimoLancamento { get; set; }

    /// <summary>Data usada como "hoje" nos cálculos do lembrete.</summary>
    public DateTime DataReferencia { get; set; } = DateTime.Today;

    public decimal PercentualMeta =>
        MetaDoMes is null || MetaDoMes.ValorMeta == 0
            ? 0
            : Math.Clamp(Math.Round((ValorAlcancadoMeta / MetaDoMes.ValorMeta) * 100, 1), 0, 100);

    /// <summary>
    /// Frase em linguagem simples sobre o maior gasto do mês, por exemplo:
    /// "Seu maior gasto este mês foi Insumos: R$ 1.200,00, 35% das saídas."
    /// </summary>
    public string FraseMaiorGasto =>
        MaiorGastoDoMes is null
            ? "Você ainda não registrou despesas neste mês."
            : $"Seu maior gasto este mês foi {MaiorGastoDoMes.Categoria}: " +
              $"R$ {MaiorGastoDoMes.Valor.ToString("N2", PtBr)}, " +
              $"{MaiorGastoDoMes.Percentual.ToString("0", PtBr)}% das saídas.";

    /// <summary>Dias corridos desde o último lançamento (null se nunca lançou nada).</summary>
    public int? DiasSemLancar =>
        UltimoLancamento is null
            ? null
            : Math.Max(0, (DataReferencia.Date - UltimoLancamento.Value.Date).Days);

    /// <summary>
    /// Aviso do Painel para quem está há alguns dias sem lançar (ou nunca
    /// lançou). É só uma mensagem na tela: não há e-mail, push nem serviço
    /// em segundo plano. Retorna null quando não há nada a avisar.
    /// </summary>
    public string? MensagemLembrete
    {
        get
        {
            if (DiasSemLancar is null)
                return "Você ainda não registrou nenhum lançamento. Que tal começar agora?";

            var dias = DiasSemLancar.Value;
            if (dias < DiasParaLembrete)
                return null;

            return $"Você não lança nada há {dias} dias.";
        }
    }
}

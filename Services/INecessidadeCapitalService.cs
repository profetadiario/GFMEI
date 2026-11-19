namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Indicadores de necessidade de capital (pós-TCC): quanto o negócio
/// gasta por mês, quanto precisa ter guardado para atravessar meses fracos
/// (reserva de emergência / capital de giro), quanto falta e em quanto
/// tempo dá para formar essa reserva. Usa os mesmos 12 meses do relatório
/// para o banco, então os números batem com ele.
/// </summary>
public interface INecessidadeCapitalService
{
    Task<NecessidadeCapital> CalcularAsync(int usuarioId, DateTime hoje, int mesesCobertura, decimal? guardarPorMes);
}

public class NecessidadeCapital
{
    public const int MesesCoberturaPadrao = 3;
    public const int MesesCoberturaMinimo = 1;
    public const int MesesCoberturaMaximo = 12;

    public bool TemDados { get; set; }
    public int MesesConsiderados { get; set; }
    public int MesesCobertura { get; set; }

    /// <summary>Saídas médias por mês: despesas do negócio + retiradas pessoais.</summary>
    public decimal CustoMensalMedio { get; set; }

    public decimal SaldoAtual { get; set; }
    public decimal SobraMedia { get; set; }

    /// <summary>Reserva recomendada: custo mensal médio × meses de cobertura.</summary>
    public decimal ReservaNecessaria => Math.Round(CustoMensalMedio * MesesCobertura, 2);

    public decimal Falta => Math.Max(0m, ReservaNecessaria - Math.Max(0m, SaldoAtual));

    public bool ReservaCompleta => TemDados && Falta == 0m;

    /// <summary>Por quantos meses o caixa de hoje paga as saídas, se as vendas pararem.</summary>
    public decimal? MesesDeFolego => CustoMensalMedio <= 0 ? null : Math.Round(Math.Max(0m, SaldoAtual) / CustoMensalMedio, 1);

    /// <summary>Quanto a usuária pretende guardar por mês (padrão: 30% da sobra média).</summary>
    public decimal GuardarPorMes { get; set; }

    /// <summary>Meses para completar a reserva guardando <see cref="GuardarPorMes"/> (null se não dá).</summary>
    public int? MesesParaFormarReserva =>
        Falta == 0m ? 0 : GuardarPorMes <= 0 ? null : (int)Math.Ceiling(Falta / GuardarPorMes);
}

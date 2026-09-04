namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Contrato do serviço responsável pelos cálculos de fluxo de caixa
/// e análise financeira (competências: fluxo de caixa e análise financeira).
/// </summary>
public interface IFluxoCaixaService
{
    Task<ResumoFinanceiro> ObterResumoAsync(int usuarioId, int? mes = null, int? ano = null);
}

/// <summary>
/// Resultado consolidado de receitas, despesas, saldo e histórico mensal
/// de um usuário.
/// </summary>
public class ResumoFinanceiro
{
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo => TotalReceitas - TotalDespesas;
    public List<ResumoMensal> HistoricoMensal { get; set; } = new();
}

/// <summary>
/// Totais de receitas e despesas de um mês específico, usados para
/// montar o gráfico do dashboard.
/// </summary>
public class ResumoMensal
{
    public string Mes { get; set; } = string.Empty;
    public decimal Receitas { get; set; }
    public decimal Despesas { get; set; }
}

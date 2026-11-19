namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Contrato do serviço responsável pelos cálculos de fluxo de caixa
/// e análise financeira (competências: fluxo de caixa e análise financeira).
/// </summary>
public interface IFluxoCaixaService
{
    Task<ResumoFinanceiro> ObterResumoAsync(int usuarioId, int? mes = null, int? ano = null);

    /// <summary>
    /// Reúne, em uma única leitura dos lançamentos, tudo o que o Painel
    /// precisa: resumo do ano corrente, resumo geral (histórico), gastos por
    /// categoria do mês e data do último lançamento. Existe para o Painel não
    /// carregar os lançamentos do banco várias vezes a cada acesso.
    /// </summary>
    /// <param name="hoje">Data de referência ("hoje"), recebida como parâmetro para facilitar testes.</param>
    Task<PainelFinanceiro> ObterPainelAsync(int usuarioId, DateTime hoje);
}

/// <summary>
/// Resultado consolidado de receitas, despesas, saldo e histórico mensal
/// de um usuário.
/// </summary>
public class ResumoFinanceiro
{
    public decimal TotalReceitas { get; set; }

    /// <summary>Despesas do negócio. NÃO inclui as retiradas pessoais (pró-labore).</summary>
    public decimal TotalDespesas { get; set; }

    /// <summary>Retiradas pessoais (pró-labore), mostradas em linha separada das despesas.</summary>
    public decimal TotalRetiradasPessoais { get; set; }

    /// <summary>Saldo em caixa: receitas menos despesas e menos retiradas pessoais.</summary>
    public decimal Saldo => TotalReceitas - TotalDespesas - TotalRetiradasPessoais;

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

    /// <summary>Despesas do negócio no mês (sem as retiradas pessoais).</summary>
    public decimal Despesas { get; set; }

    public decimal RetiradasPessoais { get; set; }
}

/// <summary>
/// Quanto o negócio gastou em uma categoria de despesa no mês, e que
/// percentual isso representa do total de despesas do mês.
/// </summary>
public class GastoPorCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
}

/// <summary>
/// Dados do Painel calculados a partir de uma única leitura dos lançamentos.
/// </summary>
public class PainelFinanceiro
{
    /// <summary>Resumo de janeiro até hoje, do ano corrente.</summary>
    public ResumoFinanceiro ResumoAnoCorrente { get; set; } = new();

    /// <summary>Resumo de todo o histórico (saldo geral e gráfico dos últimos meses).</summary>
    public ResumoFinanceiro ResumoGeral { get; set; } = new();

    /// <summary>
    /// Despesas do negócio do mês corrente por categoria, da maior para a
    /// menor. Quando há mais de seis categorias, as menores são agrupadas em
    /// "Outras categorias" para o gráfico continuar legível.
    /// </summary>
    public List<GastoPorCategoria> GastosPorCategoriaDoMes { get; set; } = new();

    /// <summary>A categoria de maior gasto do mês, sem agrupamento (null se não houve despesas).</summary>
    public GastoPorCategoria? MaiorGastoDoMes { get; set; }

    /// <summary>Data do último lançamento até hoje (null se a usuária ainda não lançou nada).</summary>
    public DateTime? UltimoLancamento { get; set; }
}

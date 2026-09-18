namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Contrato do serviço que monta o fluxo de caixa de um ano inteiro,
/// mês a mês (janeiro a dezembro), com saldo inicial carregado do saldo
/// final do mês anterior — no formato orientado pela banca (mesma
/// estrutura do exemplo semanal que ela enviou, aplicada por mês em vez de
/// por dia da semana).
/// </summary>
public interface IFluxoCaixaMensalService
{
    Task<FluxoCaixaAnual> ObterFluxoAnualAsync(int usuarioId, int ano);
}

/// <summary>
/// Uma linha de categoria dentro do fluxo de caixa (ex.: "Vendas",
/// "Aluguel"), com um valor para cada um dos 12 meses do ano.
/// </summary>
public class LinhaCategoriaFluxo
{
    public string Nome { get; set; } = string.Empty;
    public decimal[] Valores { get; set; } = new decimal[12];
}

/// <summary>
/// Fluxo de caixa completo de um ano: saldo inicial, entradas e saídas
/// detalhadas por categoria, saldo operacional e saldo final de cada mês.
/// </summary>
public class FluxoCaixaAnual
{
    public int Ano { get; set; }
    public string[] NomesMeses { get; set; } = new string[12];

    public decimal[] SaldoInicial { get; set; } = new decimal[12];
    public List<LinhaCategoriaFluxo> Entradas { get; set; } = new();
    public decimal[] TotalEntradas { get; set; } = new decimal[12];
    public List<LinhaCategoriaFluxo> Saidas { get; set; } = new();
    public decimal[] TotalSaidas { get; set; } = new decimal[12];
    public decimal[] SaldoOperacional { get; set; } = new decimal[12];
    public decimal[] SaldoFinal { get; set; } = new decimal[12];
}

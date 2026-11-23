namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Calculadora de preço pelo método da planilha da Adriana (decisão 9, Annik,
/// 22/11/2026). Todo valor mensal é rateado pelo tempo de produção da unidade
/// (valor ÷ dias ÷ horas ÷ 60 × minutos). Base do markup = custos fixos +
/// custos variáveis da unidade. DF% e DV% = despesas fixas e variáveis ÷ total
/// geral. Markup = 100 ÷ (100 − (DF% + DV% + lucro%)); preço = base × markup.
/// Tudo com 2 casas decimais, truncando, como na planilha.
/// </summary>
public interface ICalculadoraPrecoService
{
    ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada);

    /// <summary>
    /// Valores mensais sugeridos a partir dos lançamentos: média por categoria
    /// nos meses já fechados (antes do mês de <paramref name="hoje"/>, dentro
    /// dos últimos 12) que tiveram lançamento. Despesa fixa (com o pró-labore)
    /// vai para custos fixos; dedução ou imposto (DAS) para despesas fixas;
    /// despesa variável para despesas variáveis. O pró-labore aparece sempre.
    /// </summary>
    Task<SugestaoValoresMensais> ObterValoresSugeridosAsync(int usuarioId, DateTime hoje);
}

/// <summary>Um valor mensal digitado pela empreendedora (ex.: energia, R$ 150,00).</summary>
public class ItemMensal
{
    public string? Nome { get; set; }
    public decimal? Valor { get; set; }

    public bool Preenchido => Valor.HasValue && Valor.Value != 0m;
}

public class EntradaCalculadoraPreco
{
    public decimal? DiasTrabalho { get; set; }
    public decimal? HorasPorDia { get; set; }
    public decimal? MinutosPorUnidade { get; set; }

    /// <summary>Custos variáveis de uma unidade (materiais), já por unidade.</summary>
    public decimal? CustoVariavelUnidade { get; set; }

    public decimal? LucroPercentual { get; set; }

    /// <summary>Custos fixos mensais: energia, internet, pró-labore, aluguel...</summary>
    public List<ItemMensal> CustosFixos { get; set; } = new();

    /// <summary>Despesas fixas mensais: DAS, IPTU...</summary>
    public List<ItemMensal> DespesasFixas { get; set; } = new();

    /// <summary>Despesas variáveis mensais: gasolina, estacionamento...</summary>
    public List<ItemMensal> DespesasVariaveis { get; set; } = new();

    public bool FoiPreenchida => MinutosPorUnidade.HasValue || CustoVariavelUnidade.HasValue;
}

/// <summary>Valor mensal rateado para uma unidade.</summary>
public class ItemRateado
{
    public string Nome { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public decimal PorUnidade { get; set; }
}

public class ResultadoCalculadoraPreco
{
    public List<string> Erros { get; } = new();
    public bool Valido => Erros.Count == 0;

    public List<ItemRateado> CustosFixos { get; set; } = new();
    public List<ItemRateado> DespesasFixas { get; set; } = new();
    public List<ItemRateado> DespesasVariaveis { get; set; } = new();

    public decimal CustosFixosUnidade { get; set; }
    public decimal CustoVariavelUnidade { get; set; }
    public decimal DespesasFixasUnidade { get; set; }
    public decimal DespesasVariaveisUnidade { get; set; }

    /// <summary>Base do markup = custos fixos + custos variáveis (sem as despesas).</summary>
    public decimal Base { get; set; }

    /// <summary>Base + despesas fixas + despesas variáveis.</summary>
    public decimal TotalGeral { get; set; }

    public decimal DespesasFixasPercentual { get; set; }
    public decimal DespesasVariaveisPercentual { get; set; }
    public decimal LucroPercentual { get; set; }

    public decimal Markup { get; set; }
    public decimal PrecoSugerido { get; set; }

    /// <summary>Lucro em reais por unidade: lucro% do preço (truncado).</summary>
    public decimal LucroPorUnidade { get; set; }
}

public class ValorSugerido
{
    public string Nome { get; set; } = string.Empty;
    public decimal MediaMensal { get; set; }
}

public class SugestaoValoresMensais
{
    public int MesesConsiderados { get; set; }
    public bool TemSugestao => MesesConsiderados > 0;

    public List<ValorSugerido> CustosFixos { get; set; } = new();
    public List<ValorSugerido> DespesasFixas { get; set; } = new();
    public List<ValorSugerido> DespesasVariaveis { get; set; } = new();
}

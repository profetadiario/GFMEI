using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Classifica a natureza contábil de uma categoria de despesa, usada para
/// montar o Demonstrativo de Resultado do Exercício (DRE). Só é relevante
/// para categorias do tipo Despesa; em categorias de Receita o valor é
/// ignorado pelos cálculos.
///
/// O valor padrão é DespesaVariavel (0) de propósito: categorias criadas
/// antes dessa funcionalidade existir (incluindo as já cadastradas por
/// usuárias reais no ambiente publicado) caem automaticamente nesse "balde"
/// genérico em vez de gerar erro, e podem ser reclassificadas depois em
/// Categorias → Editar.
/// </summary>
public enum NaturezaDespesa
{
    [Display(Name = "Despesa variável")]
    DespesaVariavel = 0,

    [Display(Name = "Dedução ou imposto (ex.: DAS)")]
    DeducaoOuImposto = 1,

    [Display(Name = "Custo da mercadoria vendida (CMV)")]
    CustoMercadoriaVendida = 2,

    [Display(Name = "Despesa fixa")]
    DespesaFixa = 3,

    /// <summary>
    /// LEGADO (v1.4–v1.5). Antiga "retirada pessoal", que saiu do sistema
    /// pela decisão 10 (23/11/2026): a empreendedora se remunera só pelo
    /// pró-labore, lançado no fechamento do mês como despesa fixa. O valor
    /// continua no enum apenas para não quebrar lançamentos já gravados; não
    /// aparece em nenhuma tela e, nos cálculos, conta como despesa fixa (ver
    /// <see cref="NaturezaDespesaExtensions.ParaCalculo"/>).
    /// </summary>
    [Display(Name = "Despesa fixa")]
    RetiradaPessoal = 4
}

public static class NaturezaDespesaExtensions
{
    /// <summary>
    /// Natureza usada nos cálculos (DRE, relatório, calculadora). Lançamentos
    /// antigos de retirada pessoal contam como despesa fixa, igual ao
    /// pró-labore, até serem convertidos.
    /// </summary>
    public static NaturezaDespesa ParaCalculo(this NaturezaDespesa natureza) =>
        natureza == NaturezaDespesa.RetiradaPessoal ? NaturezaDespesa.DespesaFixa : natureza;
}

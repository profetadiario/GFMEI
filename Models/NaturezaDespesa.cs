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
    DespesaFixa = 3
}

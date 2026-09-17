using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Categoria usada para classificar receitas e despesas
/// (ex.: Vendas, Aluguel, Fornecedores).
/// </summary>
public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(60)]
    [Display(Name = "Nome da categoria")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tipo")]
    public TipoTransacao Tipo { get; set; }

    /// <summary>
    /// Natureza contábil usada para montar o DRE. Só se aplica quando
    /// Tipo == Despesa; para categorias de Receita fica com o valor padrão
    /// e não é usada em nenhum cálculo.
    /// </summary>
    [Display(Name = "Natureza (para o DRE)")]
    public NaturezaDespesa NaturezaDespesa { get; set; } = NaturezaDespesa.DespesaVariavel;

    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}

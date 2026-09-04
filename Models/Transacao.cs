using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Lançamento financeiro (entrada ou saída) que compõe o fluxo de caixa
/// e o controle de custos/despesas do negócio.
/// </summary>
public class Transacao
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe uma descrição.")]
    [StringLength(150)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor.")]
    [Range(0.01, 1_000_000, ErrorMessage = "O valor deve ser maior que zero.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Valor (R$)")]
    public decimal Valor { get; set; }

    [Required]
    [Display(Name = "Tipo")]
    public TipoTransacao Tipo { get; set; }

    [Required(ErrorMessage = "Informe a data.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public Categoria? Categoria { get; set; }

    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}

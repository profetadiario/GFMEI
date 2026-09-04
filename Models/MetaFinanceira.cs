using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Meta de planejamento financeiro definida pela empreendedora para um
/// determinado mês (ex.: meta de faturamento).
/// </summary>
public class MetaFinanceira
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a descrição da meta.")]
    [StringLength(150)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor da meta.")]
    [Range(0.01, 10_000_000, ErrorMessage = "O valor deve ser maior que zero.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Valor da meta (R$)")]
    public decimal ValorMeta { get; set; }

    [Required(ErrorMessage = "Informe o mês de referência.")]
    [DataType(DataType.Date)]
    [Display(Name = "Mês de referência")]
    public DateTime MesReferencia { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}

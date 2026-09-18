using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Registro de captação de recursos (empréstimo, microcrédito produtivo
/// orientado etc.) obtido pela empreendedora para o negócio.
/// </summary>
public class CaptacaoRecurso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a instituição financeira.")]
    [StringLength(120)]
    [Display(Name = "Instituição financeira")]
    public string InstituicaoFinanceira { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor captado.")]
    [Range(0.01, 10_000_000, ErrorMessage = "O valor deve ser maior que zero.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Valor captado (R$)")]
    public decimal Valor { get; set; }

    // Usa o construtor de Range baseado em decimal (em vez de Range(0, 100),
    // que compara usando int e faz o valor ser arredondado antes da checagem
    // — o que faria -0.01 virar 0 e 100.01 virar 100, escapando da validação).
    [Range(typeof(decimal), "0", "100", ErrorMessage = "Informe uma taxa entre 0 e 100.")]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Taxa de juros ao mês (%)")]
    public decimal TaxaJurosMensal { get; set; }

    [Required(ErrorMessage = "Informe a finalidade do recurso.")]
    [StringLength(200)]
    public string Finalidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de obtenção.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de obtenção")]
    public DateTime DataObtencao { get; set; } = DateTime.Today;

    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}

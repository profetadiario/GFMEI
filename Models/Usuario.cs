using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.Models;

/// <summary>
/// Representa a microempreendedora individual (MEI) que utiliza o sistema.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(120)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome do seu negócio.")]
    [StringLength(100)]
    [Display(Name = "Nome do negócio (MEI)")]
    public string NomeNegocio { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    public string SenhaSalt { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<MetaFinanceira> Metas { get; set; } = new List<MetaFinanceira>();
    public ICollection<CaptacaoRecurso> Captacoes { get; set; } = new List<CaptacaoRecurso>();
}

using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.ViewModels;

public class RegistroViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(120)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome do seu negócio.")]
    [StringLength(100)]
    [Display(Name = "Nome do negócio (MEI)")]
    public string NomeNegocio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

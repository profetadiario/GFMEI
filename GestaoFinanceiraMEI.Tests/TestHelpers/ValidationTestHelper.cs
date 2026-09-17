using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.Tests.TestHelpers;

/// <summary>
/// Executa as validações de DataAnnotations de um objeto (Models/ViewModels)
/// do mesmo modo que o model binding do ASP.NET Core faria, permitindo testar
/// os atributos [Required], [StringLength], [Range], [EmailAddress],
/// [Compare] etc. sem precisar de um pipeline HTTP completo.
/// </summary>
public static class ValidationTestHelper
{
    public static IList<ValidationResult> Validar(object modelo)
    {
        var contexto = new ValidationContext(modelo, serviceProvider: null, items: null);
        var resultados = new List<ValidationResult>();

        // validateAllProperties: true reproduz o comportamento do model
        // binding do ASP.NET Core, que valida todas as propriedades
        // decoradas do objeto (e não só as passadas em ValidationContext).
        Validator.TryValidateObject(modelo, contexto, resultados, validateAllProperties: true);

        return resultados;
    }

    public static bool TemErroEm(this IList<ValidationResult> resultados, string nomeDoMembro) =>
        resultados.Any(r => r.MemberNames.Contains(nomeDoMembro));
}

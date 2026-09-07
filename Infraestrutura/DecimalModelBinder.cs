using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace GestaoFinanceiraMEI.Infraestrutura;

/// <summary>
/// Model binder para campos "decimal" e "decimal?" que aceita tanto vírgula
/// quanto ponto como separador decimal (ex.: "3,45" ou "3.45"), além de
/// formatos com separador de milhar (ex.: "7.000,00" ou "7,000.00").
///
/// Isso resolve dois problemas do comportamento padrão do ASP.NET Core:
/// 1) O binder padrão usa a cultura corrente do servidor para interpretar
///    o texto digitado. Em uma cultura pt-BR, o ponto é tratado como
///    separador de milhar e é simplesmente removido, então "7000.00"
///    era interpretado como 700000 em vez de 7000,00.
/// 2) Em uma cultura en-US (ou Invariant), a vírgula era rejeitada,
///    impedindo a usuária de digitar "3,45".
///
/// A heurística usada é "o último separador encontrado é o decimal":
/// tudo antes dele é tratado como separador de milhar (removido) e ele
/// próprio é convertido para o ponto decimal invariante antes do parse.
/// </summary>
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var valorTexto = valueProviderResult.FirstValue;

        // Campo vazio: para decimal? (nullable) o resultado é null; para
        // decimal não-anulável, deixa o pipeline padrão de validação
        // (obrigatoriedade) reportar o erro.
        if (string.IsNullOrWhiteSpace(valorTexto))
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) != null)
                bindingContext.Result = ModelBindingResult.Success(null);

            return Task.CompletedTask;
        }

        if (TentarConverterDecimal(valorTexto, out var resultado))
        {
            bindingContext.Result = ModelBindingResult.Success(resultado);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                "O campo {0} deve ser um valor numérico válido (ex.: 3,45 ou 3.45)."
                    .Replace("{0}", bindingContext.ModelMetadata.GetDisplayName()));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converte um texto de entrada de usuário em decimal aceitando tanto
    /// vírgula quanto ponto como separador decimal, e tolerando separador
    /// de milhar em qualquer um dos dois padrões (pt-BR ou en-US).
    /// </summary>
    private static bool TentarConverterDecimal(string valorOriginal, out decimal resultado)
    {
        resultado = 0;
        var valor = valorOriginal.Trim();
        if (valor.Length == 0)
            return false;

        var posicaoVirgula = valor.LastIndexOf(',');
        var posicaoPonto = valor.LastIndexOf('.');
        string normalizado;

        if (posicaoVirgula >= 0 && posicaoPonto >= 0)
        {
            // Os dois separadores aparecem: o que estiver mais à direita é
            // o decimal; o outro é separador de milhar e é descartado.
            // Ex.: "7.000,00" -> decimal="," -> 7000.00
            //      "7,000.00" -> decimal="." -> 7000.00
            normalizado = posicaoVirgula > posicaoPonto
                ? valor.Replace(".", string.Empty).Replace(',', '.')
                : valor.Replace(",", string.Empty);
        }
        else if (posicaoVirgula >= 0)
        {
            // Só vírgula: é o separador decimal. Ex.: "3,45" -> "3.45"
            normalizado = valor.Replace(',', '.');
        }
        else
        {
            // Só ponto ou nenhum separador: já está no formato invariante.
            // Ex.: "3.45" -> 3.45 ; "7000.00" -> 7000.00 ; "100" -> 100
            normalizado = valor;
        }

        return decimal.TryParse(
            normalizado,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out resultado);
    }
}

/// <summary>
/// Registra o <see cref="DecimalModelBinder"/> para propriedades e
/// parâmetros do tipo <c>decimal</c> e <c>decimal?</c>.
/// </summary>
public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tipoModelo = context.Metadata.ModelType;
        var tipoSubjacente = Nullable.GetUnderlyingType(tipoModelo) ?? tipoModelo;

        if (tipoSubjacente == typeof(decimal))
            return new DecimalModelBinder();

        return null;
    }
}

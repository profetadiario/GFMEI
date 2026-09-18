using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace GestaoFinanceiraMEI.Tests.Infraestrutura;

/// <summary>
/// IValueProvider mínimo para testar model binders isoladamente, sem
/// precisar montar uma requisição HTTP real com querystring/form. Representa
/// uma única chave/valor — ou a ausência completa da chave, quando
/// <paramref name="valor"/> é null, reproduzindo ValueProviderResult.None.
/// </summary>
internal class FakeValueProvider : IValueProvider
{
    private readonly string _chave;
    private readonly string? _valor;

    public FakeValueProvider(string chave, string? valor)
    {
        _chave = chave;
        _valor = valor;
    }

    public bool ContainsPrefix(string prefix) => prefix == _chave && _valor is not null;

    public ValueProviderResult GetValue(string key)
    {
        if (key != _chave || _valor is null)
            return ValueProviderResult.None;

        return new ValueProviderResult(new StringValues(_valor), CultureInfo.InvariantCulture);
    }
}

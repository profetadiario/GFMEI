using GestaoFinanceiraMEI.Infraestrutura;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Infraestrutura;

/// <summary>
/// Testa o DecimalModelBinder isoladamente, montando um
/// DefaultModelBindingContext "de bancada" (sem subir um pipeline HTTP real
/// do ASP.NET Core) — mesma técnica usada nos testes do próprio ASP.NET Core
/// para model binders customizados.
/// </summary>
[TestFixture]
public class DecimalModelBinderTests
{
    private static readonly EmptyModelMetadataProvider MetadataProvider = new();

    private static ModelBindingContext CriarContexto(Type tipoModelo, string? valorBruto)
    {
        const string modelName = "Valor";

        var metadata = MetadataProvider.GetMetadataForType(tipoModelo);
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var valueProvider = new FakeValueProvider(modelName, valorBruto);

        return DefaultModelBindingContext.CreateBindingContext(
            actionContext,
            valueProvider,
            metadata,
            bindingInfo: new BindingInfo(),
            modelName: modelName);
    }

    private static async Task<ModelBindingContext> ExecutarBindAsync(Type tipoModelo, string? valorBruto)
    {
        var contexto = CriarContexto(tipoModelo, valorBruto);
        await new DecimalModelBinder().BindModelAsync(contexto);
        return contexto;
    }

    [TestCase("3,45", 3.45)]
    [TestCase("3.45", 3.45)]
    [TestCase("100", 100)]
    [TestCase("0,5", 0.5)]
    [TestCase("7.000,00", 7000.00)]
    [TestCase("7,000.00", 7000.00)]
    [TestCase("-3,45", -3.45)]
    public async Task BindModelAsync_AceitaFormatosValidos_EConverteParaODecimalEsperado(string entrada, double esperadoDouble)
    {
        var contexto = await ExecutarBindAsync(typeof(decimal), entrada);

        Assert.That(contexto.Result.IsModelSet, Is.True);
        Assert.That((decimal)contexto.Result.Model!, Is.EqualTo((decimal)esperadoDouble));
    }

    [TestCase("abc")]
    [TestCase("3,45,67")]
    [TestCase("R$ 10")]
    public async Task BindModelAsync_TextoInvalido_AdicionaErroDeModelState_ENaoDefineResultado(string entradaInvalida)
    {
        var contexto = await ExecutarBindAsync(typeof(decimal), entradaInvalida);

        Assert.That(contexto.ModelState.ErrorCount, Is.GreaterThan(0));
        Assert.That(contexto.Result.IsModelSet, Is.False);
    }

    [Test]
    public async Task BindModelAsync_CampoVazio_TipoNaoAnulavel_NaoDefineResultado_DeixaValidacaoDeRequiredAgir()
    {
        var contexto = await ExecutarBindAsync(typeof(decimal), "");

        Assert.That(contexto.Result.IsModelSet, Is.False);
        Assert.That(contexto.ModelState.ErrorCount, Is.EqualTo(0));
    }

    [Test]
    public async Task BindModelAsync_CampoComEspacos_TipoNaoAnulavel_TratadoComoVazio()
    {
        var contexto = await ExecutarBindAsync(typeof(decimal), "   ");

        Assert.That(contexto.Result.IsModelSet, Is.False);
        Assert.That(contexto.ModelState.ErrorCount, Is.EqualTo(0));
    }

    [Test]
    public async Task BindModelAsync_CampoVazio_TipoAnulavel_DefineResultadoComoNull()
    {
        var contexto = await ExecutarBindAsync(typeof(decimal?), "");

        Assert.That(contexto.Result.IsModelSet, Is.True);
        Assert.That(contexto.Result.Model, Is.Null);
    }

    [Test]
    public async Task BindModelAsync_FormatoValido_TipoAnulavel_DefineResultadoComValor()
    {
        var contexto = await ExecutarBindAsync(typeof(decimal?), "12,50");

        Assert.That(contexto.Result.IsModelSet, Is.True);
        Assert.That(contexto.Result.Model, Is.EqualTo(12.50m));
    }

    [Test]
    public async Task BindModelAsync_ValorNaoFornecidoNaRequisicao_NaoDefineResultado_ENaoAdicionaErro()
    {
        var contexto = await ExecutarBindAsync(typeof(decimal), valorBruto: null);

        Assert.That(contexto.Result.IsModelSet, Is.False);
        Assert.That(contexto.ModelState.ErrorCount, Is.EqualTo(0));
    }

    [Test]
    public void BindModelAsync_ContextoNulo_LancaArgumentNullException()
    {
        var binder = new DecimalModelBinder();

        Assert.ThrowsAsync<ArgumentNullException>(async () => await binder.BindModelAsync(null!));
    }
}

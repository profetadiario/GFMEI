using GestaoFinanceiraMEI.Infraestrutura;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Infraestrutura;

[TestFixture]
public class DecimalModelBinderProviderTests
{
    private static readonly EmptyModelMetadataProvider MetadataProvider = new();
    private readonly DecimalModelBinderProvider _provider = new();

    /// <summary>
    /// Implementação mínima de ModelBinderProviderContext (classe abstrata
    /// sem implementação pública própria) só com o necessário para exercitar
    /// DecimalModelBinderProvider.GetBinder em teste unitário.
    /// </summary>
    private class FakeModelBinderProviderContext : ModelBinderProviderContext
    {
        public FakeModelBinderProviderContext(ModelMetadata metadata)
        {
            Metadata = metadata;
        }

        public override BindingInfo BindingInfo { get; } = new();
        public override ModelMetadata Metadata { get; }
        public override IModelMetadataProvider MetadataProvider => DecimalModelBinderProviderTests.MetadataProvider;
        public override IModelBinder CreateBinder(ModelMetadata metadata) => throw new NotSupportedException();
    }

    [TestCase(typeof(decimal))]
    [TestCase(typeof(decimal?))]
    public void GetBinder_ParaTipoDecimalOuNulavel_RetornaDecimalModelBinder(Type tipo)
    {
        var metadata = MetadataProvider.GetMetadataForType(tipo);
        var contexto = new FakeModelBinderProviderContext(metadata);

        var binder = _provider.GetBinder(contexto);

        Assert.That(binder, Is.InstanceOf<DecimalModelBinder>());
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(int))]
    [TestCase(typeof(int?))]
    [TestCase(typeof(DateTime))]
    public void GetBinder_ParaOutrosTipos_RetornaNull(Type tipo)
    {
        var metadata = MetadataProvider.GetMetadataForType(tipo);
        var contexto = new FakeModelBinderProviderContext(metadata);

        var binder = _provider.GetBinder(contexto);

        Assert.That(binder, Is.Null);
    }

    [Test]
    public void GetBinder_ContextoNulo_LancaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetBinder(null!));
    }
}

using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

/// <summary>
/// Calculadora pelo método da planilha da Adriana (decisão 9). Os valores
/// mensais do caso de teste foram escolhidos para reproduzir, item a item
/// rateado, os números da planilha registrados no DECISOES (24 dias, 8 h,
/// 90 min; custos fixos 21,33; materiais 14,95; despesas fixas 0,60 + 1,31;
/// despesas variáveis 1,17 + 0,31).
/// </summary>
[TestFixture]
public class CalculadoraPrecoServiceTests
{
    private AppDbContext _context = null!;
    private CalculadoraPrecoService _service = null!;
    private const int UsuarioId = 1;
    private static readonly DateTime Hoje = new(2026, 11, 23);

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _service = new CalculadoraPrecoService(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    /// <summary>Caso da planilha da Adriana.</summary>
    public static EntradaCalculadoraPreco CasoAdriana(decimal lucro = 30m) => new()
    {
        DiasTrabalho = 24,
        HorasPorDia = 8,
        MinutosPorUnidade = 90,
        CustoVariavelUnidade = 14.95m,
        LucroPercentual = lucro,
        CustosFixos = new()
        {
            new() { Nome = "Energia", Valor = 150m },                  // 1,17
            new() { Nome = "Água", Valor = 32m },                      // 0,25
            new() { Nome = "Internet", Valor = 250m },                 // 1,95
            new() { Nome = "Aluguel", Valor = 300m },                  // 2,34
            new() { Nome = "Pró-labore (seu salário)", Valor = 2000m } // 15,625 → 15,62
        },
        DespesasFixas = new()
        {
            new() { Nome = "DAS", Valor = 76.90m },  // 0,60
            new() { Nome = "IPTU", Valor = 168m }    // 1,31
        },
        DespesasVariaveis = new()
        {
            new() { Nome = "Gasolina", Valor = 150m },       // 1,17
            new() { Nome = "Estacionamento", Valor = 40m }   // 0,31
        }
    };

    // ---------------- caso da planilha da Adriana ----------------

    [Test]
    public void Calcular_CasoAdriana_Lucro30_Markup162_Preco5877()
    {
        var r = _service.Calcular(CasoAdriana());

        Assert.That(r.Valido, Is.True, string.Join("; ", r.Erros));
        Assert.That(r.CustosFixosUnidade, Is.EqualTo(21.33m));
        Assert.That(r.CustoVariavelUnidade, Is.EqualTo(14.95m));
        Assert.That(r.Base, Is.EqualTo(36.28m));
        Assert.That(r.DespesasFixasUnidade, Is.EqualTo(1.91m));
        Assert.That(r.DespesasVariaveisUnidade, Is.EqualTo(1.48m));
        Assert.That(r.TotalGeral, Is.EqualTo(39.67m));
        Assert.That(r.DespesasFixasPercentual, Is.EqualTo(4.81m));
        Assert.That(r.DespesasVariaveisPercentual, Is.EqualTo(3.73m));
        Assert.That(r.Markup, Is.EqualTo(1.62m));
        Assert.That(r.PrecoSugerido, Is.EqualTo(58.77m));
    }

    [Test]
    public void Calcular_CasoAdriana_Lucro20_Markup139_Preco5042()
    {
        var r = _service.Calcular(CasoAdriana(lucro: 20m));

        Assert.That(r.Markup, Is.EqualTo(1.39m));
        Assert.That(r.PrecoSugerido, Is.EqualTo(50.42m));
    }

    [Test]
    public void Calcular_CasoAdriana_RateiaCadaItemTruncandoEmDuasCasas()
    {
        var r = _service.Calcular(CasoAdriana());

        Assert.That(r.CustosFixos.Select(i => i.PorUnidade), Is.EqualTo(new[] { 1.17m, 0.25m, 1.95m, 2.34m, 15.62m }));
        Assert.That(r.DespesasFixas.Select(i => i.PorUnidade), Is.EqualTo(new[] { 0.60m, 1.31m }));
        Assert.That(r.DespesasVariaveis.Select(i => i.PorUnidade), Is.EqualTo(new[] { 1.17m, 0.31m }));
        Assert.That(r.CustosFixos[^1].ValorMensal, Is.EqualTo(2000m));
    }

    [Test]
    public void Calcular_CasoAdriana_LucroEmReaisEhOPercentualDoPreco()
    {
        var r = _service.Calcular(CasoAdriana());

        Assert.That(r.LucroPorUnidade, Is.EqualTo(17.63m)); // 30% de 58,77 = 17,631
    }

    [TestCase(1.3889, 1.38)]
    [TestCase(15.625, 15.62)]
    [TestCase(1.958, 1.95)]
    [TestCase(0.999, 0.99)]
    [TestCase(2, 2)]
    public void Truncar_CortaEmDuasCasas_SemArredondar(decimal valor, decimal esperado)
    {
        Assert.That(CalculadoraPrecoService.Truncar(valor), Is.EqualTo(esperado));
    }

    [Test]
    public void Calcular_SoMateriais_SemValoresMensais_PrecoEhMateriaisVezesMarkupDoLucro()
    {
        var r = _service.Calcular(new EntradaCalculadoraPreco { DiasTrabalho = 20, HorasPorDia = 8, MinutosPorUnidade = 30, CustoVariavelUnidade = 10m, LucroPercentual = 50m });

        Assert.That(r.Valido, Is.True);
        Assert.That(r.Markup, Is.EqualTo(2m));
        Assert.That(r.PrecoSugerido, Is.EqualTo(20m));
        Assert.That(r.DespesasFixasPercentual, Is.EqualTo(0m));
    }

    [Test]
    public void Calcular_IgnoraLinhasEmBrancoOuZeradas()
    {
        var entrada = CasoAdriana();
        entrada.CustosFixos.Add(new ItemMensal { Nome = "Sem valor" });
        entrada.CustosFixos.Add(new ItemMensal { Nome = "Zerado", Valor = 0m });
        entrada.DespesasFixas.Add(new ItemMensal());

        var r = _service.Calcular(entrada);

        Assert.That(r.CustosFixos, Has.Count.EqualTo(5));
        Assert.That(r.PrecoSugerido, Is.EqualTo(58.77m));
    }

    // ---------------- limites ----------------

    [Test]
    public void Calcular_SomaDosPercentuaisChegaA100_Bloqueia()
    {
        var r = _service.Calcular(CasoAdriana(lucro: 91.46m)); // 4,81 + 3,73 + 91,46 = 100

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("menos de 100%"));
    }

    [Test]
    public void Calcular_SomaLogoAbaixoDe100_Calcula()
    {
        var r = _service.Calcular(CasoAdriana(lucro: 91.45m));

        Assert.That(r.Valido, Is.True);
        Assert.That(r.Markup, Is.EqualTo(10000m));
    }

    [TestCase(null, 8, 90)]
    [TestCase(0, 8, 90)]
    [TestCase(32, 8, 90)]
    [TestCase(24, null, 90)]
    [TestCase(24, 25, 90)]
    [TestCase(24, 8, null)]
    [TestCase(24, 8, 0)]
    public void Calcular_TempoInvalido_Bloqueia(int? dias, int? horas, int? minutos)
    {
        var entrada = CasoAdriana();
        entrada.DiasTrabalho = dias;
        entrada.HorasPorDia = horas;
        entrada.MinutosPorUnidade = minutos;

        Assert.That(_service.Calcular(entrada).Valido, Is.False);
    }

    [Test]
    public void Calcular_ValoresNegativos_Bloqueia()
    {
        var entrada = CasoAdriana();
        entrada.DespesasVariaveis[0].Valor = -1m;
        Assert.That(_service.Calcular(entrada).Erros, Has.Some.Contains("não podem ser negativos"));

        Assert.That(_service.Calcular(new EntradaCalculadoraPreco { DiasTrabalho = 24, HorasPorDia = 8, MinutosPorUnidade = 90, CustoVariavelUnidade = -5m }).Valido, Is.False);
        Assert.That(_service.Calcular(CasoAdriana(lucro: -1m)).Valido, Is.False);
    }

    [Test]
    public void Calcular_SemNenhumCusto_PedeUmCusto()
    {
        var r = _service.Calcular(new EntradaCalculadoraPreco { DiasTrabalho = 24, HorasPorDia = 8, MinutosPorUnidade = 90, LucroPercentual = 30m });

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("pelo menos um custo"));
    }

    // ---------------- valores sugeridos pelos lançamentos ----------------

    private void Categoria(int id, string nome, NaturezaDespesa natureza, TipoTransacao tipo = TipoTransacao.Despesa, int usuario = UsuarioId)
    {
        _context.Categorias.Add(new Categoria { Id = id, Nome = nome, Tipo = tipo, NaturezaDespesa = natureza, UsuarioId = usuario });
        _context.SaveChanges();
    }

    private void Lancar(int categoriaId, decimal valor, DateTime data, TipoTransacao tipo = TipoTransacao.Despesa, int usuario = UsuarioId)
    {
        _context.Transacoes.Add(new Transacao { Descricao = "x", Valor = valor, Tipo = tipo, Data = data, CategoriaId = categoriaId, UsuarioId = usuario });
        _context.SaveChanges();
    }

    [Test]
    public async Task Sugestao_SemHistorico_SoMostraOProLaboreEmBranco()
    {
        var s = await _service.ObterValoresSugeridosAsync(UsuarioId, Hoje);

        Assert.That(s.TemSugestao, Is.False);
        Assert.That(s.CustosFixos.Single().Nome, Is.EqualTo(CategoriasPadrao.NomeProLabore));
        Assert.That(s.CustosFixos.Single().MediaMensal, Is.EqualTo(0m));
        Assert.That(s.DespesasFixas, Is.Empty);
    }

    [Test]
    public async Task Sugestao_MediaDosMesesFechados_PorGrupo_ComProLaboreNosCustosFixos()
    {
        Categoria(1, "Vendas", NaturezaDespesa.DespesaVariavel, TipoTransacao.Receita);
        Categoria(2, CategoriasPadrao.NomeProLabore, NaturezaDespesa.DespesaFixa);
        Categoria(3, "Aluguel", NaturezaDespesa.DespesaFixa);
        Categoria(4, "Impostos (DAS)", NaturezaDespesa.DeducaoOuImposto);
        Categoria(5, "Transporte", NaturezaDespesa.DespesaVariavel);
        Categoria(6, "Insumos", NaturezaDespesa.CustoMercadoriaVendida);

        foreach (var mes in new[] { 9, 10 })
        {
            var fim = new DateTime(2026, mes, 1).AddMonths(1).AddDays(-1);
            Lancar(1, 5000m, fim, TipoTransacao.Receita);
            Lancar(2, 2000m, fim);
            Lancar(3, 300m, fim);
            Lancar(4, 76.90m, fim);
            Lancar(6, 900m, fim);
        }
        Lancar(5, 100m, new DateTime(2026, 9, 10));
        Lancar(5, 200m, new DateTime(2026, 10, 10));

        var s = await _service.ObterValoresSugeridosAsync(UsuarioId, Hoje);

        Assert.That(s.MesesConsiderados, Is.EqualTo(2));
        Assert.That(s.CustosFixos.Select(v => (v.Nome, v.MediaMensal)),
            Is.EqualTo(new[] { (CategoriasPadrao.NomeProLabore, 2000m), ("Aluguel", 300m) }));
        Assert.That(s.DespesasFixas.Single().MediaMensal, Is.EqualTo(76.90m));
        Assert.That(s.DespesasVariaveis.Single().MediaMensal, Is.EqualTo(150m));
        // Insumos (CMV) ficam de fora: os materiais são digitados por unidade.
        Assert.That(s.CustosFixos.Concat(s.DespesasFixas).Concat(s.DespesasVariaveis).Any(v => v.Nome == "Insumos"), Is.False);
    }

    [Test]
    public async Task Sugestao_IgnoraOMesCorrente_PorqueOProLaboreSoEntraNoFechamento()
    {
        Categoria(2, CategoriasPadrao.NomeProLabore, NaturezaDespesa.DespesaFixa);
        Categoria(3, "Aluguel", NaturezaDespesa.DespesaFixa);
        Lancar(2, 2000m, new DateTime(2026, 10, 31));
        Lancar(3, 300m, new DateTime(2026, 10, 5));
        Lancar(3, 300m, new DateTime(2026, 11, 5)); // novembro ainda aberto

        var s = await _service.ObterValoresSugeridosAsync(UsuarioId, Hoje);

        Assert.That(s.MesesConsiderados, Is.EqualTo(1));
        Assert.That(s.CustosFixos.Single(v => v.Nome == "Aluguel").MediaMensal, Is.EqualTo(300m));
        Assert.That(s.CustosFixos.Single(v => v.Nome == CategoriasPadrao.NomeProLabore).MediaMensal, Is.EqualTo(2000m));
    }

    [Test]
    public async Task Sugestao_RetiradaAntigaEntraNosCustosFixos_EOutraUsuariaNaoInterfere()
    {
        Categoria(7, "Retirada pessoal", NaturezaDespesa.RetiradaPessoal);
        Categoria(8, "Aluguel", NaturezaDespesa.DespesaFixa, usuario: 99);
        Lancar(7, 500m, new DateTime(2026, 10, 20));
        Lancar(8, 999m, new DateTime(2026, 10, 20), usuario: 99);

        var s = await _service.ObterValoresSugeridosAsync(UsuarioId, Hoje);

        Assert.That(s.CustosFixos.Single(v => v.Nome == "Retirada pessoal").MediaMensal, Is.EqualTo(500m));
        Assert.That(s.CustosFixos.Any(v => v.MediaMensal == 999m), Is.False);
    }

    [Test]
    public async Task Sugestao_ComAsMediasDaAdriana_DaOMesmoPrecoDaPlanilha()
    {
        // Os valores sugeridos, levados à calculadora com o tempo e os
        // materiais da Adriana, reproduzem os R$ 58,77 da planilha.
        var caso = CasoAdriana();
        var id = 10;
        void Grupo(IEnumerable<ItemMensal> itens, NaturezaDespesa natureza)
        {
            foreach (var item in itens)
            {
                Categoria(++id, item.Nome!, natureza);
                Lancar(id, item.Valor!.Value, new DateTime(2026, 9, 30));
                Lancar(id, item.Valor!.Value, new DateTime(2026, 10, 31));
            }
        }
        Grupo(caso.CustosFixos, NaturezaDespesa.DespesaFixa);
        Grupo(caso.DespesasFixas, NaturezaDespesa.DeducaoOuImposto);
        Grupo(caso.DespesasVariaveis, NaturezaDespesa.DespesaVariavel);

        var s = await _service.ObterValoresSugeridosAsync(UsuarioId, Hoje);
        List<ItemMensal> Itens(List<ValorSugerido> v) => v.Select(x => new ItemMensal { Nome = x.Nome, Valor = x.MediaMensal }).ToList();
        var r = _service.Calcular(new EntradaCalculadoraPreco
        {
            DiasTrabalho = 24, HorasPorDia = 8, MinutosPorUnidade = 90, CustoVariavelUnidade = 14.95m, LucroPercentual = 30m,
            CustosFixos = Itens(s.CustosFixos), DespesasFixas = Itens(s.DespesasFixas), DespesasVariaveis = Itens(s.DespesasVariaveis)
        });

        Assert.That(r.PrecoSugerido, Is.EqualTo(58.77m));
    }
}

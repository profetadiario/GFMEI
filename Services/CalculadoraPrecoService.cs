using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <inheritdoc cref="ICalculadoraPrecoService"/>
public class CalculadoraPrecoService : ICalculadoraPrecoService
{
    private readonly AppDbContext _context;

    public CalculadoraPrecoService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Corta (não arredonda) em 2 casas decimais, como a planilha.</summary>
    public static decimal Truncar(decimal valor) => Math.Truncate(valor * 100m) / 100m;

    public ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada)
    {
        var r = new ResultadoCalculadoraPreco();

        var dias = entrada.DiasTrabalho ?? 0m;
        var horas = entrada.HorasPorDia ?? 0m;
        var minutos = entrada.MinutosPorUnidade ?? 0m;
        var materiais = entrada.CustoVariavelUnidade ?? 0m;
        var lucro = entrada.LucroPercentual ?? 0m;

        if (dias <= 0 || dias > 31) r.Erros.Add("Informe os dias de trabalho no mês (de 1 a 31).");
        if (horas <= 0 || horas > 24) r.Erros.Add("Informe as horas trabalhadas por dia (de 1 a 24).");
        if (minutos <= 0) r.Erros.Add("Informe o tempo de produção de uma unidade, em minutos.");
        if (materiais < 0) r.Erros.Add("O custo variável da unidade (materiais) não pode ser negativo.");
        if (lucro < 0) r.Erros.Add("O lucro desejado não pode ser negativo.");

        var itens = entrada.CustosFixos.Concat(entrada.DespesasFixas).Concat(entrada.DespesasVariaveis);
        if (itens.Any(i => i.Valor < 0)) r.Erros.Add("Os valores mensais não podem ser negativos.");

        if (!r.Valido) return r;

        // Rateio de cada valor mensal: valor × minutos ÷ (dias × horas × 60),
        // numa divisão só, para não acumular erro antes de truncar.
        var minutosNoMes = dias * horas * 60m;
        List<ItemRateado> Ratear(List<ItemMensal> lista) => lista
            .Where(i => i.Preenchido)
            .Select(i => new ItemRateado
            {
                Nome = string.IsNullOrWhiteSpace(i.Nome) ? "Sem nome" : i.Nome.Trim(),
                ValorMensal = i.Valor!.Value,
                PorUnidade = Truncar(i.Valor.Value * minutos / minutosNoMes)
            })
            .ToList();

        r.CustosFixos = Ratear(entrada.CustosFixos);
        r.DespesasFixas = Ratear(entrada.DespesasFixas);
        r.DespesasVariaveis = Ratear(entrada.DespesasVariaveis);

        r.CustosFixosUnidade = r.CustosFixos.Sum(i => i.PorUnidade);
        r.CustoVariavelUnidade = Truncar(materiais);
        r.DespesasFixasUnidade = r.DespesasFixas.Sum(i => i.PorUnidade);
        r.DespesasVariaveisUnidade = r.DespesasVariaveis.Sum(i => i.PorUnidade);

        r.Base = r.CustosFixosUnidade + r.CustoVariavelUnidade;
        r.TotalGeral = r.Base + r.DespesasFixasUnidade + r.DespesasVariaveisUnidade;
        r.LucroPercentual = lucro;

        if (r.Base <= 0)
        {
            r.Erros.Add("Informe pelo menos um custo (fixo ou variável) para calcular o preço.");
            return r;
        }

        r.DespesasFixasPercentual = Truncar(r.DespesasFixasUnidade / r.TotalGeral * 100m);
        r.DespesasVariaveisPercentual = Truncar(r.DespesasVariaveisUnidade / r.TotalGeral * 100m);

        var soma = r.DespesasFixasPercentual + r.DespesasVariaveisPercentual + lucro;
        if (soma >= 100m)
        {
            r.Erros.Add("Despesas fixas + despesas variáveis + lucro precisam somar menos de 100%. Com 100% ou mais, nenhum preço cobre a conta.");
            return r;
        }

        r.Markup = Truncar(100m / (100m - soma));
        r.PrecoSugerido = Truncar(r.Base * r.Markup);
        r.LucroPorUnidade = Truncar(r.PrecoSugerido * lucro / 100m);
        return r;
    }

    public async Task<SugestaoValoresMensais> ObterValoresSugeridosAsync(int usuarioId, DateTime hoje)
    {
        var inicioMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
        var inicioJanela = inicioMesAtual.AddMonths(-12);

        // Só meses fechados: o pró-labore é lançado no fechamento do mês, então
        // o mês corrente ainda está incompleto e puxaria a média para baixo.
        var transacoes = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId && t.Data >= inicioJanela && t.Data < inicioMesAtual)
            .Select(t => new { t.Data, t.Valor, t.Tipo, t.CategoriaId })
            .ToListAsync();

        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId && c.Tipo == TipoTransacao.Despesa)
            .Select(c => new { c.Id, c.Nome, c.NaturezaDespesa })
            .ToListAsync();

        var meses = transacoes.Select(t => new { t.Data.Year, t.Data.Month }).Distinct().Count();
        var sugestao = new SugestaoValoresMensais { MesesConsiderados = meses };

        List<ValorSugerido> Medias(NaturezaDespesa natureza) => categorias
            .Where(c => c.NaturezaDespesa.ParaCalculo() == natureza)
            .Select(c => new ValorSugerido
            {
                Nome = c.Nome,
                MediaMensal = meses == 0
                    ? 0m
                    : Math.Round(transacoes.Where(t => t.Tipo == TipoTransacao.Despesa && t.CategoriaId == c.Id).Sum(t => t.Valor) / meses, 2)
            })
            .Where(v => v.MediaMensal > 0)
            .OrderByDescending(v => v.MediaMensal)
            .ThenBy(v => v.Nome)
            .ToList();

        sugestao.CustosFixos = Medias(NaturezaDespesa.DespesaFixa);
        sugestao.DespesasFixas = Medias(NaturezaDespesa.DeducaoOuImposto);
        sugestao.DespesasVariaveis = Medias(NaturezaDespesa.DespesaVariavel);

        // O pró-labore aparece sempre nos custos fixos (decisão 10), mesmo
        // antes do primeiro lançamento, para a empreendedora não esquecer dele.
        if (!sugestao.CustosFixos.Any(v => v.Nome == CategoriasPadrao.NomeProLabore))
            sugestao.CustosFixos.Add(new ValorSugerido { Nome = CategoriasPadrao.NomeProLabore });

        return sugestao;
    }
}

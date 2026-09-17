using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Implementação do cálculo do DRE mensal, a partir dos lançamentos
/// (Transacao) e da natureza contábil de cada categoria de despesa
/// (Categoria.NaturezaDespesa).
/// </summary>
public class DreService : IDreService
{
    private readonly AppDbContext _context;

    public DreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DreMensal> ObterDreAsync(int usuarioId, int mes, int ano)
    {
        // SQL Server (via EF Core) não traduz Sum sobre "decimal" agrupado por
        // enum de forma direta em todo provider, então materializamos a lista
        // com ToListAsync() e somamos em memória (mesma estratégia já usada
        // no restante do sistema para evitar problemas de tradução de LINQ).
        var transacoesDoMes = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId && t.Data.Month == mes && t.Data.Year == ano)
            .ToListAsync();

        // Busca a natureza das categorias envolvidas separadamente (em vez de
        // Include(t => t.Categoria)): como CategoriaId é uma FK obrigatória,
        // o Include gera um INNER JOIN, que descartaria silenciosamente
        // qualquer lançamento cuja categoria não existe mais — perdendo valor
        // do cálculo em vez de classificá-lo como despesa variável.
        var categoriaIds = transacoesDoMes.Select(t => t.CategoriaId).Distinct().ToList();
        var naturezaPorCategoria = await _context.Categorias
            .Where(c => categoriaIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NaturezaDespesa);

        var receitaBruta = transacoesDoMes
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        var despesas = transacoesDoMes.Where(t => t.Tipo == TipoTransacao.Despesa);

        decimal SomaPorNatureza(NaturezaDespesa natureza) =>
            despesas.Where(t => naturezaPorCategoria.GetValueOrDefault(t.CategoriaId, NaturezaDespesa.DespesaVariavel) == natureza)
                    .Sum(t => t.Valor);

        return new DreMensal
        {
            Mes = mes,
            Ano = ano,
            ReceitaBrutaTotal = receitaBruta,
            DeducoesEImpostos = SomaPorNatureza(NaturezaDespesa.DeducaoOuImposto),
            CustoMercadoriaVendida = SomaPorNatureza(NaturezaDespesa.CustoMercadoriaVendida),
            DespesasVariaveis = SomaPorNatureza(NaturezaDespesa.DespesaVariavel),
            DespesasFixas = SomaPorNatureza(NaturezaDespesa.DespesaFixa)
        };
    }
}

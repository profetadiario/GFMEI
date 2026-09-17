using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Implementação do fluxo de caixa anual mês a mês, com saldo carregado de
/// um mês para o outro (e do ano anterior para janeiro).
/// </summary>
public class FluxoCaixaMensalService : IFluxoCaixaMensalService
{
    private readonly AppDbContext _context;

    // Abreviações fixas em vez de CultureInfo("pt-BR").ToString("MMM"):
    // o formato "MMM" do .NET para pt-BR depende dos dados de globalização
    // (ICU) instalados na máquina e pode incluir um ponto (ex.: "jan."),
    // o que tornaria a exibição inconsistente entre ambientes.
    private static readonly string[] NomesMesesAbreviados =
        { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

    public FluxoCaixaMensalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FluxoCaixaAnual> ObterFluxoAnualAsync(int usuarioId, int ano)
    {
        // Saldo acumulado de todos os lançamentos anteriores a 1º de janeiro
        // do ano consultado — vira o saldo inicial de janeiro.
        var transacoesAnteriores = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId && t.Data.Year < ano)
            .ToListAsync();

        var saldoAcumulado = transacoesAnteriores.Sum(t =>
            t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor);

        var transacoesDoAno = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId && t.Data.Year == ano)
            .ToListAsync();

        // Busca os nomes das categorias envolvidas separadamente (em vez de
        // Include(t => t.Categoria)): como CategoriaId é uma FK obrigatória,
        // o Include gera um INNER JOIN, que descartaria silenciosamente
        // qualquer lançamento cuja categoria não existe mais, em vez de
        // agrupá-lo como "Sem categoria".
        var categoriaIds = transacoesDoAno.Select(t => t.CategoriaId).Distinct().ToList();
        var nomePorCategoria = await _context.Categorias
            .Where(c => categoriaIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nome);

        var fluxo = new FluxoCaixaAnual
        {
            Ano = ano,
            NomesMeses = (string[])NomesMesesAbreviados.Clone()
        };

        fluxo.Entradas = MontarLinhasPorCategoria(transacoesDoAno, TipoTransacao.Receita, nomePorCategoria);
        fluxo.Saidas = MontarLinhasPorCategoria(transacoesDoAno, TipoTransacao.Despesa, nomePorCategoria);

        for (int mes = 1; mes <= 12; mes++)
        {
            var indice = mes - 1;

            fluxo.TotalEntradas[indice] = transacoesDoAno
                .Where(t => t.Tipo == TipoTransacao.Receita && t.Data.Month == mes)
                .Sum(t => t.Valor);

            fluxo.TotalSaidas[indice] = transacoesDoAno
                .Where(t => t.Tipo == TipoTransacao.Despesa && t.Data.Month == mes)
                .Sum(t => t.Valor);

            fluxo.SaldoInicial[indice] = saldoAcumulado;
            fluxo.SaldoOperacional[indice] = fluxo.TotalEntradas[indice] - fluxo.TotalSaidas[indice];
            fluxo.SaldoFinal[indice] = fluxo.SaldoInicial[indice] + fluxo.SaldoOperacional[indice];

            saldoAcumulado = fluxo.SaldoFinal[indice];
        }

        return fluxo;
    }

    private static List<LinhaCategoriaFluxo> MontarLinhasPorCategoria(List<Transacao> transacoesDoAno, TipoTransacao tipo, Dictionary<int, string> nomePorCategoria)
    {
        var porCategoria = transacoesDoAno
            .Where(t => t.Tipo == tipo)
            .GroupBy(t => nomePorCategoria.GetValueOrDefault(t.CategoriaId, "Sem categoria"))
            .OrderBy(g => g.Key);

        var linhas = new List<LinhaCategoriaFluxo>();
        foreach (var grupo in porCategoria)
        {
            var linha = new LinhaCategoriaFluxo { Nome = grupo.Key };
            foreach (var transacao in grupo)
            {
                linha.Valores[transacao.Data.Month - 1] += transacao.Valor;
            }
            linhas.Add(linha);
        }

        return linhas;
    }
}

using System.Globalization;
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
    private static readonly CultureInfo PtBr = new("pt-BR");

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
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == usuarioId && t.Data.Year == ano)
            .ToListAsync();

        var fluxo = new FluxoCaixaAnual
        {
            Ano = ano,
            NomesMeses = Enumerable.Range(1, 12)
                .Select(m => new DateTime(ano, m, 1).ToString("MMM", PtBr).ToUpper(PtBr))
                .ToArray()
        };

        fluxo.Entradas = MontarLinhasPorCategoria(transacoesDoAno, TipoTransacao.Receita);
        fluxo.Saidas = MontarLinhasPorCategoria(transacoesDoAno, TipoTransacao.Despesa);

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

    private static List<LinhaCategoriaFluxo> MontarLinhasPorCategoria(List<Transacao> transacoesDoAno, TipoTransacao tipo)
    {
        var porCategoria = transacoesDoAno
            .Where(t => t.Tipo == tipo)
            .GroupBy(t => t.Categoria?.Nome ?? "Sem categoria")
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

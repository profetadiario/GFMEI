using System.Globalization;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <inheritdoc cref="IRelatorioCreditoService"/>
public class RelatorioCreditoService : IRelatorioCreditoService
{
    private static readonly CultureInfo PtBr = new("pt-BR");
    private readonly AppDbContext _context;

    public RelatorioCreditoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioCredito> ObterRelatorioAsync(int usuarioId, DateTime hoje)
    {
        var inicioMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
        var inicioJanela = inicioMesAtual.AddMonths(-11);
        var fimJanela = inicioMesAtual.AddMonths(1); // exclusivo

        // Três consultas somente leitura: lançamentos até o fim do mês atual
        // (o histórico anterior à janela entra só no saldo acumulado),
        // categorias (natureza de cada despesa) e captações.
        var transacoes = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId && t.Data < fimJanela)
            .Select(t => new { t.Data, t.Valor, t.Tipo, t.CategoriaId })
            .ToListAsync();

        var naturezaPorCategoria = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .ToDictionaryAsync(c => c.Id, c => c.NaturezaDespesa);

        var captacoes = await _context.Captacoes
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => c.Valor)
            .ToListAsync();

        // Mesma regra do DRE: despesa sem natureza conhecida conta como variável.
        NaturezaDespesa NaturezaDe(int categoriaId) =>
            naturezaPorCategoria.GetValueOrDefault(categoriaId, NaturezaDespesa.DespesaVariavel);

        var saldoAntesDaJanela = transacoes
            .Where(t => t.Data < inicioJanela)
            .Sum(t => t.Tipo == TipoTransacao.Receita ? t.Valor : -t.Valor);

        var relatorio = new RelatorioCredito { EmitidoEm = hoje };
        var saldo = saldoAntesDaJanela;

        for (var i = 0; i < 12; i++)
        {
            var inicio = inicioJanela.AddMonths(i);
            var doMes = transacoes.Where(t => t.Data >= inicio && t.Data < inicio.AddMonths(1)).ToList();
            var despesas = doMes.Where(t => t.Tipo == TipoTransacao.Despesa).ToList();

            decimal Soma(NaturezaDespesa natureza) =>
                despesas.Where(t => NaturezaDe(t.CategoriaId) == natureza).Sum(t => t.Valor);

            var mes = new MesRelatorio
            {
                Mes = inicio.Month,
                Ano = inicio.Year,
                Nome = inicio.ToString("MMM/yyyy", PtBr),
                Receitas = doMes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
                DeducoesEImpostos = Soma(NaturezaDespesa.DeducaoOuImposto),
                CustoMercadoriaVendida = Soma(NaturezaDespesa.CustoMercadoriaVendida),
                DespesasVariaveis = Soma(NaturezaDespesa.DespesaVariavel),
                DespesasFixas = Soma(NaturezaDespesa.DespesaFixa),
                RetiradasPessoais = Soma(NaturezaDespesa.RetiradaPessoal),
                TeveMovimento = doMes.Count > 0
            };

            saldo += mes.Receitas - despesas.Sum(t => t.Valor);
            mes.SaldoFinal = saldo;
            relatorio.Meses.Add(mes);
        }

        // Para as médias, só valem os meses a partir do primeiro com
        // lançamento. Os meses anteriores continuam na tabela (zerados), para
        // o banco ver a janela completa de 12 meses.
        var primeiroComMovimento = relatorio.Meses.FindIndex(m => m.TeveMovimento);
        relatorio.MesesConsiderados = primeiroComMovimento < 0 ? 0 : 12 - primeiroComMovimento;

        relatorio.TotalCaptado = captacoes.Sum();
        relatorio.QuantidadeCaptacoes = captacoes.Count;

        return relatorio;
    }
}

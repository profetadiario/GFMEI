using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// CRUD de metas financeiras, correspondendo à competência de
/// planejamento financeiro do referencial teórico do TCC.
/// </summary>
public class MetasController : AutenticadoController
{
    private readonly AppDbContext _context;

    public MetasController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var metas = await _context.Metas
            .Where(m => m.UsuarioId == UsuarioId)
            .OrderByDescending(m => m.MesReferencia)
            .ToListAsync();

        var itens = new List<MetaProgressoViewModel>();
        foreach (var meta in metas)
        {
            // SQLite (via EF Core) não traduz Sum/Average sobre "decimal" para SQL,
            // então materializamos a lista com ToListAsync() e somamos em memória
            // (LINQ to Objects) em vez de usar SumAsync diretamente na query.
            var receitasDoMes = (await _context.Transacoes
                .Where(t => t.UsuarioId == UsuarioId
                            && t.Tipo == TipoTransacao.Receita
                            && t.Data.Month == meta.MesReferencia.Month
                            && t.Data.Year == meta.MesReferencia.Year)
                .ToListAsync())
                .Sum(t => t.Valor);

            itens.Add(new MetaProgressoViewModel
            {
                Meta = meta,
                ValorAlcancado = receitasDoMes
            });
        }

        return View(itens);
    }

    public IActionResult Create() =>
        View(new MetaFinanceira { MesReferencia = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descricao,ValorMeta,MesReferencia")] MetaFinanceira meta)
    {
        if (!ModelState.IsValid)
            return View(meta);

        meta.UsuarioId = UsuarioId;
        _context.Add(meta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == UsuarioId);
        if (meta is null) return NotFound();

        return View(meta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,ValorMeta,MesReferencia")] MetaFinanceira meta)
    {
        if (id != meta.Id) return NotFound();

        var existente = await _context.Metas.FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == UsuarioId);
        if (existente is null) return NotFound();

        if (!ModelState.IsValid)
            return View(meta);

        existente.Descricao = meta.Descricao;
        existente.ValorMeta = meta.ValorMeta;
        existente.MesReferencia = meta.MesReferencia;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == UsuarioId);
        if (meta is null) return NotFound();

        return View(meta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == UsuarioId);
        if (meta is null) return NotFound();

        _context.Metas.Remove(meta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

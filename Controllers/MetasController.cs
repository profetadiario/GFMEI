using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
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
    private readonly IDreService _dreService;

    public MetasController(AppDbContext context, IDreService dreService)
    {
        _context = context;
        _dreService = dreService;
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
            // A meta representa lucro desejado no mês, então o valor
            // "alcançado" é o Lucro Líquido apurado no DRE daquele mês
            // (e não mais a receita bruta).
            var dreDoMes = await _dreService.ObterDreAsync(UsuarioId, meta.MesReferencia.Month, meta.MesReferencia.Year);

            itens.Add(new MetaProgressoViewModel
            {
                Meta = meta,
                ValorAlcancado = dreDoMes.LucroLiquido
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

using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// CRUD de categorias de receita/despesa, usadas para classificar os
/// lançamentos financeiros de cada usuária.
/// </summary>
public class CategoriasController : AutenticadoController
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categorias = await _context.Categorias
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nome)
            .ToListAsync();
        return View(categorias);
    }

    public IActionResult Create() => View(new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Tipo,NaturezaDespesa")] Categoria categoria)
    {
        if (!ModelState.IsValid)
            return View(categoria);

        categoria.UsuarioId = UsuarioId;
        _context.Add(categoria);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (categoria is null) return NotFound();

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Tipo,NaturezaDespesa")] Categoria categoria)
    {
        if (id != categoria.Id) return NotFound();

        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (categoriaExistente is null) return NotFound();

        if (!ModelState.IsValid)
            return View(categoria);

        categoriaExistente.Nome = categoria.Nome;
        categoriaExistente.Tipo = categoria.Tipo;
        categoriaExistente.NaturezaDespesa = categoria.NaturezaDespesa;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (categoria is null) return NotFound();

        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (categoria is null) return NotFound();

        var emUso = await _context.Transacoes.AnyAsync(t => t.CategoriaId == id);
        if (emUso)
        {
            TempData["Erro"] = "Não é possível excluir uma categoria que já possui transações vinculadas.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

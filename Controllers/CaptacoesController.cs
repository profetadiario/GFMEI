using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// CRUD de captação de recursos (empréstimos, microcrédito), correspondendo
/// à competência de captação de recursos do referencial teórico do TCC.
/// </summary>
public class CaptacoesController : AutenticadoController
{
    private readonly AppDbContext _context;

    public CaptacoesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var captacoes = await _context.Captacoes
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderByDescending(c => c.DataObtencao)
            .ToListAsync();
        return View(captacoes);
    }

    public IActionResult Create() => View(new CaptacaoRecurso { DataObtencao = DateTime.Today });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("InstituicaoFinanceira,Valor,TaxaJurosMensal,Finalidade,DataObtencao")] CaptacaoRecurso captacao)
    {
        if (!ModelState.IsValid)
            return View(captacao);

        captacao.UsuarioId = UsuarioId;
        _context.Add(captacao);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var captacao = await _context.Captacoes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (captacao is null) return NotFound();

        return View(captacao);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, [Bind("Id,InstituicaoFinanceira,Valor,TaxaJurosMensal,Finalidade,DataObtencao")] CaptacaoRecurso captacao)
    {
        if (id != captacao.Id) return NotFound();

        var existente = await _context.Captacoes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (existente is null) return NotFound();

        if (!ModelState.IsValid)
            return View(captacao);

        existente.InstituicaoFinanceira = captacao.InstituicaoFinanceira;
        existente.Valor = captacao.Valor;
        existente.TaxaJurosMensal = captacao.TaxaJurosMensal;
        existente.Finalidade = captacao.Finalidade;
        existente.DataObtencao = captacao.DataObtencao;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var captacao = await _context.Captacoes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (captacao is null) return NotFound();

        return View(captacao);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var captacao = await _context.Captacoes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
        if (captacao is null) return NotFound();

        _context.Captacoes.Remove(captacao);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

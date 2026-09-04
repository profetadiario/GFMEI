using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Controller-base para todas as áreas que exigem usuária autenticada.
/// Centraliza a leitura do Id do usuário logado a partir dos claims do cookie.
/// </summary>
[Authorize]
public abstract class AutenticadoController : Controller
{
    protected int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

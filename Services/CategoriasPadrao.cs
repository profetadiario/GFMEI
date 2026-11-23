using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Categorias que toda usuária recebe. O pró-labore (decisão 10) também é
/// garantido para quem se cadastrou antes dele existir, de forma idempotente
/// e sem migração de esquema: a categoria é procurada pelo nome e só é
/// criada se ainda não existir.
/// </summary>
public static class CategoriasPadrao
{
    public const string NomeProLabore = "Pró-labore (seu salário)";

    public const string AjudaProLabore = "Lance o pró-labore uma vez, no fechamento do mês.";

    public static List<Categoria> Criar(int usuarioId) => new()
    {
        new() { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = usuarioId },
        new() { Nome = "Prestação de serviços", Tipo = TipoTransacao.Receita, UsuarioId = usuarioId },
        new() { Nome = "Outras receitas", Tipo = TipoTransacao.Receita, UsuarioId = usuarioId },
        new() { Nome = "Fornecedores/Insumos", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.CustoMercadoriaVendida, UsuarioId = usuarioId },
        new() { Nome = "Aluguel", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = usuarioId },
        new() { Nome = NomeProLabore, Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = usuarioId },
        new() { Nome = "Impostos (DAS)", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DeducaoOuImposto, UsuarioId = usuarioId },
        new() { Nome = "Transporte", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = usuarioId },
        new() { Nome = "Outras despesas", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = usuarioId }
    };

    public static bool EhProLabore(Categoria? categoria) =>
        categoria is not null && categoria.Tipo == TipoTransacao.Despesa && categoria.Nome == NomeProLabore;

    /// <summary>Devolve o Id da categoria de pró-labore da usuária, criando-a se não existir.</summary>
    public static async Task<int> GarantirProLaboreAsync(AppDbContext context, int usuarioId)
    {
        var existente = await context.Categorias
            .Where(c => c.UsuarioId == usuarioId && c.Tipo == TipoTransacao.Despesa && c.Nome == NomeProLabore)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
        if (existente.HasValue)
            return existente.Value;

        var nova = new Categoria
        {
            Nome = NomeProLabore,
            Tipo = TipoTransacao.Despesa,
            NaturezaDespesa = NaturezaDespesa.DespesaFixa,
            UsuarioId = usuarioId
        };
        context.Categorias.Add(nova);
        await context.SaveChangesAsync();
        return nova.Id;
    }
}

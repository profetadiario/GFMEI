# Decisão 10 — retiradas pessoais já gravadas no Somee

> Preparado em 23/11/2026. **Nada foi executado no banco de produção.** O passo 1 só lê; o passo 2 altera dados e **só roda com o ok do Enrique**.

## Como o sistema trata esses lançamentos agora (sem converter nada)

- O valor `NaturezaDespesa.RetiradaPessoal = 4` **continua no enum**, para não quebrar lançamentos já gravados. Ele saiu de todas as telas: não aparece no Novo lançamento nem na lista de naturezas em Categorias.
- Nos cálculos (DRE, Painel, Lançamentos, Relatório para o banco, indicador de parcela, sugestão da Calculadora), um lançamento antigo de retirada **conta como despesa fixa, igual ao pró-labore** (`NaturezaDespesaExtensions.ParaCalculo`). A categoria aparece como "Despesa fixa" na lista de Categorias, mas mantém o nome antigo ("Retirada pessoal").
- Consequência para quem já lançou retiradas: o lucro líquido dos meses com retirada **diminui** (antes a retirada ficava fora do DRE). É a leitura da decisão 10: a remuneração da empreendedora é o pró-labore, que é custo fixo.

## 1. Verificação (só leitura) — rodar no painel do Somee (*Databases → SQL query*)

```sql
-- Categorias com natureza 4 (retirada pessoal) e quantos lançamentos cada uma tem
SELECT c.UsuarioId, c.Id AS CategoriaId, c.Nome,
       COUNT(t.Id) AS Lancamentos, ISNULL(SUM(t.Valor), 0) AS Total,
       MIN(t.Data) AS Primeiro, MAX(t.Data) AS Ultimo
FROM Categorias c
LEFT JOIN Transacoes t ON t.CategoriaId = c.Id
WHERE c.NaturezaDespesa = 4
GROUP BY c.UsuarioId, c.Id, c.Nome
ORDER BY c.UsuarioId;

-- Contas de teste x usuárias reais (para separar o que é demo/Cypress)
SELECT u.Id, u.Email, u.DataCadastro
FROM Usuarios u
WHERE u.Id IN (SELECT UsuarioId FROM Categorias WHERE NaturezaDespesa = 4);
```

Se a primeira consulta voltar vazia, não há nada a converter. Nesse caso, o passo 2 não é necessário.

## 2. Conversão proposta (altera dados — só com ok)

A proposta é levar cada lançamento antigo para a categoria **"Pró-labore (seu salário)"** da mesma usuária, que tem natureza despesa fixa. Isso não muda nenhum número, porque o sistema já soma esses lançamentos como despesa fixa. O que muda é que eles passam a aparecer com o nome certo. Depois do deploy, o próprio sistema cria a categoria de pró-labore no login. Por isso o script trata os dois casos:

```sql
BEGIN TRAN;

-- (a) Usuária que ainda não tem pró-labore: renomeia a categoria antiga.
UPDATE c SET c.Nome = N'Pró-labore (seu salário)', c.NaturezaDespesa = 3
FROM Categorias c
WHERE c.NaturezaDespesa = 4
  AND NOT EXISTS (SELECT 1 FROM Categorias p
                  WHERE p.UsuarioId = c.UsuarioId AND p.Tipo = 2
                    AND p.Nome = N'Pró-labore (seu salário)');

-- (b) Usuária que já tem pró-labore: move os lançamentos e apaga a categoria antiga.
UPDATE t SET t.CategoriaId = p.Id
FROM Transacoes t
JOIN Categorias c ON c.Id = t.CategoriaId AND c.NaturezaDespesa = 4
JOIN Categorias p ON p.UsuarioId = c.UsuarioId AND p.Tipo = 2
                 AND p.Nome = N'Pró-labore (seu salário)' AND p.Id <> c.Id;

DELETE c FROM Categorias c
WHERE c.NaturezaDespesa = 4
  AND NOT EXISTS (SELECT 1 FROM Transacoes t WHERE t.CategoriaId = c.Id);

-- Conferência: tem que voltar 0.
SELECT COUNT(*) AS RestamComNatureza4 FROM Categorias WHERE NaturezaDespesa = 4;

-- COMMIT;   -- só depois de conferir o resultado acima
-- ROLLBACK; -- para desfazer
```

Antes de rodar o passo 2, faça o backup do banco (passo 2 de `docs/DEPLOY_V1.5.md`). A descrição dos lançamentos ("Retirada pessoal") fica como está. Se quiser trocar também, use `UPDATE Transacoes SET Descricao = N'Pró-labore (seu salário)' WHERE Descricao = N'Retirada pessoal'` dentro da mesma transação.

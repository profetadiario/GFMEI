# Controle Financeiro MEI

Sistema web simples de controle financeiro para microempreendedoras individuais (MEI),
desenvolvido em **C# / ASP.NET Core MVC** com **Entity Framework Core** e banco de dados
**SQL Server (MS SQL Express)**, como protótipo do Trabalho de Conclusão de Curso *"Análise
do controle financeiro dos microempreendimentos individuais (MEI) liderados por mulheres em
Juiz de Fora - MG"*.

## Como executar

Pré-requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e um SQL
Server local (por exemplo, o LocalDB que acompanha o Visual Studio).

```bash
cd GestaoFinanceiraMEI
dotnet restore
dotnet run
```

Depois, acesse `http://localhost:5100` no navegador. Na primeira execução, o esquema do
banco de dados é criado automaticamente a partir do modelo (usando `Database.EnsureCreated()`),
sem necessidade de rodar migrações formais do Entity Framework.

> **Nota técnica:** como o sistema já está publicado com dados reais de usuárias, uma
> eventual alteração de esquema (ex.: novas colunas) não pode depender só do
> `EnsureCreated()`, que não altera um banco já existente. Para esses casos, o
> `Program.cs` roda um pequeno ajuste de esquema idempotente logo na inicialização
> (verifica se a coluna já existe antes de adicioná-la) — uma opção deliberadamente mais
> simples do que adotar EF Core Migrations completo, adequada ao escopo de um protótipo
> acadêmico com poucas alterações de esquema ao longo do tempo.

## Módulos do sistema

| Módulo | Competência de gestão financeira (referencial teórico do TCC) |
|---|---|
| Metas Financeiras | Planejamento financeiro |
| Lançamentos (receitas/despesas) | Controle de custos |
| DRE (Demonstrativo de Resultado do Exercício) | Análise financeira / controle de custos |
| Fluxo de Caixa (mensal, janeiro a dezembro) | Gestão de fluxo de caixa |
| Categorias | Classificação dos lançamentos (apoio ao DRE e ao fluxo de caixa) |
| Painel (Dashboard) | Análise financeira |
| Captação de Recursos | Captação de recursos |

O DRE e o Fluxo de Caixa foram incluídos a partir de uma rodada de revisão da
orientadora do TCC, para que o sistema efetivamente calcule (e não apenas registre)
o resultado financeiro do negócio mês a mês.

## Estrutura do projeto

```
GestaoFinanceiraMEI/
├── Controllers/     Lógica de requisição/resposta (padrão MVC)
├── Models/           Entidades do domínio (mapeadas pelo EF Core)
├── ViewModels/        Modelos auxiliares específicos de tela
├── Services/          Regras de negócio (DRE, fluxo de caixa, hash de senha)
├── Data/               Contexto do banco de dados (AppDbContext)
├── Views/               Telas Razor (.cshtml)
└── wwwroot/              Arquivos estáticos (CSS)
```

## Autenticação

Cada MEI cria sua própria conta (e-mail + senha). As senhas são armazenadas com hash
PBKDF2 (nunca em texto puro) e a sessão é mantida por cookie de autenticação do
ASP.NET Core. Ao se cadastrar, um conjunto de categorias padrão de receita/despesa
(já classificadas para o DRE — ex.: Aluguel como despesa fixa, Fornecedores/Insumos
como CMV) é criado automaticamente para facilitar o primeiro uso.

## DRE e natureza das categorias

Para o DRE separar corretamente Custo da Mercadoria Vendida, despesas fixas, despesas
variáveis e deduções/impostos, cada categoria de despesa tem um campo de **natureza**
(`Categoria.NaturezaDespesa`), definido em Categorias → Nova/Editar. Categorias de
receita não usam esse campo. Categorias criadas antes dessa funcionalidade existir
recebem automaticamente a natureza "Despesa variável" e podem ser reclassificadas a
qualquer momento.

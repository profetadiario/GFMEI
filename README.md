# Controle Financeiro MEI

Sistema web simples de controle financeiro para microempreendedoras individuais (MEI),
desenvolvido em **C# / ASP.NET Core MVC** com **Entity Framework Core** e banco de dados
**SQLite**, como protótipo do Trabalho de Conclusão de Curso *"Análise do controle
financeiro dos microempreendimentos individuais (MEI) liderados por mulheres em
Juiz de Fora - MG"*.

## Como executar

Pré-requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

```bash
cd GestaoFinanceiraMEI
dotnet restore
dotnet run
```

Depois, acesse `http://localhost:5100` no navegador. Na primeira execução, o banco de
dados SQLite (`gestaofinanceira.db`) é criado automaticamente com base no modelo de
dados (usando `Database.EnsureCreated()`), sem necessidade de rodar migrações.

## Módulos do sistema

| Módulo | Competência de gestão financeira (referencial teórico do TCC) |
|---|---|
| Metas Financeiras | Planejamento financeiro |
| Categorias / Fluxo de Caixa | Controle de custos |
| Fluxo de Caixa | Gestão de fluxo de caixa |
| Painel (Dashboard) | Análise financeira |
| Captação de Recursos | Captação de recursos |

## Estrutura do projeto

```
GestaoFinanceiraMEI/
├── Controllers/     Lógica de requisição/resposta (padrão MVC)
├── Models/           Entidades do domínio (mapeadas pelo EF Core)
├── ViewModels/        Modelos auxiliares específicos de tela
├── Services/          Regras de negócio (cálculo de saldo, hash de senha)
├── Data/               Contexto do banco de dados (AppDbContext)
├── Views/               Telas Razor (.cshtml)
└── wwwroot/              Arquivos estáticos (CSS)
```

## Autenticação

Cada MEI cria sua própria conta (e-mail + senha). As senhas são armazenadas com hash
PBKDF2 (nunca em texto puro) e a sessão é mantida por cookie de autenticação do
ASP.NET Core. Ao se cadastrar, um conjunto de categorias padrão de receita/despesa é
criado automaticamente para facilitar o primeiro uso.

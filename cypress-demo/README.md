# Demo de uso (Cypress)

Demo automatizada, ponta a ponta, da jornada de uso do Controle Financeiro
MEI, com um cenário propositalmente **rico em dados**: cria uma conta nova,
cadastra 2 categorias próprias (além das 8 padrão), lança 3 receitas e 6
despesas cobrindo todas as naturezas do DRE (CMV, fixa, variável, dedução/
imposto), define 2 metas financeiras (mês atual e o próximo) e registra 2
captações de recursos — e confere que Categorias, Lançamentos, Painel, DRE
e Fluxo de Caixa mostram os números corretos. Serve tanto para **ver o
sistema funcionando** (ao vivo ou gravado em vídeo) quanto como um teste de
regressão E2E de verdade, já que cada passo tem asserções reais.

Ao final, a demo **não faz logout**: a sessão continua autenticada no
Painel, e o e-mail/senha da conta criada nesta execução são impressos no
terminal e salvos em `ultima-conta-demo.txt` (na raiz de `cypress-demo/`) —
assim dá para continuar navegando manualmente com o mesmo cenário de dados,
seja logo em seguida (rodando com `npm run demo:aberta`, a janela do
Cypress continua aberta no Painel) ou depois, fazendo login em
`http://localhost:5100/Conta/Login` com essas credenciais.

## Pré-requisitos

- [Node.js](https://nodejs.org/) 18 ou mais recente.
- O sistema principal rodando localmente em `http://localhost:5100`:

```bash
cd ../GestaoFinanceiraMEI
dotnet run
```

(Deixe esse terminal aberto rodando o site enquanto usa a demo.)

## Instalação

```bash
cd cypress-demo
npm install
```

## Como rodar

**Ao vivo, acompanhando na tela** (abre o Cypress Test Runner, você escolhe
o navegador e vê cada passo acontecer):

```bash
npm run demo:aberta
```

**Gravando um vídeo** (roda tudo em modo headless e salva o vídeo em
`cypress/videos/jornada-completa.cy.js.mp4` — útil para anexar na
apresentação do TCC):

```bash
npm run demo:gravar
```

## Observações

- Cada execução cria uma conta nova (e-mail com timestamp), então a demo
  pode ser rodada várias vezes seguidas sem precisar limpar o banco antes.
- `ultima-conta-demo.txt` é sempre sobrescrito pela execução mais recente —
  guarda as credenciais só da última vez que a demo rodou, não um
  histórico. Esse arquivo não vai para o Git (está no `.gitignore`), já que
  guarda uma senha, mesmo sendo uma senha de teste descartável.
- O sistema tem um limite de 5 tentativas de cadastro/login por minuto por
  IP (`GestaoFinanceiraMEI/Program.cs`, política `"login"`, proteção contra
  força bruta) — rodar a demo em sequência muito rápida (menos de um minuto
  entre execuções) pode eventualmente esbarrar nesse limite.
- Os seletores usam os `id`s gerados pelo próprio `asp-for` do ASP.NET Core
  MVC (ex.: `#Email`, `#Senha`) — se os nomes das propriedades nos
  ViewModels/Models ou os textos das Views mudarem, os passos correspondentes
  desta demo precisam ser atualizados junto.

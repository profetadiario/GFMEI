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

**Gravando no formato de celular, em ritmo de apresentação** (390×844,
proporção de um smartphone moderno; digitação mais lenta, pausas ao fim de
cada tela e, no final, um passeio pelo menu por Painel, DRE e Fluxo de
Caixa). Leva uns 5 minutos e gera `cypress/videos-celular/demo-celular.mp4`
(780×1688, só a tela do app, sem o painel do Cypress):

```bash
npm run demo:gravar:celular
```

**Vídeo da apresentação (desktop 1920×1080).** Cria uma conta fictícia de
confeitaria com dois meses de histórico (`cypress/apresentacao/1-preparo.cy.js`)
e grava o roteiro de 7 telas (`2-apresentacao.cy.js`): Painel, lançamento
rápido, pró-labore lançado no fechamento do mês, DRE do mês fechado, Fluxo de
caixa, Calculadora (caso da planilha da Adriana, R$ 58,77) e Relatório para o
banco com o Ctrl+P. Leva uns 8
minutos e gera em `docs/demo/` o vídeo completo (~2 min 20 s), a versão curta
de reserva (< 1 min) e as capturas `capturas/01_painel.png` …
`07_relatorio_banco.png` (a 3 é `03_pro_labore.png`). Para gravar contra o site publicado, informe
`BASE_URL`:

```bash
npm run demo:apresentacao
BASE_URL=<endereço do site no Somee> npm run demo:apresentacao
```

**Pró-labore (decisão 10).** `npx cypress run --spec cypress/e2e/pro-labore.cy.js`
cria uma conta de teste e confere: sem a opção "Pessoal" no lançamento, ajuda
"lance o pró-labore uma vez, no fechamento do mês", aviso (sem bloqueio) para o
segundo pró-labore no mesmo mês, sem linha de retiradas no Painel e em
Lançamentos, e o pró-labore sugerido nos custos fixos da Calculadora (5 testes).

**Teste do PDF do relatório para o banco.** Depois da demo (que cria o
cenário), entra com a conta da demo, gera o PDF como o Ctrl+P (A4, CSS de
impressão) e confere 26 itens: valores, DRE (com o pró-labore nas despesas
fixas), tabela de 12 meses, quadro dos bancos, ausência de menu, botões e
de linha de retiradas, no máximo 2 páginas e nenhum bloco
sobreposto. Usa o Chrome ou o Edge instalados (`CHROME_PATH` para outro).
Para o Somee: `BASE_URL=... PDF_EMAIL=... PDF_SENHA=... npm run teste:pdf`.

```bash
npm run teste:pdf
```

## Observações

- Cada execução cria uma conta nova (e-mail com timestamp), então a demo
  pode ser rodada várias vezes seguidas sem precisar limpar o banco antes.
- `ultima-conta-demo.txt` é sempre sobrescrito pela execução mais recente —
  guarda as credenciais só da última vez que a demo rodou, não um
  histórico. Esse arquivo não vai para o Git (está no `.gitignore`), já que
  guarda uma senha, mesmo sendo uma senha de teste descartável.
- O sistema limita cadastros/logins por minuto **por IP**
  (`GestaoFinanceiraMEI/Program.cs`, política `"login"`, proteção contra
  força bruta; hoje 40 por minuto). A demo usa só 2 a 3 dessas requisições
  por execução, então o limite não atrapalha, mesmo rodando várias vezes
  seguidas.
- Os seletores usam os `id`s gerados pelo próprio `asp-for` do ASP.NET Core
  MVC (ex.: `#Email`, `#Senha`) — se os nomes das propriedades nos
  ViewModels/Models ou os textos das Views mudarem, os passos correspondentes
  desta demo precisam ser atualizados junto.

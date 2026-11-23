# Decisões e entregas do TCC — Controle Financeiro MEI

> **Regra de trabalho:** esta pasta `docs/` é a fonte de verdade entre o chat de sistema/código e o chat do artigo/apresentação.
> Toda decisão e toda entrega de lote são registradas aqui (data, o que entrou, telas, testes, versão).

## 1. Marcos fixos

- Banca: 04/12/2026. Orientadora: Annik Passos Marôcco. Coorientador: Danilo Oliveira Sampaio (UFJF, CAD248, turmas B e D, 58 alunos).
- Pesquisa de campo encerrada em 18/11/2026: 30 respostas, 25 válidas.
- **Congelamento do sistema: 27/11/2026** (sem deploys depois). Depois disso, só a gravação do vídeo de demonstração (Cypress rodando localmente).
- 26/11 = reserva (buffer).
- **Artigo: versão final gerada em 20/11/2026 (`TCC_Enrique_Fontes_Lacerda_201126_FINAL.pdf`), revisada contra o log do chat de sistema em 21/11/2026.** Ver seção 5b. Falta só enviar à Annik.
- **Tempo da apresentação:** ainda não informado. Preparar a fala para 15–20 min, com slides de apoio no apêndice.
- **Validação da orientadora (20/11/2026):** a Annik leu os 6 pontos da análise e seguiu a mesma linha. Pedidos dela para o artigo: (1) tratar a capacitação no Coworking Me Conecta como viés positivo (participantes mais maduras nos controles financeiros) dentro das limitações; (2) destacar que o índice de tecnologia é o mais isolado das demais competências (só se associa à captação), o que ela atribui ao desconhecimento do impacto da tecnologia na rotina do negócio; (3) usar isso nas considerações finais para justificar o sistema. Ela enviou um texto gerado por IA como base (`ANÁLISE DOS RESULTADOS DA PESQUISA - EMPREENDEDORAS CWK.docx`), com o aviso de não copiar: o chat do artigo reescreve com os números conferidos e sem linguagem causal. O quadro "em validação" sai do artigo na próxima versão.

## 2. Decisões aprovadas (18/11/2026)

| # | Decisão |
|---|---------|
| 1 | **Retirada pessoal** aparece em linha separada no Painel, fora de "Despesas". A sobra média do indicador de parcela (lote 3) desconta as retiradas pessoais. |
| 2 | **Reserva de emergência** fica como trabalho futuro. |
| 3 | **Impressão do navegador** é aceita como PDF do relatório para o banco. |
| 4 | Enrique roda os testes, o deploy e a conferência no Somee **a cada lote**, antes de começar o próximo. |
| 5 | Os prints de telas para o artigo/apresentação saem da **demo do Cypress**. |
| 6 | **Não haverá novo teste com turma** (confirmado por Enrique em 19/11/2026). O artigo cita apenas o uso nas turmas B e D da CAD248. |
| 7 | **Nome da retirada pessoal — DECIDIDO em 19/11/2026: opção (a).** O sistema mantém o comportamento (fora do DRE, dentro do saldo em caixa) e usa o nome **"Retirada pessoal"**, sem "pró-labore" nas telas, no enum e nos testes. Motivo: pró-labore é remuneração da sócia e costuma entrar no DRE como despesa; a retirada de lucros fica fora do DRE e só sai do caixa (e o MEI não tem pró-labore formal). A troca foi feita ainda no lote 1, antes do deploy da v1.4, então nenhuma categoria com o nome antigo chega ao banco de produção. Se o Danilo preferir a opção (b) (pró-labore como despesa no DRE), reabrir como mudança de lógica do DRE. **Fontes (já nas referências do artigo 191126):** Portal Tributário, "Qual é a diferença entre pro labore e lucro distribuído?" (Guia Tributário, 10/03/2020) e "Pró-labore: tratamento contábil" (27/04/2022): pró-labore vai a custos ou despesas operacionais; lucro distribuído sai de lucros acumulados. Sebrae e Banco Central, "Educação financeira dos microempreendedores individuais": cerca de metade dos MEI faz retiradas sem nenhum registro. **Termos:** nas telas, "Retirada pessoal" (com ajuda curta: "dinheiro do negócio que você tirou para uso pessoal"); no artigo, "retirada pessoal", distinta de pró-labore. |
| 8 | **(SUBSTITUÍDA PELA DECISÃO 9 em 22/11/2026)** **Calculadora de preço pelo markup divisor da orientadora — DECIDIDO em 19/11/2026 (orientação da Annik).** Fórmula: **markup divisor = 1 − (DF% + DV% + ML%) ÷ 100**; **preço de venda = custo direto da unidade ÷ markup divisor**; markup multiplicador = 1 ÷ markup divisor. DF% = despesas fixas mensais ÷ faturamento médio mensal × 100 (sugerido pelo sistema a partir dos últimos meses com lançamento). DV% = despesas que variam com a venda (taxa de cartão, comissão, frete, embalagem). ML% = lucro desejado. Se DF + DV + ML ≥ 100%, não há preço possível (avisar). **Ajuste para o MEI:** o DAS é um valor fixo mensal, então entra em DF, e não em DV. Mostrar o aviso "markup não é margem de lucro". Exemplo da orientadora para teste: custo R$ 50,00, DF 15%, DV 10%, ML 20% → divisor 0,55, multiplicador 1,818, preço R$ 90,91. Substitui a fórmula da v1.5 (custo + custos fixos ÷ unidades), que deixa de pedir "unidades vendidas por mês". Fonte para o artigo: Sebrae, *Precificação no artesanato* (resposta técnica, mark-up divisor; DF% = custos fixos médios ÷ vendas médias). **Motivo (áudio da Annik, 19/11/2026):** é a fórmula que ela ensina no Coworking Me Conecta; o sistema não pode apresentar um valor diferente do ensinado em sala. Mostrar o markup divisor e o multiplicador. **Confirmado pela Annik em 20/11/2026: o DAS entra no custo fixo (DF)**, e as empreendedoras estão cientes disso. **Pendente:** ela ofereceu exemplos reais de empreendedoras que já calcularam o markup: usar como casos de teste da calculadora. A Annik vai acessar o sistema para validar o que já foi implementado. |
| 9 | **Calculadora pelo método da planilha da Adriana (empreendedora capacitada no Me Conecta) — DECIDIDO pela Annik em 22/11/2026.** **Entradas da empreendedora:** dias de trabalho no mês, horas trabalhadas por dia, % de lucro desejado, tempo de produção da unidade (minutos), custos variáveis da unidade (materiais) e os valores mensais de custos fixos (energia, internet, **pró-labore**, aluguel...), despesas fixas (DAS, IPTU...) e despesas variáveis (gasolina, estacionamento...). **Rateio de todo valor mensal:** valor ÷ dias ÷ horas ÷ 60 × minutos de produção (a gasolina segue a mesma regra, para ficar padrão). **Base do markup = custos fixos + custos variáveis** (sem as despesas, para não contá-las duas vezes). Total geral = base + despesas fixas + despesas variáveis. DF% = despesas fixas ÷ total geral × 100; DV% = despesas variáveis ÷ total geral × 100. **Markup = 100 ÷ (100 − (DF% + DV% + lucro%))**; **preço = base × markup**. **Arredondamento: 2 casas decimais em tudo, truncando, como na planilha** (ex.: internet 1,958 → 1,95; pró-labore 15,625 → 15,62; markup 1,3889 → 1,38). Bloquear quando DF% + DV% + lucro% ≥ 100. **Caso de teste (planilha da Adriana, com a gasolina padronizada):** 24 dias, 8 h, 90 min; custos fixos 21,33; custos variáveis 14,95; base 36,28; despesas fixas 1,91 (0,60 + 1,31); despesas variáveis 1,48 (1,17 + 0,31); total geral 39,67; DF% 4,81; DV% 3,73; **lucro 30% → markup 1,62 → R$ 58,77**; **lucro 20% → markup 1,39 → R$ 50,42**. **A confirmar com a Annik:** se o denominador do DF% e do DV% é o total geral (como na planilha) e se ela aprova os valores de teste acima. |
| 10 | **Retirada pessoal sai; pró-labore como custo fixo — orientação da Annik (22–23/11/2026), detalhamento do recurso enviado a ela em 23/11.** Motivo: no controle financeiro do MEI não deve haver retirada de dinheiro do caixa para uso pessoal, e o sistema não deve dar suporte a isso; a remuneração da empreendedora é o pró-labore, que é custo fixo. **Artigo (231126):** nenhuma menção a retirada pessoal; pró-labore como custo fixo. **Aprovado pela Annik em 23/11/2026:** deixar a retirada pessoal exposta no sistema induz a entender que a qualquer momento é possível retirar dinheiro, e não é. **Regra: a empreendedora só se remunera pelo pró-labore, lançado no fechamento do mês, para não distorcer o DRE antes do fechamento.** **Sistema (a fazer antes do congelamento de 27/11):** tirar a opção "Pessoal" do Novo lançamento e a linha "Retiradas pessoais" do Painel e da lista de lançamentos; criar a categoria padrão "Pró-labore (seu salário)" com natureza despesa fixa, que entra no DRE e nos custos fixos da calculadora; o relatório para o banco passa a calcular a sobra média sem a linha de retiradas; lançamentos antigos com natureza RetiradaPessoal: decidir conversão (ver prompt do chat do artigo). **Apresentação, quiz, vídeo e capturas:** atualizar depois do deploy. A decisão 7 (nome "Retirada pessoal") fica superada por esta. |

## 3. Plano em três lotes

| Lote | Versão | Janela | Conteúdo |
|------|--------|--------|----------|
| 1 | v1.4 | ~20–21/11 | Lançamento rápido (negócio × pessoal), saldo em destaque, gráfico de gastos por categoria + frase do maior gasto, lembrete de "dias sem lançar", consultas unificadas do Painel |
| 2+3 | **v1.5 (entrega única)** | 19/11 (código) → deploy pelo Enrique | **Lotes 2 e 3 juntos, por decisão de Enrique em 19/11/2026:** textos em linguagem simples (inclui corrigir "protótipo"), calculadora de preço, relatório para o banco, indicador de capacidade de pagamento e texto sobre o que os bancos pedem ao MEI. Não haverá v1.6: a v1.5 fecha a parte de sistema para a banca. |

Trabalho futuro (fora do escopo): reserva de emergência, OCR de notas, ranking de produtos, WhatsApp, estoque, PDF gerado no servidor, lembretes por e-mail/push, data de criação e saldo inicial.

Redação sugerida para o artigo: trocar "indicadores de necessidade de capital" por "relatório de apoio ao crédito e indicador de capacidade de pagamento" (é o que o sistema realmente entrega).

## 4. Entregas

### Lote 1 — v1.4 (no ar em 19/11/2026, confirmado por Enrique; release v1.4 a criar no GitHub, se ainda não criada)

**O que entrou**

- **Lançamento rápido** (tela *Lançamentos → Novo lançamento*): campos grandes para celular; ordem Tipo → Valor → Categoria; descrição opcional (se vazia, usa o nome da categoria); escolha "Do negócio" / "Pessoal" para saídas; botão **Salvar e lançar outro**.
- **Retirada pessoal:** novo valor de enum `NaturezaDespesa.RetiradaPessoal = 4`, **sem migração de banco**. A categoria "Retirada pessoal" é criada automaticamente por usuária na primeira retirada. Fica **fora do DRE** (não altera o lucro líquido) e **dentro do saldo em caixa**.
- **Painel:** saldo geral em destaque; linha separada "Retiradas pessoais"; gráfico de rosca "Para onde foi o dinheiro este mês" (até 6 fatias, demais agrupadas em "Outras categorias") com a frase do maior gasto; aviso quando passam 3 dias ou mais sem lançamento.
- **Lançamentos (lista):** cards separados para Receitas, Despesas, Retiradas pessoais e Saldo do mês.
- **Desempenho:** o Painel passou a usar uma única chamada `ObterPainelAsync` (2 consultas) no lugar de ler todos os lançamentos duas vezes.

**Telas afetadas:** Painel (Dashboard/Index), Lançamentos (Transacoes/Index), Novo lançamento (Transacoes/Create).

**Testes:** +40 testes novos (controller de lançamentos +11, controller do Painel +3, enums +2, ViewModel do Painel +12, serviço de fluxo de caixa +11, DRE +1). Total após o lote: **276 testes, 276 passando** (`dotnet test`, 19/11/2026).

**Versão:** v1.4 (tag e release a criar no GitHub após o deploy conferido).

**Pendências assumidas**

- Bootstrap, Chart.js e jQuery continuam carregando do CDN jsdelivr (hospedar localmente não foi feito: precisa baixar os arquivos na máquina do Enrique). Anotado como melhoria opcional.
- Nada foi compilado fora da máquina do Enrique; a validação é o `dotnet test` dele.
- Os textos da demo do Cypress continuam compatíveis (ids `#Descricao #Tipo #CategoriaId #Valor #Data`, botão "Salvar", textos "Saldo geral", "Receitas em", "Despesas em"), e a demo foi rodada **localmente** em 19/11/2026: `npm run demo:gravar` (1280×800, ritmo de teste, ~30 s) e `npm run demo:gravar:celular` (390×844, ritmo de apresentação com pausas e passeio final por Painel → DRE → Fluxo de Caixa, ~4 min; vídeo recortado só na tela do app, 780×1688): **11/11 passos passando nos dois formatos**. Ainda falta rodar contra o sistema publicado.

### Entrega única (lotes 2 + 3) — v1.5 (sistema fechado em 20/11/2026; deploy pelo Enrique até 24/11, checklist em `docs/DEPLOY_V1.5.md`)

**O que entrou**

- **Linguagem simples:** "protótipo" saiu das telas (rodapé: "sistema desenvolvido em um Trabalho de Conclusão de Curso"; página inicial reescrita em linguagem do dia a dia). README também passa a dizer "sistema".
- **Calculadora de preço pelo markup da Annik (decisão 8)** (*Ferramentas → Calculadora de preço*, `/Calculadora`): sem persistência (formulário por GET). Entradas: custo direto da unidade, **DF%**, **DV%** e **ML%**. `markup divisor = 1 − (DF + DV + ML) ÷ 100`; `preço = custo ÷ divisor`; `multiplicador = 1 ÷ divisor`. **DF% sugerido** = (despesas fixas + DAS e impostos) ÷ faturamento, somados nos meses com lançamento dos últimos 12 meses (mesma janela do relatório); sem faturamento lançado, não sugere e a usuária digita. Mostra preço, markup divisor, multiplicador e para onde vai cada venda (custo, DF, DV, lucro — as quatro partes somam exatamente o preço) e o aviso **"markup não é margem de lucro"**. Bloqueia quando DF + DV + ML ≥ 100%. Saiu o campo "unidades vendidas por mês". Exemplo da Annik conferido: custo R$ 50, DF 15%, DV 10%, ML 20% → divisor 0,55, multiplicador 1,818, **R$ 90,91**. Observação: "DAS e impostos" = tudo o que está em categorias de natureza *Dedução ou imposto* (no MEI, na prática, o DAS).
- **Relatório para o banco** (*Ferramentas → Relatório para o banco*, `/Relatorio`): identificação (negócio, responsável, período, data de emissão), faturamento dos 12 meses e médio, sobra média, saldo em caixa, **DRE resumida dos 12 meses** (retirada pessoal à parte, fora do DRE), **tabela mês a mês** (receitas, despesas, retiradas, sobra, saldo final acumulado) e captações já registradas. Botão **Imprimir / salvar em PDF** (impressão do navegador, decisão 3) com CSS de impressão: some menu, rodapé, botões e formulários; A4 retrato, **2 páginas**.
- **Correção encontrada pelo teste do PDF (20/11):** na impressão, o card "DRE resumida" saía **cortado** (só o título; o resto ficava escondido embaixo da tabela mês a mês) por causa da altura fixa dos cards em linhas flex. Corrigido no CSS de impressão; o teste agora confere a ordem real dos blocos na página para pegar esse tipo de corte.
- **Indicador de capacidade de pagamento:** sobra média mensal = (receitas − despesas do negócio − **retiradas pessoais**, decisão 1) ÷ meses considerados (do 1º mês com lançamento na janela até o mês atual). Parcela "confortável" = até **30%** da sobra média; 30–60% = "atenção"; acima de 60% ou sem sobra = "arriscada". As faixas são **referência de prudência do sistema, não regra de banco** (escrito na tela).
- **O que os bancos costumam pedir ao MEI:** quadro no relatório, com aviso de que cada banco tem exigências próprias. Fonte: ver seção 5 (**pendente de conferência**).
- **Menu:** submenu **Ferramentas** (Calculadora de preço, Relatório para o banco); "Captação de Recursos" virou "Captações"; o menu recolhe abaixo de 1200 px.

**Telas novas:** Calculadora de preço (Calculadora/Index), Relatório para o banco (Relatorio/Index). **Telas alteradas:** menu e rodapé (_Layout), página inicial (Home/Index).

**Testes:** **316 testes, 316 passando** (`dotnet test`, 20/11/2026). Calculadora: serviço 18 + controller 3 (exemplo da Annik, composição do preço, limites de 100%, custo zero/negativo, DF% sem histórico, sem faturamento, com DAS e com média de meses); relatório: serviço 14 + controller 5.

**Teste do PDF:** `npm run teste:pdf` (em `cypress-demo/`) — gera o PDF do relatório como o Ctrl+P e confere **24 itens** (valores, DRE, 12 linhas, quadro dos bancos, sem menu/botões/formulário, ≤ 2 páginas A4, nenhum bloco sobreposto). **24/24** em 20/11/2026.

**Demo Cypress:** passo 11 refeito com o exemplo da Annik (DF% sugerido 25,6% no cenário da demo; R$ 90,91; 0,55; 1,818); passo 12 (relatório) igual. `npm run demo:gravar` **13/13** em 20/11/2026.

**CI/CD:** novo `.github/workflows/ci.yml` (build + `dotnet test` a cada push/PR na `develop` e na `master`); o `deploy-somee.yml` agora chama o CI como job `testes` e só publica se ele passar.

**Vídeo da apresentação (desktop, 1920×1080, dados fictícios da confeitaria "Doces da Ana"):** gravado localmente em 20/11/2026 com `npm run demo:apresentacao`, contra a `develop` (a v1.5 ainda não está publicada; pode ser regravado contra o Somee com `BASE_URL`).
- `docs/demo/apresentacao_sistema_1080p.mp4` — **2 min 17 s**, ritmo de apresentação, roteiro: login e Painel → lançamento rápido (venda e despesa, "Salvar e lançar outro") → retirada pessoal → DRE → Fluxo de caixa → Calculadora (R$ 90,91) → Relatório para o banco com simulação de parcela e Ctrl+P.
- `docs/demo/apresentacao_sistema_curta_1080p.mp4` — **51 s**, mesmo roteiro com pausas mínimas (reserva se faltar tempo).
- `docs/demo/capturas/` — 7 PNG 1920×1080: `01_painel`, `02_lancamento_rapido`, `03_retirada_pessoal`, `04_dre`, `05_fluxo_caixa`, `06_calculadora`, `07_relatorio_banco` (para o artigo e os slides).

**Diagramas finais (v1.5):** `docs/diagramas/finais/` — SVG (artigo/documentação) e PNG com fundo branco e largura ≥ 2.500 px (slides), mesmo estilo e fonte, gerados das fontes em `docs/diagramas/fontes/`: `fig1_arquitetura`, `fig2_casos_de_uso`, `fig3a_classes_dominio`, `fig3b_classes_servicos`, `fig3c_classes_apoio_decisao` (campos novos da calculadora), `fig4_atividades_lancamento`, `fig5_sequencia_lancamento`, `fig6_der`. **Mudaram com o fechamento:** 2 (calcular preço deixou de "incluir a DRE"), 3c (calculadora nova) e **4 e 5**, que estavam desatualizadas desde a v1.4 (agora mostram o lançamento rápido, a retirada pessoal e "Salvar e lançar outro"). **DER (6) sem mudança de estrutura.** Os arquivos antigos `docs/diagramas/Figura*.png/svg` foram removidos: usar só `finais/`.

**Documentação técnica:** `docs/Documentacao_Tecnica_Sistema_MEI.docx/.pdf` atualizada com RF11 (markup divisor), seção 1.3, textos das seções 5.1–5.4, linha da calculadora no mapeamento e **todas as 8 figuras** trocadas pelas finais.

**Versão:** v1.5. Tag e release no GitHub depois do deploy conferido (texto pronto em `docs/DEPLOY_V1.5.md`).

**Trabalho futuro (fora da v1.5):** ranking de produtos que dão mais lucro (pedido por 19 de 25 no questionário), alertas de gastos e de contas a vencer, relatório para a DASN-SIMEI, exportação da DRE e do fluxo de caixa para planilha, reserva de emergência (pronta na branch `pos-tcc-improvements`, decisão 2), sugestão automática do DV%, PDF gerado no servidor, OCR de notas, WhatsApp, estoque, lembretes por e-mail/push, EF Core Migrations completo, CI rodando também o Cypress e o teste do PDF (hoje rodam localmente).

**Pendências assumidas**

- Exemplos reais de markup da Annik: quando chegarem, viram casos de teste extras (não bloqueiam o deploy).
- Conferir o Ctrl+P no Somee depois do deploy (o teste automatizado usa o motor de impressão do Chrome, não a impressora).
- Fonte do quadro "o que os bancos pedem" (seção 5) precisa ser conferida por uma pessoa.

**Deploy da v1.5 (21/11/2026):** `develop` mesclada na `master` (`b06611d`), CI e deploy verdes no GitHub Actions, site no ar. A tag `v1.5` foi movida de `4e10eef` para `b06611d`, com o ok do Enrique: a release criada antes apontava para o código antigo.

**Sincronização da `develop` (23/11/2026):** a `develop` foi avançada até a `master` (`dd5ee6c`, com o hotfix abaixo) e enviada ao GitHub. As duas branches estão iguais. A v1.6 continua **só** na `feat/pro-labore`, sem commit.

**Hotfix da legenda do DRE (23/11/2026, pedido do Enrique, sobe antes da v1.6):** a legenda do gráfico de pizza "Composição do resultado" saía em cinza escuro sobre o card escuro e ficava ilegível. Com `generateLabels` próprio, o Chart.js usa o `fontColor` de cada item e ignora `labels.color`, então a cor branca foi posta no item. Commit `dd5ee6c`, feito numa branch a partir da `master`, com 316/316 testes. **No ar em 23/11/2026:** o Enrique autorizou o push na `master`, e o CI e o deploy no Somee passaram; o site voltou com 200 depois de cerca de 1 min de 503. A mesma correção está na `feat/pro-labore`.

### Fechamento — decisões 9 e 10 (v1.6; código pronto em 23/11/2026, branch `feat/pro-labore`; deploy pelo Enrique até 26/11)

**O que entrou**

- **Pró-labore no lugar da retirada pessoal (decisão 10):**
  - **Novo lançamento:** saiu a escolha "Do negócio / Pessoal"; toda saída é do negócio.
  - **Painel e Lançamentos:** saíram o card e a linha "Retiradas pessoais". O saldo é receitas − despesas.
  - **Categoria padrão:** "Pró-labore (seu salário)", com natureza despesa fixa. É criada no cadastro e, para quem já tinha conta, no login e ao abrir o Novo lançamento. A criação é idempotente (procura pelo nome e não duplica) e **não exige migração de esquema**.
  - **Ajuda e aviso:** ao escolher o pró-labore aparece a ajuda **"Lance o pró-labore uma vez, no fechamento do mês."** Se já houver pró-labore no mesmo mês, o sistema **avisa** antes de salvar ("Você já lançou pró-labore em outubro/2026 (R$ 2.000,00)…"). É só um aviso: clicar em Salvar de novo grava.
  - **Onde ele entra:** no DRE, como despesa fixa (a linha agora diz "Despesas Fixas (com o pró-labore)"), e nos custos fixos da calculadora.
  - **Relatório para o banco e indicador de parcela:** sobra média = receitas − despesas do negócio, com o pró-labore dentro. Saíram a linha "Retiradas pessoais" da DRE resumida e a coluna "Retiradas" da tabela mês a mês.
- **Dados antigos:** o valor `NaturezaDespesa.RetiradaPessoal = 4` **continua no enum**, para não quebrar lançamentos gravados. Ele saiu das telas, inclusive da lista de naturezas em Categorias. Nos cálculos, esses lançamentos **contam como despesa fixa**, igual ao pró-labore (`ParaCalculo()`). Nada foi convertido no banco. A consulta de verificação no Somee e a conversão proposta estão em `docs/DECISAO10_DADOS_ANTIGOS.md`; a conversão **só roda com o ok do Enrique**. Isso também cobre a "divergência a resolver" da seção 5b: a consulta procura por natureza, não por nome.
- **Calculadora pelo método da planilha da Adriana (decisão 9), substituindo o markup divisor da decisão 8:**
  - **Entradas:** dias de trabalho, horas por dia, minutos por unidade, materiais da unidade, lucro % e três grupos de valores mensais (custos fixos, despesas fixas e despesas variáveis), com nome e valor. O botão "+ outra linha" acrescenta linhas.
  - **Conta:** rateio = valor × minutos ÷ (dias × horas × 60); base = custos fixos + materiais; DF% e DV% sobre o total geral; markup = 100 ÷ (100 − (DF% + DV% + lucro%)); preço = base × markup. Tudo **truncado em 2 casas**, como na planilha. O cálculo é bloqueado quando DF% + DV% + lucro% ≥ 100.
  - **Valores sugeridos:** média de cada categoria nos **meses já fechados** dos últimos 12. O mês corrente fica de fora porque o pró-labore só entra no fechamento. Despesa fixa (com o pró-labore) vai para custos fixos, dedução/imposto (DAS) para despesas fixas e despesa variável para despesas variáveis. O CMV fica de fora, porque os materiais são digitados por unidade. O pró-labore aparece sempre nos custos fixos, mesmo em branco.
  - **Caso de teste conferido:** 24 dias, 8 h, 90 min, materiais 14,95 → base 36,28, total 39,67, DF 4,81%, DV 3,73%. Com lucro de 30%: **markup 1,62 → R$ 58,77**. Com lucro de 20%: **1,39 → R$ 50,42**.
  - **Valores mensais reconstruídos:** a planilha não está no repositório, então os valores do caso de teste foram reconstruídos para reproduzir item a item os números do DECISOES. Custos fixos: energia 150, água 32, internet 250, aluguel 300, pró-labore 2.000. Despesas fixas: DAS 76,90, IPTU 168. Despesas variáveis: gasolina 150, estacionamento 40. **Se a Annik mandar os valores originais, trocar no teste.** Continua a confirmar com ela o denominador do DF%/DV% (total geral).
- **Textos:** "retirada pessoal" saiu das telas, do README do Cypress e da documentação técnica. Lá só resta o nome técnico do valor legado do enum.

**Testes:** **327 testes, 327 passando** (`dotnet test`, 23/11/2026).
- Calculadora: 27 (serviço) + 4 (controller).
- Pró-labore: categoria idempotente, sem usar a de outra usuária, aviso no mês repetido, confirmação, outro mês sem aviso, login garante o pró-labore.
- Retirada antiga contando como despesa fixa no DRE, no relatório e no caixa.

**Cypress:**
- `jornada-completa` **13/13**: categorias padrão agora são 9; o passo 11 usa o caso da Adriana (R$ 58,77, markup 1,62, DF 4,81%, DV 3,73%).
- Spec novo `cypress/e2e/pro-labore.cy.js` **5/5**: sem "Pessoal", ajuda do fechamento, aviso e confirmação, sem linha de retiradas, pró-labore sugerido na calculadora.

**Teste do PDF:** `npm run teste:pdf` **26/26**. Os dois itens novos conferem "Despesas fixas (com o pró-labore)" e a ausência de "Retirada".

**Vídeo e capturas (refeitos em 23/11/2026, local, 1920×1080, dados fictícios da "Doces da Ana"):**
- Roteiro: Painel → lançamento rápido → **pró-labore lançado no fechamento do mês passado** (com a ajuda na tela) → **DRE do mês fechado** (despesas fixas R$ 2.732,00, com o pró-labore) → Fluxo de caixa → **Calculadora no caso da Adriana (R$ 58,77)**, com os valores do mês vindos dos lançamentos → Relatório para o banco + Ctrl+P.
- `docs/demo/apresentacao_sistema_1080p.mp4`: **2 min 18 s**.
- `docs/demo/apresentacao_sistema_curta_1080p.mp4`: **56 s**.
- `docs/demo/capturas/`: `01_painel`, `02_lancamento_rapido`, **`03_pro_labore`** (substitui `03_retirada_pessoal`, removida), `04_dre`, `05_fluxo_caixa`, `06_calculadora`, `07_relatorio_banco`.

**Figuras novas:** `docs/diagramas/finais/`, em SVG e PNG, geradas das fontes em `docs/diagramas/fontes/`.
- **2 (casos de uso):** "Lançar pró-labore no fechamento do mês" substitui a retirada; a calculadora inclui o pró-labore.
- **3a:** o enum marca RetiradaPessoal como legado.
- **3b:** sai `TotalRetiradasPessoais`.
- **3c:** calculadora nova (entrada com tempo e itens mensais, resultado com base, DF%, DV% e markup, sugestão de valores mensais) e `MesRelatorio` sem retiradas.
- **4 (atividades):** ajuda do pró-labore e aviso de repetido no mês.
- **5 (sequência):** criação idempotente da categoria e aviso.
- **6 (DER):** só o comentário da natureza.
- A 1 (arquitetura) não muda.

**Documentação técnica:** `docs/Documentacao_Tecnica_Sistema_MEI.docx/.pdf`. Mudaram as seções 1.3 (v1.4 e v1.5), os RF de lançamentos, saldo, painel, natureza, RF11 e indicador, os textos das figuras 2, 3, 4, 5 e 6, a tabela de mapeamento (linha "Pró-labore" e calculadora) e as considerações. As 8 figuras foram trocadas.

**Pendências:**
- Rodar a verificação do Somee (passo 1 de `docs/DECISAO10_DADOS_ANTIGOS.md`) e decidir a conversão.
- Merge na `develop` e na `master`, deploy, conferência no Somee e tag/release.
- Apresentação, quiz e artigo: trocar as capturas 03, 04 e 06 e o vídeo (chat do artigo).

**Para o agente documental (artigo, slides e quiz; situação em 23/11/2026, fim do dia)**

- **O que está no ar (Somee):** a v1.5 com o hotfix da legenda do DRE. **Ainda não estão no ar** o pró-labore nem a calculadora da planilha. O artigo pode descrevê-los como "versão final do sistema" (v1.6), mas sem afirmar que já estão publicados até o deploy ser registrado aqui.
- **Números que mudaram em relação ao artigo e aos slides:**
  - Testes: 316 → **327**, todos passando.
  - Demo Cypress: **13/13**, mais o spec novo do pró-labore, **5/5**.
  - Teste do PDF: 24/24 → **26/26**.
  - Vídeo: 2 min 17 s → **2 min 18 s**; versão curta: 51 s → **56 s**.
  - Calculadora: o exemplo de R$ 90,91 (markup divisor, decisão 8) **saiu**. Agora o exemplo é o caso da Adriana, **R$ 58,77** com lucro de 30% (markup 1,62), ou **R$ 50,42** com lucro de 20%.
- **Termos:**
  - Não usar "retirada pessoal" em nada.
  - Usar "pró-labore (seu salário)": custo fixo, lançado uma vez, no fechamento do mês.
  - Usar "markup da planilha" (rateio pelo tempo de produção; DF% e DV% sobre o total geral), e não "markup divisor".
  - A sobra média do indicador de parcela é "receitas − despesas do negócio, com o pró-labore dentro".
- **Arquivos para trocar:**
  - Capturas `docs/demo/capturas/03_pro_labore.png`, `04_dre.png` (agora com a legenda branca) e `06_calculadora.png`. As outras foram regravadas, mas não mudam de conteúdo.
  - Os dois vídeos em `docs/demo/`.
  - Figuras 2, 3a, 3b, 3c, 4, 5 e 6 em `docs/diagramas/finais/`.
  - `docs/Documentacao_Tecnica_Sistema_MEI.pdf`.
- **RF11 e requisitos:** o novo texto do RF11 e os RF de lançamentos, saldo, painel, natureza e indicador estão na documentação técnica (seção 1.3 e tabela de requisitos). Usar a mesma redação no artigo.
- **Fontes:** as do Portal Tributário (decisão 7) servem para justificar o pró-labore como custo ou despesa operacional. A referência Kaneoya e Ciola (markup divisor) deixa de ser o método da calculadora. Se ficar no artigo, que seja como contexto, não como a fórmula usada. A planilha da Adriana não é publicação: citar como "planilha de precificação usada na capacitação do Coworking Me Conecta", conforme a orientação da Annik.
- **A confirmar com a Annik antes de fechar o texto:** o denominador do DF%/DV% (total geral) e os valores do caso de teste. Os valores mensais foram reconstruídos, porque a planilha original não está no repositório.
- Confirmar com a Annik o denominador do DF%/DV% e os valores do caso de teste.


## 5. Fonte do texto "o que os bancos pedem ao MEI" (para as referências do artigo)

Sugestão do chat do artigo: o artigo já cita SEBRAE, "Acesso a crédito para pequenos negócios". Se servir para o texto da tela, usar a mesma fonte; se usar outra, registrar abaixo.

**Situação em 19/11/2026: pendente de conferência humana.** O texto da tela é uma lista geral e **não cita fonte** de propósito, até alguém confirmar.

- Candidata: SEBRAE. *Crédito sem ciladas: obtenha crédito de forma segura, consciente e sem cair em armadilhas* (guia prático, PDF de 2025). Link: https://sebrae.com.br/content/dam/portal-sebrae/na/midias/documentos/pdfs/Guia_pr%C3%A1tico_Credito-sem-ciladas.pdf. Acesso em: 19 nov. 2026.
- **Não verificado:** o PDF é todo em imagem (sem texto), e o chat de sistema não conseguiu ler o conteúdo. Não dá para afirmar que ele traz a lista de documentos da tela nem as faixas de 30%/60%, **que são do sistema**.
- Ação: Enrique (ou o chat do artigo) abre o PDF e confirma. Se bater, a tela pode ganhar a linha "Fonte: SEBRAE, Crédito sem ciladas (2025)". Se não bater, usar "Acesso a crédito para pequenos negócios" (já nas referências do artigo) e registrar aqui.

## 5b. Artigo e apresentação finais (20/11/2026, chat do artigo)

- **Artigo final:** `TCC_Enrique_Fontes_Lacerda_201126_FINAL.pdf` (20 páginas, limite do IF), com versão marcada e fonte LaTeX. Inclui: v1.5 (calculadora pelo markup divisor com DAS em DF, relatório de apoio ao crédito, indicador de capacidade de pagamento, 316 testes, teste do PDF, CI); 13 requisitos funcionais; resposta da Annik de 20/11 (núcleo planejamento–custos–fluxo–análise, tecnologia como dimensão mais isolada, viés positivo da capacitação nas limitações, fechamento justificando o sistema). Quadros de pendência removidos. Nova referência: KANEOYA, P.; CIOLA, F. *Precificação no artesanato* (Sebrae, resposta técnica, [2014?]) para o markup divisor.
- **Fora do artigo de propósito:** a lista "o que os bancos pedem ao MEI" (fonte da seção 5 ainda não conferida); o artigo descreve só que o relatório traz orientações gerais.
- **Apresentação (Slides, versão 28):** n = 25 em todos os resultados; slide novo "A tecnologia é a dimensão mais isolada"; dois slides de telas (capturas 01, 02, 06 e 07); vídeo de 2 min 17 s e versão curta de 51 s (início com clique); diagramas finais 1, 2, 3a, 3b, 3c (slide novo), 4, 5 e 6; requisitos com 13 RF; qualidade com 316 testes; coorientador com nome completo. Correção de legibilidade: cards escuros saíam com fundo claro em slides de fundo alternativo, deixando o texto quase invisível (ex.: requisitos não funcionais); cor de texto secundário escurecida em todo o deck.
- **Conferência contra o log do chat de sistema (21/11/2026):** artigo e slides batem com o log (316 testes, Cypress 13/13, PDF 24/24 em 2 páginas A4, CI, vídeos de 2 min 17 s e 51 s, figuras finais 1–6, DF% com DAS na janela de 12 meses). Correções feitas: (1) "dois erros reais" → três (o terceiro é o corte do DRE no PDF); (2) a calculadora é a **segunda** funcionalidade mais apontada como indispensável (68%), não a terceira; (3) a v1.4 atende à funcionalidade mais indispensável e ao relatório mais pedido (não "aos dois requisitos mais demandados"); (4) **"cobertura de 100% da lógica de negócio" saiu do artigo e do slide de qualidade** porque não foi medida na v1.5 — se o chat de sistema confirmar com relatório de cobertura, volta.
- **Divergência a resolver (chat de sistema):** a decisão 7 diz que a troca para "Retirada pessoal" foi feita antes do deploy da v1.4 e que "nenhuma categoria com o nome antigo chega ao banco de produção". O log de 20/11 diz o contrário: a renomeação **ainda não está no ar** e entra com o deploy da v1.5. Confirmar se existem, no banco do Somee, categorias criadas com o nome antigo ("Retirada pessoal (pró-labore)" ou similar) e, se existirem, se a v1.5 as renomeia; corrigir a frase da decisão 7.
- **Se a v1.5 não for publicada até 24/11:** o artigo continua correto (fala em "versão 1.5", sem afirmar que está no ar); só o slide de demonstração depende do vídeo, que já está gravado.

## 6. Lembretes de fechamento (pedidos de Enrique)

- **To-dos do sistema (dev, qualidade, devops) e validação contra o questionário:** ver `docs/TODO_SISTEMA.md` (19/11/2026). Inclui o teste do PDF do relatório e a falta de CI de testes.

- Atualizar o artigo com a situação atual do sistema.
- Atualizar a apresentação com diagramas corretos e alinhados às funcionalidades.
- Atualizar os testes Cypress e gerar uma nova demo prática, completa e lúdica (gravação local após o congelamento).
- Documentação de entrega para a orientadora (README, manual, lista de segredos).

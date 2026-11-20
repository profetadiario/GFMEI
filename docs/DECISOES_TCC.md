# Decisões e entregas do TCC — Controle Financeiro MEI

> **Regra de trabalho:** esta pasta `docs/` é a fonte de verdade entre o chat de sistema/código e o chat do artigo/apresentação.
> Toda decisão e toda entrega de lote são registradas aqui (data, o que entrou, telas, testes, versão).

## 1. Marcos fixos

- Banca: 04/12/2026. Orientadora: Annik Passos Marôcco. Coorientador: Danilo Oliveira Sampaio (UFJF, CAD248, turmas B e D, 58 alunos).
- Pesquisa de campo encerrada em 18/11/2026: 30 respostas, 25 válidas.
- **Congelamento do sistema: 27/11/2026** (sem deploys depois). Depois disso, só a gravação do vídeo de demonstração (Cypress rodando localmente).
- 26/11 = reserva (buffer).
- **Artigo: entregar o quanto antes (prazo da Annik).** A versão 191126 já descreve o lote 1 (v1.4). Só ficam pendentes o parágrafo da entrega única v1.5 (lotes 2 + 3, seção 8; ver seção 4), a linha de captação da tabela de módulos e a validação da Annik. A versão final sai assim que a v1.5 estiver no ar.
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
| 8 | **Calculadora de preço pelo markup divisor da orientadora — DECIDIDO em 19/11/2026 (orientação da Annik).** Fórmula: **markup divisor = 1 − (DF% + DV% + ML%) ÷ 100**; **preço de venda = custo direto da unidade ÷ markup divisor**; markup multiplicador = 1 ÷ markup divisor. DF% = despesas fixas mensais ÷ faturamento médio mensal × 100 (sugerido pelo sistema a partir dos últimos meses com lançamento). DV% = despesas que variam com a venda (taxa de cartão, comissão, frete, embalagem). ML% = lucro desejado. Se DF + DV + ML ≥ 100%, não há preço possível (avisar). **Ajuste para o MEI:** o DAS é um valor fixo mensal, então entra em DF, e não em DV. Mostrar o aviso "markup não é margem de lucro". Exemplo da orientadora para teste: custo R$ 50,00, DF 15%, DV 10%, ML 20% → divisor 0,55, multiplicador 1,818, preço R$ 90,91. Substitui a fórmula da v1.5 (custo + custos fixos ÷ unidades), que deixa de pedir "unidades vendidas por mês". Fonte para o artigo: Sebrae, *Precificação no artesanato* (resposta técnica, mark-up divisor; DF% = custos fixos médios ÷ vendas médias). **Motivo (áudio da Annik, 19/11/2026):** é a fórmula que ela ensina no Coworking Me Conecta; o sistema não pode apresentar um valor diferente do ensinado em sala. Mostrar o markup divisor e o multiplicador. **Confirmado pela Annik em 20/11/2026: o DAS entra no custo fixo (DF)**, e as empreendedoras estão cientes disso. **Pendente:** ela ofereceu exemplos reais de empreendedoras que já calcularam o markup: usar como casos de teste da calculadora. A Annik vai acessar o sistema para validar o que já foi implementado. |

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

## 5. Fonte do texto "o que os bancos pedem ao MEI" (para as referências do artigo)

Sugestão do chat do artigo: o artigo já cita SEBRAE, "Acesso a crédito para pequenos negócios". Se servir para o texto da tela, usar a mesma fonte; se usar outra, registrar abaixo.

**Situação em 19/11/2026: pendente de conferência humana.** O texto da tela é uma lista geral e **não cita fonte** de propósito, até alguém confirmar.

- Candidata: SEBRAE. *Crédito sem ciladas: obtenha crédito de forma segura, consciente e sem cair em armadilhas* (guia prático, PDF de 2025). Link: https://sebrae.com.br/content/dam/portal-sebrae/na/midias/documentos/pdfs/Guia_pr%C3%A1tico_Credito-sem-ciladas.pdf. Acesso em: 19 nov. 2026.
- **Não verificado:** o PDF é todo em imagem (sem texto), e o chat de sistema não conseguiu ler o conteúdo. Não dá para afirmar que ele traz a lista de documentos da tela nem as faixas de 30%/60%, **que são do sistema**.
- Ação: Enrique (ou o chat do artigo) abre o PDF e confirma. Se bater, a tela pode ganhar a linha "Fonte: SEBRAE, Crédito sem ciladas (2025)". Se não bater, usar "Acesso a crédito para pequenos negócios" (já nas referências do artigo) e registrar aqui.

## 6. Lembretes de fechamento (pedidos de Enrique)

- **To-dos do sistema (dev, qualidade, devops) e validação contra o questionário:** ver `docs/TODO_SISTEMA.md` (19/11/2026). Inclui o teste do PDF do relatório e a falta de CI de testes.

- Atualizar o artigo com a situação atual do sistema.
- Atualizar a apresentação com diagramas corretos e alinhados às funcionalidades.
- Atualizar os testes Cypress e gerar uma nova demo prática, completa e lúdica (gravação local após o congelamento).
- Documentação de entrega para a orientadora (README, manual, lista de segredos).

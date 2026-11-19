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
- **Validação da orientadora pendente:** a Annik está revisando o relatório estatístico completo de 18/11/2026 (n = 25). As leituras da seção 8, as comparações entre grupos e a codificação das respostas abertas ficam marcadas como "em validação" no artigo até a aprovação dela.

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

### Entrega única (lotes 2 + 3) — v1.5 (código pronto em 19/11/2026; aguardando deploy e release)

**O que entrou**

- **Linguagem simples:** "protótipo" saiu das telas (rodapé: "sistema desenvolvido em um Trabalho de Conclusão de Curso"; página inicial reescrita em linguagem do dia a dia). README também passa a dizer "sistema".
- **Calculadora de preço** (*Ferramentas → Calculadora de preço*, `/Calculadora`): sem persistência (formulário por GET, nada gravado). Campos: custo da unidade, custos fixos do mês (**já vem sugerido com as despesas fixas do mês anterior**, do DRE), unidades vendidas por mês, taxas sobre a venda (%) e lucro desejado (%). Método do mark-up divisor: `preço = (custo da unidade + custos fixos ÷ unidades) ÷ (1 − (taxas + lucro) ÷ 100)`. Mostra para onde vai cada venda (custo, parte dos fixos, taxas, lucro) e o faturamento/lucro do mês. Avisa quando taxas + lucro ≥ 100% (nenhum preço cobre).
- **Relatório para o banco** (*Ferramentas → Relatório para o banco*, `/Relatorio`): identificação (negócio, responsável, período, data de emissão), faturamento dos 12 meses e médio, sobra média, saldo em caixa, **DRE resumida dos 12 meses** (retirada pessoal à parte, fora do DRE), **tabela mês a mês** (receitas, despesas, retiradas, sobra, saldo final acumulado) e captações já registradas. Botão **Imprimir / salvar em PDF** (impressão do navegador, decisão 3) com CSS de impressão: some menu, rodapé, botões e formulários; A4 retrato.
- **Indicador de capacidade de pagamento:** sobra média mensal = (receitas − despesas do negócio − **retiradas pessoais**, decisão 1) ÷ meses considerados (do 1º mês com lançamento na janela até o mês atual, para não diluir a média de quem começou há pouco). Parcela "confortável" = até **30%** da sobra média; 30–60% = "atenção"; acima de 60% ou sem sobra = "arriscada". A usuária simula uma parcela e vê o percentual e a faixa. As faixas são **referência de prudência do sistema, não regra de banco** (isso está escrito na tela).
- **O que os bancos costumam pedir ao MEI:** quadro no relatório (CNPJ/CCMEI, DASN-SIMEI e DAS em dia, RG/CPF/comprovante de endereço, prova de faturamento, nome sem restrição e parcela que caiba no caixa), com aviso de que cada banco tem exigências próprias. Fonte: ver seção 5 (**pendente de conferência**).
- **Menu:** novo submenu **Ferramentas** (Calculadora de preço, Relatório para o banco); "Captação de Recursos" virou "Captações" no menu; o menu passa a recolher abaixo de 1200 px, para não quebrar linha em 1280 px.

**Telas novas:** Calculadora de preço (Calculadora/Index), Relatório para o banco (Relatorio/Index). **Telas alteradas:** menu e rodapé (_Layout), página inicial (Home/Index).

**Testes:** +34 testes (calculadora: serviço +13, controller +2; relatório: serviço +14, controller +5, contando os casos parametrizados). Total: **310 testes, 310 passando** (`dotnet test`, 19/11/2026).

**Demo Cypress:** +2 passos (11: calculadora → R$ 53,33 por peça; 12: relatório → sobra média R$ 1.515,00, parcela confortável R$ 454,50, simulação de R$ 400,00 = 26,4%, "cabe com folga"). Rodada localmente em 19/11/2026: `npm run demo:gravar` **13/13** e `npm run demo:gravar:celular` **13/13** (~5 min, vídeo 780×1688).

**Versão:** v1.5 (tag e release a criar no GitHub após o deploy conferido).

**Pendências assumidas**

- A impressão do relatório (CSS de impressão) não foi conferida em papel/PDF: só a tela. Conferir com Ctrl+P no Somee antes da banca.
- Fonte do quadro "o que os bancos pedem" (seção 5) precisa ser conferida por uma pessoa.

## 5. Fonte do texto "o que os bancos pedem ao MEI" (para as referências do artigo)

Sugestão do chat do artigo: o artigo já cita SEBRAE, "Acesso a crédito para pequenos negócios". Se servir para o texto da tela, usar a mesma fonte; se usar outra, registrar abaixo.

**Situação em 19/11/2026: pendente de conferência humana.** O texto da tela é uma lista geral e **não cita fonte** de propósito, até alguém confirmar.

- Candidata: SEBRAE. *Crédito sem ciladas: obtenha crédito de forma segura, consciente e sem cair em armadilhas* (guia prático, PDF de 2025). Link: https://sebrae.com.br/content/dam/portal-sebrae/na/midias/documentos/pdfs/Guia_pr%C3%A1tico_Credito-sem-ciladas.pdf. Acesso em: 19 nov. 2026.
- **Não verificado:** o PDF é todo em imagem (sem texto), e o chat de sistema não conseguiu ler o conteúdo. Não dá para afirmar que ele traz a lista de documentos da tela nem as faixas de 30%/60%, **que são do sistema**.
- Ação: Enrique (ou o chat do artigo) abre o PDF e confirma. Se bater, a tela pode ganhar a linha "Fonte: SEBRAE, Crédito sem ciladas (2025)". Se não bater, usar "Acesso a crédito para pequenos negócios" (já nas referências do artigo) e registrar aqui.

## 6. Lembretes de fechamento (pedidos de Enrique)

- Atualizar o artigo com a situação atual do sistema.
- Atualizar a apresentação com diagramas corretos e alinhados às funcionalidades.
- Atualizar os testes Cypress e gerar uma nova demo prática, completa e lúdica (gravação local após o congelamento).
- Documentação de entrega para a orientadora (README, manual, lista de segredos).

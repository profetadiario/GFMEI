# Decisões e entregas do TCC — Controle Financeiro MEI

> **Regra de trabalho:** esta pasta `docs/` é a fonte de verdade entre o chat de sistema/código e o chat do artigo/apresentação.
> Toda decisão e toda entrega de lote são registradas aqui (data, o que entrou, telas, testes, versão).

## 1. Marcos fixos

- Banca: 04/12/2026. Orientadora: Annik Passos Marôcco. Coorientador: Danilo Oliveira Sampaio (UFJF, CAD248, turmas B e D, 58 alunos).
- Pesquisa de campo encerrada em 18/11/2026: 30 respostas, 25 válidas.
- **Congelamento do sistema: 27/11/2026** (sem deploys depois). Depois disso, só a gravação do vídeo de demonstração (Cypress rodando localmente).
- 26/11 = reserva (buffer).

## 2. Decisões aprovadas (18/11/2026)

| # | Decisão |
|---|---------|
| 1 | **Retirada pessoal (pró-labore)** aparece em linha separada no Painel, fora de "Despesas". A sobra média do indicador de parcela (lote 3) desconta o pró-labore. |
| 2 | **Reserva de emergência** fica como trabalho futuro. |
| 3 | **Impressão do navegador** é aceita como PDF do relatório para o banco. |
| 4 | Enrique roda os testes, o deploy e a conferência no Somee **a cada lote**, antes de começar o próximo. |
| 5 | Os prints de telas para o artigo/apresentação saem da **demo do Cypress**. |
| 6 | O teste com a turma depende do Danilo; Enrique avisa se acontecer. |

## 3. Plano em três lotes

| Lote | Versão | Janela | Conteúdo |
|------|--------|--------|----------|
| 1 | v1.4 | ~20–21/11 | Lançamento rápido (negócio × pessoal), saldo em destaque, gráfico de gastos por categoria + frase do maior gasto, lembrete de "dias sem lançar", consultas unificadas do Painel |
| 2 | v1.5 | ~23–24/11 | Textos em linguagem simples (inclui corrigir "protótipo"), calculadora de preço (sem persistência; custos fixos sugeridos do mês anterior) |
| 3 | v1.6 | 25/11 | Relatório para o banco (12 meses, DRE resumida, fluxo, CSS de impressão) + indicador de capacidade de pagamento de parcela + texto sobre o que os bancos pedem ao MEI (com fonte citada) |

Trabalho futuro (fora do escopo): reserva de emergência, OCR de notas, ranking de produtos, WhatsApp, estoque, PDF gerado no servidor, lembretes por e-mail/push, data de criação e saldo inicial.

Redação sugerida para o artigo: trocar "indicadores de necessidade de capital" por "relatório de apoio ao crédito e indicador de capacidade de pagamento" (é o que o sistema realmente entrega).

## 4. Entregas

### Lote 1 — v1.4 (código pronto em 19/11/2026; aguardando teste, deploy e release)

**O que entrou**

- **Lançamento rápido** (tela *Lançamentos → Novo lançamento*): campos grandes para celular; ordem Tipo → Valor → Categoria; descrição opcional (se vazia, usa o nome da categoria); escolha "Do negócio" / "Pessoal (pró-labore)" para saídas; botão **Salvar e lançar outro**.
- **Retirada pessoal (pró-labore):** novo valor de enum `NaturezaDespesa.RetiradaPessoal = 4`, **sem migração de banco**. A categoria "Retirada pessoal (pró-labore)" é criada automaticamente por usuária na primeira retirada. Fica **fora do DRE** (não altera o lucro líquido) e **dentro do saldo em caixa**.
- **Painel:** saldo geral em destaque; linha separada "Retiradas pessoais (pró-labore)"; gráfico de rosca "Para onde foi o dinheiro este mês" (até 6 fatias, demais agrupadas em "Outras categorias") com a frase do maior gasto; aviso quando passam 3 dias ou mais sem lançamento.
- **Lançamentos (lista):** cards separados para Receitas, Despesas, Retiradas pessoais e Saldo do mês.
- **Desempenho:** o Painel passou a usar uma única chamada `ObterPainelAsync` (2 consultas) no lugar de ler todos os lançamentos duas vezes.

**Telas afetadas:** Painel (Dashboard/Index), Lançamentos (Transacoes/Index), Novo lançamento (Transacoes/Create).

**Testes:** +40 testes novos (controller de lançamentos +11, controller do Painel +3, enums +2, ViewModel do Painel +12, serviço de fluxo de caixa +11, DRE +1). Total após o lote: *baseline anterior (>190, valor exato na última execução) + 40* — **preencher com o número do `dotnet test`**: ______.

**Versão:** v1.4 (tag e release a criar no GitHub após o deploy conferido).

**Pendências assumidas**

- Bootstrap, Chart.js e jQuery continuam carregando do CDN jsdelivr (hospedar localmente não foi feito: precisa baixar os arquivos na máquina do Enrique). Anotado como melhoria opcional.
- Nada foi compilado fora da máquina do Enrique; a validação é o `dotnet test` dele.
- Os textos da demo do Cypress continuam compatíveis (ids `#Descricao #Tipo #CategoriaId #Valor #Data`, botão "Salvar", textos "Saldo geral", "Receitas em", "Despesas em"), mas a demo ainda não foi rodada contra o sistema publicado.

### Lote 2 — v1.5
(a preencher)

### Lote 3 — v1.6
(a preencher)

## 5. Fonte do texto "o que os bancos pedem ao MEI" (para as referências do artigo)

(a preencher no lote 3)

- Autor/instituição:
- Título:
- Link:
- Data de acesso:

## 6. Lembretes de fechamento (pedidos de Enrique)

- Atualizar o artigo com a situação atual do sistema.
- Atualizar a apresentação com diagramas corretos e alinhados às funcionalidades.
- Atualizar os testes Cypress e gerar uma nova demo prática, completa e lúdica (gravação local após o congelamento).
- Documentação de entrega para a orientadora (README, manual, lista de segredos).

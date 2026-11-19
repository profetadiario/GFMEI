# Pós-TCC — branch `pos-tcc-improvements`

> Branch criada em 19/11/2026 a partir da `develop` (v1.5). **Não vai para a banca** e **não entra na `master`** sem verificação do Enrique depois da apresentação.

## Por que existe

A proposta original do TCC falava em **"indicadores de necessidade de capital"**. Como o sistema entregue na banca (v1.5) oferece relatório de apoio ao crédito e indicador de capacidade de pagamento, o artigo trocou o termo (ver `DECISOES_TCC.md`, seção 3). A **reserva de emergência** ficou como trabalho futuro (decisão 2). Esta branch adianta esses dois pontos.

## O que entrou

- **Reserva e capital de giro** (*Ferramentas → Reserva e capital de giro*, `/NecessidadeCapital`), sem persistência (GET):
  - **Saídas médias por mês** = (despesas do negócio + retiradas pessoais) ÷ meses com lançamentos nos últimos 12 meses (mesma janela do relatório para o banco).
  - **Reserva necessária** = saídas médias × meses de cobertura (padrão 3; de 1 a 12).
  - **Falta guardar** = reserva − saldo em caixa (saldo negativo não desconta).
  - **Meses de fôlego** = saldo em caixa ÷ saídas médias ("dá para X meses sem vender").
  - **Prazo para formar a reserva**, guardando por mês o valor informado (padrão: 30% da sobra média, o mesmo limite da parcela confortável).
- Testes: +9 (serviço +8, controller +1). Total na branch: **319/319**.
- Cypress: +1 passo (13). Cenário da demo: saídas R$ 2.535,00 → reserva de 3 meses R$ 7.605,00; caixa R$ 1.515,00 → falta R$ 6.090,00; fôlego 0,6 mês; guardando R$ 454,50/mês → 14 meses. **14/14** no desktop (19/11/2026).

## Para avaliar depois do TCC

- Validar com a Annik se os indicadores seguem a linha que ela esperava (e se faltam outros, ex.: necessidade de capital de giro pelo ciclo de caixa, que exige prazos médios de recebimento/pagamento que o sistema não registra hoje).
- Confirmar a recomendação de 3 a 6 meses de reserva com uma fonte (Sebrae/BCB) antes de citar.
- Itens do trabalho futuro ainda não feitos: OCR de notas, ranking de produtos, WhatsApp, estoque, PDF no servidor, lembretes por e-mail/push, data de criação e saldo inicial.

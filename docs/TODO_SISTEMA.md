# To-dos do sistema (desenvolvimento, qualidade e devops)

> Frente do **chat de sistema**. Artigo e apresentação ficam com o outro chat. Decisões continuam em `DECISOES_TCC.md`.
> Atualizado em 23/11/2026. Marcos: **congelamento 27/11** (último deploy), reserva 26/11, **banca 04/12**.

Legenda: **[Sistema]** = este chat · **[Enrique]** = só você pode fazer · **[Annik]** = depende dela · ⛔ = bloqueado

## Caminho crítico até o congelamento

1. ~~Annik responde a decisão 8~~ **Desbloqueado em 20/11/2026: o DAS entra no custo fixo (DF).** Exemplos reais da Annik continuam pendentes, mas **não bloqueiam o deploy** (viram casos de teste extras quando chegarem).
2. ✅ Calculadora refeita e mesclada na `develop` (20/11).
3. ✅ Teste do PDF do relatório passando, 24/24 (20/11) — achou e corrigiu o card da DRE cortado na impressão.
4. ✅ v1.5 no ar em 21/11 (master `b06611d`, tag `v1.5`).
5. ✅ **Hotfix da legenda do DRE** (`dd5ee6c`) no ar em 23/11 (master).
6. ✅ **Decisões 9 e 10 no código** (branch `feat/pro-labore`, 23/11): pró-labore no lugar da retirada pessoal, calculadora da planilha da Adriana; 327 testes, Cypress 13/13 + 5/5, PDF 26/26; vídeo, capturas, figuras e documentação técnica refeitos.
7. Verificar retiradas antigas no Somee (`docs/DECISAO10_DADOS_ANTIGOS.md`, passo 1) [Enrique] → decidir conversão.
8. Deploy da v1.6 — checklist em `docs/DEPLOY_V1.6.md`.

Meta: **v1.6 no ar até 26/11**; 27/11 congelamento.

## 1. Desenvolvimento

- [x] **Calculadora pelo markup da Annik (decisão 8)** [Sistema] — feita na `feat/markup-annik` e mesclada na `develop` em 20/11.
  - Entradas: custo direto da unidade, DF%, DV%, ML%. Sai "unidades vendidas por mês".
  - DF% sugerido = (despesas fixas + DAS) médias ÷ faturamento médio dos últimos meses com lançamento; editável à mão (quem lança pouco não tem histórico).
  - Mostrar preço, markup divisor, markup multiplicador e o aviso "markup não é margem de lucro". Bloquear quando DF + DV + ML ≥ 100%.
  - ~~[Annik] DAS em DF ou DV~~ **Decidido em 20/11/2026: DAS em DF.**
- [x] **Calculadora pela planilha da Adriana (decisão 9)** [Sistema, 23/11] — caso de teste R$ 58,77 / R$ 50,42.
- [ ] **Valores originais da planilha da Adriana e denominador do DF%/DV%** [Annik] — os valores mensais do teste foram reconstruídos; trocar se ela mandar os originais.
- [x] **Pró-labore no lugar da retirada pessoal (decisão 10)** [Sistema, 23/11].
- [ ] **Fonte do quadro "o que os bancos pedem"** [Enrique] — conferir o PDF do Sebrae (seção 5 do DECISOES). Se bater, [Sistema] coloca a linha de fonte na tela.
- [ ] **Branch `pos-tcc-improvements`** [Sistema, depois da banca] — rebase na `develop` depois do merge da calculadora nova. Não vai para a banca.

## 2. Qualidade

- [x] **Teste do PDF do relatório para o banco** [Sistema] — `npm run teste:pdf`, 24/24 em 20/11.
  - Script Node (`npm run teste:pdf` em `cypress-demo/`), com Chrome headless: entra com a conta da demo, abre `/Relatorio?parcela=400`, gera o PDF em A4 como o Ctrl+P do navegador e lê o texto do PDF.
  - Verifica: identificação (negócio, responsável, período); faturamento R$ 4.050,00; sobra média R$ 1.515,00; parcela confortável R$ 454,50; resultado "Cabe com folga" 26,4%; DRE com lucro líquido; 12 linhas na tabela mês a mês; quadro dos bancos (DASN-SIMEI).
  - Verifica que **não** aparecem: menu, rodapé, botão "Imprimir", formulário de simulação.
  - Verifica que o PDF tem no máximo 2 páginas A4 e que nenhum card é cortado no meio da página.
  - Rodar após o passo 12 da demo (depende do cenário criado por ela) e também contra o Somee após o deploy.
- [x] **Testes da calculadora nova** [Sistema, 20/11: 21 testes; total 316/316] — unitários com o exemplo da Annik (custo R$ 50, DF 15%, DV 10%, ML 20% → divisor 0,55, multiplicador 1,818, preço R$ 90,91), limites (soma ≥ 100%, negativos, vazio), DF% sugerido sem histórico e com histórico; atualizar o passo 11 do Cypress.
- [ ] **Conferência manual do Ctrl+P no Somee** [Enrique] — Chrome e celular, depois do deploy.
- [ ] **Smoke test em produção após cada deploy** [Enrique + Sistema] — rodar a demo contra o Somee com uma conta de teste (`CYPRESS_BASE_URL`), sem usar dados de usuárias reais.
- [ ] **Validação contra o questionário** [Sistema, feito em 19/11 — ver seção 4] — refazer quando a Annik validar as leituras do relatório estatístico.

## 3. DevOps

- [x] **CI de build e testes** [Sistema] — `.github/workflows/ci.yml` (20/11).
- [x] **Deploy só com testes verdes** [Sistema] — `deploy-somee.yml` chama o CI como job `testes` (20/11).
- [ ] **Merge `develop` → `master` da v1.5** [Enrique] — depende da calculadora (decisão 8) e do teste do PDF.
- [x] **Plano de volta** — a tag `v1.4` já aponta para o commit no ar (`4e10eef`); como voltar está em `docs/DEPLOY_V1.5.md`.
- [ ] **Backup do banco do Somee antes do deploy** [Enrique] — a v1.5 não muda o esquema, mas o backup é barato.
- [ ] **Releases v1.4 e v1.5 no GitHub** [Enrique] — textos prontos em `docs/DEPLOY_V1.5.md`.
- [x] **Lista de segredos e variáveis** [Sistema] — no fim de `docs/DEPLOY_V1.5.md`.
- [x] **Documentação técnica** [Sistema] — RF11, 1.3, 5.1–5.4 e as 8 figuras finais; PDF regerado (20/11).

## 4. Validação contra o questionário (19/11/2026, relatório estatístico de 18/11, n = 25)

As leituras do relatório estão **em validação pela Annik**; os números abaixo são os do relatório.

| O que a pesquisa mostrou | O que o sistema entrega | Alinhado? |
|---|---|---|
| Lançamento rápido indispensável para 23 de 25; poucos cliques (18/24); celular (15/24); linguagem simples (21/24) | Lançamento rápido com campos grandes e "Salvar e lançar outro" (v1.4); textos simples (v1.5); demo em celular | Sim |
| Calculadora de preço indispensável para 17 de 25; precificação é o tema mais citado nas abertas (8 participantes), apesar de 84% dizerem precificar pelos custos | Calculadora pela planilha de markup do curso (decisão 9, v1.6) | Sim, com ressalva: 68% dizem conhecer custos fixos e variáveis, então DF%/DV% são viáveis, mas o DF% precisa vir **sugerido** e as explicações precisam ser curtas |
| Oscilação das vendas é a dificuldade nº 1 do caixa (12/24) | DF% pela **média** dos últimos meses (decisão 8); sobra média no indicador de parcela | Sim |
| Misturar dinheiro pessoal e do negócio (12/23); separação indispensável para 12 de 25 | Decisão 10: retirada pessoal saiu; pró-labore como despesa fixa, lançado no fechamento do mês (v1.6) | Sim (pró-labore deixa explícita a remuneração) |
| Gastos por categoria é o relatório mais pedido (20 de 25) | Gráfico "Para onde foi o dinheiro" (v1.4) | Sim |
| Lucros e prejuízos mensal (17 de 25) | DRE mensal | Sim |
| 15 de 25 não sabem o que o banco exige; afirmação 17 com 56% de discordância | Quadro "o que os bancos pedem" no relatório | Sim (fonte pendente) |
| Controles que ajudariam no crédito: DRE (8), fluxo positivo contínuo (7), histórico de faturamento (6) | Relatório: DRE 12 meses, saldo final mês a mês, faturamento mês a mês | Sim |
| Afirmação 18 (avaliar se consegue pagar um empréstimo): mediana 4, 36% discordam; 3 de 4 que pediram crédito foram recusadas | Indicador de parcela (30% / 60% da sobra) | Sim |
| Metas de reserva é a função de investimento mais escolhida (18 de 24) | Fora da v1.5 (decisão 2); feito na branch `pos-tcc-improvements` | Pós-TCC |
| Ranking de produtos que dão mais lucro (19 de 25) | Não existe | **Lacuna** — trabalho futuro (seção 3 do DECISOES); citar no artigo |
| Alertas de gastos (15/24); contas a vencer (8 de 25) | Só o aviso de "dias sem lançar" | Parcial — trabalho futuro |
| Relatório para a DASN-SIMEI (7 de 25) | Não existe | Baixa prioridade — trabalho futuro |

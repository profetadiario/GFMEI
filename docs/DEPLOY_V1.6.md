# Deploy da v1.6: pró-labore e calculadora da planilha (decisões 9 e 10)

> Preparado em 23/11/2026. O último deploy é em **27/11** (congelamento); 26/11 fica de reserva. O código está na branch `feat/pro-labore`.

## Antes

- [x] **Hotfix da legenda do DRE no ar** (`dd5ee6c`, na master desde 23/11). Ao mesclar a v1.6, a master já tem esse commit; a `feat/pro-labore` traz a mesma mudança, então o merge não deve dar conflito.
- [ ] `dotnet test`: **327/327**.
- [ ] Cypress local: `jornada-completa` **13/13**, `pro-labore` **5/5**, `npm run teste:pdf` **26/26**.
- [ ] **Verificação dos dados antigos no Somee:** passo 1 de `docs/DECISAO10_DADOS_ANTIGOS.md`, que só lê. Anotar o resultado no DECISOES.
- [ ] **Backup do banco do Somee.** A v1.6 não muda o esquema; ela só acrescenta a categoria de pró-labore, por usuária, no login.

## Passo a passo

1. Mesclar `feat/pro-labore` na `develop` e conferir que o CI fica verde.
2. Mesclar `develop` na `master`. O deploy roda sozinho: o job de testes vem antes da publicação.
3. **Conferência no Somee** (com uma conta de teste):
   - [ ] **Novo lançamento:** não aparece mais "De onde saiu o dinheiro?". Escolhendo "Pró-labore (seu salário)", aparece a ajuda "Lance o pró-labore uma vez, no fechamento do mês".
   - [ ] **Pró-labore repetido:** lançar um segundo no mesmo mês mostra o aviso; clicar em Salvar de novo grava.
   - [ ] **Painel e Lançamentos:** não há linha nem card de "Retiradas pessoais".
   - [ ] **DRE:** a linha "Despesas Fixas (com o pró-labore)" aparece e a legenda do gráfico está branca.
   - [ ] **Calculadora:** 24 dias, 8 h, 90 min, materiais 14,95, lucro 30%, com os valores do caso da Adriana → **R$ 58,77**, markup 1,62.
   - [ ] **Relatório:** não há coluna "Retiradas"; a DRE resumida mostra "Despesas fixas (com o pró-labore)"; o Ctrl+P sai em 2 páginas.
4. Se o passo 1 de "Antes" achou lançamentos antigos, rodar a conversão (passo 2 de `docs/DECISAO10_DADOS_ANTIGOS.md`) **só com o ok do Enrique**.
5. Tag `v1.6` no commit da `master` e release com o texto abaixo.
6. Registrar no DECISOES a data e hora do deploy e o resultado da conferência.

## Texto da release v1.6

**Título:** v1.6: Pró-labore no lugar da retirada pessoal e calculadora da planilha de markup

Fechamento do sistema para a banca, com as duas últimas orientações da professora orientadora.

- **Pró-labore:** a empreendedora se remunera pelo pró-labore, lançado uma vez, no fechamento do mês. A categoria padrão "Pró-labore (seu salário)" é despesa fixa e entra no DRE, no relatório para o banco e nos custos fixos da calculadora. O sistema mostra essa orientação ao escolher a categoria e avisa, sem bloquear, quando já existe pró-labore no mesmo mês.
- **Saiu a retirada pessoal:** não há mais a opção "Pessoal" no lançamento nem as linhas de retiradas no Painel, em Lançamentos e no Relatório. Lançamentos antigos continuam contando, como despesa fixa.
- **Calculadora de preço:** segue a planilha de markup usada no Coworking Me Conecta.
  - Cada conta do mês é rateada pelo tempo de produção da unidade.
  - A base é formada pelos custos fixos mais os materiais.
  - DF% e DV% são calculados sobre o total geral.
  - O markup é 100 ÷ (100 − (DF% + DV% + lucro%)), com duas casas truncadas.
  - Os valores do mês vêm sugeridos pela média dos meses fechados.
  - Exemplo: 24 dias, 8 h, 90 min, lucro de 30% → **R$ 58,77**.
- **Qualidade:** 327 testes automatizados, demo Cypress 13/13 e teste novo do pró-labore 5/5. O teste do PDF passa em 26/26.

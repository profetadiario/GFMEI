# Deploy da v1.5 — checklist (Enrique executa)

> Preparado em 20/11/2026. Meta: **v1.5 no ar até 24/11**; 26/11 reserva; **27/11 congelamento** (nenhum deploy depois).
> A `develop` já tem tudo: calculadora pelo markup da Annik (decisão 8), relatório, indicador de parcela, teste do PDF, CI.

## Antes de começar

- [ ] O CI da `develop` está verde no GitHub (aba **Actions → CI (build e testes)**, último push da `develop`).
- [ ] `dotnet test` local: **316/316**.

## Passo a passo

1. **Tag v1.4 no commit que está no ar** — ✅ **já existe**: `v1.4` aponta para `4e10eef`, que é o topo atual da `master` (o que está no Somee). Só conferir:
   ```powershell
   git fetch --tags; git rev-list -n 1 v1.4; git rev-parse origin/master
   ```
   Os dois têm que dar `4e10eef…`. **Plano de volta:** se a v1.5 der problema, `git checkout master; git reset --hard v1.4; git push --force-with-lease origin master` republica a v1.4 (o deploy roda de novo sozinho). Use só em último caso.

2. **Backup do banco no Somee** — painel do Somee → *Databases* → banco do GFMEI → *Backup* (ou *Download backup*). Guardar o `.bak` fora do repositório, com a data no nome. A v1.5 **não muda o esquema** (só lê os dados e acrescenta o valor 4 ao enum, já usado desde a v1.4 local), mas o backup é barato.

3. **Merge `develop` → `master`** (dispara o deploy automático):
   ```powershell
   git checkout master; git pull; git merge --no-ff develop -m "release: v1.5"; git push origin master; git checkout develop
   ```
   No GitHub, **Actions → Deploy para o Somee.com**: o job **testes** roda primeiro; o **build-e-deploy** só começa se ele passar. O site fica fora do ar por ~1 minuto (página "Atualizando o sistema").

4. **Conferência no Somee** (com uma conta de teste, nunca de usuária real):
   - [ ] **Painel:** saldo geral em destaque, gráfico "Para onde foi o dinheiro", retiradas pessoais em linha separada.
   - [ ] **Novo lançamento:** venda e despesa com "Salvar e lançar outro"; uma saída "Pessoal" vira "Retirada pessoal".
   - [ ] **Calculadora** (*Ferramentas*): custo 50, DF 15, DV 10, ML 20 → **R$ 90,91**, divisor **0,55**, multiplicador **1,818**, aviso "markup não é margem de lucro".
   - [ ] **Relatório para o banco** (*Ferramentas*): simular uma parcela e ver a faixa.
   - [ ] **Ctrl+P** no relatório (Chrome e celular): 2 páginas A4, sem menu nem botões, DRE inteira (não cortada).

5. **Smoke test do Cypress contra o Somee** (cria uma conta de teste nova; o Somee limita cadastros/logins por minuto por IP, então rode uma vez só):
   ```powershell
   cd cypress-demo
   $env:CYPRESS_BASE_URL = "<endereço do site no Somee>"; npx cypress run --spec cypress/e2e/jornada-completa.cy.js --config video=false
   $env:BASE_URL = "<endereço do site no Somee>"; npm run teste:pdf
   ```
   Esperado: **13/13** na demo e **24/24** no teste do PDF.

6. **Release v1.5 no GitHub** — *Releases → Draft a new release*, tag `v1.5` no commit do merge na `master`, título e texto abaixo. Se a release **v1.4** ainda não existir, criar antes a partir da tag `v1.4`.

7. **Avisar o chat do artigo** (registrar no DECISOES): data/hora do deploy, resultado da conferência e do smoke test.

8. **Opcional, depois do deploy:** regravar o vídeo contra o site publicado: `BASE_URL=<endereço do site no Somee> npm run demo:apresentacao` (gera de novo os arquivos em `docs/demo/`).

---

## Texto da release v1.4

**Título:** v1.4 — Lançamento rápido e retirada pessoal

O sistema ficou mais rápido no dia a dia e passou a separar o dinheiro do negócio do dinheiro de uso pessoal.

- **Lançamento rápido:** campos grandes, pensados para o celular, na ordem tipo → valor → categoria. A descrição é opcional (se ficar vazia, usa o nome da categoria) e o botão **Salvar e lançar outro** permite registrar várias vendas seguidas.
- **Retirada pessoal:** nas saídas, a opção **Pessoal** registra o dinheiro que a empreendedora tirou do negócio para uso próprio. Ela fica **fora do DRE** (não é despesa do negócio) e **dentro do saldo em caixa**. Não exige mudança no banco de dados.
- **Painel:** saldo geral em destaque, retiradas pessoais em linha separada, gráfico **"Para onde foi o dinheiro este mês"** com a frase do maior gasto e aviso quando passam 3 dias ou mais sem lançamento.
- **Lançamentos:** cards separados para receitas, despesas, retiradas pessoais e saldo do mês.
- **Desempenho:** o Painel passou a ler os lançamentos uma vez só.

## Texto da release v1.5

**Título:** v1.5 — Calculadora de preço, relatório para o banco e indicador de parcela

Entrega única que fecha a parte de sistema do TCC: ferramentas de apoio à decisão, em linguagem simples.

- **Calculadora de preço** (*Ferramentas → Calculadora de preço*): usa o **markup divisor** ensinado às empreendedoras — markup divisor = 1 − (DF% + DV% + ML%) ÷ 100 e preço = custo direto ÷ markup divisor. O **DF%** (despesas fixas em % do faturamento, **com o DAS incluído**) já vem sugerido a partir dos lançamentos e pode ser editado. Mostra o preço, o markup divisor, o multiplicador e para onde vai cada venda (custo, despesas fixas, despesas variáveis e lucro), com o aviso de que **markup não é margem de lucro**. Bloqueia a conta quando DF + DV + ML chega a 100%. Exemplo: custo R$ 50,00, DF 15%, DV 10%, lucro 20% → **R$ 90,91**.
- **Relatório para o banco** (*Ferramentas → Relatório para o banco*): os últimos 12 meses do negócio — faturamento total e médio, DRE resumida, tabela mês a mês com sobra e saldo final, captações registradas — e um quadro com o que os bancos costumam pedir ao MEI. Botão **Imprimir / salvar em PDF**, com layout de impressão em até 2 páginas A4.
- **Indicador de parcela:** compara uma parcela com a sobra média do mês (já sem as retiradas pessoais): até 30% cabe com folga, de 30% a 60% pede atenção, acima disso é arriscada. As faixas são uma referência do sistema, não uma regra de banco.
- **Textos:** "protótipo" saiu das telas; página inicial reescrita em linguagem do dia a dia; menu com o submenu **Ferramentas**.
- **Qualidade:** 316 testes automatizados; teste do PDF do relatório (`npm run teste:pdf`); CI no GitHub Actions — o deploy só acontece com os testes passando.

---

## Segredos e variáveis usados pelo deploy (só os nomes, nunca os valores)

Ficam em **GitHub → Settings → Secrets and variables → Actions** do repositório:

| Segredo | Para que serve |
|---|---|
| `SOMEE_DB_CONNECTION_STRING` | Connection string do SQL Server do Somee; o deploy troca `__SOMEE_CONNECTION_STRING__` em `appsettings.Production.json` por ela. |
| `SOMEE_FTP_HOST` | Servidor FTP do Somee. |
| `SOMEE_FTP_USER` | Usuário do FTP. |
| `SOMEE_FTP_PASSWORD` | Senha do FTP. |

Variáveis locais (não são segredos): `BASE_URL` / `CYPRESS_BASE_URL` (endereço do site para a demo e o teste do PDF), `PDF_EMAIL` / `PDF_SENHA` (conta **de teste** para o teste do PDF), `CHROME_PATH` (opcional).

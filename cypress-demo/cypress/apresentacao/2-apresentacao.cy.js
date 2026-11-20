// =======================================================================
// Vídeo da apresentação — PARTE 2 (é esta que vira o vídeo).
//
// Roteiro, na ordem da narrativa da pesquisa:
//   1. Login e Painel          5. Fluxo de caixa
//   2. Lançamento rápido       6. Calculadora (exemplo da orientadora)
//   3. Retirada pessoal        7. Relatório para o banco + Ctrl+P
//   4. DRE do mês
// Dados fictícios (conta criada pela parte 1). Tira uma captura 1920x1080
// de cada tela (01_painel.png ... 07_relatorio_banco.png).
//
// O ritmo vem de Cypress.env('pausas'): multiplica as esperas (2 = apresentação,
// 0.4 = versão curta de reserva, 0 = nenhuma)
// (versão curta de reserva). Rodado por scripts/gravar-apresentacao.js.
// =======================================================================

const fator = () => Number(Cypress.env('pausas') ?? 1);
const espera = (ms) => {
  if (fator() > 0) cy.wait(ms * fator(), { log: false });
};
const captura = (nome) => {
  if (Cypress.env('capturas')) cy.screenshot(nome, { capture: 'viewport', overwrite: true });
};
const rolarAte = (seletorOuElemento, topo = -120) =>
  cy.get(seletorOuElemento).first().scrollIntoView({ duration: fator() ? 900 : 0, offset: { top: topo, left: 0 } });

const imprimir = (ligado) =>
  Cypress.automation('remote:debugger:protocol', {
    command: 'Emulation.setEmulatedMedia',
    params: { media: ligado ? 'print' : '' },
  });

describe('Vídeo da apresentação', () => {
  it('mostra o sistema em 7 telas', () => {
    cy.readFile('cypress/videos-apresentacao/conta.json').then((conta) => {
      // 1. Login e Painel
      cy.visit('/Conta/Login');
      espera(800);
      cy.get('#Email').type(conta.email);
      cy.get('#Senha').type(conta.senha);
      espera(400);
      cy.contains('button', 'Entrar').click();
      cy.location('pathname').should('eq', '/Dashboard');
      cy.contains('.card', 'Saldo geral').should('be.visible');
      espera(2500);
      captura('01_painel');
      rolarAte('#graficoGastosCategoria', -260);
      espera(3500);

      // 2. Lançamento rápido: uma venda e uma despesa ("Salvar e lançar outro")
      cy.visit('/Transacoes/Create');
      espera(1000);
      cy.get('#Tipo').select('1');
      cy.get('#Valor').clear().type('180,00');
      cy.get('#CategoriaId').select('Vendas (Receita)');
      cy.get('#Descricao').type('Bolo de aniversário (encomenda)');
      espera(1200);
      cy.contains('button', 'Salvar e lançar outro').click();
      cy.contains('Lançamento salvo').should('be.visible');
      espera(1500);
      cy.get('#Tipo').select('2');
      cy.get('#Valor').clear().type('95,00');
      cy.get('label[for=origemNegocio]').click();
      cy.get('#CategoriaId').select('Fornecedores/Insumos (Despesa)');
      cy.get('#Descricao').type('Chocolate e leite condensado');
      espera(1200);
      captura('02_lancamento_rapido');
      cy.get('form').contains('button', /^\s*Salvar\s*$/).click();
      cy.location('pathname').should('eq', '/Transacoes');
      espera(2000);

      // 3. Retirada pessoal: separada das despesas e fora do DRE
      cy.visit('/Transacoes/Create');
      espera(800);
      cy.get('#Tipo').select('2');
      cy.get('#Valor').clear().type('400,00');
      cy.get('label[for=origemPessoal]').click();
      espera(1500);
      cy.get('form').contains('button', /^\s*Salvar\s*$/).click();
      cy.location('pathname').should('eq', '/Transacoes');
      cy.contains('.card', 'Retiradas pessoais').should('contain', '400,00');
      espera(1500);
      captura('03_retirada_pessoal');
      espera(2000);

      // 4. DRE do mês: a retirada não entra no lucro
      cy.irPeloMenu('DRE');
      cy.location('pathname').should('eq', '/Dre');
      espera(2000);
      cy.contains('tr', 'LUCRO LÍQUIDO DO MÊS').should('be.visible');
      rolarAte('#graficoDre', -420);
      espera(1500);
      captura('04_dre');
      espera(2500);

      // 5. Fluxo de caixa: o mês atual, com o saldo que passa de um mês para o outro
      cy.irPeloMenu('Fluxo de Caixa');
      cy.location('pathname').should('eq', '/FluxoCaixa');
      espera(1500);
      cy.contains('tr', 'SALDO FINAL DO CAIXA').find('td').eq(new Date().getMonth() + 1)
        .scrollIntoView({ duration: fator() ? 1200 : 0, offset: { top: -380, left: -700 } });
      espera(1500);
      captura('05_fluxo_caixa');
      espera(2500);

      // 6. Calculadora pelo markup divisor (exemplo da orientadora)
      cy.irPeloMenu('Calculadora de preço');
      cy.location('pathname').should('eq', '/Calculadora');
      cy.get('#sugestaoDf').should('contain', 'Pelos seus lançamentos');
      espera(2000);
      cy.get('#CustoUnitario').clear().type('50,00');
      cy.get('#DespesasFixasPercentual').clear().type('15');
      cy.get('#DespesasVariaveisPercentual').clear().type('10');
      cy.get('#LucroPercentual').clear().type('20');
      espera(800);
      cy.contains('button', 'Calcular preço').click();
      cy.get('#precoSugerido').should('contain', '90,91');
      cy.get('#markupDivisor').should('have.text', '0,55');
      cy.get('#markupMultiplicador').should('have.text', '1,818');
      espera(1500);
      captura('06_calculadora');
      espera(3000);

      // 7. Relatório para o banco: simulação da parcela e Ctrl+P
      cy.irPeloMenu('Relatório para o banco');
      cy.location('pathname').should('eq', '/Relatorio');
      espera(2000);
      rolarAte('#indicadorParcela', -100);
      cy.get('#parcela').clear().type('400,00');
      espera(600);
      cy.contains('button', 'Ver se cabe').click();
      cy.get('#resultadoParcela').should('contain', 'Cabe com folga');
      cy.scrollTo('top', { duration: fator() ? 800 : 0 });
      espera(1500);
      captura('07_relatorio_banco');
      espera(2500);

      // "Imprimir / salvar em PDF": em vez de abrir a janela do sistema, mostra
      // a página como sai no papel (CSS de impressão: sem menu e sem botões).
      cy.scrollTo('top', { duration: fator() ? 600 : 0 });
      cy.window().then((win) => cy.stub(win, 'print').as('imprimir'));
      cy.get('#botaoImprimir').click();
      cy.get('@imprimir').should('have.been.calledOnce');
      cy.then(() => imprimir(true));
      cy.get('nav.navbar').should('not.be.visible');
      espera(2000);
      cy.scrollTo('bottom', { duration: fator() ? 3500 : 0 });
      espera(2500);
      cy.then(() => imprimir(false));
    });
  });
});

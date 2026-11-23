// =======================================================================
// Pró-labore no lugar da retirada pessoal (decisão 10, Annik, 23/11/2026).
//
//   npx cypress run --spec cypress/e2e/pro-labore.cy.js
//
// Confere que a saída "Pessoal" sumiu do lançamento, que o pró-labore
// mostra a ajuda "lance uma vez, no fechamento do mês", que um segundo
// pró-labore no mesmo mês gera só um aviso (dá para salvar mesmo assim) e
// que Painel e Lançamentos não têm mais a linha de retiradas pessoais.
// Cria uma conta de teste própria (dados fictícios).
// =======================================================================

describe('Pró-labore (decisão 10)', () => {
  const usuaria = {
    nome: 'Joana Teste',
    email: `joana.prolabore.${Date.now()}@teste.com`,
    nomeNegocio: 'Bolos da Joana',
    senha: 'SenhaForte@123',
  };

  // Último dia do mês passado: o pró-labore é lançado no fechamento do mês.
  const hoje = new Date();
  const fechamento = new Date(hoje.getFullYear(), hoje.getMonth(), 0);
  const dataInput = `${fechamento.getFullYear()}-${String(fechamento.getMonth() + 1).padStart(2, '0')}-${String(fechamento.getDate()).padStart(2, '0')}`;

  const abrirProLabore = () => {
    cy.visit('/Transacoes/Create');
    cy.get('#Tipo').select('Despesa');
    cy.get('#CategoriaId').select('Pró-labore (seu salário) (Despesa)');
    cy.get('#Valor').clear().type('2000,00');
    cy.get('#Data').invoke('val', dataInput).trigger('input').trigger('change');
  };

  it('Cadastro já traz o pró-labore como despesa fixa', () => {
    cy.registrar(usuaria);
    cy.location('pathname').should('eq', '/Dashboard');
    cy.visit('/Categorias');
    cy.contains('table tbody tr', 'Pró-labore (seu salário)').should('contain', 'Despesa fixa');
  });

  it('Novo lançamento: sem "Pessoal", e o pró-labore mostra a ajuda do fechamento', () => {
    cy.loginComo(usuaria);
    cy.visit('/Transacoes/Create');
    cy.contains('De onde saiu o dinheiro?').should('not.exist');
    cy.get('#origemPessoal').should('not.exist');

    cy.get('#Tipo').select('Despesa');
    cy.get('#CategoriaId').select('Aluguel (Despesa)');
    cy.get('#ajudaProLabore').should('not.be.visible');
    cy.get('#CategoriaId').select('Pró-labore (seu salário) (Despesa)');
    cy.get('#ajudaProLabore').should('be.visible').and('have.text', 'Lance o pró-labore uma vez, no fechamento do mês.');
  });

  it('Primeiro pró-labore do mês salva direto; o segundo só avisa, e salva se confirmar', () => {
    cy.loginComo(usuaria);
    abrirProLabore();
    cy.contains('button', 'Salvar').click();
    cy.location('pathname').should('eq', '/Transacoes');

    abrirProLabore();
    cy.contains('button', 'Salvar').click();
    cy.location('pathname').should('eq', '/Transacoes/Create');
    cy.get('#avisoProLabore').should('contain', 'Você já lançou pró-labore em').and('contain', '2.000,00');

    // É só um aviso: clicar em Salvar de novo grava.
    cy.contains('button', 'Salvar').click();
    cy.location('pathname').should('eq', '/Transacoes');
    cy.visit(`/Transacoes?mes=${fechamento.getMonth() + 1}&ano=${fechamento.getFullYear()}`);
    cy.get('table tbody tr').filter(':contains("Pró-labore")').should('have.length', 2);
    cy.contains('.card', 'Despesas em').should('contain', '4.000,00');
  });

  it('Painel e Lançamentos não têm mais a linha de retiradas pessoais', () => {
    cy.loginComo(usuaria);
    cy.visit('/Dashboard');
    cy.contains(/retiradas? pessoa/i).should('not.exist');
    cy.contains('.card', 'Despesas no período').should('exist');
    cy.visit('/Transacoes');
    cy.contains(/retiradas? pessoa/i).should('not.exist');
  });

  it('Calculadora sugere o pró-labore do mês fechado nos custos fixos', () => {
    cy.loginComo(usuaria);
    cy.visit('/Calculadora');
    cy.get('#sugestaoValores').should('contain', '1 mês fechado');
    cy.get('#CustosFixos_0__Nome').should('have.value', 'Pró-labore (seu salário)');
    cy.get('#CustosFixos_0__Valor').should('have.value', '4000');
  });
});

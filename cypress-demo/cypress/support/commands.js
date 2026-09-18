// ---------------------------------------------------------------------
// Comandos customizados usados pela demo de jornada completa do sistema.
// ---------------------------------------------------------------------

/**
 * Cria uma nova conta preenchendo o formulário de Conta/Registro pela UI.
 * O próprio Registro já autentica a usuária e redireciona para o Painel
 * (ver ContaController.Registro), então nenhum login separado é preciso
 * logo em seguida.
 */
Cypress.Commands.add('registrar', (usuario) => {
  cy.visit('/Conta/Registro');
  cy.get('#Nome').type(usuario.nome);
  cy.get('#Email').type(usuario.email);
  cy.get('#NomeNegocio').type(usuario.nomeNegocio);
  cy.get('#Senha').type(usuario.senha);
  cy.get('#ConfirmarSenha').type(usuario.senha);
  cy.contains('button', 'Criar conta').click();
});

/**
 * Autentica como a usuária informada, reaproveitando o cookie entre os
 * "it" do arquivo via cy.session — assim cada bloco da demo (Categorias,
 * Lançamentos, Painel, DRE...) não precisa repetir a tela de Login: só a
 * primeira chamada de fato passa pelo formulário, as seguintes restauram
 * a sessão já autenticada instantaneamente.
 */
Cypress.Commands.add('loginComo', (usuario) => {
  cy.session(
    usuario.email,
    () => {
      cy.visit('/Conta/Login');
      cy.get('#Email').type(usuario.email);
      cy.get('#Senha').type(usuario.senha);
      cy.contains('button', 'Entrar').click();
      cy.location('pathname').should('eq', '/Dashboard');
    },
    { cacheAcrossSpecs: true },
  );
});

/**
 * Cadastra uma categoria em Categorias/Create.
 *
 * O <select> de Tipo tem id="campoTipo" explícito na view (usado pelo
 * script que mostra/esconde o campo de natureza) — diferente do <select>
 * de Transacoes/Create, que usa o id="Tipo" padrão gerado pelo asp-for.
 * O campo de natureza só existe/importa para categorias de Despesa.
 */
Cypress.Commands.add('criarCategoria', (categoria) => {
  cy.visit('/Categorias/Create');
  cy.get('#Nome').type(categoria.nome);
  cy.get('#campoTipo').select(categoria.tipo);
  if (categoria.tipo === 'Despesa') {
    cy.get('#NaturezaDespesa').select(categoria.natureza);
  }
  cy.contains('button', 'Salvar').click();
  cy.location('pathname').should('eq', '/Categorias');
});

/**
 * Lança uma receita ou despesa em Transacoes/Create. O texto de cada opção
 * de categoria segue o formato "{Nome} ({Receita|Despesa})" (definido em
 * TransacoesController.Create). #Data já vem preenchido com a data de hoje,
 * então não é necessário digitá-lo.
 */
Cypress.Commands.add('criarTransacao', (transacao) => {
  cy.visit('/Transacoes/Create');
  cy.get('#Descricao').type(transacao.descricao);
  cy.get('#Tipo').select(transacao.tipo);
  cy.get('#CategoriaId').select(`${transacao.categoria} (${transacao.tipo})`);
  cy.get('#Valor').type(transacao.valor);
  cy.contains('button', 'Salvar').click();
  cy.location('pathname').should('eq', '/Transacoes');
});

/**
 * Define uma meta financeira em Metas/Create. #MesReferencia já vem
 * preenchido com o 1º dia do mês atual; quando `mesReferencia` é informado
 * (string "yyyy-MM-dd"), o valor é trocado via JS (cy.invoke('val', ...) +
 * eventos input/change) em vez de digitado — inputs type="date" nativos do
 * Chrome exigem que o cy.type() siga a ordem de teclado do seletor visual
 * (mês/dia/ano), o que é frágil em CI; setar o value diretamente é mais
 * confiável e o ASP.NET Core só precisa do evento "input"/"change" disparado
 * para o form capturar o novo valor.
 */
Cypress.Commands.add('criarMeta', (meta) => {
  cy.visit('/Metas/Create');
  cy.get('#Descricao').type(meta.descricao);
  cy.get('#ValorMeta').type(meta.valorMeta);
  if (meta.mesReferencia) {
    cy.get('#MesReferencia').invoke('val', meta.mesReferencia).trigger('input').trigger('change');
  }
  cy.contains('button', 'Salvar').click();
  cy.location('pathname').should('eq', '/Metas');
});

/**
 * Registra uma captação de recursos em Captacoes/Create. #DataObtencao já
 * vem preenchido com a data de hoje, então não é necessário digitá-lo.
 */
Cypress.Commands.add('criarCaptacao', (captacao) => {
  cy.visit('/Captacoes/Create');
  cy.get('#InstituicaoFinanceira').type(captacao.instituicao);
  cy.get('#Valor').type(captacao.valor);
  cy.get('#TaxaJurosMensal').type(captacao.taxa);
  cy.get('#Finalidade').type(captacao.finalidade);
  cy.contains('button', 'Salvar').click();
  cy.location('pathname').should('eq', '/Captacoes');
});

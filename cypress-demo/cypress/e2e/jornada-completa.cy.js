// =======================================================================
// Demo de uso do Controle Financeiro MEI, ponta a ponta, via Cypress.
//
// Conta a jornada de uma nova usuária com um cenário propositalmente rico
// em dados: várias categorias próprias, várias receitas e despesas, duas
// metas financeiras e duas captações de recursos — para que Painel, DRE e
// Fluxo de Caixa fiquem bem preenchidos ao final, em vez de um exemplo
// mínimo de um único lançamento de cada tipo.
//
// Pensada tanto para "ver o sistema funcionando" (rode com
// `npm run demo:aberta` para acompanhar ao vivo, ou `npm run demo:gravar`
// para gerar um vídeo em cypress/videos/) quanto para servir de teste de
// regressão E2E real, já que cada passo tem asserções de verdade.
//
// Ao final, a demo NÃO faz logout: a sessão continua autenticada no
// Painel, e o e-mail/senha da conta criada nesta execução são impressos no
// terminal e salvos em cypress-demo/ultima-conta-demo.txt — para quem
// quiser continuar navegando manualmente com o mesmo cenário de dados.
//
// Pré-requisito: o sistema precisa estar rodando em http://localhost:5100
// (`dotnet run` na pasta do projeto principal) antes de executar esta demo.
// =======================================================================

describe('Jornada completa de uso do Controle Financeiro MEI', () => {
  // E-mail com timestamp: cada execução cria uma conta nova, então a demo
  // pode ser rodada repetidamente sem precisar limpar o banco antes.
  const usuaria = {
    nome: 'Maria da Silva',
    email: `maria.demo.${Date.now()}@teste.com`,
    nomeNegocio: 'Ateliê da Maria',
    senha: 'SenhaForte@123',
  };

  // Categorias próprias, além das 8 padrão já criadas no cadastro.
  const novasCategorias = [
    { nome: 'Internet', tipo: 'Despesa', natureza: 'Despesa fixa' },
    { nome: 'Marketing', tipo: 'Despesa', natureza: 'Despesa variável' },
  ];

  // Receitas do mês — três categorias diferentes.
  const receitas = [
    { descricao: 'Venda de peças em setembro', categoria: 'Vendas', valor: '1500,00', valorExibido: '1.500,00' },
    { descricao: 'Serviço de costura sob medida', categoria: 'Prestação de serviços', valor: '2200,00', valorExibido: '2.200,00' },
    { descricao: 'Comissão por indicação de cliente', categoria: 'Outras receitas', valor: '350,00', valorExibido: '350,00' },
  ];

  // Despesas do mês — uma de cada natureza (CMV, fixa, dedução/imposto,
  // variável) mais uma segunda fixa e uma segunda variável, para exercitar
  // todas as linhas do DRE ao mesmo tempo.
  const despesas = [
    { descricao: 'Compra de tecidos e aviamentos', categoria: 'Fornecedores/Insumos', valor: '980,00', valorExibido: '980,00' },
    { descricao: 'Aluguel do ateliê', categoria: 'Aluguel', valor: '600,00', valorExibido: '600,00' },
    { descricao: 'Internet e telefone do ateliê', categoria: 'Internet', valor: '150,00', valorExibido: '150,00' },
    { descricao: 'DAS de setembro', categoria: 'Impostos (DAS)', valor: '285,00', valorExibido: '285,00' },
    { descricao: 'Combustível para entregas', categoria: 'Transporte', valor: '220,00', valorExibido: '220,00' },
    { descricao: 'Anúncios patrocinados no Instagram', categoria: 'Marketing', valor: '300,00', valorExibido: '300,00' },
  ];

  // Totais resultantes do cenário acima (conferidos nos Passos 8, 9 e 10):
  //   Receita Bruta Total   = 1.500 + 2.200 + 350             = 4.050,00
  //   Deduções e Impostos   = 285 (DAS)                       =   285,00
  //   Receita Líquida       = 4.050 - 285                     = 3.765,00
  //   CMV                   = 980 (Fornecedores/Insumos)      =   980,00
  //   Lucro Bruto           = 3.765 - 980                     = 2.785,00
  //   Despesas Variáveis    = 220 (Transporte) + 300 (Mkt)    =   520,00
  //   Despesas Fixas        = 600 (Aluguel) + 150 (Internet)  =   750,00
  //   Lucro Líquido do Mês  = 2.785 - 520 - 750               = 1.515,00
  //   Total de despesas     = 980+600+150+285+220+300         = 2.535,00

  // Duas metas: uma para o mês atual (já com resultado) e outra para o mês
  // seguinte (ainda zerada), para mostrar a tela de Metas com mais de um
  // card e em estágios diferentes de progresso.
  const hoje = new Date();
  const primeiroDiaProximoMes = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 1);
  const paraInputDate = (data) =>
    `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-01`;

  const metas = [
    { descricao: 'Meta de lucro de setembro', valorMeta: '2000,00', valorExibido: '2.000,00', alcancadoExibido: '1.515,00' },
    {
      descricao: 'Meta de expansão para o próximo mês',
      valorMeta: '3000,00',
      valorExibido: '3.000,00',
      alcancadoExibido: '0,00',
      mesReferencia: paraInputDate(primeiroDiaProximoMes),
    },
  ];

  // Duas captações, de instituições diferentes.
  const captacoes = [
    { instituicao: 'Banco do Povo', valor: '5000,00', valorExibido: '5.000,00', taxa: '1,90', finalidade: 'Compra de uma máquina de costura nova' },
    { instituicao: 'Cooperativa Sicoob', valor: '3200,00', valorExibido: '3.200,00', taxa: '2,45', finalidade: 'Capital de giro para o estoque de fim de ano' },
  ];

  it('Passo 1 — Página inicial apresenta o sistema e leva ao cadastro', () => {
    cy.visit('/');
    cy.contains('h1', 'Controle Financeiro para MEI').should('be.visible');
    cy.contains('a', 'Criar minha conta').click();
    cy.location('pathname').should('eq', '/Conta/Registro');
  });

  it('Passo 2 — Cria uma conta nova e cai automaticamente no Painel', () => {
    cy.registrar(usuaria);

    cy.location('pathname').should('eq', '/Dashboard');
    cy.contains('h2', `Olá, ${usuaria.nomeNegocio}`).should('be.visible');

    // O cadastro já cria 8 categorias padrão (Vendas, Aluguel, Impostos...)
    // para facilitar o primeiro uso — é o que os próximos passos usam.
    cy.contains('.nav-link', 'Painel').should('be.visible');
  });

  it('Passo 3 — Categorias padrão já existem, e a usuária cadastra mais duas', () => {
    cy.loginComo(usuaria);
    cy.visit('/Categorias');

    cy.get('table tbody tr').should('have.length', 8);
    cy.contains('table tbody tr', 'Vendas').should('contain', 'Receita');
    cy.contains('table tbody tr', 'Aluguel').should('contain', 'Despesa fixa');

    novasCategorias.forEach((categoria, indice) => {
      cy.contains('a', '+ Nova categoria').click();
      cy.location('pathname').should('eq', '/Categorias/Create');
      cy.criarCategoria(categoria);

      cy.get('table tbody tr').should('have.length', 8 + indice + 1);
      cy.contains('table tbody tr', categoria.nome).should('contain', categoria.natureza);
    });
  });

  it('Passo 4 — Registra as receitas do mês (três categorias diferentes)', () => {
    cy.loginComo(usuaria);

    receitas.forEach((receita) => {
      cy.criarTransacao({ ...receita, tipo: 'Receita' });
      cy.contains('table tbody tr', receita.descricao).within(() => {
        cy.contains('Receita');
        cy.contains(receita.valorExibido);
      });
    });

    // Resumo do mês em Lançamentos já soma as três receitas.
    cy.contains('Receitas em').parent().should('contain', '4.050,00');
  });

  it('Passo 5 — Registra as despesas do mês (CMV, fixas, variáveis e imposto)', () => {
    cy.loginComo(usuaria);

    despesas.forEach((despesa) => {
      cy.criarTransacao({ ...despesa, tipo: 'Despesa' });
      cy.contains('table tbody tr', despesa.descricao).within(() => {
        cy.contains('Despesa');
        cy.contains(despesa.valorExibido);
      });
    });

    // Com receitas e despesas lançadas, o resumo do mês mostra os dois totais.
    cy.contains('Receitas em').parent().should('contain', '4.050,00');
    cy.contains('Despesas em').parent().should('contain', '2.535,00');
  });

  it('Passo 6 — Define duas metas financeiras (mês atual e o próximo)', () => {
    cy.loginComo(usuaria);

    metas.forEach((meta) => {
      cy.criarMeta(meta);
      cy.contains('.card', meta.descricao).within(() => {
        cy.contains(meta.valorExibido);
        cy.contains(meta.alcancadoExibido);
      });
    });

    cy.get('.card.card-resumo').should('have.length', metas.length);
  });

  it('Passo 7 — Registra duas captações de recursos', () => {
    cy.loginComo(usuaria);

    captacoes.forEach((captacao) => {
      cy.criarCaptacao(captacao);
      cy.contains('table tbody tr', captacao.instituicao).within(() => {
        cy.contains(captacao.valorExibido);
        cy.contains(`${captacao.taxa}%`);
      });
    });

    cy.get('table tbody tr').should('have.length', captacoes.length);
  });

  it('Passo 8 — Painel consolida receitas, despesas, saldo, captações e a meta do mês', () => {
    cy.loginComo(usuaria);
    cy.visit('/Dashboard');

    cy.contains('.card', 'Receitas no período').should('contain', '4.050,00');
    cy.contains('.card', 'Despesas no período').should('contain', '2.535,00');
    cy.contains('.card', 'Saldo geral').should('contain', '1.515,00');
    // Total captado soma as duas captações: 5.000,00 + 3.200,00.
    cy.contains('.card', 'Total captado').should('contain', '8.200,00');

    cy.contains('h5', 'Meta do mês')
      .parents('.card')
      .should('contain', 'Meta de lucro de setembro');

    cy.get('#graficoFluxoCaixa').should('be.visible');
  });

  it('Passo 9 — DRE calcula corretamente todas as linhas do resultado do mês', () => {
    cy.loginComo(usuaria);
    cy.visit('/Dre');

    cy.contains('tr', 'RECEITA BRUTA TOTAL').should('contain', '4.050,00');
    cy.contains('tr', 'Deduções e Impostos').should('contain', '285,00');
    cy.contains('tr', 'RECEITA LÍQUIDA').should('contain', '3.765,00');
    cy.contains('tr', 'Custo da Mercadoria Vendida').should('contain', '980,00');
    cy.contains('tr', 'LUCRO BRUTO').should('contain', '2.785,00');
    cy.contains('tr', 'Despesas Variáveis').should('contain', '520,00');
    cy.contains('tr', 'Despesas Fixas').should('contain', '750,00');
    cy.contains('tr', 'LUCRO LÍQUIDO DO MÊS').should('contain', '1.515,00');

    // Não houve prejuízo neste mês, então o gráfico de composição (pizza)
    // deve aparecer.
    cy.get('#graficoDre').should('be.visible');
  });

  it('Passo 10 — Fluxo de Caixa anual mostra o saldo acumulado mês a mês', () => {
    cy.loginComo(usuaria);
    cy.visit('/FluxoCaixa');

    cy.contains('th', 'JAN').should('be.visible');
    cy.contains('th', 'DEZ').should('be.visible');

    const mesAtual = new Date().getMonth(); // 0 = janeiro
    cy.contains('tr', 'SALDO FINAL DO CAIXA')
      .find('td')
      .eq(mesAtual + 1) // a 1ª coluna é o rótulo da linha
      .should('contain', '1.515,00');
  });

  it('Passo final — mantém a sessão ativa e disponibiliza o e-mail e a senha da conta', () => {
    cy.loginComo(usuaria);
    cy.visit('/Dashboard');
    cy.contains('h2', `Olá, ${usuaria.nomeNegocio}`).should('be.visible');

    const resumoConta = [
      'Conta criada por esta execução da demo Cypress:',
      `  E-mail: ${usuaria.email}`,
      `  Senha:  ${usuaria.senha}`,
      '',
      'Acesse http://localhost:5100/Conta/Login com esses dados para continuar',
      'explorando o sistema com este mesmo cenário — nada é apagado ao final da demo.',
    ].join('\n');

    // cy.log() só aparece no Command Log da UI; cy.task('log', ...) também
    // imprime no terminal (funciona tanto em `demo:aberta` quanto em
    // `demo:gravar`, que roda headless).
    cy.task('log', `\n${'='.repeat(72)}\n${resumoConta}\n${'='.repeat(72)}\n`);
    cy.writeFile('ultima-conta-demo.txt', `${resumoConta}\n`);

    // Sem logout de propósito: a demo termina com a sessão autenticada e o
    // Painel na tela, pronta para continuar sendo explorada manualmente.
  });
});

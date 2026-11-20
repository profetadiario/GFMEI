// =======================================================================
// Vídeo da apresentação — PARTE 1 (preparo, não entra no vídeo).
//
// Cria a conta fictícia de uma confeitaria e lança dois meses anteriores
// e o começo do mês atual, para Painel, DRE, Fluxo de caixa, Calculadora
// e Relatório já terem histórico quando a gravação começar.
// Todos os dados são fictícios. Rodado por scripts/gravar-apresentacao.js.
// =======================================================================

const conta = {
  nome: 'Ana Souza',
  email: `ana.apresentacao.${Date.now()}@teste.com`,
  nomeNegocio: 'Doces da Ana — Confeitaria',
  senha: 'SenhaForte@123',
};

const hoje = new Date();
const dia = (mesesAtras, d) => {
  const data = new Date(hoje.getFullYear(), hoje.getMonth() - mesesAtras, d);
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-${String(data.getDate()).padStart(2, '0')}`;
};

// [meses atrás, dia, tipo (1 = entrada, 2 = saída), categoria, valor, descrição, pessoal?]
const lancamentos = [];
const mesAnterior = (m, vendas, encomendas, insumos) => {
  lancamentos.push(
    [m, 5, 1, 'Vendas', vendas, 'Vendas de bolos e doces'],
    [m, 12, 1, 'Prestação de serviços', encomendas, 'Encomendas para festas'],
    [m, 3, 2, 'Fornecedores/Insumos', insumos, 'Farinha, ovos, chocolate e embalagens'],
    [m, 5, 2, 'Aluguel', '900,00', 'Aluguel da cozinha'],
    [m, 8, 2, 'Gás e energia', '260,00', 'Gás e energia da cozinha'],
    [m, 20, 2, 'Impostos (DAS)', '81,05', 'DAS do MEI'],
    [m, 15, 2, 'Transporte', '180,00', 'Entregas por aplicativo'],
    [m, 28, 2, null, '1500,00', 'Retirada do mês', true],
  );
};
mesAnterior(2, '4800,00', '1200,00', '1900,00');
mesAnterior(1, '5600,00', '900,00', '2100,00');
// Mês atual até ontem (nunca no futuro), para o Painel não mostrar o aviso
// de "dias sem lançar" e o saldo do mês ficar positivo.
const ontem = Math.max(1, hoje.getDate() - 1);
lancamentos.push(
  [0, ontem, 1, 'Vendas', '3900,00', 'Vendas do mês até agora'],
  [0, 1, 2, 'Fornecedores/Insumos', '820,00', 'Insumos do mês'],
  [0, 1, 2, 'Aluguel', '900,00', 'Aluguel da cozinha'],
  [0, 1, 2, 'Gás e energia', '260,00', 'Gás e energia da cozinha'],
  [0, ontem, 2, 'Impostos (DAS)', '81,05', 'DAS do MEI'],
  [0, ontem, 2, 'Transporte', '95,00', 'Entregas por aplicativo'],
);

describe('Preparo do vídeo da apresentação', () => {
  it('cria a conta da confeitaria e o histórico de lançamentos', () => {
    cy.registrar(conta);
    cy.location('pathname').should('eq', '/Dashboard');
    cy.writeFile('cypress/videos-apresentacao/conta.json', conta);

    cy.criarCategoria({ nome: 'Gás e energia', tipo: 'Despesa', natureza: 'Despesa fixa' });

    lancamentos.forEach(([mesesAtras, d, tipo, categoria, valor, descricao, pessoal]) => {
      cy.visit('/Transacoes/Create');
      cy.get('#Tipo').select(String(tipo));
      if (pessoal) {
        cy.get('label[for=origemPessoal]').click();
      } else {
        cy.get('#CategoriaId').select(`${categoria} (${tipo === 1 ? 'Receita' : 'Despesa'})`);
      }
      cy.get('#Valor').clear().type(valor);
      cy.get('#Descricao').clear().type(descricao);
      cy.get('#Data').invoke('val', dia(mesesAtras, d)).trigger('input').trigger('change');
      cy.get('form').contains('button', /^\s*Salvar\s*$/).click();
      cy.location('pathname').should('eq', '/Transacoes');
    });
  });
});

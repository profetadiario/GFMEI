// =======================================================================
// Vídeo da apresentação — PARTE 1 (preparo, não entra no vídeo).
//
// Cria a conta fictícia de uma confeitaria e lança dois meses anteriores
// e o começo do mês atual, para Painel, DRE, Fluxo de caixa, Calculadora
// e Relatório já terem histórico quando a gravação começar.
//
// Os valores mensais são os do caso da planilha da Adriana (decisão 9):
// energia 150, água 32, internet 250, aluguel 300 e pró-labore 2.000
// (custos fixos); DAS 76,90 e IPTU 168 (despesas fixas); gasolina 150 e
// estacionamento 40 (despesas variáveis). Assim, a média dos meses fechados
// que a Calculadora sugere leva aos mesmos R$ 58,77 da planilha.
//
// O pró-labore é lançado no fechamento do mês (decisão 10): o mês retrasado
// já tem o dele; o do mês passado é lançado AO VIVO no vídeo (parte 2).
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
  // d = 0 é o último dia do mês (fechamento).
  const data = d === 0
    ? new Date(hoje.getFullYear(), hoje.getMonth() - mesesAtras + 1, 0)
    : new Date(hoje.getFullYear(), hoje.getMonth() - mesesAtras, d);
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-${String(data.getDate()).padStart(2, '0')}`;
};

const categorias = [
  { nome: 'Energia', tipo: 'Despesa', natureza: 'Despesa fixa' },
  { nome: 'Água', tipo: 'Despesa', natureza: 'Despesa fixa' },
  { nome: 'Internet', tipo: 'Despesa', natureza: 'Despesa fixa' },
  { nome: 'IPTU', tipo: 'Despesa', natureza: 'Dedução ou imposto (ex.: DAS)' },
  { nome: 'Gasolina', tipo: 'Despesa', natureza: 'Despesa variável' },
  { nome: 'Estacionamento', tipo: 'Despesa', natureza: 'Despesa variável' },
];

// [meses atrás, dia, tipo (1 = entrada, 2 = saída), categoria, valor, descrição]
const lancamentos = [];
const mesFechado = (m, vendas, encomendas, insumos, comProLabore) => {
  lancamentos.push(
    [m, 5, 1, 'Vendas', vendas, 'Vendas de bolos e doces'],
    [m, 12, 1, 'Prestação de serviços', encomendas, 'Encomendas para festas'],
    [m, 3, 2, 'Fornecedores/Insumos', insumos, 'Farinha, ovos, chocolate e embalagens'],
    [m, 5, 2, 'Aluguel', '300,00', 'Aluguel da cozinha'],
    [m, 8, 2, 'Energia', '150,00', 'Conta de luz'],
    [m, 8, 2, 'Água', '32,00', 'Conta de água'],
    [m, 10, 2, 'Internet', '250,00', 'Internet e celular'],
    [m, 20, 2, 'Impostos (DAS)', '76,90', 'DAS do MEI'],
    [m, 10, 2, 'IPTU', '168,00', 'IPTU (parcela)'],
    [m, 15, 2, 'Gasolina', '150,00', 'Gasolina das entregas'],
    [m, 15, 2, 'Estacionamento', '40,00', 'Estacionamento'],
  );
  if (comProLabore) lancamentos.push([m, 0, 2, 'Pró-labore (seu salário)', '2000,00', 'Pró-labore do mês']);
};
mesFechado(2, '4800,00', '1200,00', '1900,00', true);
mesFechado(1, '5600,00', '900,00', '2100,00', false); // pró-labore entra no vídeo
// Mês atual até ontem (nunca no futuro), para o Painel não mostrar o aviso
// de "dias sem lançar" e o saldo do mês ficar positivo. Sem pró-labore:
// o mês ainda não fechou.
const ontem = Math.max(1, hoje.getDate() - 1);
lancamentos.push(
  [0, ontem, 1, 'Vendas', '3900,00', 'Vendas do mês até agora'],
  [0, 1, 2, 'Fornecedores/Insumos', '820,00', 'Insumos do mês'],
  [0, 1, 2, 'Aluguel', '300,00', 'Aluguel da cozinha'],
  [0, 1, 2, 'Energia', '150,00', 'Conta de luz'],
  [0, 1, 2, 'Internet', '250,00', 'Internet e celular'],
  [0, ontem, 2, 'Impostos (DAS)', '76,90', 'DAS do MEI'],
  [0, ontem, 2, 'Gasolina', '95,00', 'Gasolina das entregas'],
);

describe('Preparo do vídeo da apresentação', () => {
  it('cria a conta da confeitaria e o histórico de lançamentos', () => {
    cy.registrar(conta);
    cy.location('pathname').should('eq', '/Dashboard');
    cy.writeFile('cypress/videos-apresentacao/conta.json', conta);

    categorias.forEach((c) => cy.criarCategoria(c));

    lancamentos.forEach(([mesesAtras, d, tipo, categoria, valor, descricao]) => {
      cy.visit('/Transacoes/Create');
      cy.get('#Tipo').select(String(tipo));
      cy.get('#CategoriaId').select(`${categoria} (${tipo === 1 ? 'Receita' : 'Despesa'})`);
      cy.get('#Valor').clear().type(valor);
      cy.get('#Descricao').clear().type(descricao);
      cy.get('#Data').invoke('val', dia(mesesAtras, d)).trigger('input').trigger('change');
      cy.get('form').contains('button', /^\s*Salvar\s*$/).click();
      cy.location('pathname').should('eq', '/Transacoes');
    });
  });
});

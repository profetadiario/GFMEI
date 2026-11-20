/**
 * Teste do PDF do "Relatório para o banco" (npm run teste:pdf).
 *
 * Faz o mesmo que a usuária faz com Ctrl+P → "Salvar como PDF": entra no
 * sistema, abre o relatório com uma parcela simulada, gera o PDF em A4 pelo
 * motor de impressão do Chrome e confere o conteúdo do arquivo gerado.
 *
 * Pré-requisito: a demo Cypress já ter rodado (ela cria a conta e o cenário
 * da confeitaria, e grava as credenciais em ultima-conta-demo.txt).
 *
 * Variáveis opcionais:
 *   BASE_URL      (padrão http://localhost:5100; use a URL do Somee no smoke test)
 *   PDF_EMAIL / PDF_SENHA  (padrão: lidas de ultima-conta-demo.txt)
 *   CHROME_PATH   (padrão: Chrome ou Edge instalados)
 */
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');
const pdfParse = require('pdf-parse/lib/pdf-parse.js');

const BASE_URL = (process.env.BASE_URL || 'http://localhost:5100').replace(/\/$/, '');
const SAIDA = path.join(__dirname, '..', 'relatorio-teste.pdf');

function credenciais() {
  if (process.env.PDF_EMAIL && process.env.PDF_SENHA) {
    return { email: process.env.PDF_EMAIL, senha: process.env.PDF_SENHA };
  }
  const arquivo = path.join(__dirname, '..', 'ultima-conta-demo.txt');
  if (!fs.existsSync(arquivo)) {
    throw new Error('Rode a demo Cypress antes (ela cria a conta) ou informe PDF_EMAIL e PDF_SENHA.');
  }
  const texto = fs.readFileSync(arquivo, 'utf8');
  return {
    email: texto.match(/E-mail:\s*(\S+)/)[1],
    senha: texto.match(/Senha:\s*(\S+)/)[1],
  };
}

function navegador() {
  const candidatos = [
    process.env.CHROME_PATH,
    'C:/Program Files/Google/Chrome/Application/chrome.exe',
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    '/usr/bin/google-chrome',
    '/usr/bin/chromium-browser',
  ].filter(Boolean);
  const achado = candidatos.find((c) => fs.existsSync(c));
  if (!achado) throw new Error('Chrome não encontrado: informe CHROME_PATH.');
  return achado;
}

const resultados = [];
function confere(descricao, ok, detalhe = '') {
  resultados.push({ descricao, ok });
  console.log(`${ok ? '  ✔' : '  ✘'} ${descricao}${!ok && detalhe ? ` — ${detalhe}` : ''}`);
}

(async () => {
  const { email, senha } = credenciais();
  const browser = await puppeteer.launch({ executablePath: navegador(), headless: true, args: ['--no-sandbox'] });
  try {
    const page = await browser.newPage();
    await page.setViewport({ width: 1280, height: 900 });

    // 1. Login
    await page.goto(`${BASE_URL}/Conta/Login`, { waitUntil: 'networkidle0' });
    await page.type('#Email', email);
    await page.type('#Senha', senha);
    await Promise.all([page.waitForNavigation({ waitUntil: 'networkidle0' }), page.click('button[type=submit]')]);
    if (!page.url().includes('/Dashboard')) throw new Error(`Login falhou (${page.url()}).`);

    // 2. Relatório com a simulação da parcela de R$ 400,00
    await page.goto(`${BASE_URL}/Relatorio?parcela=400`, { waitUntil: 'networkidle0' });

    console.log('\nNa impressão (CSS @media print):');
    await page.emulateMediaType('print');
    const tela = await page.evaluate(() => {
      const oculto = (seletor) => {
        const els = [...document.querySelectorAll(seletor)];
        return els.length > 0 && els.every((el) => getComputedStyle(el).display === 'none');
      };
      return {
        menu: oculto('nav.navbar'),
        rodape: oculto('footer.footer'),
        botao: oculto('#botaoImprimir'),
        formulario: oculto('#indicadorParcela form'),
        linhas: document.querySelectorAll('#tabelaMeses tbody tr').length,
      };
    });
    confere('menu escondido', tela.menu);
    confere('rodapé escondido', tela.rodape);
    confere('botão "Imprimir" escondido', tela.botao);
    confere('formulário de simulação escondido', tela.formulario);
    confere('tabela mês a mês com 12 linhas', tela.linhas === 12, `${tela.linhas} linhas`);

    // 3. Gera o PDF como o Ctrl+P (A4, margens do @page do CSS)
    const pdf = await page.pdf({ path: SAIDA, preferCSSPageSize: true, printBackground: true });

    // Guarda a posição de cada trecho de texto (página + distância do topo),
    // para conferir que os blocos saem na ordem certa, sem um cobrir o outro.
    const itens = [];
    const { text, numpages } = await pdfParse(pdf, {
      pagerender: async (pagina) => {
        const conteudo = await pagina.getTextContent();
        const altura = pagina.view[3];
        for (const item of conteudo.items) {
          itens.push({ pagina: pagina.pageIndex, topo: altura - item.transform[5], texto: item.str });
        }
        return conteudo.items.map((i) => i.str).join(' ');
      },
    });
    const t = text.replace(/\s+/g, ' ');
    const posicao = (trecho) => itens.find((i) => i.texto.includes(trecho));
    const antes = (a, b) => a && b && (a.pagina < b.pagina || (a.pagina === b.pagina && a.topo < b.topo));

    console.log(`\nNo PDF gerado (${numpages} página(s), ${path.basename(SAIDA)}):`);
    const deve = [
      ['título', 'Relatório para o banco'],
      ['identificação: negócio', 'Negócio'],
      ['identificação: responsável', 'Responsável'],
      ['identificação: período', 'Período'],
      ['faturamento em 12 meses', 'R$ 4.050,00'],
      ['sobra média', 'R$ 1.515,00'],
      ['parcela confortável', 'R$ 454,50'],
      ['resultado da simulação', 'Cabe com folga'],
      ['percentual da simulação', '26,4%'],
      ['DRE resumida', 'Lucro líquido'],
      ['quadro dos bancos', 'DASN-SIMEI'],
    ];
    // O PDF quebra palavras em pedaços: compara sem espaços.
    const semEspaco = (x) => x.replace(/\s+/g, '');
    const tc = semEspaco(t);
    for (const [descricao, trecho] of deve) confere(`${descricao} ("${trecho}")`, tc.includes(semEspaco(trecho)));

    const naoDeve = [
      ['sem botão de imprimir', 'Imprimir / salvar em PDF'],
      ['sem botão da simulação', 'Ver se cabe'],
      ['sem menu', 'Ferramentas'],
    ];
    for (const [descricao, trecho] of naoDeve) confere(descricao, !tc.includes(semEspaco(trecho)));
    confere('no máximo 2 páginas A4', numpages <= 2, `${numpages} páginas`);

    // Um card cortado na paginação deixa o texto no arquivo, mas escondido
    // embaixo do bloco seguinte. Por isso confere a ordem real na página.
    const ordem = ['DRE r', 'Receita bruta', 'Lucro líquido', 'Mês a mês', 'costumam pedir'];
    for (let i = 0; i < ordem.length - 1; i++) {
      const a = posicao(ordem[i]);
      const b = posicao(ordem[i + 1]);
      confere(`"${ordem[i]}" aparece antes de "${ordem[i + 1]}" (nada sobreposto)`, antes(a, b),
        `${JSON.stringify(a)} x ${JSON.stringify(b)}`);
    }
  } finally {
    await browser.close();
  }

  const falhas = resultados.filter((r) => !r.ok).length;
  console.log(`\n${resultados.length - falhas}/${resultados.length} verificações passaram.`);
  process.exit(falhas ? 1 : 0);
})().catch((erro) => {
  console.error(`\nErro: ${erro.message}`);
  process.exit(1);
});

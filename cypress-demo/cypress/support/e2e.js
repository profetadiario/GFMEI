import './commands';

// Formato celular (scripts/gravar-celular.js passa --env janelaCelular=390x844).
// O viewport é aplicado aqui porque o bloco "e2e" do cypress.config.js
// prevalece sobre o viewport passado pela linha de comando.
beforeEach(() => {
  const janela = Cypress.env('janelaCelular');
  if (janela) {
    const [largura, altura] = String(janela).split('x').map(Number);
    cy.viewport(largura, altura);
  }
});

// Guarda onde o app aparece dentro da janela gravada (medido no fim de cada
// passo, com o layout do runner já estável), para scripts/gravar-celular.js
// recortar o vídeo exatamente nessa área.
afterEach(() => {
  if (!Cypress.env('janelaCelular')) return;
  const iframe = window.top.document.querySelector('iframe.aut-iframe');
  if (!iframe) return;
  const r = iframe.getBoundingClientRect();
  const dpr = window.top.devicePixelRatio || 1;
  cy.task('salvarAreaApp', {
    x: Math.round(r.left * dpr), y: Math.round(r.top * dpr),
    largura: Math.round(r.width * dpr), altura: Math.round(r.height * dpr),
  }, { log: false });
});

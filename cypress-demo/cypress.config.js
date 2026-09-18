const { defineConfig } = require('cypress');

module.exports = defineConfig({
  e2e: {
    // O sistema roda localmente via `dotnet run` (perfil "http" em
    // launchSettings.json) em http://localhost:5100 — precisa estar de pé
    // antes de rodar esta demo (não sobe o servidor automaticamente).
    baseUrl: 'http://localhost:5100',

    // Grava um vídeo por spec em cypress/videos/ ao rodar `cypress run`
    // (modo headless) — útil para anexar a demo gravada na apresentação do TCC.
    video: true,

    viewportWidth: 1280,
    viewportHeight: 800,
    defaultCommandTimeout: 8000,

    setupNodeEvents(on, config) {
      on('task', {
        // cy.log() só aparece no Command Log da UI do Cypress; em modo
        // headless (`cypress run` / `npm run demo:gravar`) ninguém vê o
        // terminal ali. Esta task imprime direto no terminal do Node (o
        // processo que roda o `cypress run`), então as credenciais da
        // conta criada aparecem de qualquer forma que a demo seja rodada.
        log(mensagem) {
          console.log(mensagem);
          return null;
        },
      });
      return config;
    },
  },
});

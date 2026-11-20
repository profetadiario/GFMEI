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
      // Gravação no formato celular (scripts/gravar-celular.js): abre a
      // janela do Electron do tamanho do aparelho + a barra do runner, para
      // o app aparecer em escala 1:1 e o vídeo poder ser recortado.
      on('before:browser:launch', (browser, launchOptions) => {
        const janela = config.env.janelaCelular;
        if (janela && browser.name === 'electron') {
          const [largura, altura] = janela.split('x').map(Number);
          launchOptions.preferences.width = largura + 80;
          launchOptions.preferences.height = altura + 200;
        }
        return launchOptions;
      });

      on('task', {
        // cy.log() só aparece no Command Log da UI do Cypress; em modo
        // headless (`cypress run` / `npm run demo:gravar`) ninguém vê o
        // terminal ali. Esta task imprime direto no terminal do Node (o
        // processo que roda o `cypress run`), então as credenciais da
        // conta criada aparecem de qualquer forma que a demo seja rodada.
        salvarAreaApp(area) {
          const fs = require('fs');
          const path = require('path');
          // Vai para a pasta de vídeos da gravação (celular ou apresentação).
          const destino = path.join(path.resolve(config.projectRoot, config.videosFolder), 'area-app.json');
          fs.mkdirSync(path.dirname(destino), { recursive: true });
          fs.writeFileSync(destino, JSON.stringify(area));
          return null;
        },
        log(mensagem) {
          console.log(mensagem);
          return null;
        },
      });
      return config;
    },
  },
});

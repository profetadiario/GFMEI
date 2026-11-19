// =======================================================================
// Grava a demo no formato de um smartphone moderno (390 x 844 CSS px,
// proporção 19,5:9 — iPhone 14/15), em ritmo "humano".
//
//   npm run demo:gravar:celular
//
// O vídeo bruto do Cypress grava a janela inteira do runner (barra de URL
// + painel de comandos), o que deixa o resultado "quadrado". Aqui:
//   1. o painel de comandos é escondido (CYPRESS_NO_COMMAND_LOG=1);
//   2. a janela do Electron é aberta do tamanho do aparelho + a barra do
//      runner, para o app aparecer em escala 1:1;
//   3. depois da gravação, o ffmpeg (o mesmo que vem com o Cypress) recorta
//      só a tela do app e amplia 2x (780 x 1688) para ficar nítido.
// Resultado: cypress/videos-celular/demo-celular.mp4
// =======================================================================
process.env.CYPRESS_NO_COMMAND_LOG = '1';

const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');
const cypress = require('cypress');

const LARGURA = 390;
const ALTURA = 844;
const PASTA_VIDEOS = path.join(__dirname, '..', 'cypress', 'videos-celular');

function acharFfmpeg() {
  const versao = require('cypress/package.json').version;
  const candidatos = [
    process.env.FFMPEG_PATH,
    path.join(process.env.LOCALAPPDATA || '', 'Cypress', 'Cache', versao, 'Cypress', 'resources', 'app', 'node_modules', '@ffmpeg-installer', 'win32-x64', 'ffmpeg.exe'),
    path.join(os.homedir(), '.cache', 'Cypress', versao, 'Cypress', 'resources', 'app', 'node_modules', '@ffmpeg-installer', 'linux-x64', 'ffmpeg'),
  ].filter(Boolean);
  return candidatos.find((c) => fs.existsSync(c)) || 'ffmpeg';
}

(async () => {
  fs.rmSync(path.join(PASTA_VIDEOS, 'area-app.json'), { force: true });
  const resultado = await cypress.run({
    spec: 'cypress/e2e/jornada-completa.cy.js',
    browser: 'electron',
    config: {
      viewportWidth: LARGURA,
      viewportHeight: ALTURA,
      videosFolder: 'cypress/videos-celular',
      screenshotsFolder: 'cypress/screenshots-celular',
      defaultCommandTimeout: 15000,
    },
    env: { ritmo: 'humano', janelaCelular: `${LARGURA}x${ALTURA}` },
  });

  if (resultado.status === 'failed' || resultado.totalFailed > 0) {
    console.error('A demo falhou; vídeo não recortado.');
    process.exit(1);
  }

  const bruto = path.join(PASTA_VIDEOS, 'jornada-completa.cy.js.mp4');
  const final = path.join(PASTA_VIDEOS, 'demo-celular.mp4');
  const ffmpeg = acharFfmpeg();

  // O tamanho real do vídeo é lido do próprio arquivo.
  let info = '';
  try {
    execFileSync(ffmpeg, ['-hide_banner', '-i', bruto], { stdio: 'pipe' });
  } catch (e) {
    info = String(e.stderr);
  }
  const m = info.match(/Video:.*?(\d{2,5})x(\d{2,5})/);
  if (!m) {
    console.error('Não consegui ler o tamanho do vídeo; ficou só o bruto em', bruto);
    process.exit(0);
  }
  const [lv, av] = [Number(m[1]), Number(m[2])];
  // Área do app medida pelo próprio runner (cypress/support/e2e.js);
  // se não existir, assume o app centralizado no rodapé da janela.
  const arquivoArea = path.join(PASTA_VIDEOS, 'area-app.json');
  const area = fs.existsSync(arquivoArea)
    ? JSON.parse(fs.readFileSync(arquivoArea, 'utf8'))
    : { x: Math.round((lv - LARGURA) / 2), y: av - ALTURA, largura: LARGURA, altura: ALTURA };
  const par = (n) => n - (n % 2);
  const w = par(Math.min(area.largura, lv - area.x));
  const h = par(Math.min(area.altura, av - area.y));
  const filtro = `crop=${w}:${h}:${area.x}:${area.y},scale=${LARGURA * 2}:${ALTURA * 2}:flags=lanczos`;
  console.log(`Vídeo bruto ${lv}x${av}; área do app ${w}x${h} em (${area.x},${area.y}).`);
  execFileSync(ffmpeg, ['-y', '-hide_banner', '-loglevel', 'error', '-i', bruto, '-vf', filtro, '-c:v', 'libx264', '-crf', '18', '-pix_fmt', 'yuv420p', final], { stdio: 'inherit' });
  console.log(`\nVídeo no formato celular (${LARGURA * 2}x${ALTURA * 2}): ${final}`);
})();

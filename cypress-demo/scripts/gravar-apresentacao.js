// =======================================================================
// Grava o vídeo da apresentação (desktop, 1920 x 1080) e as capturas.
//
//   npm run demo:apresentacao                 (contra http://localhost:5100)
//   BASE_URL=https://... npm run demo:apresentacao   (contra o site publicado)
//
// Gera em docs/demo/:
//   apresentacao_sistema_1080p.mp4        ritmo de apresentação (~2–3 min)
//   apresentacao_sistema_curta_1080p.mp4  pausas mínimas (reserva, até 90 s)
//   capturas/01_painel.png ... 07_relatorio_banco.png (1920 x 1080)
//
// Cada versão usa uma conta fictícia nova (cypress/apresentacao/1-preparo).
// O vídeo bruto do Cypress mostra a janela inteira do runner; aqui o
// painel de comandos fica escondido e o ffmpeg do próprio Cypress recorta
// só a tela do app, que é gravada em escala 1:1 (1920 x 1080).
// =======================================================================
process.env.CYPRESS_NO_COMMAND_LOG = '1';

const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');
const cypress = require('cypress');

const LARGURA = 1920;
const ALTURA = 1080;
const RAIZ = path.join(__dirname, '..');
const PASTA_VIDEOS = path.join(RAIZ, 'cypress', 'videos-apresentacao');
const PASTA_CAPTURAS = path.join(RAIZ, 'cypress', 'screenshots-apresentacao');
const DESTINO = path.join(RAIZ, '..', 'docs', 'demo');

function acharFfmpeg() {
  const versao = require('cypress/package.json').version;
  const candidatos = [
    process.env.FFMPEG_PATH,
    path.join(process.env.LOCALAPPDATA || '', 'Cypress', 'Cache', versao, 'Cypress', 'resources', 'app', 'node_modules', '@ffmpeg-installer', 'win32-x64', 'ffmpeg.exe'),
    path.join(os.homedir(), '.cache', 'Cypress', versao, 'Cypress', 'resources', 'app', 'node_modules', '@ffmpeg-installer', 'linux-x64', 'ffmpeg'),
  ].filter(Boolean);
  return candidatos.find((c) => fs.existsSync(c)) || 'ffmpeg';
}

function infoVideo(ffmpeg, arquivo) {
  let info = '';
  try {
    execFileSync(ffmpeg, ['-hide_banner', '-i', arquivo], { stdio: 'pipe' });
  } catch (e) {
    info = String(e.stderr);
  }
  const m = info.match(/Video:.*?(\d{2,5})x(\d{2,5})/);
  const d = info.match(/Duration: (\d+):(\d+):([\d.]+)/);
  return {
    largura: m && Number(m[1]),
    altura: m && Number(m[2]),
    segundos: d ? Number(d[1]) * 3600 + Number(d[2]) * 60 + Number(d[3]) : null,
  };
}

async function gravar({ pausas, capturas, nomeFinal }) {
  fs.rmSync(PASTA_VIDEOS, { recursive: true, force: true });
  const resultado = await cypress.run({
    browser: 'electron',
    spec: ['cypress/apresentacao/1-preparo.cy.js', 'cypress/apresentacao/2-apresentacao.cy.js'],
    config: {
      specPattern: 'cypress/apresentacao/*.cy.js',
      baseUrl: process.env.BASE_URL || 'http://localhost:5100',
      viewportWidth: LARGURA,
      viewportHeight: ALTURA,
      videosFolder: 'cypress/videos-apresentacao',
      screenshotsFolder: 'cypress/screenshots-apresentacao',
      screenshotOnRunFailure: false,
      redirectionLimit: 200, // o preparo salva ~25 lançamentos no mesmo teste
      trashAssetsBeforeRuns: false,
      defaultCommandTimeout: 15000,
    },
    env: { ritmo: 'humano', pausas, capturas, janelaCelular: `${LARGURA}x${ALTURA}` },
  });
  if (resultado.status === 'failed' || resultado.totalFailed > 0) {
    throw new Error(`A gravação "${nomeFinal}" falhou; nada foi copiado.`);
  }

  const ffmpeg = acharFfmpeg();
  const bruto = path.join(PASTA_VIDEOS, '2-apresentacao.cy.js.mp4');
  const { largura: lv, altura: av } = infoVideo(ffmpeg, bruto);
  const area = JSON.parse(fs.readFileSync(path.join(PASTA_VIDEOS, 'area-app.json'), 'utf8'));
  const par = (n) => n - (n % 2);
  const w = par(Math.min(area.largura, lv - area.x));
  const h = par(Math.min(area.altura, av - area.y));
  console.log(`Vídeo bruto ${lv}x${av}; área do app ${w}x${h} em (${area.x},${area.y}).`);
  if (w !== LARGURA || h !== ALTURA) {
    console.warn(`Aviso: o app não foi gravado em 1:1 (${w}x${h}); o vídeo será redimensionado.`);
  }

  fs.mkdirSync(DESTINO, { recursive: true });
  const final = path.join(DESTINO, nomeFinal);
  const filtro = `crop=${w}:${h}:${area.x}:${area.y},scale=${LARGURA}:${ALTURA}:flags=lanczos`;
  execFileSync(ffmpeg, ['-y', '-hide_banner', '-loglevel', 'error', '-i', bruto, '-vf', filtro,
    '-c:v', 'libx264', '-preset', 'slow', '-crf', '20', '-pix_fmt', 'yuv420p', '-movflags', '+faststart', final], { stdio: 'inherit' });
  const { segundos } = infoVideo(ffmpeg, final);
  console.log(`→ ${final} (${segundos ? segundos.toFixed(0) : '?'} s)`);
  return segundos;
}

(async () => {
  fs.rmSync(PASTA_CAPTURAS, { recursive: true, force: true });
  const longo = await gravar({ pausas: 2, capturas: true, nomeFinal: 'apresentacao_sistema_1080p.mp4' });

  const origem = path.join(PASTA_CAPTURAS, '2-apresentacao.cy.js');
  const destinoCapturas = path.join(DESTINO, 'capturas');
  fs.mkdirSync(destinoCapturas, { recursive: true });
  for (const arquivo of fs.readdirSync(origem).filter((a) => a.endsWith('.png'))) {
    fs.copyFileSync(path.join(origem, arquivo), path.join(destinoCapturas, arquivo));
  }

  const curto = await gravar({ pausas: 0.4, capturas: false, nomeFinal: 'apresentacao_sistema_curta_1080p.mp4' });
  console.log(`\nVersão completa: ${longo?.toFixed(0)} s · versão curta: ${curto?.toFixed(0)} s`);
})().catch((erro) => {
  console.error(erro.message);
  process.exit(1);
});

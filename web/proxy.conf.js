// Encaminha /auth e /tasks para a API (.NET) durante `ng serve`, exceto quando a
// requisição é uma navegação de página (Accept: text/html) — nesse caso o dev
// server do Angular deve servir a SPA, não a API. Ver design.md - Decisão 8:
// as rotas do Angular podem existir sob os mesmos paths que a API usa
// (ex.: /tasks), então a distinção não pode ser feita só pela lista de paths.
function bypassNavigationRequests(req) {
  const accept = req.headers.accept || '';
  if (req.method === 'GET' && accept.includes('text/html')) {
    return '/index.html';
  }
  return null;
}

module.exports = {
  '/auth': {
    target: 'http://localhost:5246',
    secure: false,
    bypass: bypassNavigationRequests,
  },
  '/tasks': {
    target: 'http://localhost:5246',
    secure: false,
    bypass: bypassNavigationRequests,
  },
};

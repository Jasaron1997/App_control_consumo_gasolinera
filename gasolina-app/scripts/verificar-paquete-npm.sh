#!/usr/bin/env bash
# Verifica un paquete de npm ANTES de instalarlo, contra ataques a la cadena
# de suministro: fecha de publicación de la versión (¿salió ayer?),
# mantenedores/repo declarados, volumen de descargas, y si tiene scripts de
# instalación que npm ejecutaría solo con `npm install` (sin --ignore-scripts).
#
# Uso:
#   ./scripts/verificar-paquete-npm.sh <paquete> [version]
#   ./scripts/verificar-paquete-npm.sh @tailwindcss/vite
#   ./scripts/verificar-paquete-npm.sh tailwindcss 4.3.3
#
# No instala nada — solo consulta el registro de npm (npm view) y la API
# pública de descargas. Revisar la salida a mano antes de instalar.
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "Uso: $0 <paquete> [version]" >&2
  exit 1
fi

PAQUETE="$1"
VERSION="${2:-}"
SPEC="$PAQUETE"
if [ -n "$VERSION" ]; then
  SPEC="$PAQUETE@$VERSION"
fi

echo "=== $SPEC ==="

INFO=$(npm view "$SPEC" --json)

VERSION_REAL=$(node -e "console.log(JSON.parse(process.argv[1]).version)" "$INFO")
echo "Versión resuelta: $VERSION_REAL"
node -e "
const p = JSON.parse(process.argv[1]);
console.log('Repositorio:', p.repository?.url ?? p.repository ?? '(sin repo declarado — sospechoso)');
console.log('Licencia:', p.license ?? '(sin license)');
console.log('Mantenedores:', JSON.stringify(p.maintainers ?? []));
" "$INFO"

echo
echo "--- Fecha de publicación de esta versión ---"
npm view "$PAQUETE" time --json | node -e "
const t = JSON.parse(require('fs').readFileSync(0, 'utf8'));
const v = process.argv[1];
const fecha = t[v];
if (!fecha) { console.log('No se encontró fecha de publicación para', v); process.exit(0); }
const dias = (Date.now() - new Date(fecha)) / 86400000;
console.log(v, '-> publicada:', fecha, \`(hace \${dias.toFixed(0)} días)\`);
if (dias < 14) {
  console.log('  ADVERTENCIA: publicada hace menos de 14 días. Esperar unos días o revisar el código fuente a mano antes de confiar en ella — una versión recién salida es el momento típico de un paquete comprometido (cuenta de mantenedor robada, dependencia inyectada).');
}
" "$VERSION_REAL"

echo
echo "--- Scripts que npm correría automáticamente con install/postinstall/preinstall/prepare ---"
node -e "
const p = JSON.parse(process.argv[1]);
const s = p.scripts ?? {};
const riesgosos = ['preinstall', 'install', 'postinstall', 'prepare'];
const encontrados = riesgosos.filter((k) => s[k]);
if (encontrados.length === 0) {
  console.log('Ninguno — instalar con --ignore-scripts no rompe nada de este paquete.');
} else {
  console.log('ATENCION: tiene scripts que npm ejecuta SOLO si no usás --ignore-scripts:');
  encontrados.forEach((k) => console.log(\`  \${k}: \${s[k]}\`));
  console.log('Revisar manualmente qué hace ese script antes de permitir que corra.');
}
" "$INFO"

echo
echo "--- Descargas última semana (señal de adopción/vigilancia comunitaria) ---"
curl -s "https://api.npmjs.org/downloads/point/last-week/$PAQUETE" | node -e "
const d = JSON.parse(require('fs').readFileSync(0, 'utf8'));
const n = d.downloads ?? 0;
console.log(n.toLocaleString('es-GT'), 'descargas la última semana');
if (n < 1000) {
  console.log('  ADVERTENCIA: muy pocas descargas — poco escrutinio de la comunidad, revisar con más cuidado.');
}
"

echo
echo "Recordatorio: instalar con --ignore-scripts salvo que este reporte diga lo contrario,"
echo "y fijar la versión exacta (sin ^ ni ~) si el paquete es nuevo/poco probado."

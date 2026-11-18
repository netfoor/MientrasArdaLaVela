#!/usr/bin/env bash
# Capturas del juego EN MARCHA. No confundir con `./tools/unity.sh capturar`, que renderiza
# las escenas desde el editor: esto fotografía la partida, con su HUD, su diálogo y su luz.
#
#   ./tools/capturas-juego.sh [carpeta_destino] [ancho] [alto]
#
# Cómo funciona: levanta una pantalla X virtual, arranca la compilación de Linux con
# -capturas (que recorre el menú, la ofrenda, los cuatro recuerdos y el epílogo) y dispara
# la foto cuando el juego avisa por el log de que la pantalla está lista. La foto la toma
# xwd y la convierte ffmpeg, porque el proyecto no trae el módulo ScreenCapture de Unity.
#
# Necesita el build al día:  ./tools/unity.sh build-linux
set -uo pipefail
PROYECTO="$(cd "$(dirname "$0")/.." && pwd)"
JUEGO="$PROYECTO/Builds/Linux/MientrasArdaLaVela.x86_64"
DESTINO="${1:-$PROYECTO/Logs/capturas-juego}"
ANCHO="${2:-1280}"; ALTO="${3:-720}"
LOG="$PROYECTO/Logs/cli/capturas-juego.log"
PANTALLA=":77"

[[ -x "$JUEGO" ]] || { echo "✗ falta el build: corre ./tools/unity.sh build-linux"; exit 1; }
for h in Xvfb xwd ffmpeg; do command -v $h >/dev/null || { echo "✗ falta $h"; exit 127; }; done

mkdir -p "$DESTINO" "$(dirname "$LOG")"
rm -f "$DESTINO"/*.png "$LOG"

limpiar() { kill "${JUEGO_PID:-}" "${XVFB_PID:-}" 2>/dev/null; wait 2>/dev/null; }
trap limpiar EXIT

Xvfb "$PANTALLA" -screen 0 "${ANCHO}x${ALTO}x24" >/dev/null 2>&1 &
XVFB_PID=$!
sleep 2

DISPLAY="$PANTALLA" "$JUEGO" -capturas -logFile "$LOG" \
  -screen-width "$ANCHO" -screen-height "$ALTO" -screen-fullscreen 0 >/dev/null 2>&1 &
JUEGO_PID=$!

echo "→ capturando a ${ANCHO}x${ALTO} en $DESTINO"
vistos=""
for _ in $(seq 1 600); do
  kill -0 "$JUEGO_PID" 2>/dev/null || break
  # El juego anuncia "### LISTO 01_menu ###" y se queda ahí unos segundos.
  marca=$(grep -oE '### LISTO [0-9]{2}_[a-z0-9_]+ ###' "$LOG" 2>/dev/null | tail -1 | awk '{print $3}')
  if [[ -n "$marca" && "$vistos" != *"|$marca|"* ]]; then
    vistos="$vistos|$marca|"
    if DISPLAY="$PANTALLA" xwd -root -silent > "$DESTINO/$marca.xwd" 2>/dev/null \
       && ffmpeg -y -loglevel error -i "$DESTINO/$marca.xwd" "$DESTINO/$marca.png" 2>/dev/null; then
      echo "  ✓ $marca.png"
    else
      echo "  ✗ $marca"
    fi
    rm -f "$DESTINO/$marca.xwd"
  fi
  sleep 0.5
done

n=$(ls -1 "$DESTINO"/*.png 2>/dev/null | wc -l)
echo "  $n capturas en $DESTINO"
[[ "$n" -gt 0 ]] || { echo "✗ ninguna capturada; revisa $LOG"; exit 1; }

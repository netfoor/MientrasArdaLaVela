#!/usr/bin/env bash
# Arma el paquete de entrega: los dos documentos, el código fuente y el ejecutable de
# Windows, comprimidos en un solo .zip dentro de entregables/.
#
#   ./tools/armar-entrega.sh
#
# El código fuente sale de `git archive HEAD`, así que entra exactamente lo versionado
# (sin Library/, Temp/ ni Builds/) y hay que tener los cambios ya commiteados.
# El ejecutable sale de Builds/Windows: compílalo antes con ./tools/unity.sh build-windows.
set -euo pipefail
PROYECTO="$(cd "$(dirname "$0")/.." && pwd)"
cd "$PROYECTO"
SALIDA="entregables"
NOMBRE="RomeroMantillaFortino_MientrasArdaLaVela"
ARMADO="$SALIDA/.armado/MientrasArdaLaVela"

[[ -f Builds/Windows/MientrasArdaLaVela.exe ]] || {
  echo "✗ falta el ejecutable: corre ./tools/unity.sh build-windows"; exit 1; }
[[ -z "$(git status --porcelain --untracked-files=no)" ]] || {
  echo "✗ hay cambios sin commitear: el código fuente sale de git archive HEAD"; exit 1; }

rm -rf "$SALIDA/.armado" "$SALIDA/$NOMBRE.zip"
mkdir -p "$ARMADO/codigo-fuente" "$ARMADO/ejecutable-windows"

cp RomeroMantillaFortino_diseño.docx RomeroMantillaFortino_producción.xlsx "$ARMADO/"
[[ -f "$SALIDA/LEEME.txt" ]] && cp "$SALIDA/LEEME.txt" "$ARMADO/LEEME.txt"
git archive HEAD | tar -x -C "$ARMADO/codigo-fuente"
cp -r Builds/Windows/. "$ARMADO/ejecutable-windows/"
# Símbolos de depuración: Unity misma avisa de que no se entregan.
rm -rf "$ARMADO/ejecutable-windows/"*BackUpThisFolder*

( cd "$SALIDA/.armado" && zip -q -r -9 "../$NOMBRE.zip" MientrasArdaLaVela )
rm -rf "$SALIDA/.armado"

unzip -tq "$SALIDA/$NOMBRE.zip" >/dev/null && echo "✓ $SALIDA/$NOMBRE.zip ($(du -h "$SALIDA/$NOMBRE.zip" | cut -f1))"

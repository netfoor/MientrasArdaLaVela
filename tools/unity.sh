#!/usr/bin/env bash
# Envoltorio del editor de Unity en línea de comandos para este proyecto.
#
#   tools/unity.sh <orden> [...]
#
# Órdenes:
#   verificar          los tres verificadores (Mapa 0, MainMenu, Animator)
#   pruebas            las pruebas EditMode
#   construir-mapa0    reconstruye y guarda Mapa0_Ofrenda
#   construir-menu     reconstruye y guarda MainMenu (hazlo cada vez que cambie CREDITOS.txt)
#   construir-recuerdo1  construye y guarda Recuerdo1_Panaderia y la añade a la lista de escenas
#   construir-recuerdo2  construye y guarda Recuerdo2_Boda
#   construir-recuerdo3  construye y guarda Recuerdo3_Horno
#   construir-recuerdo4  construye y guarda Recuerdo4_Campo
#   construir-epilogo  construye y guarda Epilogo_DosVelas
#   construir-todo     Mapa 0, MainMenu, los recuerdos y el epílogo
#   build-windows      compila el juego para Windows 10/11 en Builds/Windows (F26)
#   build-linux        compila para Linux, para probarlo en esta máquina
#   capturar           renderiza ambas escenas a PNG (usa un display virtual)
#   animator           regenera clips y controller de Ámbar
#   cortar-hojas       corta las 13 hojas de personaje en sprites
#   metodo <Clase.Metodo>   ejecuta cualquier -executeMethod
#   todo               verificar + pruebas
set -uo pipefail

PROYECTO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY="$HOME/Unity/Hub/Editor/6000.6.0f1/Editor/Unity"
NS="MientrasArdaLaVela.EditorTools"
LOGS="$PROYECTO/Logs/cli"; mkdir -p "$LOGS"   # Temp/ no sirve: Unity lo vacía al arrancar

[[ -x "$UNITY" ]] || { echo "No encuentro el editor en $UNITY"; exit 127; }

# Unity no admite dos instancias sobre el mismo proyecto: si hay una abierta,
# lanzar batchmode falla a medias y puede dejar la Library inconsistente.
comprobar_libre() {
  local lock="$PROYECTO/Temp/UnityLockfile"
  if [[ -f "$lock" ]] && fuser "$lock" &>/dev/null; then
    echo "✗ El editor de Unity está abierto con este proyecto."
    echo "  Ciérralo antes de usar la línea de comandos (o usa el menú del editor)."
    exit 2
  fi
}

correr() {                       # correr <nombre> <método> [extra...]
  local nombre="$1" metodo="$2"; shift 2
  local log="$LOGS/$nombre.log"
  echo "→ $nombre"
  "$UNITY" -batchmode -nographics -quit -projectPath "$PROYECTO" -logFile "$log" \
           -executeMethod "$NS.$metodo" "$@" >/dev/null 2>&1
  local code=$?
  if [[ ! -s "$log" ]]; then
    echo "  ✗ Unity no generó log (salida=$code). ¿Arrancó siquiera?"
    return 1
  fi
  local fallos
  fallos=$(grep -cE "^FALLA|error CS" "$log" 2>/dev/null); fallos=${fallos:-0}
  if (( code != 0 )) || (( fallos > 0 )); then
    echo "  ✗ salida=$code, $fallos problemas. Detalle:"
    grep -E "error CS|^FALLA|Exception" "$log" | sort -u | head -12 | sed 's/^/     /'
    return 1
  fi
  grep -E "^### |^\[" "$log" | sort -u | head -6 | sed 's/^/     /'
  echo "  ✓ ok   (log: ${log#$PROYECTO/})"
}

orden="${1:-todo}"; shift || true
comprobar_libre
case "$orden" in
  verificar)
    r=0
    correr verif-mapa0   Mapa0Verificador.Verificar          || r=1
    correr verif-menu    MainMenuVerificador.Verificar       || r=1
    correr verif-animator AmbarAnimatorVerificador.Verificar || r=1
    correr verif-recuerdo1 Recuerdo1Verificador.Verificar    || r=1
    correr verif-recuerdo2 Recuerdo2Verificador.Verificar    || r=1
    correr verif-recuerdo3 Recuerdo3Verificador.Verificar    || r=1
    correr verif-recuerdo4 Recuerdo4Verificador.Verificar    || r=1
    correr verif-epilogo   EpilogoVerificador.Verificar      || r=1
    exit $r ;;
  pruebas)
    log="$LOGS/pruebas.log"; res="$LOGS/resultados.xml"
    echo "→ pruebas EditMode"
    "$UNITY" -batchmode -nographics -projectPath "$PROYECTO" -logFile "$log" \
             -runTests -testPlatform EditMode -testResults "$res" >/dev/null 2>&1
    python3 - "$res" <<'PY'
import sys, xml.etree.ElementTree as ET, os
p=sys.argv[1]
if not os.path.exists(p): print("  ✗ no se generaron resultados"); sys.exit(1)
r=ET.parse(p).getroot()
print(f"  {'✓' if r.get('result')=='Passed' else '✗'} {r.get('passed')}/{r.get('total')} pruebas")
for tc in r.iter('test-case'):
    if tc.get('result')!='Passed': print("     FALLA", tc.get('name'))
sys.exit(0 if r.get('result')=='Passed' else 1)
PY
    exit $? ;;
  construir-mapa0) correr construir-mapa0 Mapa0Builder.CrearEscenaYConstruir ;;
  construir-menu)  correr construir-menu  MainMenuBuilder.CrearEscenaYConstruir ;;
  construir-recuerdo1) correr construir-recuerdo1 Recuerdo1Builder.CrearEscenaYConstruir ;;
  construir-recuerdo2) correr construir-recuerdo2 Recuerdo2Builder.CrearEscenaYConstruir ;;
  construir-recuerdo3) correr construir-recuerdo3 Recuerdo3Builder.CrearEscenaYConstruir ;;
  construir-recuerdo4) correr construir-recuerdo4 Recuerdo4Builder.CrearEscenaYConstruir ;;
  construir-epilogo)   correr construir-epilogo   EpilogoBuilder.CrearEscenaYConstruir ;;
  construir-todo)
    r=0
    correr construir-mapa0     Mapa0Builder.CrearEscenaYConstruir     || r=1
    correr construir-menu      MainMenuBuilder.CrearEscenaYConstruir  || r=1
    correr construir-recuerdo1 Recuerdo1Builder.CrearEscenaYConstruir || r=1
    correr construir-recuerdo2 Recuerdo2Builder.CrearEscenaYConstruir || r=1
    correr construir-recuerdo3 Recuerdo3Builder.CrearEscenaYConstruir || r=1
    correr construir-recuerdo4 Recuerdo4Builder.CrearEscenaYConstruir || r=1
    correr construir-epilogo   EpilogoBuilder.CrearEscenaYConstruir   || r=1
    exit $r ;;
  build-windows)   correr build-windows   Build.Windows ;;
  build-linux)     correr build-linux     Build.Linux ;;
  animator)        correr animator        AmbarAnimator.Generar ;;
  cortar-hojas)    correr cortar-hojas    HojaLPC.CortarTodas ;;
  capturar)
    command -v xvfb-run >/dev/null || { echo "✗ falta xvfb-run"; exit 127; }
    dir="${CAPTURA_DIR:-$PROYECTO/Logs/capturas}"; mkdir -p "$dir"
    for par in "cap-mapa0:Mapa0Captura.Capturar" "cap-menu:MainMenuCaptura.Capturar" "cap-recuerdo1:Mapa0Captura.CapturarRecuerdo1" "cap-recuerdo2:Mapa0Captura.CapturarRecuerdo2" "cap-recuerdo3:Mapa0Captura.CapturarRecuerdo3" "cap-recuerdo4:Mapa0Captura.CapturarRecuerdo4"; do
      n="${par%%:*}"; m="${par#*:}"; echo "→ $n"
      CAPTURA_DIR="$dir" xvfb-run -a --server-args="-screen 0 1920x1080x24" \
        "$UNITY" -batchmode -quit -projectPath "$PROYECTO" -logFile "$LOGS/$n.log" \
        -executeMethod "$NS.$m" >/dev/null 2>&1 && echo "  ✓ ok" || echo "  ✗ ver $LOGS/$n.log"
    done
    echo "PNG en $dir"; ls -1 "$dir"/*.png 2>/dev/null | sed 's/^/   /' ;;
  metodo) [[ $# -ge 1 ]] || { echo "uso: metodo <Clase.Metodo>"; exit 1; }
          correr "metodo" "$1" ;;
  todo)
    r=0
    correr verif-mapa0    Mapa0Verificador.Verificar          || r=1
    correr verif-menu     MainMenuVerificador.Verificar       || r=1
    correr verif-animator AmbarAnimatorVerificador.Verificar  || r=1
    correr verif-recuerdo1 Recuerdo1Verificador.Verificar     || r=1
    correr verif-recuerdo2 Recuerdo2Verificador.Verificar     || r=1
    correr verif-recuerdo3 Recuerdo3Verificador.Verificar     || r=1
    correr verif-recuerdo4 Recuerdo4Verificador.Verificar     || r=1
    correr verif-epilogo   EpilogoVerificador.Verificar       || r=1
    "$0" pruebas || r=1
    exit $r ;;
  *) sed -n '2,20p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'; exit 1 ;;
esac

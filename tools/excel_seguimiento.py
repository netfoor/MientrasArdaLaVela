# /// script
# requires-python = ">=3.11"
# ///
"""
Llena el seguimiento del plan de producción (RomeroMantillaFortino_producción.xlsx) con lo que
de verdad pasó: estados del Feature Log, del Sprint Plan y de los tres Sprint Backlogs, el
esfuerzo restante por día de los burn-down y la Buglist completa.

Escribe el XML dentro del .xlsx a mano, celda por celda. Las librerías de Excel para Python
pierden las gráficas al guardar, y los tres burn-down charts son parte de la entrega.

Las fechas no se inventan: salen de los commits del repositorio, de ESTADO.md y de los
Docs/BUGLIST_SPRINT*.md. Donde una tarea se cerró antes de que su sprint empezara, así queda
registrado; es lo que ocurrió.

Uso: uv run --script tools/excel_seguimiento.py RomeroMantillaFortino_producción.xlsx
"""
import datetime, json, re, shutil, sys, zipfile
import xml.etree.ElementTree as ET

NS = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
NSR = 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'
N, R = '{%s}' % NS, '{%s}' % NSR
ET.register_namespace('', NS)
ET.register_namespace('r', NSR)

EPOCA = datetime.date(1899, 12, 30)


def serial(iso):
    """Fecha ISO -> número de serie de Excel."""
    return (datetime.date.fromisoformat(iso) - EPOCA).days


def col(n):
    """0 -> A, 25 -> Z, 26 -> AA."""
    s = ''
    while True:
        n, resto = divmod(n, 26)
        s = chr(65 + resto) + s
        if n == 0:
            return s
        n -= 1


def indice(ref):
    """'AC12' -> (12, 28)."""
    letras, numeros = re.match(r'([A-Z]+)(\d+)', ref).groups()
    n = 0
    for c in letras:
        n = n * 26 + (ord(c) - 64)
    return int(numeros), n - 1


# ---------------------------------------------------------------- escritura de celdas
class Hoja:
    def __init__(self, xml):
        self.raiz = ET.fromstring(xml)
        self.datos = self.raiz.find(N + 'sheetData')

    def fila(self, n):
        for f in self.datos.findall(N + 'row'):
            if int(f.get('r')) == n:
                return f
        nueva = ET.Element(N + 'row', {'r': str(n)})
        hermanas = self.datos.findall(N + 'row')
        pos = sum(1 for f in hermanas if int(f.get('r')) < n)
        self.datos.insert(pos, nueva)
        return nueva

    def celda(self, ref, crear=True):
        nf, nc = indice(ref)
        f = self.fila(nf)
        for c in f.findall(N + 'c'):
            if c.get('r') == ref:
                return c
        if not crear:
            return None
        nueva = ET.Element(N + 'c', {'r': ref})
        pos = sum(1 for c in f.findall(N + 'c') if indice(c.get('r'))[1] < nc)
        f.insert(pos, nueva)
        return nueva

    def estilo_de(self, ref):
        c = self.celda(ref, crear=False)
        return c.get('s') if c is not None else None

    def poner(self, ref, valor, estilo=None):
        """valor: str -> texto en línea; int/float -> número. None borra el contenido."""
        c = self.celda(ref)
        for hijo in list(c):
            c.remove(hijo)
        c.attrib.pop('t', None)
        if estilo:
            c.set('s', estilo)
        if valor is None:
            return
        if isinstance(valor, str):
            c.set('t', 'inlineStr')
            is_ = ET.SubElement(c, N + 'is')
            t = ET.SubElement(is_, N + 't')
            t.text = valor
            t.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')
        else:
            v = ET.SubElement(c, N + 'v')
            v.text = repr(valor) if isinstance(valor, float) else str(valor)

    def cachear(self, ref, valor):
        """
        Cambia el valor GUARDADO de una celda de fórmula, dejando la fórmula intacta.

        Hace falta porque un .xlsx guarda, junto a cada fórmula, el último resultado conocido.
        Excel lo recalcula al abrir, pero una vista previa (Drive, Classroom, GitHub) enseña el
        valor guardado: sin esto, el resumen seguiría diciendo "26 Planeado / 0 Terminado".
        """
        c = self.celda(ref, crear=False)
        if c is None or c.find(N + 'f') is None:
            return False
        for v in c.findall(N + 'v'):
            c.remove(v)
        c.attrib.pop('t', None)
        v = ET.SubElement(c, N + 'v')
        v.text = repr(round(valor, 10)) if isinstance(valor, float) else str(valor)
        return True

    def xml(self):
        return ET.tostring(self.raiz, encoding='UTF-8', xml_declaration=True)


def limpiar(texto):
    """Quita el marcado de Markdown de los textos que vienen de los .md."""
    texto = re.sub(r'\*\*(.+?)\*\*', r'\1', texto)
    texto = texto.replace('`', '').replace('«', '"').replace('»', '"')
    return re.sub(r'\s+', ' ', texto).strip()


# ---------------------------------------------------------------- los datos reales
# Cuándo se cerró cada cosa. Evidencia: commits del repositorio y fechas de ESTADO.md.
#   2026-09-21  arranque: proyecto, arte LPC, Mapa 0, MainMenu, guardado
#   2026-09-22  créditos con scroll, automatización del Dialogue System, Buglist del Sprint 1
#   2026-09-23  B12;  2026-09-24  B13 y los bugs abiertos del Sprint 1
#   2026-09-27  Recuerdos 1 y 2 jugados completos; B05, B07, B10, B14, B15, B16
#   2026-09-29  Recuerdos 3 y 4 y epílogo jugados completos
#   2026-10-06  partida entera: 20/20 fragmentos, 25/25 pétalos, 8/9 logros
#   2026-10-22  F22 música y efectos, F26 build de Windows
#   2026-10-23  F25 pulido; segunda partida completa
SPRINTS = {1: '2026-09-19', 2: '2026-10-10', 3: '2026-10-31'}

# Feature Log: fila -> estado. F01 está en la fila 4 y F26 en la 29.
FEATURES = {fila: 'Terminado' for fila in range(4, 30)}

# Sprint Plan. Los tres quedan Terminado: las 48 tareas de los backlogs están cerradas y el
# juego se presenta terminado, así que el estado refleja el trabajo y no el día del calendario.
PLAN = {'E4': 'Terminado', 'E5': 'Terminado', 'E6': 'Terminado'}

# Para cada backlog: fila de la tarea -> día del sprint en que quedó en cero.
# Un día negativo significa que la tarea ya estaba terminada antes de que el sprint empezara.
CIERRES = {
    'Sprint 1 Backlog': {
        # 21/09 = día 2: todo el núcleo jugable en la primera jornada de trabajo
        **{f: 2 for f in (13, 14, 15, 16, 17, 18, 19, 21, 22, 23, 24, 25, 26, 28)},
        20: 3,   # Yarn Spinner: el Dialogue System se automatizó el 22/09
        27: 3,   # menú principal: los créditos con scroll son del 22/09
        29: 5,   # pruebas Alfa: cierran al corregir B12 y B13, el 24/09
    },
    # Sprint 2 (10/10 - 30/10): F11 a F18 estaban jugadas el 27/09, trece días antes de empezar.
    'Sprint 2 Backlog': {f: -1 for f in range(13, 29)},
    'Sprint 3 Backlog': {
        **{f: -1 for f in range(13, 27)},   # de los Recuerdos 3 y 4 al build: todo antes del 31/10
        27: 4,   # pruebas Alfa: el juego se jugó entero el 23/10, pero la Buglist se llenó hoy
    },
}

# Nota que se escribe en A11 de cada backlog (fuera de A13:A42, que es donde se cuentan tareas).
NOTAS = {
    'Sprint 1 Backlog': 'Esfuerzo restante tomado de los commits del repositorio. El grueso del '
                        'sprint se trabajó el 21 y 22 de septiembre; las pruebas Alfa cerraron el 24.',
    'Sprint 2 Backlog': 'Las dieciséis tareas estaban terminadas el 27 de septiembre, antes de que '
                        'empezara este sprint el 10 de octubre: el burn-down cae a cero el primer día.',
    'Sprint 3 Backlog': 'Todo el contenido del sprint se adelantó: los Recuerdos 3 y 4 y el epílogo '
                        'quedaron el 29 de septiembre, los logros y pétalos el 6 de octubre, la '
                        'música y el build el 22 y el pulido el 23, antes de que el sprint empezara '
                        'el 31 de octubre. Sólo las pruebas Alfa se cerraron dentro, el 4 de noviembre.',
}

# Buglist: lo que no sale de los .md.
FECHA_BUG = {**{f'B{n:02d}': '2026-09-22' for n in range(1, 12)},
             'B12': '2026-09-23', 'B13': '2026-09-24',
             'B14': '2026-09-27', 'B15': '2026-09-27', 'B16': '2026-09-27',
             'B17': '2026-10-22', 'B18': '2026-10-23', 'B19': '2026-11-04'}
# ---------------------------------------------------------------- llenado de cada hoja
def llenar_backlog(hoja, nombre, cierres, nota):
    """Estado de cada tarea y esfuerzo restante por día (columnas I a AC = días 1 a 21)."""
    estilo_dia = hoja.estilo_de('H13')
    estilo_estado = hoja.estilo_de('F24') or hoja.estilo_de('F13')
    hoja.poner('A11', nota)
    esfuerzo = [0.0] * 22          # horas que quedaban cada día
    tareas = [0] * 22              # tareas sin terminar cada día
    for fila, dia_cierre in cierres.items():
        horas = hoja.celda(f'G{fila}', crear=False)
        if horas is None or horas.find(N + 'v') is None:
            raise SystemExit(f'{nombre}: la fila {fila} no tiene esfuerzo estimado en G')
        estimado = float(horas.find(N + 'v').text)
        hoja.poner(f'F{fila}', 'Terminado', estilo_estado)
        esfuerzo[0] += estimado                       # el día 0 es la estimación
        tareas[0] += 1
        for dia in range(1, 22):
            restante = 0.0 if dia >= dia_cierre else estimado
            hoja.poner(f'{col(7 + dia)}{fila}', restante, estilo_dia)
            esfuerzo[dia] += restante
            tareas[dia] += 1 if restante > 0 else 0

    # Valores guardados de las filas que resumen el burn-down (y de sus copias ocultas, que
    # alimentan las gráficas). Sin esto las gráficas saldrían con los números de antes.
    total = esfuerzo[0]
    pendiente = (total - esfuerzo[21]) / 21 if total else 0.0    # tendencia en horas por día
    hoja.cachear('H3', pendiente)
    for dia in range(22):
        c = f'{col(7 + dia)}'
        hoja.cachear(f'{c}5', esfuerzo[dia])
        hoja.cachear(f'{c}45', esfuerzo[dia])
        hoja.cachear(f'{c}6', max(0.0, total - pendiente * dia))
        hoja.cachear(f'{c}46', max(0.0, total - pendiente * dia))
        hoja.cachear(f'{c}8', tareas[dia])
        hoja.cachear(f'{c}48', tareas[dia])
    return esfuerzo, tareas


def llenar_buglist(hoja, bugs):
    """Una fila por bug, desde la 5 (la 4 es el ejemplo B000 que trae la plantilla)."""
    estilo_fecha = hoja.estilo_de('I5') or hoja.estilo_de('I4')
    orden = sorted(bugs, key=lambda b: int(b[1:]))
    for i, bid in enumerate(orden):
        b, fila = bugs[bid], 5 + i
        hoja.poner(f'A{fila}', bid)
        hoja.poner(f'B{fila}', limpiar(b['Descripción']))
        hoja.poner(f'C{fila}', limpiar(b['Descripción técnica (archivo/línea)']))
        hoja.poner(f'D{fila}', 'Fortino Romero Mantilla')
        hoja.poner(f'E{fila}', 'Cerrado')             # ESTADO.md: no queda ninguno abierto
        hoja.poner(f'F{fila}', limpiar(b['FeatureID']))
        hoja.poner(f'G{fila}', b['Sprint'])
        hoja.poner(f'H{fila}', limpiar(b['Severidad']))
        hoja.poner(f'I{fila}', serial(b['Fecha']), estilo_fecha)
    return len(orden)


def main(ruta, docs):
    bugs = json.loads(_extraer_bugs(docs))
    for bid, fecha in FECHA_BUG.items():
        if bid in bugs:
            bugs[bid]['Fecha'] = fecha
    faltan = [b for b in bugs if 'Fecha' not in bugs[b]]
    if faltan:
        raise SystemExit(f'sin fecha documentada: {faltan}')

    z = zipfile.ZipFile(ruta)
    contenido = {n: z.read(n) for n in z.namelist()}
    z.close()

    # Qué XML corresponde a cada hoja.
    rels = {r.get('Id'): r.get('Target') for r in ET.fromstring(contenido['xl/_rels/workbook.xml.rels'])}
    wb = ET.fromstring(contenido['xl/workbook.xml'])
    destino = {s.get('name'): 'xl/' + rels[s.get(R + 'id')].lstrip('/').replace('xl/', '')
               for s in wb.iter(N + 'sheet')}

    resumen = []
    h = Hoja(contenido[destino['Feature Log']])
    estilo = h.estilo_de('D28')
    for fila, estado in FEATURES.items():
        h.poner(f'D{fila}', estado, estilo)
    contenido[destino['Feature Log']] = h.xml()
    resumen.append(f'Feature Log: {len(FEATURES)} features marcadas')

    h = Hoja(contenido[destino['Sprint Plan']])
    for ref, estado in PLAN.items():
        h.poner(ref, estado, h.estilo_de('D4'))
    contenido[destino['Sprint Plan']] = h.xml()
    resumen.append('Sprint Plan: 3 sprints marcados')

    for nombre, cierres in CIERRES.items():
        h = Hoja(contenido[destino[nombre]])
        llenar_backlog(h, nombre, cierres, NOTAS[nombre])
        contenido[destino[nombre]] = h.xml()
        resumen.append(f'{nombre}: {len(cierres)} tareas x 21 días')

    # Hoja de resumen: cuenta los estados que se acaban de escribir. Se deja calculada porque
    # es la primera que ve quien abre el libro, y antes decía "26 Planeado / 0 Terminado".
    h = Hoja(contenido[destino['Detalles del Proyecto']])
    sprints = list(PLAN.values())
    por_sprint = {'Planeado': 'B15', 'En desarrollo': 'B16', 'Sin planear': 'B17', 'Terminado': 'B18'}
    for estado, ref in por_sprint.items():
        h.cachear(ref, sprints.count(estado) / len(sprints))
    h.cachear('B19', 1)
    features = list(FEATURES.values())
    por_feature = {'Planeado': 'B23', 'Sin planear': 'B24', 'En desarrollo': 'B25',
                   'Terminado': 'B26', 'Feature Creep': 'B27', 'Eliminado': 'B28'}
    for estado, ref in por_feature.items():
        h.cachear(ref, features.count(estado))
    h.cachear('B29', len(features))
    contenido[destino['Detalles del Proyecto']] = h.xml()
    resumen.append(f'Detalles del Proyecto: {features.count("Terminado")}/{len(features)} features terminadas')

    h = Hoja(contenido[destino['Buglist']])
    n = llenar_buglist(h, bugs)
    contenido[destino['Buglist']] = h.xml()
    resumen.append(f'Buglist: {n} bugs')

    # Que Excel recalcule todo al abrir, y fuera la cadena de cálculo, que ya no cuadra.
    texto = contenido['xl/workbook.xml'].decode()
    if '<calcPr' in texto:
        texto = re.sub(r'<calcPr[^/>]*/?>', '<calcPr calcId="191029" fullCalcOnLoad="1"/>', texto, count=1)
    else:
        texto = texto.replace('</workbook>', '<calcPr calcId="191029" fullCalcOnLoad="1"/></workbook>')
    contenido['xl/workbook.xml'] = texto.encode()
    contenido.pop('xl/calcChain.xml', None)

    shutil.copy2(ruta, str(ruta) + '.bak')
    with zipfile.ZipFile(ruta, 'w', zipfile.ZIP_DEFLATED) as salida:
        for nombre, datos in contenido.items():
            salida.writestr(nombre, datos)
    print('\n'.join(resumen))
    print(f'Respaldo del original en {ruta}.bak')


def _extraer_bugs(docs):
    """Lee la tabla de los tres Docs/BUGLIST_SPRINT*.md."""
    import pathlib
    bugs = {}
    for n in (1, 2, 3):
        cab = None
        for linea in (pathlib.Path(docs) / f'BUGLIST_SPRINT{n}.md').read_text(encoding='utf-8').splitlines():
            if not linea.strip().startswith('|'):
                continue
            cols = [c.strip() for c in linea.strip().strip('|').split('|')]
            if cols and cols[0] == 'BugID':
                cab = cols
            elif cab and re.fullmatch(r'B\d{2}', cols[0]):
                bugs[cols[0]] = {**dict(zip(cab, cols)), 'Sprint': n}
    return json.dumps(bugs, ensure_ascii=False)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else 'Docs')

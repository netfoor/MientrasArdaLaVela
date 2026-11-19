# /// script
# requires-python = ">=3.11"
# dependencies = ["pillow"]
# ///
"""
Añade al Documento de Diseño un anexo con capturas del juego terminado.

Las capturas salen de tools/capturas-juego.sh, que fotografía la compilación en marcha. Se
insertan al final, después de Referencias, numeradas a partir de la Figura 11 (la 10 es la
última del cuerpo) y con el mismo formato de figura que usa el documento.

Escribe en otro archivo; no toca el original.
Uso: uv run --script anexo.py entrada.docx salida.docx carpeta_de_capturas
"""
import pathlib, re, sys, zipfile
from PIL import Image

ANCHO_EMU = 5486400          # 6 pulgadas: el ancho de texto de la página
ANCHO_PX = 960               # se reescala: 1280 px es más de lo que la página necesita

FIGURAS = [
    ("01_menu.png", "Menú principal",
     "La pantalla desde la que se inicia la partida, se continúa la guardada, se abren las "
     "opciones y se consultan los créditos. Aquí se escriben también los códigos secretos."),
    ("02_ofrenda_prologo.png", "Prólogo en la ofrenda, con la caja de diálogo y el HUD",
     "Arriba a la izquierda, el nombre del mapa, los cuatro espacios de fragmentos y el "
     "contador de pétalos; a la derecha, la vela que marca el progreso. El diálogo avanza con "
     "E, Espacio o clic."),
    ("03_ofrenda.png", "Mapa 0, La ofrenda (2025)",
     "El camino de pétalos de cempasúchil entra por el patio y termina en la cruz, al pie del "
     "altar. Sirve de prólogo, de tutorial y de epílogo."),
    ("04_recuerdo1_panaderia.png", "Recuerdo 1, La panadería cerrada (2015)",
     "El letrero de La Espiga y el cartel de CERRADO cuentan, sin diálogo, el punto de partida "
     "del recuerdo. La silueta gris de la puerta es un Eco."),
    ("05_recuerdo2_boda.png", "Recuerdo 2, La silla vacía (1988)",
     "La boda de Marisol, con papel picado y luces cálidas. La mecánica del Olvido oculta "
     "aquí la parte del pasado que don Aurelio no quiere mirar."),
    ("06_recuerdo3_horno.png", "Recuerdo 3, La noche del horno (1966)",
     "El obrador y el cuarto de los hermanos. En este recuerdo la niebla del Olvido sólo se "
     "abre acercando la Luz de vela."),
    ("07_recuerdo4_campo.png", "Recuerdo 4, El campo de cempasúchil (1955)",
     "El último recuerdo, al atardecer. No tiene Ecos: el Olvido se retira solo, franja por "
     "franja, conforme don Aurelio acepta la verdad."),
    ("10_puzle_mosaico.png", "Puzle de mosaico",
     "Al reunir los cuatro fragmentos de un recuerdo se abre un mosaico de 16 piezas que se "
     "giran hasta formar la imagen. Resolverlo lleva al recuerdo anterior. Tizne da una pista "
     "con H, y usarla cuesta el logro «Memoria propia»."),
    ("08_epilogo.png", "Epílogo, Dos velas",
     "El cierre de la historia: la segunda vela encendida en la misma ofrenda."),
]


def esc(t):
    return t.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')


def parrafo(runs, espacio_despues=120, alineacion='both'):
    return (f'<w:p><w:pPr><w:spacing w:after="{espacio_despues}" w:before="0" w:lineRule="auto"/>'
            f'<w:jc w:val="{alineacion}"/></w:pPr>{runs}</w:p>')


def run(texto, negrita=False, cursiva=False):
    pr = '<w:rPr>'
    if negrita: pr += '<w:b w:val="1"/>'
    if cursiva: pr += '<w:i w:val="1"/>'
    pr += '<w:rtl w:val="0"/></w:rPr>'
    return f'<w:r>{pr}<w:t xml:space="preserve">{esc(texto)}</w:t></w:r>'


def imagen(rid, idx, nombre, alto_emu):
    return ('<w:p><w:pPr><w:keepNext w:val="1"/><w:spacing w:after="120" w:before="0" '
            'w:lineRule="auto"/><w:jc w:val="center"/></w:pPr><w:r><w:rPr/><w:drawing>'
            f'<wp:inline distB="0" distT="0" distL="0" distR="0">'
            f'<wp:extent cx="{ANCHO_EMU}" cy="{alto_emu}"/>'
            '<wp:effectExtent b="0" l="0" r="0" t="0"/>'
            f'<wp:docPr id="{idx}" name="{nombre}"/>'
            '<a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">'
            f'<pic:pic><pic:nvPicPr><pic:cNvPr id="0" name="{nombre}"/>'
            '<pic:cNvPicPr preferRelativeResize="0"/></pic:nvPicPr><pic:blipFill>'
            f'<a:blip r:embed="{rid}"/><a:srcRect b="0" l="0" r="0" t="0"/>'
            '<a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm>'
            f'<a:off x="0" y="0"/><a:ext cx="{ANCHO_EMU}" cy="{alto_emu}"/></a:xfrm>'
            '<a:prstGeom prst="rect"/><a:ln/></pic:spPr></pic:pic></a:graphicData></a:graphic>'
            '</wp:inline></w:drawing></w:r></w:p>')


def main(entrada, salida, capturas):
    capturas = pathlib.Path(capturas)
    z = zipfile.ZipFile(entrada)
    piezas = {n: z.read(n) for n in z.namelist()}
    z.close()

    rels = piezas['word/_rels/document.xml.rels'].decode()
    usados = {int(n) for n in re.findall(r'Id="rId(\d+)"', rels)}
    siguiente = max(usados) + 1
    docpr = max([int(n) for n in re.findall(r'<wp:docPr id="(\d+)"', piezas['word/document.xml'].decode())] or [0]) + 1

    cuerpo, nuevas_rels = [], []
    cuerpo.append(parrafo(run('Anexo. Capturas del juego terminado', negrita=True), 160, 'left'))
    cuerpo.append(parrafo(run(
        'Las imágenes de este anexo son capturas de la versión final compilada, tomadas con el '
        'juego en marcha; no son bocetos ni vistas del editor. Recorren el menú, los seis mapas, '
        'el puzle de mosaico y el cierre de la historia.')))

    for i, (archivo, titulo, nota) in enumerate(FIGURAS):
        origen = capturas / archivo
        if not origen.exists():
            raise SystemExit(f'falta la captura {origen}')
        im = Image.open(origen).convert('RGB')
        alto_px = round(im.height * ANCHO_PX / im.width)
        im = im.resize((ANCHO_PX, alto_px), Image.LANCZOS)
        destino = f'word/media/captura{i + 1}.png'
        import io
        buf = io.BytesIO(); im.save(buf, 'PNG', optimize=True)
        piezas[destino] = buf.getvalue()

        rid = f'rId{siguiente}'
        nuevas_rels.append(f'<Relationship Id="{rid}" Type="http://schemas.openxmlformats.org/'
                           f'officeDocument/2006/relationships/image" Target="media/captura{i + 1}.png"/>')
        siguiente += 1

        alto_emu = round(ANCHO_EMU * alto_px / ANCHO_PX)
        cuerpo.append(parrafo(run(f'Figura {11 + i}', negrita=True), 0, 'left'))
        cuerpo.append(parrafo(run(titulo, cursiva=True), 120, 'left'))
        cuerpo.append(imagen(rid, docpr + i, f'captura{i + 1}.png', alto_emu))
        cuerpo.append(parrafo(run('Nota. ', cursiva=True) + run(nota), 240))

    rels = rels.replace('</Relationships>', ''.join(nuevas_rels) + '</Relationships>')
    piezas['word/_rels/document.xml.rels'] = rels.encode()

    doc = piezas['word/document.xml'].decode()
    # El anexo va al final del cuerpo, pero antes de <w:sectPr>, que cierra la sección.
    corte = doc.rfind('<w:sectPr')
    if corte == -1:
        corte = doc.rfind('</w:body>')
    doc = doc[:corte] + ''.join(cuerpo) + doc[corte:]
    piezas['word/document.xml'] = doc.encode()

    with zipfile.ZipFile(salida, 'w', zipfile.ZIP_DEFLATED) as s:
        for n, b in piezas.items():
            s.writestr(n, b)
    print(f'{len(FIGURAS)} figuras añadidas (Figura 11 a {10 + len(FIGURAS)}) -> {salida}')


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2], sys.argv[3])

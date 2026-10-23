# /// script
# requires-python = ">=3.10"
# dependencies = ["pillow"]
# ///
# Arte propio (Fortino Romero Mantilla) — F25, pulido: la calle del Recuerdo 1.
#
# La franja de calle frente a La Espiga tenía dos macetas y una banca en 28 x 4 tiles. Es el
# primer sitio al que sale Ámbar tras desmentir al Eco de la puerta, y estaba vacío. Lo que se
# dibuja aquí lo pide la escena: la panadería está CERRADA en 2015, así que el letrero y el
# cartel en la puerta son los que cuentan ese dato sin una línea de diálogo.
# Uso: uv run --script tools/arte/calle_f25.py
from PIL import Image, ImageDraw
import pathlib, random

DESTINO = pathlib.Path(__file__).resolve().parents[2] / 'Assets/_Proyecto/Art/Objetos'
rng = random.Random(2015)

NEGRO  = (46, 34, 30, 255)
MADERA = [(146, 100, 58, 255), (116, 78, 44, 255), (88, 58, 32, 255)]
CEMENTO = [(176, 170, 158, 255), (158, 152, 140, 255), (140, 134, 122, 255)]
METAL  = [(126, 130, 138, 255), (94, 98, 106, 255), (168, 172, 180, 255)]
PAPEL  = (238, 230, 206, 255)
CRISTAL = (182, 206, 206, 255)

# Tipografía de 3x5 px, sólo las letras que hacen falta. Cada fila es una máscara de 3 bits.
GLIFOS = {
    'L': ['100', '100', '100', '100', '111'], 'A': ['010', '101', '111', '101', '101'],
    'E': ['111', '100', '110', '100', '111'], 'S': ['111', '100', '111', '001', '111'],
    'P': ['110', '101', '110', '100', '100'], 'I': ['111', '010', '010', '010', '111'],
    'G': ['111', '100', '101', '101', '111'], 'R': ['110', '101', '110', '101', '101'],
    'D': ['110', '101', '101', '101', '110'], 'O': ['111', '101', '101', '101', '111'],
    'C': ['111', '100', '100', '100', '111'], ' ': ['000', '000', '000', '000', '000'],
}


def escribir(d, texto, x, y, color):
    for letra in texto:
        for fy, fila in enumerate(GLIFOS[letra]):
            for fx, bit in enumerate(fila):
                if bit == '1':
                    d.point((x + fx, y + fy), fill=color)
        x += 4
    return x


def letrero():
    """Letrero colgante de La Espiga, 64x32: tabla de madera con dos cadenas."""
    im = Image.new('RGBA', (64, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for x in (12, 51):                                  # cadenas
        d.line([(x, 0), (x, 7)], fill=METAL[1])
    d.rectangle([6, 7, 57, 26], fill=MADERA[2])
    d.rectangle([7, 8, 56, 25], fill=MADERA[0])
    d.rectangle([9, 10, 54, 23], fill=MADERA[1])
    escribir(d, 'LA ESPIGA', 13, 13, (236, 222, 186, 255))
    for _ in range(18):                                 # veta y desgaste
        x, y = rng.randint(10, 53), rng.randint(11, 22)
        d.point((x, y), fill=MADERA[2])
    im.save(DESTINO / 'letrero_espiga.png')


def cartel_cerrado():
    """El cartel de CERRADO que cuelga del vidrio de la puerta, 32x32."""
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.line([(16, 2), (16, 7)], fill=(120, 110, 96, 255))
    d.rectangle([1, 7, 30, 22], fill=NEGRO)
    d.rectangle([2, 8, 29, 21], fill=PAPEL)
    escribir(d, 'CERRADO', 3, 12, (168, 52, 36, 255))   # 7 letras x 4 px: cabe justo
    im.save(DESTINO / 'cartel_cerrado.png')


def farol():
    """Farol de poste, 32x64. Sin luz propia: la de la escena es de día."""
    im = Image.new('RGBA', (32, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([14, 18, 17, 62], fill=METAL[1])
    d.rectangle([15, 18, 15, 62], fill=METAL[0])
    d.ellipse([10, 58, 21, 63], fill=METAL[1])          # base
    d.polygon([(16, 2), (24, 10), (8, 10)], fill=METAL[1])
    d.rectangle([9, 10, 23, 20], fill=METAL[1])
    d.rectangle([11, 12, 21, 18], fill=CRISTAL)
    d.point((16, 15), fill=(252, 238, 190, 255))
    im.save(DESTINO / 'farol.png')


def banqueta(esquina):
    """Tile de banqueta de cemento. 'esquina' marca el borde que da a la calle."""
    im = Image.new('RGBA', (32, 32), CEMENTO[0])
    px = im.load()
    for y in range(32):
        for x in range(32):
            if rng.random() < 0.2:
                px[x, y] = rng.choice(CEMENTO[1:])
    for x in range(32):                                  # junta entre losas
        px[x, 0] = CEMENTO[2]
    for y in range(32):
        px[0, y] = CEMENTO[2]
    if esquina:
        for x in range(32):
            px[x, 31] = CEMENTO[2]
            px[x, 30] = CEMENTO[1]
    im.save(DESTINO / ('banqueta_borde.png' if esquina else 'banqueta.png'))


def cajas_reparto():
    """Cajas de madera apiladas con charolas vacías, 32x32."""
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([3, 14, 20, 29], fill=MADERA[2]); d.rectangle([4, 15, 19, 28], fill=MADERA[0])
    d.line([(4, 21), (19, 21)], fill=MADERA[2])
    d.rectangle([13, 5, 28, 18], fill=MADERA[2]); d.rectangle([14, 6, 27, 17], fill=MADERA[1])
    d.line([(14, 12), (27, 12)], fill=MADERA[2])
    im.save(DESTINO / 'cajas_reparto.png')


if __name__ == '__main__':
    letrero(); cartel_cerrado(); farol(); banqueta(False); banqueta(True); cajas_reparto()
    print('Calle: letrero_espiga.png, cartel_cerrado.png, farol.png, banqueta.png, '
          'banqueta_borde.png, cajas_reparto.png')

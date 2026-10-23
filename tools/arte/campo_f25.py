# /// script
# requires-python = ">=3.10"
# dependencies = ["pillow"]
# ///
# Arte propio (Fortino Romero Mantilla) — F25, pulido: el campo del Recuerdo 4.
#
# El campo se pintaba con un recorte del tileset LPC de plantas repetido en diagonales
# regulares, sobre el empedrado del patio: parecía papel tapiz, no un campo de flores. Y el
# Documento de Diseño pide "atardecer en el campo de cempasúchil" y "1955 en dorado de
# atardecer", así que el suelo tampoco debía ser gris.
#
# Aquí se dibujan ocho matas distintas —misma flor que flor_alta_*.png, que ya estaba en la
# escena— más un brote sin flor para los claros, y dos tierras labradas. Con cuatro variantes
# todavía se veía la cuadrícula de 32 px; con ocho, de alturas y densidades distintas, no.
# Uso: uv run --script tools/arte/campo_f25.py
from PIL import Image, ImageDraw
import pathlib, random

DESTINO = pathlib.Path(__file__).resolve().parents[2] / 'Assets/_Proyecto/Art/Objetos'
rng = random.Random(1955)                      # el año del recuerdo, para que sea reproducible

TALLO = [(58, 112, 46, 255), (74, 134, 58, 255), (92, 152, 70, 255)]
FLOR  = [(252, 168, 34, 255), (240, 132, 16, 255), (255, 198, 72, 255), (214, 96, 20, 255)]
CORAZON = (170, 70, 14, 255)
TIERRA = [(142, 110, 70, 255), (124, 94, 58, 255), (158, 126, 84, 255), (110, 82, 50, 255)]
# Barda de piedra de campo: la que cierra una parcela, no el sillar de castillo del tileset.
# Tonos cercanos a propósito: el tile se repite a lo largo de toda la barda, y con piedras muy
# contrastadas el ojo reconoce el patrón y ve la costura cada 32 px.
PIEDRA = [(148, 138, 122, 255), (132, 122, 106, 255), (160, 150, 134, 255), (120, 111, 96, 255)]
JUNTA = (74, 66, 56, 255)


def mata(alto_max, cuantas):
    """Una mata de cempasúchil: tallos que salen del borde de abajo y flores arriba."""
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for _ in range(cuantas):
        x = rng.randint(3, 28)
        alto = rng.randint(alto_max - 7, alto_max)
        pie = rng.randint(28, 33)                      # sale del borde: así las matas se encadenan
        d.line([(x + rng.randint(-2, 2), pie), (x, 32 - alto + 4)], fill=rng.choice(TALLO))
        for lado in (-1, 1):                           # un par de hojas
            hy = 32 - alto // 2 + rng.randint(-2, 2)
            d.line([(x, hy), (x + 3 * lado, hy - 2)], fill=TALLO[2])
        cx, cy = x, 32 - alto
        d.ellipse([cx - 3, cy - 2, cx + 3, cy + 4], fill=rng.choice(FLOR))
        d.point((cx, cy + 1), fill=CORAZON)
    return im


def tierra(surco):
    """Tierra labrada. 'surco' añade la huella del arado, que da dirección al campo."""
    im = Image.new('RGBA', (32, 32), TIERRA[0])
    px = im.load()
    for y in range(32):
        for x in range(32):
            if rng.random() < 0.28:
                px[x, y] = rng.choice(TIERRA[1:])
    if surco:
        for y in (7, 23):
            for x in range(32):
                px[x, y] = TIERRA[3]
                px[x, (y + 1) % 32] = TIERRA[1]
    return im


def barda(desde_arriba):
    """
    Barda de piedra de campo: canto rodado irregular, no sillar.

    Se reparten semillas por el tile y cada píxel toma el tono de la más cercana, midiendo la
    distancia por el camino corto en los dos ejes: así las piedras cruzan el borde del tile y la
    barda queda continua al repetirse. Con hiladas de rectángulos quedaba un muro de ladrillo,
    que es exactamente lo que se quería evitar.
    """
    n = 11 if desde_arriba else 14
    semillas = [(rng.randrange(32), rng.randrange(32), rng.choice(PIEDRA)) for _ in range(n)]
    im = Image.new('RGBA', (32, 32))
    px = im.load()
    duena = {}
    for y in range(32):
        for x in range(32):
            mejor, md = None, 9999
            for i, (sx, sy, tono) in enumerate(semillas):
                dx = min(abs(x - sx), 32 - abs(x - sx))      # distancia que da la vuelta al tile
                dy = min(abs(y - sy), 32 - abs(y - sy))
                d2 = dx * dx + dy * dy
                if d2 < md: mejor, md = i, d2
            duena[(x, y)] = mejor
            px[x, y] = semillas[mejor][2]
    # Junta: el píxel que linda con otra piedra se oscurece.
    juntas = [(x, y) for y in range(32) for x in range(32)
              if any(duena[((x + dx) % 32, (y + dy) % 32)] != duena[(x, y)]
                     for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))]
    for x, y in juntas:
        px[x, y] = JUNTA
    for _ in range(26):                                      # grano
        x, y = rng.randrange(32), rng.randrange(32)
        if (x, y) not in juntas:
            px[x, y] = rng.choice(PIEDRA)
    if desde_arriba:                                         # musgo y hierba en el remate
        for _ in range(14):
            px[rng.randrange(32), rng.randrange(32)] = rng.choice([(96, 128, 72, 255), (118, 146, 84, 255)])
    return im


if __name__ == '__main__':
    hechas = []
    for k, (alto, cuantas) in enumerate([(30, 5), (26, 6), (22, 4), (28, 7),
                                         (19, 3), (24, 5), (16, 4), (31, 6)]):
        mata(alto, cuantas).save(DESTINO / f'campo_mata_{k}.png')
        hechas.append(f'campo_mata_{k}.png')
    brote = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    db = ImageDraw.Draw(brote)
    for _ in range(5):
        x, alto = rng.randint(4, 27), rng.randint(6, 12)
        db.line([(x, 32), (x + rng.randint(-2, 2), 32 - alto)], fill=rng.choice(TALLO))
    brote.save(DESTINO / 'campo_brote.png')
    barda(False).save(DESTINO / 'campo_barda.png')
    barda(True).save(DESTINO / 'campo_barda_arriba.png')
    tierra(False).save(DESTINO / 'campo_tierra.png')
    tierra(True).save(DESTINO / 'campo_tierra_surco.png')
    print('Campo:', ', '.join(hechas + ['campo_brote.png', 'campo_barda.png',
                                        'campo_barda_arriba.png', 'campo_tierra.png',
                                        'campo_tierra_surco.png']))

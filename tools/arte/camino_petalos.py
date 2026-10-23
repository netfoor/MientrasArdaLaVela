# /// script
# requires-python = ">=3.10"
# dependencies = ["pillow"]
# ///
# Arte propio (Fortino Romero Mantilla) — F25, pulido: el camino de pétalos de cempasúchil
# del Mapa 0 (sección 5.1 del Documento de Diseño: "patio con camino de pétalos" y el
# fragmento M0_petalos, "el camino que se divide en dos").
#
# Antes se pintaba con un tile de plantas del tileset LPC, que de lejos parecía hierba
# suelta y repetía el mismo cuadro: no se leía como un camino. Aquí se dibujan tres
# densidades para alternarlas, más una flor entera que se siembra de vez en cuando.
# Cada pétalo es una gota de 4x3 px con tres tonos, dibujada a mano y no esparcida al azar:
# así el camino tiene grano pero conserva su borde.
# Uso: uv run --script tools/arte/camino_petalos.py
from PIL import Image
import pathlib, math

DESTINO = pathlib.Path(__file__).resolve().parents[2] / 'Assets/_Proyecto/Art/Objetos'

# Cempasúchil: del naranja quemado al amarillo de vela. Los tonos son claros a propósito:
# el Mapa 0 y el epílogo pasan de noche, con la luz global muy baja, y una primera versión
# más apagada desaparecía sobre la duela en cuanto se miraba a la escala del juego.
OSC = (214, 104, 24, 255)
MED = (250, 158, 40, 255)
CLA = (255, 212, 104, 255)
ROJ = (206, 82, 34, 255)      # algún pétalo más maduro, para que no sea un solo naranja


def petalo(px, x, y, giro, tono):
    """Una gota de 5x4 px, caída en diagonal. 'giro' 0-3 la recuesta hacia un lado u otro."""
    base, luz = (ROJ, MED) if tono == 2 else (OSC, CLA if tono else MED)
    cuerpo = [(2, 0), (3, 0), (1, 1), (2, 1), (3, 1), (4, 1),
              (0, 2), (1, 2), (2, 2), (3, 2), (0, 3), (1, 3)]
    brillo = [(2, 1), (1, 2)]
    for odx, ody in cuerpo:
        dx, dy = odx, ody
        if giro == 1:   dx, dy = ody, odx
        elif giro == 2: dx, dy = -odx, ody
        elif giro == 3: dx, dy = ody, -odx
        if 0 <= x + dx < 32 and 0 <= y + dy < 32:
            px[x + dx, y + dy] = luz if (odx, ody) in brillo else base


def flor(px, x, y):
    """Una flor entera de 9 px: corazón oscuro, corona de pétalos y ocho puntas."""
    for dy in range(-4, 5):
        for dx in range(-4, 5):
            r = math.hypot(dx, dy)
            ix, iy = x + dx, y + dy
            if not (0 <= ix < 32 and 0 <= iy < 32):
                continue
            if r <= 1.4:
                px[ix, iy] = OSC
            elif r <= 2.6:
                px[ix, iy] = CLA
            elif r <= 3.6:
                px[ix, iy] = MED
            elif r <= 4.4 and (dx == 0 or dy == 0 or abs(dx) == abs(dy)):
                px[ix, iy] = OSC          # las ocho puntas, que recortan el disco

def tile(nombre, siembra, con_flor=False):
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    px = im.load()
    for x, y, giro, tono in siembra:
        petalo(px, x, y, giro, tono)
    if con_flor:
        flor(px, 15, 16)
    im.save(DESTINO / nombre)
    return nombre


# Las posiciones están escritas a mano para que ningún pétalo toque el borde del tile:
# si se tocaran, al repetir el camino aparecerían costuras rectas.
DENSO = [(3, 4, 0, 0), (12, 2, 2, 1), (21, 5, 1, 0), (27, 3, 3, 2),
         (7, 9, 1, 1), (16, 8, 0, 2), (25, 11, 2, 0), (2, 14, 3, 1),
         (11, 15, 0, 0), (19, 17, 1, 2), (27, 16, 0, 1), (5, 21, 2, 0),
         (14, 22, 3, 1), (23, 23, 0, 0), (9, 27, 1, 2), (18, 27, 2, 1)]
MEDIO = [(5, 3, 1, 1), (17, 6, 0, 0), (26, 9, 2, 2), (3, 12, 0, 1),
         (13, 14, 3, 0), (22, 18, 1, 1), (8, 20, 2, 2), (16, 25, 0, 0),
         (27, 26, 3, 1)]
RALO = [(9, 5, 2, 1), (20, 11, 0, 0), (6, 18, 3, 2), (24, 22, 1, 1), (14, 27, 0, 0)]

if __name__ == '__main__':
    hechos = [tile('camino_denso.png', DENSO),
              tile('camino_medio.png', MEDIO),
              tile('camino_ralo.png', RALO),
              tile('camino_flor.png', RALO, con_flor=True)]
    print('Camino de pétalos:', ', '.join(hechos))

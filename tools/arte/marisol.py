# /// script
# requires-python = ">=3.10"
# dependencies = ["pillow"]
# ///
# Arte propio (Fortino Romero Mantilla) — F25, pulido: las dos hojas de Marisol, la hija de
# don Aurelio. El generador LPC no tenía un personaje que sirviera para ella, así que hasta
# ahora el Recuerdo 1 usaba la hoja de Rosa (¡su madre!) con un tinte del SpriteRenderer y el
# Recuerdo 2 la de Lucía de 19 años, que además es la nieta del epílogo. Las dos colisiones
# hacían que tres personajes distintos se vieran iguales.
#
# Un tinte multiplica TODA la hoja: también la piel y los ojos. Aquí se recolorean solo dos
# rampas de la hoja de Rosa —el vestido y el cabello— respetando el sombreado original:
#   · marisol_45_walk    Recuerdo 1 (2015): vestido verde jade y cabello castaño.
#   · marisol_novia_walk Recuerdo 2 (1988): el mismo cabello y vestido de novia color hueso.
# Mismo cabello en las dos épocas para que se lea como la misma persona, como se hizo con
# Refugio en sus tres edades. Es un retoque propio sobre arte LPC: va anotado en CREDITOS.txt.
# Uso: uv run --script tools/arte/marisol.py
from PIL import Image
import pathlib

PERSONAJES = pathlib.Path(__file__).resolve().parents[2] / 'Assets/_Proyecto/Art/Personajes'
BASE = PERSONAJES / 'rosa_40_walk.png'

# Rampas de la hoja de Rosa, de la sombra al brillo. Comprobadas sobre la hoja: la primera
# cubre exactamente el vestido y la segunda exactamente el cabello.
VESTIDO = [(40, 30, 65), (50, 45, 106), (60, 73, 173), (70, 106, 201), (97, 160, 239)]
CABELLO = [(8, 10, 10), (16, 20, 20), (28, 34, 34)]

JADE   = [(26, 44, 36), (38, 74, 58), (52, 104, 78), (74, 140, 104), (120, 190, 140)]
HUESO  = [(72, 64, 58), (152, 142, 130), (198, 188, 174), (234, 228, 214), (252, 250, 242)]
CASTANO = [(44, 28, 18), (72, 46, 28), (108, 72, 44)]


def recolorear(destino, vestido):
    im = Image.open(BASE).convert('RGBA')
    mapa = {(*v, 255): (*n, 255) for v, n in zip(VESTIDO, vestido)}
    mapa.update({(*v, 255): (*n, 255) for v, n in zip(CABELLO, CASTANO)})
    px = im.load()
    ancho, alto = im.size
    cambiados = 0
    for y in range(alto):
        for x in range(ancho):
            nuevo = mapa.get(px[x, y])
            if nuevo is not None:
                px[x, y] = nuevo
                cambiados += 1
    im.save(PERSONAJES / destino)
    print(f'{destino}: {cambiados} px recoloreados')


if __name__ == '__main__':
    recolorear('marisol_45_walk.png', JADE)
    recolorear('marisol_novia_walk.png', HUESO)

# /// script
# requires-python = ">=3.10"
# dependencies = ["pillow"]
# ///
# Arte propio (Fortino Romero Mantilla) — F25, pulido: los fragmentos de memoria, los
# mementos y los rombos del HUD, que hasta ahora eran cuadros de color de relleno.
# Cada pieza es pixel art de 32x32 (32 px = 1 unidad) con contorno oscuro y 3-4 tonos,
# para que case con los tilesets LPC del juego. Lo que dibuja cada una sale de las
# Tablas 8 a 13 del Documento de Diseño (los cuatro fragmentos de cada mapa).
# Uso: uv run --script tools/arte/objetos_f25.py
from PIL import Image, ImageDraw
import pathlib

RAIZ = pathlib.Path(__file__).resolve().parents[2] / 'Assets/_Proyecto/Art'
OBJ, UI = RAIZ / 'Objetos', RAIZ / 'UI'
UI.mkdir(parents=True, exist_ok=True)

NEGRO = (46, 34, 30, 255)          # contorno común
# Paletas por material
PAPEL  = [(236, 224, 196, 255), (206, 190, 156, 255), (166, 148, 112, 255)]
SEPIA  = [(222, 196, 152, 255), (176, 142, 104, 255), (108, 84, 64, 255)]
TELA   = [(222, 214, 196, 255), (184, 174, 150, 255), (146, 136, 112, 255)]
FLOR   = [(255, 186, 56, 255), (240, 138, 28, 255), (192, 86, 20, 255)]
PALMA  = [(226, 192, 118, 255), (192, 154, 84, 255), (150, 116, 60, 255)]
METAL  = [(198, 202, 210, 255), (146, 150, 160, 255), (102, 106, 118, 255)]
MASA   = [(228, 184, 120, 255), (198, 146, 80, 255), (158, 108, 56, 255)]
MADERA = [(150, 104, 62, 255), (116, 78, 46, 255), (84, 56, 34, 255)]
VIDRIO = [(214, 232, 238, 255), (168, 196, 208, 255), (120, 150, 166, 255)]
VINO   = [(158, 36, 56, 255), (116, 24, 40, 255)]
ROJO   = [(206, 82, 52, 255), (166, 56, 36, 255), (122, 38, 26, 255)]
AZUL   = [(104, 158, 232, 255), (56, 104, 196, 255), (30, 62, 140, 255)]
CREMA  = (246, 238, 214, 255)

def lienzo():
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)

def contorno(im, color=NEGRO):
    """Rodea de oscuro todo lo dibujado: es lo que hace legible un sprite de 32 px."""
    a = im.split()[3]
    px, out = a.load(), im.load()
    nuevos = []
    for y in range(32):
        for x in range(32):
            if px[x, y] > 0:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                vx, vy = x + dx, y + dy
                if 0 <= vx < 32 and 0 <= vy < 32 and px[vx, vy] > 128:
                    nuevos.append((x, y)); break
    for p in nuevos:
        out[p] = color
    return im

def brillo(d, x, y, c=(255, 255, 255, 190)):
    d.point((x, y), c)

def guardar(im, carpeta, nombre, borde=True):
    if borde:
        contorno(im)
    im.save(carpeta / nombre)
    return nombre

# ---------------------------------------------------------------- Mapa 0
def foto_joven():                      # "la fotografía de Aurelio joven"
    im, d = lienzo()
    d.rectangle([6, 8, 25, 24], fill=PAPEL[0])
    d.rectangle([8, 10, 23, 20], fill=SEPIA[0])          # la imagen
    d.ellipse([13, 12, 18, 17], fill=SEPIA[1])           # cabeza
    d.rectangle([12, 17, 19, 20], fill=SEPIA[1])         # hombros
    d.line([8, 21, 23, 21], fill=SEPIA[2])
    d.line([6, 24, 25, 24], fill=PAPEL[2])               # borde inferior gastado
    return im

def delantal():                        # "su delantal"
    im, d = lienzo()
    d.arc([12, 3, 19, 11], 180, 360, fill=TELA[2])       # lazo del cuello (un aro, no dos antenas)
    d.polygon([(12, 8), (19, 8), (20, 15), (11, 15)], fill=TELA[0])    # peto
    d.polygon([(8, 15), (23, 15), (24, 25), (22, 27), (9, 27), (7, 25)], fill=TELA[0])   # falda
    d.rectangle([7, 15, 24, 17], fill=TELA[2])           # cinta a la cintura
    d.line([7, 16, 3, 19], fill=TELA[2]); d.line([24, 16, 28, 19], fill=TELA[2])   # tiras que amarran
    d.line([9, 24, 22, 24], fill=TELA[1])                # caída de la tela
    d.rectangle([13, 19, 18, 23], fill=TELA[1])          # bolsillo
    d.line([14, 21, 17, 21], fill=MASA[2])               # harina en el bolsillo
    return im

def petalos_camino():                  # "el camino de pétalos que se divide en dos"
    im, d = lienzo()
    for x, y in ((15, 27), (16, 24), (15, 21), (16, 18)):            # tramo común
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=FLOR[1])
        d.point((x, y - 1), FLOR[0])
    for x, y in ((12, 15), (9, 12), (7, 9)):                          # ramal izquierdo
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=FLOR[1]); d.point((x, y - 1), FLOR[0])
    for x, y in ((20, 15), (23, 12), (25, 9)):                        # ramal derecho
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=FLOR[2]); d.point((x, y - 1), FLOR[1])
    return im

def sombrero_palma():                  # "un sombrero de palma de niño"
    im, d = lienzo()
    d.ellipse([3, 15, 28, 25], fill=PALMA[1])            # ala
    d.ellipse([6, 16, 25, 23], fill=PALMA[0])
    d.ellipse([10, 8, 21, 19], fill=PALMA[0])            # copa
    d.arc([10, 8, 21, 19], 200, 340, fill=PALMA[2])
    d.rectangle([10, 15, 21, 17], fill=MADERA[1])        # toquilla
    for x in range(5, 27, 3):
        d.point((x, 20), PALMA[2])                       # trenzado de la palma
    return im

# ---------------------------------------------------------------- Recuerdo 1
def foto_rota():                       # "la fotografía rota" (cortada con tijeras)
    im, d = lienzo()
    d.polygon([(6, 8), (17, 8), (16, 24), (6, 24)], fill=PAPEL[0])   # sólo la mitad izquierda
    d.rectangle([8, 10, 15, 20], fill=SEPIA[0])
    d.ellipse([10, 12, 15, 17], fill=SEPIA[1])
    d.rectangle([9, 17, 16, 20], fill=SEPIA[1])
    for y in range(8, 25, 2):                                        # el corte, en zigzag
        d.point((17 - (y % 4 == 0), y), PAPEL[2])
        d.point((18 - (y % 4 == 0), y), SEPIA[2])
    d.line([6, 24, 16, 24], fill=PAPEL[2])
    return im

def lata_cartas():                     # "la lata con cartas sin abrir"
    im, d = lienzo()
    d.rectangle([7, 12, 24, 26], fill=METAL[1])          # cuerpo
    d.rectangle([7, 12, 24, 14], fill=METAL[0])          # ceja de luz
    d.rectangle([9, 16, 22, 18], fill=ROJO[1])           # franja pintada
    for i, x in enumerate((11, 15, 19)):                 # sobres asomando
        d.polygon([(x, 6 + i), (x + 6, 6 + i), (x + 6, 12), (x, 12)], fill=PAPEL[0])
        d.line([x, 6 + i, x + 3, 9 + i], fill=PAPEL[2]); d.line([x + 6, 6 + i, x + 3, 9 + i], fill=PAPEL[2])
    brillo(d, 9, 13)
    return im

def charola_pan():                     # "la charola con dos piezas de pan apartadas"
    im, d = lienzo()
    d.ellipse([3, 17, 28, 27], fill=METAL[1])            # charola
    d.ellipse([5, 18, 26, 25], fill=METAL[0])
    for cx in (11, 20):                                  # dos panes
        d.ellipse([cx - 5, 11, cx + 5, 21], fill=MASA[1])
        d.ellipse([cx - 4, 11, cx + 4, 19], fill=MASA[0])
        d.point((cx, 13), MASA[2]); d.point((cx, 14), MASA[2])        # la canilla
        d.point((cx - 2, 15), CREMA); d.point((cx + 2, 15), CREMA)    # azúcar
    return im

# ---------------------------------------------------------------- Recuerdo 2
def tarjeta_refugio():                 # "la tarjeta con el nombre de Refugio"
    im, d = lienzo()
    d.polygon([(4, 12), (27, 12), (27, 23), (4, 23)], fill=PAPEL[0])
    d.polygon([(4, 12), (27, 12), (25, 9), (6, 9)], fill=PAPEL[1])   # doblez de pie
    for y, (x0, x1) in ((15, (8, 23)), (18, (8, 19))):               # el nombre escrito a mano
        d.line([x0, y, x1, y], fill=SEPIA[2])
    d.line([8, 20, 14, 20], fill=PAPEL[2])
    return im

def telegrama():                       # "los pedazos del telegrama"
    im, d = lienzo()
    d.polygon([(4, 10), (16, 8), (15, 20), (5, 21)], fill=PAPEL[0])  # pedazo grande
    d.polygon([(17, 14), (28, 16), (27, 25), (16, 23)], fill=PAPEL[1])  # pedazo chico
    for y in (12, 14, 16):
        d.line([6, y, 13, y], fill=SEPIA[2])
    for y in (19, 21):
        d.line([19, y, 25, y], fill=SEPIA[2])
    d.line([16, 9, 15, 20], fill=PAPEL[2])
    return im

def copa_rota():                       # "el brindis interrumpido"
    im, d = lienzo()
    d.polygon([(10, 7), (21, 7), (19, 17), (12, 17)], fill=VIDRIO[0])   # cáliz
    d.polygon([(12, 12), (19, 12), (19, 17), (12, 17)], fill=VINO[0])   # vino que queda
    d.rectangle([15, 17, 16, 23], fill=VIDRIO[1])                       # tallo
    d.ellipse([11, 22, 20, 26], fill=VIDRIO[1])                         # pie
    d.polygon([(21, 7), (26, 4), (24, 9)], fill=VIDRIO[2])              # astilla que salta
    d.point((27, 11), VINO[1]); d.point((25, 13), VINO[1])              # salpicadura
    brillo(d, 12, 9)
    return im

# ---------------------------------------------------------------- Recuerdo 3
def cama_vacia():                      # "la cama vacía de Aurelio"
    im, d = lienzo()
    d.rectangle([4, 14, 28, 24], fill=MADERA[1])         # armazón
    d.rectangle([5, 15, 27, 21], fill=TELA[0])           # colchón
    d.rectangle([5, 15, 13, 21], fill=TELA[1])           # cobija echada a un lado
    d.rectangle([17, 11, 26, 16], fill=CREMA)            # almohada sin hundir
    d.rectangle([4, 24, 6, 27], fill=MADERA[2])          # patas
    d.rectangle([26, 24, 28, 27], fill=MADERA[2])
    return im

def nota_doblada():                    # "la nota para Rosa"
    im, d = lienzo()
    d.polygon([(6, 9), (26, 9), (26, 25), (6, 25)], fill=PAPEL[0])
    d.polygon([(6, 9), (26, 9), (16, 15)], fill=PAPEL[1])            # marca del doblez
    for y, x1 in ((17, 22), (20, 24), (23, 18)):
        d.line([9, y, x1, y], fill=SEPIA[2])
    return im

# ---------------------------------------------------------------- Recuerdo 4
def promesa():                         # "la promesa bajo el árbol": las iniciales talladas
    im, d = lienzo()
    d.rectangle([7, 4, 25, 28], fill=MADERA[1])          # corteza
    for x in (9, 14, 19, 23):
        d.line([x, 4, x, 28], fill=MADERA[2])            # vetas
    d.rectangle([10, 11, 22, 21], fill=MADERA[0])        # zona lisa de la talla
    d.line([12, 13, 12, 19], fill=PALMA[0]); d.line([12, 13, 15, 13], fill=PALMA[0])   # A
    d.line([12, 16, 15, 16], fill=PALMA[0]); d.line([15, 13, 15, 19], fill=PALMA[0])
    d.line([18, 13, 18, 19], fill=PALMA[0]); d.line([18, 13, 20, 14], fill=PALMA[0])   # R
    d.line([18, 16, 20, 16], fill=PALMA[0]); d.line([20, 17, 21, 19], fill=PALMA[0])
    return im

# ---------------------------------------------------------------- mementos
def memento_pan():                     # pan de muerto de dos piezas (M0)
    im, d = lienzo()
    d.ellipse([3, 10, 19, 26], fill=MASA[1])
    d.ellipse([4, 10, 18, 24], fill=MASA[0])
    d.ellipse([14, 8, 28, 22], fill=MASA[1])
    d.ellipse([15, 8, 27, 20], fill=MASA[0])
    for cx, cy in ((11, 17), (21, 14)):                  # canillas en cruz y bolita
        d.line([cx, cy - 5, cx, cy + 5], fill=MASA[2])
        d.line([cx - 5, cy, cx + 5, cy], fill=MASA[2])
        d.ellipse([cx - 1, cy - 1, cx + 1, cy + 1], fill=MASA[1])
    for p in ((7, 13), (15, 21), (24, 11), (19, 18)):
        d.point(p, CREMA)                                # azúcar
    return im

def memento_foto():                    # la fotografía (R1), ya completa y enmarcada
    im, d = lienzo()
    d.rectangle([4, 6, 27, 26], fill=MADERA[1])          # marco
    d.rectangle([6, 8, 25, 24], fill=PAPEL[0])
    d.rectangle([7, 9, 24, 22], fill=SEPIA[0])
    for cx in (12, 19):                                  # los dos hermanos
        d.ellipse([cx - 3, 11, cx + 3, 17], fill=SEPIA[1])
        d.rectangle([cx - 4, 17, cx + 4, 22], fill=SEPIA[1])
    d.line([15, 18, 16, 18], fill=SEPIA[2])              # las manos juntas
    return im

def memento_cerillos():                # caja de cerillos (R2)
    im, d = lienzo()
    d.rectangle([5, 12, 27, 25], fill=ROJO[1])
    d.rectangle([5, 12, 27, 14], fill=ROJO[0])
    d.rectangle([7, 16, 25, 22], fill=PAPEL[1])          # etiqueta
    d.rectangle([7, 16, 25, 17], fill=ROJO[2])
    d.line([9, 19, 23, 19], fill=SEPIA[2])
    d.polygon([(16, 6), (18, 6), (18, 12), (16, 12)], fill=PALMA[1])   # cerillo encendido
    d.ellipse([15, 3, 19, 8], fill=FLOR[1]); d.ellipse([16, 4, 18, 7], fill=FLOR[0])
    return im

def memento_canica():                  # canica azul (R3)
    im, d = lienzo()
    d.ellipse([6, 6, 26, 26], fill=AZUL[1])
    d.ellipse([7, 7, 24, 24], fill=AZUL[0])
    d.arc([6, 6, 26, 26], 20, 160, fill=AZUL[2])
    d.polygon([(13, 12), (19, 10), (21, 17), (15, 20)], fill=CREMA)    # la cinta de dentro
    d.polygon([(14, 13), (18, 12), (19, 16), (15, 18)], fill=AZUL[0])
    d.ellipse([10, 9, 13, 12], fill=(255, 255, 255, 220))              # reflejo
    return im

def memento_promesa():                 # la promesa (R4): el árbol con la talla
    im, d = lienzo()
    d.ellipse([3, 2, 29, 18], fill=(92, 132, 70, 255))   # copa
    d.ellipse([5, 3, 22, 15], fill=(118, 160, 86, 255))
    for p in ((8, 6), (18, 5), (13, 11), (23, 10)):
        d.point(p, FLOR[1])                              # cempasúchil entre las ramas
    d.rectangle([13, 16, 19, 29], fill=MADERA[1])        # tronco
    d.line([14, 16, 14, 29], fill=MADERA[2])
    d.rectangle([14, 20, 18, 25], fill=MADERA[0])
    d.line([15, 21, 15, 24], fill=PALMA[0]); d.line([17, 21, 17, 24], fill=PALMA[0])   # A R
    return im

def destello():
    """
    El brillo que flota sobre un fragmento sin recoger. Era un cuadro blanco de relleno y
    es lo que más se notaba en pantalla. Va sin contorno: es luz, no un objeto.
    """
    im, d = lienzo()
    # Estrella de cuatro puntas: puntas largas en vertical/horizontal y talle estrecho.
    d.polygon([(16, 3), (18, 13), (28, 15), (18, 17), (16, 27), (14, 17), (4, 15), (14, 13)],
              fill=(255, 196, 92, 210))
    d.polygon([(16, 7), (17, 14), (24, 15), (17, 16), (16, 23), (15, 16), (8, 15), (15, 14)],
              fill=(255, 232, 170, 255))
    d.ellipse([14, 13, 18, 17], fill=(255, 252, 240, 255))
    for p, c in (((10, 9), (255, 214, 130, 150)), ((23, 21), (255, 214, 130, 150))):
        d.point(p, c)                                     # dos chispas sueltas
    return im

# ---------------------------------------------------------------- HUD
def rombo(lleno):
    """El HUD los gira 45°, así que se dibujan como cuadros biselados y simétricos."""
    im, d = lienzo()
    base, luz, som = ((FLOR[1], FLOR[0], FLOR[2]) if lleno
                      else ((78, 72, 86, 255), (104, 98, 112, 255), (52, 48, 58, 255)))
    d.rectangle([4, 4, 27, 27], fill=base)
    d.line([4, 4, 27, 4], fill=luz); d.line([4, 4, 4, 27], fill=luz)
    d.line([27, 4, 27, 27], fill=som); d.line([4, 27, 27, 27], fill=som)
    if lleno:
        d.ellipse([12, 12, 19, 19], fill=FLOR[0])
        d.ellipse([14, 14, 17, 17], fill=CREMA)
    return im

PIEZAS = [
    (OBJ, 'frag_foto_joven.png', foto_joven), (OBJ, 'frag_delantal.png', delantal),
    (OBJ, 'frag_petalos.png', petalos_camino), (OBJ, 'frag_sombrero.png', sombrero_palma),
    (OBJ, 'frag_foto_rota.png', foto_rota), (OBJ, 'frag_lata.png', lata_cartas),
    (OBJ, 'frag_charola.png', charola_pan), (OBJ, 'frag_tarjeta.png', tarjeta_refugio),
    (OBJ, 'frag_telegrama.png', telegrama), (OBJ, 'frag_copa.png', copa_rota),
    (OBJ, 'frag_cama.png', cama_vacia), (OBJ, 'frag_nota.png', nota_doblada),
    (OBJ, 'frag_promesa.png', promesa),
    (OBJ, 'memento_pan.png', memento_pan), (OBJ, 'memento_foto.png', memento_foto),
    (OBJ, 'memento_cerillos.png', memento_cerillos), (OBJ, 'memento_canica.png', memento_canica),
    (OBJ, 'memento_promesa.png', memento_promesa),
    (UI, 'rombo_vacio.png', lambda: rombo(False)), (UI, 'rombo_lleno.png', lambda: rombo(True)),
]
SIN_BORDE = {'destello.png'}

if __name__ == '__main__':
    PIEZAS.append((OBJ, 'destello.png', destello))
    hoja = Image.new('RGBA', (32 * 7, 32 * 3), (24, 22, 28, 255))
    for i, (carpeta, nombre, fn) in enumerate(PIEZAS):
        im = fn()
        guardar(im, carpeta, nombre, borde=nombre not in SIN_BORDE)
        hoja.paste(im, ((i % 7) * 32, (i // 7) * 32), im)
        print(carpeta.name + '/' + nombre)
    hoja.resize((hoja.width * 6, hoja.height * 6), Image.NEAREST).save('/tmp/hoja_f25.png')
    print(f'{len(PIEZAS)} piezas · hoja de contacto en /tmp/hoja_f25.png')

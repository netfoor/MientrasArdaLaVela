# Sprites del Recuerdo 4 (campo de cempasúchil): árbol, huellas, cabo de vela y flores altas. Arte original.
from PIL import Image, ImageDraw
import random
random.seed(4)
# Árbol (pirul) 64x96: tronco, copa colgante
im=Image.new('RGBA',(64,96),(0,0,0,0)); d=ImageDraw.Draw(im)
d.polygon([(28,95),(36,95),(35,60),(40,48),(33,52),(31,40),(29,52),(22,46),(29,60)],fill=(92,62,40,255))
d.line([(31,80),(31,62)],fill=(70,46,30,255))
for _ in range(260):
    x=random.gauss(32,14); y=random.gauss(30,12)
    if 0<=x<64 and 0<=y<62:
        c=random.choice([(58,110,48),(72,130,56),(88,146,62),(46,92,40)])
        d.ellipse([x-2,y-2,x+2,y+3],fill=c+(255,))
for x in range(6,60,5):  # ramas colgantes del pirul
    L=random.randint(6,14); d.line([(x,40),(x+random.randint(-2,2),40+L)],fill=(62,118,50,255))
for _ in range(14):  # frutitos rosados
    x=random.randint(10,54); y=random.randint(16,48); d.point((x,y),fill=(214,90,110,255))
im.save('arbol.png')
# Huellas pequeñas 32x32
im=Image.new('RGBA',(32,32),(0,0,0,0)); d=ImageDraw.Draw(im)
for (x,y) in [(6,24),(12,18),(8,11),(15,5)]:
    d.ellipse([x,y,x+4,y+6],fill=(70,48,30,200)); d.ellipse([x+1,y-2,x+3,y],fill=(70,48,30,200))
im.save('huellas.png')
# Cabo de vela con cera derramada 32x32
im=Image.new('RGBA',(32,32),(0,0,0,0)); d=ImageDraw.Draw(im)
d.ellipse([8,22,24,28],fill=(236,228,206,255)); d.rectangle([13,14,18,25],fill=(244,238,220,255))
d.line([(15,11),(15,14)],fill=(40,30,24,255)); d.ellipse([10,24,14,27],fill=(236,228,206,255))
im.save('vela_cabo.png')
# Flores altas de cempasúchil (2 variantes)
for k in range(2):
    im=Image.new('RGBA',(32,32),(0,0,0,0)); d=ImageDraw.Draw(im)
    for _ in range(5):
        x=random.randint(4,26); top=random.randint(2,12)
        d.line([(x,31),(x+random.randint(-2,2),top+4)],fill=(60,120,50,255))
        d.ellipse([x-3,top,x+3,top+6],fill=random.choice([(250,150,20),(240,120,10),(255,180,40)])+(255,))
        d.point((x,top+3),fill=(190,80,10,255))
        d.line([(x,top+14),(x+3,top+11)],fill=(80,140,60,255))
    im.save(f'flor_alta_{k}.png')
print('ok')

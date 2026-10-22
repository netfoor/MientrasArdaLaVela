# Mosaico del Recuerdo 1: la fotografía completa (Aurelio y Refugio frente a La Espiga).
from PIL import Image, ImageDraw
import random
random.seed(11)
N=64
S=[(38,26,18),(70,50,34),(104,78,54),(140,108,76),(176,142,104),(206,176,136),(230,208,170),(246,234,206)]
im=Image.new('RGB',(N,N),S[7]); d=ImageDraw.Draw(im)
# marco blanco de la foto con esquina doblada abajo-izquierda
d.rectangle([0,0,63,63],fill=(236,228,210))
d.rectangle([4,4,59,59],fill=S[5])
# cielo con degradado
for y in range(4,24):
    d.line([(4,y),(59,y)],fill=S[6] if y<14 else S[5])

# fondo del cielo: torre de la iglesia a la izquierda, cable de luz y pájaros
d.rectangle([7,6,12,16],fill=S[4]); d.polygon([(6,6),(9,2),(13,6)],fill=S[3]); d.rectangle([9,8,10,11],fill=S[2])
d.line([(9,1),(9,3)],fill=S[2]); d.line([(8,2),(10,2)],fill=S[2])
d.line([(4,9),(30,12),(59,8)],fill=S[2])
for (x,y) in [(22,6),(26,5),(38,7)]:
    d.point((x,y),fill=S[1]); d.point((x-1,y-1),fill=S[1]); d.point((x+1,y-1),fill=S[1])
# rama de árbol arriba a la derecha
d.line([(59,4),(48,10)],fill=S[1]); d.line([(53,7),(52,4)],fill=S[1])
for (x,y) in [(50,9),(52,3),(55,6),(57,4),(47,11)]: d.ellipse([x-1,y-1,x+1,y+1],fill=S[2])

# fachada de la panadería
d.rectangle([4,16,59,52],fill=S[4])
for y in range(18,52,4): d.line([(4,y),(59,y)],fill=S[3])       # hiladas de adobe
for x0 in range(6,59,8):
    for y in range(18,52,8): d.line([(x0,y),(x0,y+4)],fill=S[3])
# letrero LA ESPIGA
d.rectangle([10,18,53,24],fill=S[1]); 
font={'L': ['100', '100', '100', '100', '111'], 'A': ['010', '101', '111', '101', '101'], 'E': ['111', '100', '110', '100', '111'], 'S': ['111', '100', '111', '001', '111'], 'P': ['110', '101', '110', '100', '100'], 'I': ['111', '010', '010', '010', '111'], 'G': ['111', '100', '101', '101', '111']}
x=12
for ch in "LA ESPIGA":
    if ch==' ': x+=2; continue
    for yy,row in enumerate(font[ch]):
        for xx,b in enumerate(row):
            if b=='1': d.point((x+xx,19+yy),fill=S[6])
    x+=4
# puerta a la izquierda y ventana con panes a la derecha
d.rectangle([7,30,15,52],fill=S[1]); d.rectangle([9,32,13,40],fill=S[2]); d.point((14,42),fill=S[6])
d.rectangle([42,29,56,40],fill=S[0]); d.rectangle([43,30,55,39],fill=S[2])
for x in (45,49,53):
    d.ellipse([x-2,35,x+1,38],fill=S[5]); d.point((x-1,36),fill=S[6])
d.line([(49,30),(49,39)],fill=S[1]); d.line([(43,34),(55,34)],fill=S[1])
# banqueta
d.rectangle([4,52,59,59],fill=S[3]); d.line([(4,52),(59,52)],fill=S[2])
# Aurelio (20), más bajo, delantal claro, a la izquierda
def persona(cx,top,alto,delantal,sombrero=False):
    d.ellipse([cx-3,top,cx+2,top+5],fill=S[3])           # cabeza
    d.rectangle([cx-3,top,cx+2,top+1],fill=S[0])         # pelo
    if sombrero:
        d.rectangle([cx-5,top-1,cx+4,top],fill=S[5]); d.rectangle([cx-3,top-3,cx+2,top-1],fill=S[5])
    d.rectangle([cx-4,top+6,cx+3,top+alto-8],fill=S[1])  # torso
    if delantal: d.rectangle([cx-3,top+9,cx+2,top+alto-6],fill=S[6])
    d.rectangle([cx-3,top+alto-8,cx-1,top+alto],fill=S[0]); d.rectangle([cx,top+alto-8,cx+2,top+alto],fill=S[0])
persona(24,31,22,True)
# Refugio (23), más alto, con sombrero de palma, a la derecha, brazo sobre Aurelio
persona(33,27,26,False,sombrero=True)
d.line([(29,34),(25,36)],fill=S[1],width=2)
# canica en la mano de Refugio (brillo)
d.point((37,41),fill=S[7]); d.point((36,41),fill=S[6])
# desgaste: manchas y esquina doblada
for _ in range(40):
    x=random.randint(4,59); y=random.randint(4,59)
    im.putpixel((x,y), tuple(min(255,c+22) for c in im.getpixel((x,y))))
d.polygon([(0,63),(0,52),(11,63)],fill=(200,190,170)); d.line([(0,52),(11,63)],fill=(150,140,120))
# fecha escrita a mano en el borde inferior derecho
for i,x in enumerate(range(44,60,3)): d.line([(x,61),(x+1,60)],fill=(90,70,60))
im=im.resize((256,256),Image.NEAREST); im.save('mosaico_R1.png')
im.resize((512,512),Image.NEAREST).save('prev_r1.png')

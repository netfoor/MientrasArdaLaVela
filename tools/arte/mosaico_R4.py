# Mosaico final (Recuerdo 4): los dos hermanos de niños con la vela, entre el cempasúchil al atardecer.
from PIL import Image, ImageDraw
import random
random.seed(21)
N=64
im=Image.new('RGB',(N,N)); d=ImageDraw.Draw(im)
# cielo de atardecer en franjas
cielo=[(52,40,86),(96,52,100),(170,70,90),(226,110,70),(246,160,80),(252,200,110)]
for y in range(0,34):
    d.line([(0,y),(63,y)],fill=cielo[min(5,y//6)])
# sol bajando a la izquierda
d.ellipse([6,24,20,38],fill=(255,226,150))
# nubes finas
for (x,y,l) in [(30,8,16),(40,14,12),(8,12,10)]: d.line([(x,y),(x+l,y)],fill=(200,120,140))
# pirul a la derecha en silueta
d.rectangle([52,18,55,40],fill=(50,30,30)); d.ellipse([42,6,64,26],fill=(40,34,50))
for x in range(44,64,3): d.line([(x,22),(x,28+random.randint(0,4))],fill=(40,34,50))
# campo de cempasúchil
for y in range(34,64):
    d.line([(0,y),(63,y)],fill=(70,90,40) if y<40 else (60,80,36))
for _ in range(420):
    x=random.randint(0,63); y=random.randint(33,63)
    c=random.choice([(250,150,20),(240,120,10),(255,180,40),(230,100,20)])
    d.point((x,y),fill=c)
    if y>44 and random.random()<0.3: d.point((x+1,y),fill=c)

# primera estrella y copa del pirul con borde irregular (arriba a la derecha)
d.point((60,2),fill=(255,250,220)); d.point((59,2),fill=(200,190,170)); d.point((60,1),fill=(200,190,170))
for x in range(44,64,2): d.point((x,5+(x%5)),fill=(52,40,86))
d.ellipse([54,8,58,11],fill=(70,58,80)); d.point((49,12),fill=(214,90,110)); d.point((57,16),fill=(214,90,110))
# flores grandes al frente con tallo (dan el "abajo" a las piezas de la última fila)
for (x,y) in [(4,54),(14,58),(26,55),(38,59),(50,54),(60,58)]:
    d.line([(x,y+2),(x,63)],fill=(50,110,40)); d.line([(x,y+5),(x+2,y+3)],fill=(70,130,50))
    d.ellipse([x-2,y-2,x+2,y+2],fill=(250,150,20)); d.point((x,y),fill=(170,70,10))

# los dos niños (siluetas contra el sol, iluminadas por la vela)
def nino(cx,top,alto,sombrero):
    d.ellipse([cx-3,top,cx+2,top+5],fill=(110,70,50))
    d.rectangle([cx-3,top,cx+2,top+1],fill=(30,20,18))
    if sombrero: d.rectangle([cx-5,top-1,cx+4,top],fill=(214,184,110)); d.rectangle([cx-3,top-3,cx+2,top-1],fill=(214,184,110))
    d.rectangle([cx-4,top+6,cx+3,top+alto-7],fill=(236,228,210))
    d.rectangle([cx-3,top+alto-7,cx+2,top+alto],fill=(60,50,80))
nino(27,30,20,False)     # Aurelio, 9 años
nino(36,26,24,True)      # Refugio, 12 años, con el sombrero de palma
# la vela en la mano de Refugio, con su halo
for r,c in [(8,(120,70,50)),(5,(200,120,60)),(3,(255,200,110))]:
    d.ellipse([41-r,34-r,41+r,34+r],outline=c)
d.rectangle([40,34,42,40],fill=(244,238,220)); d.polygon([(41,29),(43,33),(41,35),(39,33)],fill=(255,210,90))
# la canica azul en la mano de Aurelio
d.ellipse([22,40,25,43],fill=(60,120,230)); d.point((23,41),fill=(220,235,255))
# pájaros lejanos arriba a la izquierda
for (x,y) in [(18,6),(24,4)]: d.point((x,y),fill=(30,20,40)); d.point((x-1,y-1),fill=(30,20,40)); d.point((x+1,y-1),fill=(30,20,40))
im=im.resize((256,256),Image.NEAREST); im.save('mosaico_R4.png'); im.resize((512,512),Image.NEAREST).save('prev_r4.png')

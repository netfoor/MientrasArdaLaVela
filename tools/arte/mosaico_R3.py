# Mosaico del Recuerdo 3: la canica azul de Refugio entre la ceniza del obrador.
from PIL import Image, ImageDraw
import random, math
random.seed(9)
N=64
im=Image.new('RGB',(N,N)); d=ImageDraw.Draw(im)
# piso de tablas quemadas (horizontal) con juntas desplazadas
for y in range(N):
    for x in range(N):
        tabla=y//8; v=48+(tabla%2)*8+random.randint(-4,4)
        im.putpixel((x,y),(v+18,v+6,v-6))
for y in range(0,N,8): d.line([(0,y),(63,y)],fill=(28,20,16))
for y in range(0,N,8):
    off=(y//8)*13%32
    for x in range(off,N,32): d.line([(x,y),(x,y+7)],fill=(28,20,16))
# chamuscado (mancha negra) arriba a la izquierda
for _ in range(500):
    a=random.random()*6.28; r=abs(random.gauss(0,9)); x=int(12+math.cos(a)*r*1.3); y=int(10+math.sin(a)*r)
    if 0<=x<64 and 0<=y<64: im.putpixel((x,y),(22,16,14))
# ceniza gris esparcida
for _ in range(160):
    x=random.randint(0,63); y=random.randint(0,63); g=random.randint(110,170); im.putpixel((x,y),(g,g,g-6))
# brasa naranja abajo a la derecha
for (x,y) in [(54,52),(56,54),(52,55),(57,51),(55,57)]:
    d.rectangle([x,y,x+1,y+1],fill=(240,110,30)); d.point((x,y),fill=(255,200,90))
# ceniza en montoncito a la derecha
d.ellipse([46,50,62,60],fill=(120,116,110)); d.ellipse([49,52,59,58],fill=(150,146,140))
for (x,y) in [(54,52),(56,54),(52,55)]: d.rectangle([x,y,x+1,y],fill=(240,110,30))
# cerillo quemado abajo a la izquierda
d.line([(6,56),(18,50)],fill=(70,48,34)); d.ellipse([17,48,20,51],fill=(20,14,12))

# pedazo de papel quemado con renglones (abajo a la izquierda, sobre el cerillo)
d.polygon([(2,40),(15,38),(17,47),(4,48)],fill=(214,200,160)); d.line([(15,38),(17,47)],fill=(40,28,20))
for yy in (41,43,45): d.line([(4,yy),(13,yy-1)],fill=(90,80,70))
d.polygon([(15,38),(17,47),(18,40)],fill=(30,20,16))
# llave de hierro a la izquierda (fila 2)
d.ellipse([3,20,9,26],outline=(150,150,160)); d.line([(9,23),(18,23)],fill=(150,150,160)); d.line([(16,23),(16,26)],fill=(150,150,160))
# clavos en las tablas
for (x,y) in [(22,2),(46,2),(30,58),(40,42),(60,26),(4,58)]: d.rectangle([x,y,x+1,y+1],fill=(170,170,176))
# gota de cera blanca arriba en medio
d.ellipse([36,4,41,8],fill=(236,230,210)); d.line([(38,8),(38,12)],fill=(236,230,210))
# nudo de la madera abajo en medio
d.ellipse([24,52,31,57],fill=(40,28,20)); d.ellipse([26,53,29,56],fill=(70,50,36))
# huella de zapato en la ceniza, arriba a la derecha
d.ellipse([52,26,59,33],fill=(96,92,88)); d.ellipse([53,35,58,40],fill=(96,92,88))

# sombra de la canica
d.ellipse([22,30,48,42],fill=(26,20,18))
# canica azul (esfera con veta)
cx,cy,r=33,28,12
for y in range(cy-r,cy+r+1):
    for x in range(cx-r,cx+r+1):
        dx,dy=x-cx,y-cy
        if dx*dx+dy*dy<=r*r:
            luz=max(0,1-((dx+5)**2+(dy+5)**2)/(2.2*r*r))
            b=int(120+120*luz); g=int(60+90*luz); rr=int(20+60*luz)
            im.putpixel((x,y),(rr,g,min(255,b)))
# veta blanca en espiral
for t in range(0,40):
    ang=t*0.28; rad=2+t*0.22
    x=int(cx+math.cos(ang)*rad); y=int(cy+math.sin(ang)*rad*0.7)
    if (x-cx)**2+(y-cy)**2<r*r-4: d.point((x,y),fill=(230,240,255))
# brillos
d.ellipse([cx-7,cy-8,cx-3,cy-5],fill=(255,255,255)); d.point((cx+6,cy+6),fill=(160,200,255))
# hilo rojo atado (de Refugio) saliendo arriba a la derecha
d.line([(42,20),(50,12),(58,10),(63,4)],fill=(180,40,40))
im=im.resize((256,256),Image.NEAREST); im.save('mosaico_R3.png'); im.resize((512,512),Image.NEAREST).save('prev_r3.png')

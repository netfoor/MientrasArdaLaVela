# Mosaico del Recuerdo 2: la caja de cerillos de La Espiga con una esquina quemada.
from PIL import Image, ImageDraw
import random
random.seed(5)
N=64
im=Image.new('RGB',(N,N)); d=ImageDraw.Draw(im)
# mesa de madera oscura con vetas diagonales
for y in range(N):
    for x in range(N):
        v=58+int(10*((x+y*2)%9<2))+random.randint(-3,3)
        im.putpixel((x,y),(v+24,v,v-18))
for k in range(-20,80,11): d.line([(k,0),(k-18,63)],fill=(66,44,30))
# mantel de encaje en la esquina inferior derecha
d.polygon([(40,63),(63,40),(63,63)],fill=(236,228,214))
for i in range(0,26,4): d.point((63-i,40+i),fill=(200,190,176)); d.point((61-i,42+i),fill=(250,246,236))
# sombra de la caja
d.polygon([(14,20),(48,14),(54,44),(20,50)],fill=(40,26,18))
# caja (inclinada) cuerpo
caja=[(11,17),(45,11),(51,41),(17,47)]
d.polygon(caja,fill=(214,176,86))
# etiqueta roja con marco crema
d.polygon([(15,20),(43,15),(48,38),(20,43)],fill=(178,44,40))
d.polygon([(17,22),(42,17),(46,36),(21,41)],outline=(240,226,190))
# espiga dorada en diagonal
for i in range(9):
    x=22+i*2; y=36-i*2
    d.point((x,y),fill=(250,210,90)); d.point((x+1,y),fill=(250,210,90)); d.point((x-1,y-1),fill=(255,230,130))
d.line([(20,39),(22,36)],fill=(200,150,60))
# letras "LA ESPIGA" (muy pequeñas, ilegible a propósito: rayitas claras)
for i,x in enumerate(range(24,41,2)):
    d.point((x,21-(x-24)//5),fill=(250,240,210))
# esquina quemada (arriba a la derecha)
d.polygon([(36,12),(45,11),(47,20),(41,19),(38,15)],fill=(30,20,16))
d.line([(36,12),(38,15),(41,19),(47,20)],fill=(120,60,24))
for (x,y) in [(39,16),(42,18),(44,14)]: d.point((x,y),fill=(230,120,40))
# ceniza/humo pequeño arriba
for (x,y) in [(47,8),(49,6),(48,4),(51,3)]: d.point((x,y),fill=(150,140,130))
# cerillos derramados abajo a la izquierda
for (x0,y0,x1,y1) in [(4,56,16,50),(6,60,19,57),(2,50,10,44),(12,62,24,60)]:
    d.line([(x0,y0),(x1,y1)],fill=(236,210,160))
    d.ellipse([x1-1,y1-1,x1+1,y1+1],fill=(190,40,30))
# un cerillo quemado junto a la caja arriba a la izquierda
d.line([(3,10),(12,5)],fill=(60,40,30)); d.point((12,5),fill=(20,15,12)); d.point((13,4),fill=(20,15,12))

# pedazo de telegrama arriba a la izquierda (papel amarillento con renglones)
d.polygon([(1,1),(15,2),(13,12),(2,10)],fill=(226,214,170)); d.line([(15,2),(13,12)],fill=(170,150,110))
for yy in (4,6,8): d.line([(3,yy),(12,yy+1)],fill=(90,80,70))
d.line([(3,10),(8,11)],fill=(150,40,40))
# copa del brindis arriba a la derecha
d.polygon([(54,2),(62,2),(60,8),(56,8)],fill=(210,220,230)); d.rectangle([57,3,59,6],fill=(160,40,60))
d.line([(58,8),(58,14)],fill=(200,210,220)); d.line([(55,15),(61,15)],fill=(200,210,220))
# confeti
for (x,y,c) in [(52,22,(232,74,120)),(58,28,(80,180,110)),(60,20,(70,140,230)),(55,33,(250,160,40)),(3,30,(250,160,40)),(6,24,(232,74,120))]:
    d.rectangle([x,y,x+1,y],fill=c)
# pétalo de cempasúchil a la izquierda
d.ellipse([2,36,7,40],fill=(245,140,20)); d.point((4,38),fill=(190,80,10))

im=im.resize((256,256),Image.NEAREST); im.save('mosaico_R2.png')
im.resize((512,512),Image.NEAREST).save('prev_r2.png')

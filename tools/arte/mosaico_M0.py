from PIL import Image, ImageDraw
import random, math
random.seed(7)
N=64
im=Image.new('RGBA',(N,N),(0,0,0,255))
px=im.load()
# fondo: pared de adobe nocturna con degradado
for y in range(N):
    for x in range(N):
        t=y/N
        base=(int(58+20*t),int(34+12*t),int(52+6*t))
        if random.random()<0.08: base=tuple(max(0,c-8) for c in base)
        px[x,y]=base+(255,)
d=ImageDraw.Draw(im)
# papel picado arriba (asimétrico: colores distintos)
cols=[(232,74,120),(250,170,40),(90,190,120),(70,140,230),(170,90,210)]
d.line([(0,3),(63,5)],fill=(40,30,30))
for i in range(5):
    x0=i*13; c=cols[i]
    yA=3+i*2//5; 
    d.polygon([(x0+1,yA+1),(x0+12,yA+1),(x0+12,yA+9),(x0+9,yA+11),(x0+6,yA+9),(x0+3,yA+11),(x0+1,yA+9)],fill=c)
    d.rectangle([x0+4,yA+4,x0+5,yA+5],fill=(58,34,52)); d.rectangle([x0+8,yA+4,x0+9,yA+5],fill=(58,34,52))

# retrato enmarcado en la pared (izquierda)
d.rectangle([4,16,17,31],fill=(92,58,30)); d.rectangle([6,18,15,29],fill=(214,200,170))
d.ellipse([8,19,13,24],fill=(110,90,80)); d.rectangle([8,24,13,29],fill=(110,90,80))
d.point((17,15),fill=(40,30,20)); d.line([(10,14),(17,15)],fill=(40,30,20)); d.line([(10,14),(4,16)],fill=(40,30,20))
# guirnalda de cempasúchil que cae en diagonal hacia la derecha
for i in range(0,26):
    x=20+i; y=int(15+ (i/25.0)**1.6*12)
    d.point((x,y),fill=(250,150,20) if i%2==0 else (230,110,10))
    if i%3==0: d.point((x,y+1),fill=(255,190,50))


# flor grande de cempasúchil al inicio de la guirnalda
for (x,y) in [(21,15),(23,15),(22,14),(22,16),(20,16),(24,17),(21,17)]:
    d.point((x,y),fill=(250,150,20))
d.point((22,15),fill=(200,80,10)); d.line([(22,17),(24,21)],fill=(60,120,50))
d.point((25,20),fill=(80,150,60)); d.point((26,22),fill=(80,150,60))


# milagrito (corazón de hojalata) y calaverita colgando de la guirnalda
d.line([(28,17),(28,24)],fill=(40,30,20))
d.polygon([(26,25),(28,24),(30,25),(30,27),(28,30),(26,27)],fill=(200,200,210)); d.point((27,26),fill=(250,250,255))
d.line([(40,20),(40,26)],fill=(40,30,20))
d.ellipse([37,26,43,31],fill=(250,246,240)); d.point((39,28),fill=(40,30,40)); d.point((41,28),fill=(40,30,40)); d.point((40,27),fill=(232,74,120))

# mantel / mesa
d.rectangle([0,44,63,63],fill=(236,226,206))
for x in range(0,64,4): d.line([(x,44),(x,63)],fill=(222,208,186))
d.line([(0,44),(63,44)],fill=(200,70,60)); d.line([(0,46),(63,46)],fill=(200,70,60))
# charola
d.ellipse([6,40,50,54],fill=(150,110,70)); d.ellipse([8,41,48,52],fill=(176,134,86))
def pan(cx,cy,r,shade=0):
    d.ellipse([cx-r,cy-r+2,cx+r,cy+r-1],fill=(150-shade,78-shade,30))
    d.ellipse([cx-r+1,cy-r+2,cx+r-1,cy+r-3],fill=(196-shade,112-shade,44))
    # huesitos en cruz
    d.line([(cx-r+2,cy),(cx+r-2,cy)],fill=(128,64,24),width=2)
    d.line([(cx,cy-r+3),(cx,cy+r-3)],fill=(128,64,24),width=2)
    for k in (-r+3,-2,2,r-3):
        d.point((cx+k,cy-1),fill=(150,80,30)); d.point((cx,cy+k),fill=(150,80,30))
    d.ellipse([cx-2,cy-2,cx+2,cy+2],fill=(120,58,20))
    for _ in range(26):
        a=random.random()*6.28; rr=random.random()*(r-2)
        x=int(cx+math.cos(a)*rr); y=int(cy+math.sin(a)*rr*0.8)
        d.point((x,y),fill=(250,240,220))
pan(19,42,10)
pan(36,43,9,shade=10)
# vela grande a la derecha, con llama y resplandor
for rr,c in [(12,(90,54,60)),(9,(120,74,62)),(6,(160,100,60))]:
    d.ellipse([54-rr,20-rr,54+rr,20+rr],fill=c)
d.rectangle([51,24,57,46],fill=(244,236,214)); d.rectangle([56,24,57,46],fill=(214,202,180))
d.line([(54,21),(54,24)],fill=(40,30,20))
d.polygon([(54,14),(56,19),(54,22),(52,19)],fill=(255,200,60)); d.point((54,19),fill=(255,250,210))
# petalos de cempasúchil sobre el mantel (izquierda y abajo)
for _ in range(30):
    x=random.randint(0,63); y=random.randint(48,63)
    if 8<x<50 and y<54: continue
    d.point((x,y),fill=random.choice([(250,150,20),(240,120,10),(255,180,40)]))

# cempasúchil suelto y veladora pequeña, abajo a la derecha
d.ellipse([50,53,57,59],fill=(245,140,20)); d.ellipse([52,54,55,57],fill=(255,190,40)); d.point((53,55),fill=(190,80,10))
d.line([(57,59),(62,61)],fill=(60,120,50))
d.rectangle([58,50,62,56],fill=(200,60,70)); d.point((60,49),fill=(255,210,80))

# calaverita pequeña abajo a la izquierda
d.ellipse([3,52,11,60],fill=(250,246,240)); d.rectangle([5,58,9,61],fill=(250,246,240))
d.point((5,56),fill=(40,30,40)); d.point((9,56),fill=(40,30,40)); d.point((7,58),fill=(40,30,40))
d.point((6,54),fill=(232,74,120)); d.point((8,54),fill=(70,140,230))
im=im.resize((256,256),Image.NEAREST)
im.save('mosaico_M0.png')
prev=im.resize((512,512),Image.NEAREST); prev.save('prev.png')

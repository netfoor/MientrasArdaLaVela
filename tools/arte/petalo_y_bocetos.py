# Arte propio (Fortino Romero Mantilla), F23/F24:
#  · petalo.png: flor de cempasúchil coleccionable (16 px dentro de un cuadro de 32).
#  · Galería de bocetos: versión "a lápiz sobre papel" de cada mosaico (Art/Resources/Galeria).
# Uso: python3 petalo_y_bocetos.py <carpeta_mosaicos> <salida_objetos> <salida_galeria>
import sys, math, random
from PIL import Image, ImageFilter, ImageOps, ImageChops, ImageDraw
random.seed(7)

def petalo(ruta):
    im=Image.new('RGBA',(32,32),(0,0,0,0)); px=im.load()
    cx,cy=16,15
    OSC=(196,88,20,255); MED=(240,140,30,255); CLA=(255,190,60,255); CEN=(150,70,20,255)
    for y in range(32):
        for x in range(32):
            dx,dy=x-cx,y-cy; r=math.hypot(dx,dy); a=math.atan2(dy,dx)
            borde=6.2+0.9*math.sin(a*8)          # contorno festoneado de la flor
            if r<=borde:
                if r<1.6: c=CEN
                elif r>borde-1.1: c=OSC
                elif (math.sin(a*8+r)>0.2): c=CLA
                else: c=MED
                px[x,y]=c
    # tallito y hoja
    for y in range(22,27): px[16,y]=(70,120,50,255)
    px[17,24]=px[18,23]=(90,150,60,255)
    im.save(ruta)

def boceto(src,dst):
    im=Image.open(src).convert('L').resize((512,512),Image.LANCZOS)
    bordes=im.filter(ImageFilter.FIND_EDGES)
    bordes=ImageOps.autocontrast(bordes)
    lapiz=ImageOps.invert(bordes.point(lambda v:min(255,v*4))).filter(ImageFilter.MinFilter(3))
    sombra=im.filter(ImageFilter.GaussianBlur(3)).point(lambda v:int(130+v*0.49))
    mezcla=ImageChops.multiply(lapiz,sombra)
    papel=Image.new('RGB',mezcla.size,(236,224,196))
    pp=papel.load()
    for y in range(0,512,1):
        for x in range(0,512,1):
            n=random.randint(-6,6); r,g,b=pp[x,y]; pp[x,y]=(r+n,g+n,b+n)
    tinta=Image.new('RGB',mezcla.size,(70,52,40))
    salida=Image.composite(papel,tinta,mezcla)
    d=ImageDraw.Draw(salida); d.rectangle([6,6,505,505],outline=(150,120,90),width=2)
    salida.save(dst)

if __name__=='__main__':
    mos,obj,gal=sys.argv[1:4]
    petalo(f'{obj}/petalo.png')
    for k in ['M0','R1','R2','R3','R4']: boceto(f'{mos}/mosaico_{k}.png',f'{gal}/boceto_{k}.png')

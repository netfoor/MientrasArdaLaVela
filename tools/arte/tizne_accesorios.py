# Variantes de Tizne con accesorios (F23): sombrero de palma y collar alternativo.
# Parte de la variante negra de tizne_dog.png ([LPC] Cats and Dogs, bluecarrot16) y le
# dibuja encima los accesorios, cuadro por cuadro. Arte de accesorios: Fortino Romero Mantilla.
# Uso: python3 tizne_accesorios.py <tizne_dog.png> <carpeta_salida>
import sys
from PIL import Image
L=32; COL=12
FILAS=[('right',0),('up',1),('down',2),('left',3)]
PAJA=(222,186,110,255); PAJA_OSC=(170,130,70,255); CINTA=(170,40,60,255)
MAG=(214,58,140,255); TUR=(40,170,180,255); AMA=(250,200,60,255)

def opaco(px,x,y): return 0<=x<L and 0<=y<L and px[x,y][3]>128
def naranja(c): r,g,b,a=c; return a>0 and r>150 and r>g+30 and b<150

def analizar(fr):
    px=fr.load()
    nar=[(x,y) for x in range(L) for y in range(L) if naranja(px[x,y])]
    ops=[(x,y) for x in range(L) for y in range(L) if px[x,y][3]>128]
    return px,nar,ops

def sombrero(fr,dir):
    px,nar,ops=analizar(fr)
    if dir=='up':
        top=min(y for x,y in ops); xs=[x for x,y in ops if y<=top+1]; cx=round(sum(xs)/len(xs)); base=top+2
    elif dir=='down':
        cx=round(sum(x for x,y in nar)/len(nar)); base=min(y for x,y in nar)-3
    else:
        cx=round(sum(x for x,y in nar)/len(nar)); base=min(y for x,y in nar)+1
    ancho=5 if dir in('up','down') else 5
    # ala
    for x in range(cx-ancho,cx+ancho+1): fr.putpixel((x,base),PAJA_OSC if x in(cx-ancho,cx+ancho) else PAJA)
    for x in range(cx-ancho+1,cx+ancho): fr.putpixel((x,base-1),PAJA)
    # copa con cinta
    for x in range(cx-2,cx+3):
        fr.putpixel((x,base-2),CINTA)
        fr.putpixel((x,base-3),PAJA)
    for x in range(cx-1,cx+2): fr.putpixel((x,base-4),PAJA_OSC)

def collar(fr,dir):
    px,nar,ops=analizar(fr)
    if dir=='down':
        cx=round(sum(x for x,y in nar)/len(nar)); y0=max(y for x,y in nar)+3; xr=range(cx-4,cx+5)
    elif dir=='up':
        top=min(y for x,y in ops); y0=top+6; xs=[x for x,y in ops if y==y0]; xr=range(min(xs),max(xs)+1)
    else:
        cx=round(sum(x for x,y in nar)/len(nar)); y0=max(y for x,y in nar)+4
        xr=range(cx-3,cx+2) if dir=='right' else range(cx-1,cx+4)
    colores=[MAG,AMA,TUR]
    for i,x in enumerate(xr):
        for dy in (0,1):
            if opaco(px,x,y0+dy): fr.putpixel((x,y0+dy),colores[(i+dy)%3])

def main(src,out):
    im=Image.open(src).convert('RGBA')
    for nombre,(s,c) in {'collar':(False,True),'sombrero':(True,False),'ambos':(True,True)}.items():
        hoja=Image.new('RGBA',(3*L,4*L),(0,0,0,0))
        for dir,fila in FILAS:
            for k in range(3):
                fr=im.crop(((COL+k)*L,fila*L,(COL+k+1)*L,(fila+1)*L))
                if c: collar(fr,dir)
                if s: sombrero(fr,dir)
                hoja.paste(fr,(k*L,fila*L))
        hoja.save(f'{out}/tizne_{nombre}.png')
if __name__=='__main__': main(sys.argv[1],sys.argv[2])

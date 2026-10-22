# Música original de "Mientras arda la vela" (M01–M08), sintetizada con numpy.
# Un solo tema (leitmotiv) en re menor, variado por instrumentación para cada recuerdo,
# con giros mexicanos: vals en 3/4, terceras paralelas, menor armónica y son en 6/8.
# Autor: Fortino Romero Mantilla. Uso libre dentro del proyecto.
import numpy as np, subprocess, os
SR=32000
rng=np.random.default_rng(7)

def midi(n): return 440.0*2**((n-69)/12)
NOTAS={'C':0,'C#':1,'Db':1,'D':2,'D#':3,'Eb':3,'E':4,'F':5,'F#':6,'Gb':6,'G':7,'G#':8,'Ab':8,'A':9,'A#':10,'Bb':10,'B':11}
def n(s):  # "D4" -> midi
    nombre=s[:-1]; oct_=int(s[-1]); return 12*(oct_+1)+NOTAS[nombre]

# ---------------------------------------------------------------- instrumentos
def guitarra(f, dur, brillo=0.5, vol=1.0):
    N=int(SR*dur); per=max(2,int(SR/f))
    buf=rng.uniform(-1,1,per)
    # suaviza el ataque según el brillo (púa vs yema)
    for _ in range(int(3*(1-brillo))+1): buf=0.5*(buf+np.roll(buf,1))
    out=np.zeros(N); decay=0.996 - (f/12000)
    b=buf.copy(); i=0
    for k in range(N):
        out[k]=b[i]; nxt=(i+1)%per
        b[i]=decay*0.5*(b[i]+b[nxt]); i=nxt
    return out*vol

def guitarra_rapida(f, dur, vol=1.0, amort=1.0):
    # Karplus-Strong vectorizado por bloques (mucho más rápido)
    N=int(SR*dur); per=max(2,int(round(SR/f)))
    b=rng.uniform(-1,1,per)
    for _ in range(4): b=0.5*(b+np.roll(b,1))   # púa suave: menos ruido en el ataque
    b-=b.mean()
    reps=N//per+2; out=np.empty(reps*per)
    d=(0.996-f/15000)**amort
    for r in range(reps):
        out[r*per:(r+1)*per]=b
        b=d*0.5*(b+np.roll(b,-1))
    return out[:N]*vol

def env_adsr(N, a, d, s, r):
    t=np.arange(N)/SR; e=np.ones(N)*s
    A=int(a*SR); D=int(d*SR); R=int(r*SR)
    if A>0: e[:A]=np.linspace(0,1,A)
    if D>0: e[A:A+D]=np.linspace(1,s,len(e[A:A+D]))
    if R>0 and R<N: e[-R:]*=np.linspace(1,0,R)
    return e

def marimba(f, dur, vol=1.0):
    N=int(SR*max(dur,0.6)); t=np.arange(N)/SR
    s=np.sin(2*np.pi*f*t)*np.exp(-t/0.55)+0.35*np.sin(2*np.pi*f*3.93*t)*np.exp(-t/0.09)+0.1*np.sin(2*np.pi*f*9.2*t)*np.exp(-t/0.02)
    return s*np.minimum(1,t/0.002)*vol

def piano(f, dur, vol=1.0):
    N=int(SR*(dur+0.6)); t=np.arange(N)/SR; s=np.zeros(N)
    for k,amp in enumerate([1,0.55,0.3,0.18,0.1,0.06],1):
        fk=f*k*np.sqrt(1+0.0004*k*k)
        s+=amp*np.sin(2*np.pi*fk*t)*np.exp(-t*(0.9+0.5*k)*(f/260)**0.4)
    s*=np.minimum(1,t/0.004)
    cola=np.ones(N); R=int(0.25*SR); fin=int(dur*SR)
    cola[fin:]=np.linspace(1,0,N-fin) if N>fin else 1
    return s*cola*vol

def cuerdas(f, dur, vol=1.0, ataque=0.25, vib=5.5):
    N=int(SR*(dur+0.3)); t=np.arange(N)/SR
    fase=2*np.pi*np.cumsum(f*(1+0.006*np.sin(2*np.pi*vib*t)*np.minimum(1,t/0.4)))/SR
    s=sum((1/k)*np.sin(k*fase)*(0.8 if k%2 else 1.0) for k in range(1,11))
    s=s*env_adsr(N, ataque, 0.2, 0.85, 0.3)
    return s*0.35*vol

def cajita(f, dur, vol=1.0):
    N=int(SR*1.4); t=np.arange(N)/SR
    s=np.sin(2*np.pi*f*t)*np.exp(-t/0.5)+0.4*np.sin(2*np.pi*f*4.1*t)*np.exp(-t/0.12)+0.15*np.sin(2*np.pi*f*7.3*t)*np.exp(-t/0.05)
    return s*vol

def coro(f, dur, vol=1.0):
    N=int(SR*(dur+0.5)); t=np.arange(N)/SR; s=np.zeros(N)
    for det in (-0.004,0,0.005):
        ff=f*(1+det)
        for k,amp in [(1,1),(2,0.5),(3,0.35),(4,0.12),(5,0.08)]:
            s+=amp*np.sin(2*np.pi*ff*k*t+rng.uniform(0,6))
    return s*env_adsr(N,0.6,0.3,0.8,0.5)*0.12*vol

def ruido_golpe(dur=0.08, vol=1.0, tono=180):
    N=int(SR*dur); t=np.arange(N)/SR
    return (0.8*np.sin(2*np.pi*tono*t)+0.2*rng.standard_normal(N))*np.exp(-t/0.025)*vol

# ---------------------------------------------------------------- utilidades
class Pista:
    def __init__(self, segundos): self.x=np.zeros(int(SR*segundos)+SR*3)
    def pon(self, t, señal, pan=0):
        i=int(t*SR); L=min(len(señal), len(self.x)-i)
        if L>0: self.x[i:i+L]+=señal[:L]

def reverb(x, sala=0.35):
    out=x.copy()
    for d,g in [(0.029,0.5),(0.037,0.45),(0.041,0.42),(0.053,0.38),(0.079,0.3),(0.113,0.25)]:
        i=int(d*SR); y=np.zeros_like(x); y[i:]=x[:-i]*g
        for _ in range(3): y[i:]+=y[:-i]*g*0.6
        out+=y*sala
    return out

def guardar(nombre, x, dur, vol=0.8):
    from scipy.signal import butter, sosfilt
    x=sosfilt(butter(4, 5500, 'low', fs=SR, output='sos'), x)   # quita el siseo de los ataques
    x=sosfilt(butter(2, 45, 'high', fs=SR, output='sos'), x)
    x=x[:int(SR*dur)]
    # fundido corto en las orillas para que el bucle no chasquee
    F=int(0.03*SR); x[:F]*=np.linspace(0,1,F); x[-F:]*=np.linspace(1,0,F)
    x=np.tanh(x/np.max(np.abs(x))*1.2)/np.tanh(1.2)*vol
    wav=f'{nombre}.wav'
    import wave
    with wave.open(wav,'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x*32767).astype(np.int16).tobytes())
    subprocess.run(['ffmpeg','-y','-loglevel','error','-i',wav,'-c:a','libvorbis','-q:a','4',f'{nombre}.ogg'],check=True)
    os.remove(wav); print(nombre, round(dur,1),'s')

# ---------------------------------------------------------------- armonía y tema
# Progresión en re menor (compases de 3/4): i  iv  V7  i | VI  III  iv  V7 ...
ACORDES={
 'Dm':['D3','F3','A3'], 'Gm':['G2','D3','G3','Bb3'], 'A7':['A2','E3','G3','C#4'], 'Bb':['Bb2','F3','Bb3','D4'],
 'F':['F2','C3','F3','A3'], 'C':['C3','G3','C4','E4'], 'D':['D3','F#3','A3'], 'G':['G2','D3','G3','B3'], 'A':['A2','E3','A3','C#4'],
 'Em':['E2','B2','E3','G3'], 'Bm':['B2','F#3','B3','D4'], 'E7':['E2','B2','D3','G#3'],
}
PROG_A=['Dm','Dm','Gm','Gm','A7','A7','Dm','Dm','Bb','Bb','F','F','Gm','A7','Dm','Dm']
# Tema: (nota, duración en tiempos). 3 tiempos por compás; 16 compases.
TEMA=[('A4',1),('D5',1.5),('E5',0.5), ('F5',2),('E5',1), ('D5',1),('Bb4',1.5),('A4',0.5), ('G4',3),
      ('G4',1),('C#5',1.5),('D5',0.5), ('E5',2),('A4',1), ('F5',1.5),('E5',0.5),('D5',1), ('D5',3),
      ('D5',1),('F5',1.5),('D5',0.5), ('Bb4',2),('D5',1), ('C5',1),('A4',1.5),('F4',0.5), ('A4',3),
      ('Bb4',1),('D5',1),('G5',1), ('E5',1.5),('C#5',0.5),('E5',1), ('D5',3), ('R',3)]
ESCALA_RE=[n(x) for x in ['D3','E3','F3','G3','A3','Bb3','C#4','D4','E4','F4','G4','A4','Bb4','C#5','D5','E5','F5','G5','A5','Bb5','C#6','D6']]
def tercera_abajo(nota):  # terceras paralelas a la mexicana, diatónicas en re menor armónica
    m=n(nota); i=min(range(len(ESCALA_RE)), key=lambda k: abs(ESCALA_RE[k]-m))
    return ESCALA_RE[max(0,i-2)]

def transponer(nota, semis):
    return nota if nota=='R' else n(nota)+semis

def tocar_tema(p, t0, beat, instr, vol=1.0, semis=0, terceras=False, octava=0):
    t=t0
    for nota,d in TEMA:
        if nota!='R':
            m=n(nota)+semis+12*octava
            p.pon(t, instr(midi(m), d*beat, vol))
            if terceras: p.pon(t, instr(midi(tercera_abajo(nota)+semis+12*octava), d*beat, vol*0.6))
        t+=d*beat
    return t

def vals_guitarra(p, t0, beat, prog, vol=1.0, semis=0, bajo=True):
    t=t0
    for ac in prog:
        notas=[n(x)+semis for x in ACORDES[ac]]
        if bajo: p.pon(t, guitarra_rapida(midi(notas[0]-12 if notas[0]>n('C3') else notas[0]), beat*3, vol*0.9, amort=0.6))
        for k in (1,2):
            for j,m in enumerate(notas[1:]):
                p.pon(t+k*beat+j*0.012, guitarra_rapida(midi(m), beat*1.2, vol*0.45))
        t+=3*beat
    return t

def arpegio(p, t0, beat, prog, instr, vol=1.0, semis=0, patron=(0,1,2,1,2,1)):
    t=t0; paso=3*beat/len(patron)
    for ac in prog:
        notas=[n(x)+semis for x in ACORDES[ac]]
        for k,idx in enumerate(patron):
            m=notas[min(idx,len(notas)-1)]+ (12 if idx>=len(notas) else 0)
            p.pon(t+k*paso, instr(midi(m), paso*2, vol))
        t+=3*beat
    return t

def colchon(p, t0, beat, prog, vol=1.0, semis=0, instr=None):
    instr=instr or cuerdas; t=t0
    for ac in prog:
        for m in ACORDES[ac][1:]:
            p.pon(t, instr(midi(n(m)+semis), 3*beat, vol*0.4, ataque=0.5) if instr is cuerdas else instr(midi(n(m)+semis),3*beat,vol*0.4))
        t+=3*beat
    return t

# ---------------------------------------------------------------- piezas
def M01():  # Tema principal: guitarra, marimba y cuerdas (menú, créditos)
    beat=60/84; dur=16*3*beat*2; p=Pista(dur)
    t=vals_guitarra(p,0,beat,PROG_A); vals_guitarra(p,t,beat,PROG_A)
    tocar_tema(p,0,beat,marimba,0.7); tocar_tema(p,16*3*beat,beat,marimba,0.7,terceras=True)
    colchon(p,16*3*beat,beat,PROG_A,0.8)
    guardar('M01', reverb(p.x,0.3), dur)

def M02():  # La ofrenda: tranquilo, guitarra sola y marimba lejana
    beat=60/72; dur=16*3*beat; p=Pista(dur)
    arpegio(p,0,beat,PROG_A,lambda f,d,v: guitarra_rapida(f,d*1.5,v),0.6)
    t=0
    for nota,d in TEMA[:9]:
        if nota!='R': p.pon(t+8*3*beat, marimba(midi(n(nota)),d*beat,0.35))
        t+=d*beat
    guardar('M02', reverb(p.x,0.4), dur, 0.7)

def M03():  # La Espiga: vals lento de piano (2015)
    beat=60/76; prog=['Dm','Dm','Gm','Gm','A7','A7','Dm','Dm','Bb','Bb','F','F','Gm','A7','Dm','Dm']
    dur=16*3*beat; p=Pista(dur); t=0
    for ac in prog:
        notas=[n(x) for x in ACORDES[ac]]
        p.pon(t, piano(midi(notas[0]-12),beat*1.5,0.6))
        for k in (1,2):
            for m in notas[1:]: p.pon(t+k*beat, piano(midi(m),beat*0.9,0.28))
        t+=3*beat
    tocar_tema(p,0,beat,piano,0.55,octava=0)
    guardar('M03', reverb(p.x,0.4), dur, 0.75)

def M04():  # Son de boda: 6/8 alegre, jarana, guitarrón y violín (1988), en re mayor
    beat=60/165   # corchea
    prog=['D','D','A','A','A','A','D','D','G','G','D','D','A','A','D','D']
    dur=len(prog)*6*beat*2; p=Pista(dur)
    for vuelta in range(2):
        t=vuelta*len(prog)*6*beat
        for ac in prog:
            notas=[n(x) for x in ACORDES[ac]]
            # sesquiáltera: acentos 1,4 (6/8) alternando con 1,3,5 (3/4)
            for k in range(6):
                acento = k in (0,3)
                for j,m in enumerate(notas[1:]):
                    p.pon(t+k*beat+j*0.008, guitarra_rapida(midi(m+12),beat*1.1,0.35 if acento else 0.2,amort=1.4))
            p.pon(t, guitarra_rapida(midi(notas[0]-12),beat*3,0.9,amort=0.5))
            p.pon(t+3*beat, guitarra_rapida(midi(notas[0]-12+7 if ac!='A' else notas[0]-12+7),beat*3,0.7,amort=0.5))
            p.pon(t, ruido_golpe(0.07,0.25)); p.pon(t+3*beat, ruido_golpe(0.07,0.18))
            t+=6*beat
    # violín: melodía en re mayor sobre la misma armonía (duraciones en corcheas)
    VIOLIN=[('A4',2),('D5',2),('F#5',2), ('A5',4),('F#5',2), ('E5',3),('C#5',3), ('A4',6),
            ('C#5',2),('E5',2),('A5',2), ('G5',4),('E5',2), ('F#5',3),('D5',3), ('D5',6),
            ('B4',2),('D5',2),('G5',2), ('B5',4),('G5',2), ('A5',3),('F#5',3), ('D5',6),
            ('E5',2),('G5',2),('C#5',2), ('E5',3),('A4',3), ('D5',2),('F#5',2),('A5',2), ('D5',6)]
    for vuelta in (0,1):
        t=vuelta*len(prog)*6*beat
        for nota,d in VIOLIN:
            p.pon(t, cuerdas(midi(n(nota)), d*beat*0.92, 0.9, ataque=0.03, vib=6.5)); t+=d*beat
    guardar('M04', reverb(p.x,0.25), dur, 0.8)

def M05():  # Ceniza: cuerdas graves, pulso de latido (1966)
    beat=60/56; dur=16*3*beat; p=Pista(dur)
    prog=['Dm','Dm','Bb','Bb','Gm','Gm','A7','A7','Dm','Dm','Bb','Bb','Gm','A7','Dm','Dm']
    t=0
    for ac in prog:
        notas=[n(x) for x in ACORDES[ac]]
        p.pon(t, cuerdas(midi(notas[0]-12),3*beat,1.0,ataque=0.8))
        p.pon(t, cuerdas(midi(notas[1]-12),3*beat,0.5,ataque=0.9))
        p.pon(t, ruido_golpe(0.15,0.5,55)); p.pon(t+0.28, ruido_golpe(0.15,0.35,55))
        t+=3*beat
    # fragmentos del tema, graves y espaciados
    for i,(nota,d) in enumerate(TEMA[:9]):
        if nota!='R' and i%2==0: p.pon(4*3*beat+i*2*beat, cuerdas(midi(n(nota)-12),2*beat,0.55,ataque=0.4))
    guardar('M05', reverb(p.x,0.5), dur, 0.75)

def M06():  # Cempasúchil: el tema en caja de música (1955)
    beat=60/80; dur=16*3*beat*2; p=Pista(dur)
    tocar_tema(p,0,beat,cajita,0.7,octava=1); tocar_tema(p,16*3*beat,beat,cajita,0.7,octava=1,terceras=True)
    arpegio(p,0,beat,PROG_A*2,lambda f,d,v: cajita(f,d,v),0.25,patron=(0,2))
    guardar('M06', reverb(p.x,0.45), dur, 0.7)

def M07():  # Mosaico: contemplativo, en bucle
    beat=60/60; prog=['Dm','Bb','F','A7','Dm','Bb','Gm','A7']; dur=len(prog)*3*beat; p=Pista(dur)
    colchon(p,0,beat,prog,1.0,instr=coro)
    arpegio(p,0,beat,prog,marimba,0.3,patron=(0,1,2,3))
    guardar('M07', reverb(p.x,0.5), dur, 0.65)

def M08():  # Dos velas: final, el tema en re mayor con coro y cuerdas
    beat=60/70; dur=16*3*beat; p=Pista(dur)
    prog=['D','D','G','G','A','A','D','D','G','G','D','D','Em','A','D','D']
    vals_guitarra(p,0,beat,prog,0.8)
    colchon(p,0,beat,prog,1.0,instr=coro); colchon(p,0,beat,prog,0.8)
    MAYOR=[('A4',1),('D5',1.5),('E5',0.5), ('F#5',2),('E5',1), ('D5',1),('B4',1.5),('A4',0.5), ('G4',3),
           ('G4',1),('C#5',1.5),('D5',0.5), ('E5',2),('A4',1), ('F#5',1.5),('E5',0.5),('D5',1), ('D5',3),
           ('D5',1),('F#5',1.5),('D5',0.5), ('B4',2),('D5',1), ('C#5',1),('A4',1.5),('F#4',0.5), ('A4',3),
           ('B4',1),('D5',1),('G5',1), ('E5',1.5),('C#5',0.5),('E5',1), ('D5',3), ('R',3)]
    t=0
    for nota,d in MAYOR:
        if nota!='R': p.pon(t, cuerdas(midi(n(nota)), d*beat, 0.9, ataque=0.15))
        t+=d*beat
    guardar('M08', reverb(p.x,0.45), dur, 0.8)

if __name__=='__main__':
    import sys
    piezas=sys.argv[1:] or ['M01','M02','M03','M04','M05','M06','M07','M08']
    for pz in piezas: globals()[pz]()

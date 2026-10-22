# /// script
# requires-python = ">=3.10"
# dependencies = ["numpy"]
# ///
# Los cinco efectos que faltaban de la Tabla 27 del Documento de Diseño (F22):
# S01 pasos, S05 texto, S09 ladrido de Tizne, S10 vela y S14 brindis.
# Arte sonoro original del proyecto, como S02-S08, S11, S15 (sonidos.py) y S12, S13, S16.
# Uso: uv run --script tools/arte/sonidos_f22.py
import numpy as np, wave, pathlib
SR=44100
DESTINO=pathlib.Path(__file__).resolve().parents[2]/'Assets/_Proyecto/Audio/Resources/Audio'
rng=np.random.default_rng(7)      # semilla fija: el mismo .wav en cada corrida
def guardar(nombre, x, vol=0.8):
    x=x/np.max(np.abs(x))*vol
    with wave.open(str(DESTINO/nombre),'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x*32767).astype(np.int16).tobytes())
    print(nombre, f'{len(x)/SR:.2f} s')
def t(dur): return np.arange(int(SR*dur))/SR
def ruido(dur): return rng.uniform(-1,1,int(SR*dur))
def pasabajos(x, n):
    return np.convolve(x, np.ones(n)/n, mode='same')
def pasaaltos(x, n):
    return x-pasabajos(x,n)
def mezcla(partes, dur):
    out=np.zeros(int(SR*dur))
    for i0,s in partes:
        i=int(SR*i0); out[i:i+len(s)]+=s[:len(out)-i]
    return out
def reverb(x, d=0.09, g=0.35, veces=5):
    out=x.copy()
    for k in range(1,veces+1):
        i=int(SR*d*k); out[i:]+=x[:len(x)-i]*(g**k)
    return out
def campana(f, dur, r):
    tt=t(dur)
    parc=[(1.0,1.0,1.0),(2.76,0.6,0.5),(5.4,0.35,0.3),(8.9,0.2,0.2)]
    return sum(a*np.sin(2*np.pi*f*k*tt)*np.exp(-tt/(r*d)) for k,a,d in parc)*np.minimum(1,tt/0.001)

# S01 pasos: pisada seca sobre duela. Cuerpo grave + roce de suela; en el juego se
# reproduce con el tono variado ±8 % para que dos pasos seguidos no suenen iguales.
tt=t(0.13)
cuerpo=np.sin(2*np.pi*95*np.exp(-tt*9)*tt*3)*np.exp(-tt/0.035)
roce=pasaaltos(ruido(0.13),12)*np.exp(-tt/0.018)*0.5
guardar('S01.wav', pasabajos(cuerpo+roce,3), 0.5)

# S05 texto: tic cortísimo por letra del diálogo. Tiene que poder sonar 50 veces por
# segundo sin cansar: 18 ms, agudo y muy bajo.
tt=t(0.018)
tic=(np.sin(2*np.pi*1900*tt)*0.6+pasaaltos(ruido(0.018),6))*np.exp(-tt/0.004)
guardar('S05.wav', tic, 0.22)

# S09 ladrido de Tizne: un "guau" corto. Portadora que cae de 520 a 180 Hz (la cuerda
# vocal del perro) con armónicos y un soplo de aire al final.
tt=t(0.22)
f=180+340*np.exp(-tt/0.05)
fase=2*np.pi*np.cumsum(f)/SR
e=np.minimum(1,tt/0.008)*np.exp(-tt/0.075)
voz=(np.sin(fase)+0.5*np.sin(2*fase)+0.22*np.sin(3*fase))*e
aire=pasabajos(ruido(0.22),8)*np.exp(-tt/0.04)*0.35
guardar('S09.wav', reverb(voz+aire,0.06,0.2,3), 0.65)

# S10 vela: chisporroteo de la ofrenda, en bucle. Base de ruido grave muy tenue con
# chasquidos dispersos. Los últimos 0.4 s se cruzan con los primeros para que el
# bucle no deje costura audible.
dur=6.0
base=pasabajos(ruido(dur),90)*0.5
chasquidos=np.zeros(int(SR*dur))
for i0 in rng.uniform(0.05,dur-0.2,46):
    c=t(0.05); s=pasaaltos(ruido(0.05),4)*np.exp(-c/0.008)*rng.uniform(0.25,1.0)
    i=int(SR*i0); chasquidos[i:i+len(s)]+=s
x=base+chasquidos*0.7
cruce=int(SR*0.4)
rampa=np.linspace(0,1,cruce)
x[:cruce]=x[:cruce]*rampa+x[-cruce:]*(1-rampa)
x=x[:-cruce]
guardar('S10.wav', x, 0.35)

# S14 brindis: choque de copas y aplausos. Dos cristales (parciales inarmónicos muy
# agudos que se apagan rápido) y una ovación de ruido filtrado que crece y se va.
copas=mezcla([(0,campana(2640,0.9,0.22)),(0.045,0.75*campana(3130,0.8,0.2))],1.0)
dur=2.2
tt=t(dur)
palmas=np.zeros(int(SR*dur))
for i0 in rng.uniform(0.12,dur-0.1,120):
    c=t(0.035); s=pasaaltos(ruido(0.035),3)*np.exp(-c/0.006)*rng.uniform(0.2,1.0)
    i=int(SR*i0); palmas[i:i+len(s)]+=s
sobre=np.minimum(1,np.maximum(0,(tt-0.1))/0.35)*np.exp(-np.maximum(0,tt-0.8)/0.55)
guardar('S14.wav', reverb(mezcla([(0,copas*1.0)],dur)+palmas*sobre*0.5,0.1,0.3,4), 0.6)

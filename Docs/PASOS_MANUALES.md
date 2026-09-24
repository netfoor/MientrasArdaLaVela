# Lo que tienes que hacer tú

Actualizado: 2026-09-22.

## Resumen en una línea

**Ya no hay nada bloqueante.** Lo que era el bloqueante (el prefab *Dialogue System* de Yarn Spinner)
quedó automatizado. Lo que queda es **abrir Unity y jugarlo**, que es algo que yo no puedo hacer.

---

## 1. El bloqueante ya no existe

Te dije que tendrías que arrastrar a mano el prefab *Dialogue System*, asignarle el Yarn Project y
añadirle el `DialogueBridge`. Ya no: Yarn Spinner expone como público el mismo método que ejecuta su
menú `GameObject > Yarn Spinner > Dialogue System`, así que el constructor de la escena lo hace solo.

El verificador comprueba las cuatro cosas: que el Dialogue System esté en la escena, que tenga el Yarn
Project correcto, que el `DialogueBridge` esté en el mismo objeto y que haya un EventSystem para la
interfaz del diálogo. Las cuatro en verde.

---

## 2. Abre el proyecto

1. Unity Hub → *Add* → *Add project from disk* → esta carpeta.
2. Ábrelo con **Unity 6000.6.0f1** (es la versión con la que está construido todo).
3. La primera apertura tarda: reconstruye la caché del editor. No es un error.

La consola debería quedar **limpia**: 0 errores y 0 advertencias.

---

## 3. Juega la prueba de aceptación

Abre `Assets/_Proyecto/Scenes/MainMenu.unity` y dale Play. **Empieza siempre por MainMenu**, no por
`Mapa0_Ofrenda`: si arrancas directo en el mapa, el `GameManager` se crea vacío y no lee la partida
guardada, así que "Continuar" no se puede probar.

Recorre la lista de `LEEME_SETUP.md` §5 y anota en la hoja **Buglist** todo lo que falle:

- [ ] Cualquier tecla → menú. **Nueva partida** carga la ofrenda y se reproduce el prólogo.
- [ ] Ámbar camina en 4 direcciones con teclado y con gamepad, no atraviesa paredes, la cámara no sale del mapa.
- [ ] Aparece "[E] Examinar" junto a los objetos; E, Espacio o clic abren el diálogo; el jugador queda congelado mientras dura.
- [ ] Manteniendo F el farol crece y revela el sombrero.
- [ ] Al reunir el delantal, la niebla del patio se disipa.
- [ ] El contador llega a 4/4 y el pan de muerto pasa a decir "[E] Recordar".
- [ ] Al salir y elegir **Continuar**, se conservan los fragmentos, la niebla disipada y el mapa.

**Presta atención especial a los puntos 1 y 3**: son los que `Docs/BUGLIST_SPRINT1.md` señala como
puntos en riesgo (bugs B01, B03 y B04). Si fallan, ya está diagnosticada la causa y sé cómo arreglarla;
avísame.

El archivo de partida está en `~/.config/unity3d/UTP/MientrasArdaLaVela/partida.json`.
Bórralo si quieres probar desde cero.

---

## 4. Lo que sólo tú puedes decidir

Tres cosas que dejé sin tocar a propósito, porque son criterio tuyo y no defectos:

1. **La luz global de 0.25.** Con los tilesets definitivos la escena ya no se lee como noche: los
   cuadros de color de relleno eran oscuros y ayudaban, la duela real es clara. El LEEME §4.2.2 fija
   0.25 y el Documento de Diseño manda sobre el código, así que no lo cambié. Míralo en pantalla y
   dime si lo bajamos (yo probaría entre 0.12 y 0.18).
2. **El trazado del Mapa 0.** Sala, cocina y patio están donde me pareció razonable, porque no tengo
   la Figura 8 del Documento de Diseño. Las medidas están como constantes al principio de
   `Assets/_Proyecto/Scripts/Editor/Mapa0Builder.cs`; compáralas con tu plano y dime qué mover.
3. **Los nueve puntos donde el generador LPC no tenía exactamente lo que pedía el documento**
   (sin gorro de panadero para Don Ernesto, bombín en vez de sombrero para Don Aurelio, sin rebozo
   para Ámbar, sin overol para Lucía de niña…). Están todos anotados en `CREDITOS.txt`.

---

## 5. Lo que sigue pendiente del Sprint 1

- **`git push`.** No tengo la URL de tu repo. Cuando la tengas:
  `git remote add origin <url> && git push -u origin main`
- **Arte de los fragmentos.** Los cuatro (`M0_foto`, `M0_delantal`, `M0_petalos`, `M0_sombrero`)
  siguen con cuadros de relleno: no hay delantal ni sombrero de palma en lo que se descargó.
  El destello que los marca como interactuables sí funciona.
- **Sonido.** `CREDITOS.txt` lista como pendientes los efectos (S03 al reunir fragmento, S04 al
  disipar la niebla), el ambiente y la música. Los scripts ya tienen los campos `AudioSource`
  listos para recibirlos.

---

## Comandos útiles

Todo esto también está en el menú **Mientras arda la vela** dentro del editor.

```
# Reconstruir la escena del Mapa 0 desde cero
Mientras arda la vela > Construir jerarquía Mapa 0

# Comprobar que una escena cumple el LEEME
Mientras arda la vela > Verificar Mapa 0
Mientras arda la vela > Verificar MainMenu
Mientras arda la vela > Verificar Animator de Ámbar

# Ver las escenas sin abrir el editor (escribe PNG en /tmp)
Mientras arda la vela > Capturar Mapa 0
Mientras arda la vela > Capturar MainMenu

# Pruebas: Window > General > Test Runner > EditMode > Run All   (12 pruebas)
```

---

## Línea de comandos de Unity (`tools/unity.sh`)

Envoltorio del editor en batchmode. **Requiere que el editor esté cerrado**: Unity no
admite dos instancias sobre el mismo proyecto, y el script lo comprueba antes de
arrancar en vez de dejar la `Library` a medias.

```bash
./tools/unity.sh verificar         # los tres verificadores
./tools/unity.sh pruebas           # pruebas EditMode
./tools/unity.sh todo              # verificar + pruebas
./tools/unity.sh construir-mapa0   # reconstruye y guarda Mapa0_Ofrenda
./tools/unity.sh construir-menu    # reconstruye y guarda MainMenu
./tools/unity.sh capturar          # renderiza ambas escenas a PNG
./tools/unity.sh animator          # regenera los clips de Ámbar
./tools/unity.sh cortar-hojas      # corta las 13 hojas de personaje
./tools/unity.sh metodo Clase.Metodo
```

Devuelve código de salida distinto de cero si algo falla, así que sirve para un hook
de pre-commit o para integración continua. Los logs quedan en `Temp/cli-logs/`.

## Ojo con los comandos del menú del editor

`Construir jerarquía Mapa 0` y `Construir MainMenu` trabajan sobre **la escena que
tengas abierta**. Desde el 2026-09-24 comprueban cuál es y, si no es la suya, te
ofrecen abrir la correcta. Aun así, la costumbre sana es abrir primero la escena que
vas a reconstruir.

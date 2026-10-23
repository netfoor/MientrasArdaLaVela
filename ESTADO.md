# Estado del proyecto

Última actualización: **2026-10-23** · rama `main` · repositorio
[netfoor/MientrasArdaLaVela](https://github.com/netfoor/MientrasArdaLaVela) (privado)

> Este archivo es el resumen de **qué está hecho y qué no**. Si eres un agente que
> retoma el trabajo: lee esto y después `AGENTS.md`. Las órdenes originales están en
> `LEEME_SETUP.md` (montaje) y `PROMPTS_OPENCODE.md` (P1–P6); las dos quedaron
> cumplidas y así están marcadas dentro de cada archivo.

---

## En una frase

Los Sprints 1 y 2 están cerrados y **el juego completo se puede jugar de principio a fin**.
Del Sprint 3, F19–F24 están **hechas y jugadas**: la partida del 2026-10-06 llegó al epílogo
con 20/20 fragmentos, **25/25 pétalos** y **8 de los 9 logros**. F22 (música y efectos) se
cerró el 2026-10-22 con los cinco efectos que faltaban y la mezcla medida, y **F26 compiló**:
hay un ejecutable de Windows de 258 MB en `Builds/Windows/`. **F25 va muy avanzada**: los
fragmentos, los mementos, los rombos del HUD y el destello dejaron de ser cuadros de color
(2026-10-22) y el 2026-10-23 se rehicieron el camino de pétalos del Mapa 0 y las dos hojas de
Marisol. El juego **se jugó entero de nuevo el 2026-10-23**, con la música y los efectos puestos.

Lo que falta para cerrar: **jugarlo entero una vez** (sonido, códigos secretos y el build en un
segundo equipo), el resto del pulido de F25, y abrir el Excel de producción en Excel para que
recalculen sus hojas de resumen (ver "Pendiente").

## Cómo comprobarlo sin abrir Unity

```bash
./tools/unity.sh todo        # verificadores + pruebas (requiere el editor CERRADO)
```

Estado en la última corrida (**2026-10-22**, con F22 cerrada): **8 verificadores sin fallos
(Mapa 0, MainMenu, Animator, Recuerdos 1–4, Epílogo) · 31/31 pruebas EditMode**. Las dos
últimas son `SonidosTests`, que comprueba que las 8 piezas y los 14 efectos de la Tabla 27
existan y que todo sonido invocado en el código o en los diálogos tenga su clip.

---

## Feature Log

| ID | Feature | Sprint | Estado |
|---|---|---|---|
| F01 | Configuración del proyecto y recursos abiertos | 1 | ✅ Hecho |
| F02 | Movimiento de Ámbar (vista cenital) | 1 | ✅ Hecho |
| F03 | Cámara pixel perfect con seguimiento | 1 | ✅ Hecho |
| F04 | Sistema de interacción | 1 | ✅ Hecho |
| F05 | Sistema de diálogos (Yarn Spinner) | 1 | ✅ Hecho |
| F06 | Mapa 0: La ofrenda (prólogo y tutorial) | 1 | ✅ Hecho |
| F07 | Fragmentos de memoria | 1 | ✅ Hecho |
| F08 | Niebla del Olvido y Ecos | 1 | ✅ Hecho. El Mapa 0 no lleva Ecos; el primero se coloca en el Recuerdo 1 (F14) |
| F09 | Pantalla de título y menú principal | 1 | ✅ Hecho |
| F10 | Guardado y carga automáticos | 1 | ✅ Hecho |
| F11 | Tizne: compañero y pistas (Olfatear) | 2 | ✅ Hecho |
| F12 | Puzle de mosaico (rotación de piezas) | 2 | ✅ Jugado (mosaico del pan resuelto). Imágenes: pan (M0) y fotografía (R1) |
| F13 | Mementos y transición entre recuerdos | 2 | ✅ Jugado: fundido a blanco. Ahora lleva al Recuerdo 1 |
| F14 | Recuerdo 1: La panadería cerrada (2015) | 2 | ✅ Jugado completo por Fortino (2026-09-27) |
| F15 | Recuerdo 2: La silla vacía (1988) | 2 | ✅ Jugado completo por Fortino (2026-09-27) |
| F16 | HUD y Diario de recuerdos | 2 | ✅ Jugado |
| F17 | Menú de pausa y Opciones | 2 | ✅ Jugado |
| F18 | Vela: indicador de progreso | 2 | ✅ Jugado |
| F19 | Recuerdo 3: La noche del horno (1966) | 3 | ✅ Jugado completo (2026-09-29) |
| F20 | Recuerdo 4: El campo de cempasúchil (1955) | 3 | ✅ Jugado completo (2026-09-29) |
| F21 | Epílogo y créditos | 3 | ✅ Jugado completo (2026-09-29) |
| F22 | Música y efectos | 3 | ✅ Hecho (2026-10-22). M01–M08 por escena y los 14 efectos de la Tabla 27 completos y mezclados. **Falta oírlo jugando** |
| F23 | Logros y códigos secretos | 3 | ✅ Jugado: 8 de los 9 logros desbloqueados el 2026-10-06. 🟡 **Los 3 códigos secretos no se han tecleado nunca** |
| F24 | Pétalos coleccionables | 3 | ✅ Jugado: 25/25 pétalos recogidos el 2026-10-06 |
| F25 | Pulido | 3 | 🟡 Arte propio de los 16 fragmentos, los 5 mementos, los rombos del HUD, el destello, el camino de pétalos del Mapa 0 y las dos hojas de Marisol. Ritmo y textos revisados y B17 confirmado jugando. Falta: mapas más detallados |
| F26 | Build para Windows | 3 | 🟡 Compila: 258 MB, 7 escenas, `Builds/Windows/MientrasArdaLaVela.exe`. **Falta probarlo en un segundo equipo** |

## Prueba de aceptación del Sprint 1 (`LEEME_SETUP.md` §5)

| # | Punto | Estado |
|---|---|---|
| 1 | Cualquier tecla → menú; Nueva partida carga la ofrenda y suena el prólogo | ✅ jugado |
| 2 | Camina en 4 direcciones, no atraviesa paredes, la cámara no sale del mapa | ✅ jugado |
| 3 | Aparece "[E] Examinar"; E/Espacio/clic abren diálogo; el jugador se congela | ✅ jugado |
| 4 | Con F el farol crece y revela el sombrero | ✅ implícito: el sombrero es uno de los 4 y sólo se alcanza con la vela |
| 5 | Al reunir el delantal, la niebla del patio se disipa | ✅ implícito: los pétalos están tras la niebla y se reunieron |
| 6 | El contador llega a 4/4 y el pan dice "[E] Recordar" | ✅ Fortino llegó a 4/4 el 2026-09-27 |
| 7 | Al salir y elegir Continuar se conservan fragmentos, niebla y mapa | ⬜ **sin confirmar: falta probarlo** |

---

## Lo que se construyó, y dónde

### Scripts de juego — `Assets/_Proyecto/Scripts/`
Los que venían en el paquete original, ya funcionando. Cambios propios anotados en el
código con el motivo.

### Herramientas de editor — `Assets/_Proyecto/Scripts/Editor/`
Esto **no venía en las órdenes originales**; se escribió para que las escenas se
puedan reconstruir y comprobar sin montarlas a mano.

| Archivo | Qué hace |
|---|---|
| `Mapa0Builder.cs` | Construye la escena del Mapa 0 entera (LEEME §4.2). Idempotente, con Undo. |
| `MainMenuBuilder.cs` | Construye la escena MainMenu (LEEME §4.1), créditos con scroll incluidos. |
| `Mapa0Verificador.cs` | ~100 comprobaciones del Mapa 0 contra el LEEME. |
| `MainMenuVerificador.cs` | 50 comprobaciones del MainMenu. |
| `AmbarAnimatorVerificador.cs` | 68 comprobaciones del Animator. |
| `AmbarAnimator.cs` | P4: corta la hoja, genera los 8 clips y `Ambar.controller`. |
| `HojaLPC.cs` | Corta cualquier hoja LPC; las 13 de una vez. GUID deterministas. |
| `TilesetLPC.cs` | Recorta de los tilesets sólo los tiles que usa el Mapa 0. |
| `Mapa0Captura.cs`, `MainMenuCaptura.cs` | Renderizan las escenas a PNG sin abrir el editor. |
| `PixelArtImporter.cs` | Aplica PPU 32 / Point / sin compresión a todo lo que entre en `Art/`. |
| `PlaceholderArt.cs` | Sprites y tiles de relleno de color. |
| `ProyectoSetup.cs` | Arranque: recursos de TMP y Yarn Project. |

**Menú del editor:** todo está bajo *Mientras arda la vela*.

### Línea de comandos — `tools/unity.sh`
Envuelve el editor en batchmode. Detecta si el editor está abierto y aborta con un
mensaje claro. Códigos de salida útiles para CI. Logs en `Logs/cli/`.

### Documentos — `Docs/`
| Archivo | Contenido |
|---|---|
| `BUGLIST_SPRINT1.md` | 13 hallazgos con archivo/línea, FeatureID y severidad. **10 corregidos.** |
| `PASOS_MANUALES.md` | Lo que sólo puede hacer una persona, y cómo usar el CLI. |
| `PRESENTACION.md` | La presentación del Documento de Diseño (17 diapositivas) y dónde está. |
| `PROMPT_CLAUDE_CHROME_ARTE.md` | El encargo de arte que se delegó a un navegador. |

---

## Qué se agregó en F25 y F26 (2026-10-22)

- `tools/arte/objetos_f25.py`: **21 piezas de pixel art propias de 32 × 32** — los 13 fragmentos
  que eran cuadros de color (fotografía de Aurelio joven, delantal, camino de pétalos que se
  bifurca, sombrero de palma, fotografía rota, lata de cartas, charola con dos panes, tarjeta de
  Refugio, pedazos del telegrama, copa rota, cama vacía, nota y la talla del árbol), los **5
  mementos** (pan de muerto, fotografía enmarcada, caja de cerillos, canica azul y el árbol de la
  promesa), los **dos rombos del HUD** y el **destello** que flota sobre los fragmentos, que era
  un cuadro blanco y se notaba muchísimo. Lo que dibuja cada una sale de las Tablas 8 a 13.
- Los constructores ya no pasan `null`: hay un cargador común, `ConstructorEscena.Objeto(...)`,
  que avisa en consola si falta una pieza y cae en el cuadro de color. Los fragmentos de R3 y R4
  que ya tenían arte (`lena.png`, `huellas.png`, `vela_cabo.png`) se reutilizan.
- `Scripts/Editor/Build.cs` + `./tools/unity.sh build-windows` (y `build-linux`): compila
  tomando las escenas de Build Profiles y **falla con un mensaje claro** si la primera no es
  MainMenu o si falta el módulo de la plataforma. No toca `productName`: la carpeta de la
  partida guardada se forma con él y cambiarlo dejaría huérfanas las partidas que ya existen.
- La carpeta `MientrasArdaLaVela_BackUpThisFolder_ButDontShipItWithYourGame` que deja el build
  son símbolos de depuración: **no se entrega**, el resto de `Builds/Windows/` sí.

## Siguiente paso: oír el sonido y teclear los códigos

F22 quedó cerrada el 2026-10-22 y **nadie la ha escuchado todavía**. Lo que hay que hacer, en
una sola sesión de juego (empezando por MainMenu):

1. **Teclear los tres códigos** en el título o el menú: CEMPASUCHIL, TIZNE y PANDEMUERTO. Es lo
   único de F23 que no se ha probado nunca (los PlayerPrefs `codigo_*` están vacíos).
2. **Oír la mezcla**: pasos de Ámbar, tic del texto en los diálogos, chisporroteo de la vela en
   el menú y la ofrenda, ladrido de Tizne con H, campanilla al entrar al Recuerdo 1 y copas en
   el brindis del Recuerdo 2. Si algo tapa a los demás, los niveles están en una sola tabla, en
   el comentario de cabecera de `Core/Sonidos.cs`.
3. **Probar el .exe en un segundo equipo con Windows** (F26); con eso queda cerrada.
4. Lo que resta de F25: **mapas más detallados**. El ritmo y los textos se revisaron el
   2026-10-23 (557 renglones de diálogo, sin cambios: el juego se jugó entero y el ritmo
   convenció) y B17 quedó confirmado en esa misma partida.

### Qué se agregó en F22 (2026-10-22)
- `tools/arte/sonidos_f22.py`: los **cinco efectos que faltaban** de la Tabla 27 — S01 pasos,
  S05 texto, S09 ladrido de Tizne, S10 vela y S14 brindis. Arte sonoro propio, como el resto.
- `Core/Sonidos.cs`: cuatro fuentes en rueda y variación de tono por disparo (dos pasos seguidos
  ya no son el mismo golpe). Su cabecera lleva **la tabla de mezcla**: tres niveles medidos en
  RMS sobre los clips, no ajustados a ojo.
- `Core/Ambiente.cs` (nuevo): el chisporroteo de la vela (S10) en bucle en el menú, la ofrenda y
  el epílogo. Se crea solo, como la música.
- `Core/SonidoTexto.cs` (nuevo): el tic por letra (S05). Se engancha a la máquina de escribir de
  Yarn Spinner con un `ActionMarkupHandler`; lo registra `ConstructorEscena.TicDeTexto`, que
  llaman todos los constructores **incluido el del Mapa 0**. Con la velocidad "Instantánea"
  (código PANDEMUERTO) no suena.
- `PlayerMovement`: pasos por distancia recorrida, no por tiempo. `PlayerInteractor`: el clic de
  examinar (S02), que existía como clip pero no sonaba en el juego. `Tizne`: ladrido (S09).
- Diálogos: `<<sonido S13>>` abre el Recuerdo 1 (la campanilla de La Espiga) y `<<sonido S14>>`
  suena en el brindis del Recuerdo 2.
- `Tests/EditMode/SonidosTests.cs`: dos pruebas nuevas. Una exige que existan las 8 piezas y los
  14 efectos de la Tabla 27; la otra, que todo sonido invocado en el código o en un `.yarn` tenga
  clip. Nacen de un fallo real: `Sonidos.Play` calla cuando falta el clip, así que S01, S05, S09,
  S10 y S14 estuvieron invocados y mudos sin que nada lo delatara.

### Qué se agregó en F23 y F24
- `Core/Logros.cs`: catálogo de los 9 logros, condiciones (función pura `Cumplidos`, con
  pruebas), desbloqueo en `PlayerPrefs` (son del jugador, no de la partida: sobreviven a
  "Nueva partida") y los 3 códigos. `GameManager` llama a `Logros.Revisar` al completar un
  mapa, desmentir un Eco, recoger un pétalo o hablar con un personaje.
- `Memory/Petalo.cs` + `ConstructorEscena.Petalos(...)`: 5 pétalos por mapa (IDs
  `M0_petalo1`…`R4_petalo5`), se recogen al pasar encima, brillan un poco, suena S16.
- `UI/Avisos.cs`: aviso arriba al centro (logro desbloqueado con S12, pétalo n/5, código aceptado).
- Menú principal: botones **Logros** y **Galería de bocetos** (ésta, apagada hasta ganar
  "Dos velas" o teclear CEMPASUCHIL). Los códigos se teclean en el título o el menú; suena S13.
- Pausa: botón **Logros**. Diario: "Pétalos n/5" en cada página y, con "Todas las voces", la
  página extra *Nota de Ámbar*. HUD: "Pétalos n/5" junto a los rombos.
- Tizne: `tizne_collar.png`, `tizne_sombrero.png`, `tizne_ambos.png` (derivadas de la hoja LPC,
  `tools/arte/tizne_accesorios.py`). El collar sale con "Memoria propia"; el sombrero con TIZNE.
- Créditos finales: pétalos y logros en el resumen; con "Camino de pétalos", escena adicional.
- Partida **versión 3**: lista nueva `talked` (personajes con los que se habló). Migra sola.

| # | Prueba del Sprint 3 | Estado |
|---|---|---|
| 1 | Menú: aparecen Logros y Galería (gris). Teclear CEMPASUCHIL → campanilla, aviso y la Galería se enciende con 5 bocetos | ⬜ **sin probar** |
| 2 | Teclear TIZNE → al jugar, Tizne lleva sombrero de palma en las 4 direcciones; teclear otra vez lo quita | ⬜ **sin probar** |
| 3 | Teclear PANDEMUERTO → el texto de los diálogos sale completo; en Opciones se ve "Instantánea"; otra vez lo desactiva | ⬜ **sin probar** |
| 4 | En el Mapa 0 hay 5 pétalos que brillan; al pisarlos suenan, sube el contador del HUD y sale el aviso | ✅ los 5 del Mapa 0 están recogidos |
| 5 | Al completar el tutorial sale el aviso "Logro desbloqueado · Primera luz" con campanas | ✅ `primera_luz` desbloqueado |
| 6 | Pausa → Logros muestra los 9 (obtenidos en color, el resto en gris) | 🟡 hay 8 obtenidos; falta ver la pantalla con uno en gris |
| 7 | Diario: "Pétalos n/5" en cada página | 🟡 sin confirmar a ojo |
| 8 | Los pétalos recogidos no reaparecen al salir y Continuar | 🟡 los 25 están guardados en la partida; falta confirmar que al Continuar no reaparecen |
| 9 | Con los 25 pétalos, al final de los créditos aparece el camino de pétalos y el texto de Lucía | ✅ logro `camino_petalos` y mapa `EP` completado |
| 10 | B17: confirmar que no se atraviesan muros en ningún mapa | ✅ confirmado: dos partidas completas (2026-10-06 y 2026-10-23) por los 6 mapas sin atravesar un muro |

Lo que marca ✅ sale de la **partida del 2026-10-06** (`~/.config/unity3d/UTP/MientrasArdaLaVela/`):
versión 3, `completedMaps` M0–R4 y EP, 20/20 fragmentos, 5 Ecos, 25/25 pétalos, y en
`prefs` el logro `todas_las_voces` como único pendiente. **No hay ninguna pref `codigo_*`**:
de ahí que los tres códigos sigan sin probar.

Para probar rápido los logros difíciles, se pueden borrar con `PlayerPrefs` (Edit → Clear All
PlayerPrefs también borra volumen y códigos).

### Dónde están los pétalos
| Mapa | Posiciones (x, y) |
|---|---|
| M0 | sala (15.5, 19.5) · cocina (33.5, 11.5) y (26.5, 13.5) · patio (7.5, 2.5) y (28.5, 5.5) |
| R1 | salón (2.5, 14.5) y (16.5, 12.5) · trastienda (27.5, 11.5) y (20.5, 8.5) · calle (26.5, 1.5) |
| R2 | jardín (1.5, 4.5) y (1.5, 9.5) · patio (25.5, 13.5) y (6.5, 13.5) · zaguán (32.5, 6.5) |
| R3 | entrada (8.5, 2.5) · cuarto (8.5, 10.5) · obrador (27.5, 2.5) y (13.5, 2.5) · fondo (12.5, 11.5) |
| R4 | campo: (28.5, 3.5) · (2.5, 7.5) · (22.5, 5.5) · (6.5, 12.5) · (24.5, 17.5) |

## Cinemáticas (idea de Fortino, 2026-09-27)

Estilo *To the Moon*: las escenas importantes se **actúan en el motor** con comandos de Yarn
dentro del propio diálogo (`<<mover>>`, `<<mirar>>`, `<<fundido>>`...). No hace falta ningún
plugin. Para escenas "contadas" con ilustraciones hay `<<vineta clave>>`: muestra una imagen a
pantalla completa bajo la caja de diálogo; las imágenes se registran en el constructor de la
escena (`DialogueSystem(("clave", "ruta.png"))`). La primera cinemática es `R1_Escena`.
Pendiente de decidir: qué escenas llevan viñetas ilustradas y quién las dibuja.

**Cámara en cinemáticas** (pedido de Fortino): `<<camara Actor segundos>>`, `<<camara_punto x y segundos>>`,
`<<camara_jugador segundos>>`. El paneo es suave (damping 1.4) y al terminar el diálogo la cámara
vuelve sola a Ámbar.

**Final del mosaico** (pedido de Fortino): las piezas se juntan, la imagen se acerca un 8 % y se
ilumina, y el jugador la contempla hasta pulsar E. `MosaicPuzzle` ya tiene los campos `music`
(M07) y `solvedSfx` (S07) para cuando haya audio (F22, Sprint 3).

## Pendiente

### Bugs
**Sprint 2 (`Docs/BUGLIST_SPRINT2.md`)**: B14 (diálogos del Recuerdo 1 no compilados), B15 (se
atraviesan los muros) y B16 (título del HUD partido) corregidos y confirmados jugando el 2026-09-27.

Sprint 1: ninguno abierto. B05, B07 y B10 se corrigieron el 2026-09-27 (ver `Docs/BUGLIST_SPRINT1.md`).
El guardado pasó a la **versión 2** (lista `seenDialogues`) y, en el Sprint 3, a la **3** (lista `talked`); migra solo las partidas viejas.

### Arte
Los fragmentos, los mementos, los rombos del contador y el destello **ya son arte propio**
(F25, `tools/arte/objetos_f25.py`), igual que las cinco imágenes de mosaico
(`Art/Mosaicos/mosaico_M0…R4.png`), el **camino de pétalos** del Mapa 0
(`tools/arte/camino_petalos.py`) y las **dos hojas de Marisol** (`tools/arte/marisol.py`).
Lo que sigue pendiente de arte son los mapas más detallados.

### Sprint 3 (31/10 – 20/11)
F23 logros y códigos y F24 pétalos: escritos (arriba). De F25 pulido queda **mapas más
detallados** e investigar las ofrendas de siete niveles; de F26, probar el .exe en otro equipo.

**Música y efectos (F22, cerrado el 2026-10-22):** compuesta en código (`tools/arte/musica.py`): un solo tema en re menor que cambia
por recuerdo — M01 guitarra y marimba, M02 guitarra sola, M03 vals de piano, M04 son en 6/8 con
violín, M05 cuerdas graves y latido, M06 caja de música, M07 coro y marimba (mosaico), M08 el tema
en re mayor con coro (epílogo). Es propia: no requiere licencias. Si se prefiere música grabada con
instrumentos reales (CC0/CC-BY), basta con reemplazar los .ogg de `Audio/Resources/Musica/` con el
mismo nombre y anotar la atribución en `CREDITOS.txt`.

### Provisional en el Recuerdo 1
- **Marisol ya tiene hojas propias** (2026-10-23): `marisol_45_walk.png` para 2015 y
  `marisol_novia_walk.png` para la boda de 1988, recoloreadas de la de Rosa con
  `tools/arte/marisol.py`. Antes se veía igual que su madre en el Recuerdo 1 y que Lucía de 19
  —la nieta del epílogo— en el Recuerdo 2. Los invitados de la boda sí siguen con las hojas de
  don Ernesto y Lucía niña tintadas.
- El papel picado (`Art/Objetos/`) y las imágenes de los mosaicos son arte propio, dibujado con
  los scripts de `tools/arte/`.
- Los fragmentos y el memento ya son arte propio (F25), como en el Mapa 0.
- Los muebles salen de los tilesets LPC que ya estaban (mostrador, repisas, vitrina, horno).

### Entrega
El repositorio existe y está al día: **https://github.com/netfoor/MientrasArdaLaVela**, privado
porque lleva los documentos de entrega y arte con licencias mixtas. `Builds/` está en
`.gitignore`: el ejecutable no se sube, se compila con `./tools/unity.sh build-windows`.

**El Excel de producción hay que abrirlo en Excel antes de entregarlo.** La hoja *Detalles del
Proyecto* muestra "26 features Planeado / 0 Terminado" y los sprints al 0 %: son valores
cacheados que no recalcularon al escribir el archivo. El *Feature Log* sí tiene los estados
buenos. Los burn-down de los tres sprints siguen vacíos: sólo Fortino sabe las horas.

## Desviaciones conscientes (importante para la revisión)

1. **La luz global de noche está en 0.14** (antes 0.25). Se bajó el 2026-09-24 por decisión
   de Fortino. **Ya registrado** en el Documento de Diseño v1.2 y en el LEEME §4.2.2.
2. **Cinemachine es la 6.6.0, no la 3.1.7** que pedía el LEEME. Unity 6.6 la trae como
   paquete integrado y tiene prioridad; es la misma línea renumerada y la API coincide.
3. **El paso manual del prefab de Yarn Spinner ya no es manual.** El LEEME §4.2.5 y el
   P3 decían arrastrarlo a mano; Yarn Spinner expone como público el método de su propio
   menú, así que `Mapa0Builder` lo instancia, le asigna el Yarn Project y le añade el
   `DialogueBridge`.
4. **El trazado del Mapa 0 es una aproximación** a la Figura 8 del Documento de Diseño,
   que no se tuvo a la vista al construirlo. Las medidas están como constantes al
   principio de `Mapa0Builder.cs`.
5. **Nueve sustituciones en los personajes** porque el generador LPC no tenía lo que
   pedía el documento (sin gorro de panadero, bombín en vez de sombrero, sin rebozo, sin
   overol…). Todas anotadas en `CREDITOS.txt`.

6. **Tizne corre en línea recta al olfatear y cruza muros y niebla.** Es a propósito (el
   documento dice que "puede entrar a la niebla"); al seguir a Ámbar sí pisa su camino.
7. **Esc sale del mosaico sin resolverlo.** El documento no lo menciona; se añadió para no
   dejar al jugador encerrado en el puzle.

## Documentos de entrega

- `RomeroMantillaFortino_diseño.docx` — **v1.5** (historial v1.1–v1.5; la 1.5 registra logros, códigos y pétalos). Las versiones
  anteriores están en `Docs/versiones_anteriores/`.
- `RomeroMantillaFortino_producción.xlsx` — Sprints 1 y 2 y F01–F21 en *Terminado*; Sprint 3,
  F22–F24 en *En desarrollo*; B01–B17 en la hoja Buglist. **Las horas diarias del burn-down
  están vacías**: sólo Fortino sabe cuánto le dedicó cada día.

## Cosas que hay que saber para no tropezar

- **Reload Domain está desactivado** (`ProjectSettings/EditorSettings.asset`, viene de la
  plantilla 2D). Los campos `static` **sobreviven entre partidas** dentro del editor. Si
  algo "funciona la primera vez y luego no", sospecha de un estático sin reiniciar: ya
  pasó con los controles (B12) y con otros tres estáticos.
- **Los constructores de escena trabajan sobre la escena abierta.** Desde B13 comprueban
  cuál es y ofrecen abrir la correcta, pero la costumbre sana es abrirla tú primero.
- **Unity no admite dos instancias sobre el mismo proyecto.** `tools/unity.sh` lo detecta;
  si lanzas batchmode a mano con el editor abierto, dejas la `Library` a medias.
- **Las licencias del arte son mixtas** (CC-BY, CC-BY-SA, OGA-BY, GPL 3.0), capa por capa.
  Está todo en `CREDITOS.txt`, 73 KB. La pantalla de créditos del juego lo muestra con
  scroll. No quites esa atribución.

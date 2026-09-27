# Estado del proyecto

Última actualización: **2026-09-27** · rama `main` · commit `1ad512d`

> Este archivo es el resumen de **qué está hecho y qué no**. Si eres un agente que
> retoma el trabajo: lee esto y después `AGENTS.md`. Las órdenes originales están en
> `LEEME_SETUP.md` (montaje) y `PROMPTS_OPENCODE.md` (P1–P6); las dos quedaron
> cumplidas y así están marcadas dentro de cada archivo.

---

## En una frase

El **Sprint 1 está implementado y jugado**: se arranca desde el menú, Ámbar recorre
la ofrenda, habla con Yarn Spinner, usa la Luz de vela, disipa la niebla y reúne los
cuatro fragmentos. Faltan tres bugs de severidad baja, el arte de los fragmentos y
el `git push`.

## Cómo comprobarlo sin abrir Unity

```bash
./tools/unity.sh todo        # verificadores + pruebas (requiere el editor CERRADO)
```

Estado en la última corrida: **Mapa 0 sin fallos · MainMenu sin fallos · Animator sin
fallos · 12/12 pruebas EditMode · 0 errores y 0 advertencias de compilación**.

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
| F08 | Niebla del Olvido y Ecos | 1 | ⚠️ Niebla hecha; **los Ecos no se usan todavía** (la clase `Echo` existe, el Mapa 0 no lleva ninguno) |
| F09 | Pantalla de título y menú principal | 1 | ✅ Hecho |
| F10 | Guardado y carga automáticos | 1 | ✅ Hecho |
| F11–F18 | Sprint 2 | 2 | ⬜ Sin empezar |
| F19–F26 | Sprint 3 | 3 | ⬜ Sin empezar |

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

## Pendiente

### Bugs abiertos (los tres de severidad baja)
- **B05** · El guardado no es atómico: un corte durante la escritura deja la partida irrecuperable.
- **B07** · El clic izquierdo interactúa aunque el puntero esté sobre la interfaz.
- **B10** · Las banderas de "diálogo ya visto" se guardan mezcladas con los objetos revelados.

### Arte
Los **cuatro fragmentos siguen con cuadros de color**: no hay delantal ni sombrero de
palma en lo que se descargó. El destello que los marca como interactuables sí funciona.
Los rombos del contador también son de relleno.

### Entrega
**El `git push` no se ha hecho**: falta la URL del repositorio de GitHub.

---

## Desviaciones conscientes (importante para la revisión)

1. **La luz global de noche está en 0.14, y el `LEEME_SETUP.md` §4.2.2 dice 0.25.**
   Se bajó el 2026-09-24 por decisión de Fortino: con los tilesets definitivos, que son
   claros, a 0.25 la escena no se lee como noche. Está en la constante `LuzNocturna` de
   `Mapa0Builder.cs`. **Hay que actualizar el documento o revertir el valor.**
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

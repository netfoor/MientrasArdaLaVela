# Buglist — Sprint 1 (P6, cierre del sprint)

Revisión del código de `Assets/_Proyecto/Scripts` contra la lista **"Prueba de aceptación del Sprint 1"**
de `LEEME_SETUP.md`. Fecha: 2026-09-22. Rama `main`.

> **FeatureID verificados** contra el Feature Log de `RomeroMantillaFortino_producción.xlsx`
> (2026-09-23). En la primera versión de esta tabla dos estaban mal deducidos y quedaron corregidos:
> **B02 pasó de F06 a F03** (F03 es "Cámara pixel perfect con seguimiento", justo el defecto) y
> **B08 pasó de F03 a F07**, porque la Luz de vela no tiene feature propio y su función es revelar
> fragmentos.

> **Nota sobre Play Mode (2026-09-23).** Este proyecto tiene *Reload Domain* desactivado
> (viene así de la plantilla 2D de Unity). Eso hace que **todo campo `static` sobreviva de una
> partida a la siguiente** dentro del editor. Es lo que provocó B12 y lo que convirtió B06, B08
> y B09 de teóricos en reales. Si aparece cualquier comportamiento que "funciona la primera vez
> y luego no", sospecha de un estático sin reiniciar.

> **Sobre el estado.** B02 ya está corregido en este mismo commit porque era un defecto de la escena
> que generaba mi propio constructor. Los demás están **abiertos**: son cambios en scripts de
> producción y prefiero que decidas tú antes de tocarlos.

---

## Tabla para la hoja Buglist

| BugID | Descripción | Descripción técnica (archivo/línea) | FeatureID | Severidad | Rompe | Estado |
|---|---|---|---|---|---|---|
| B12 | Al entrar a Play Mode por segunda vez el juego se queda **sin ningún control**: no responde el teclado, el mouse ni el gamepad. La primera vez funciona. | `Core/GameInput.cs` configuraba las acciones una sola vez, protegido por un `static bool initialized`. El proyecto tiene *Enter Play Mode Options* con **Reload Domain desactivado** (`ProjectSettings/EditorSettings.asset:27`, valor heredado de la plantilla 2D de Unity), así que los estáticos sobreviven al salir de Play; pero el Input System deshabilita todas las acciones en ese momento. En la segunda partida el guardia hacía volver a `Init()` sin reactivar nada. | F01 | **Crítica** | Todos | **Corregido** |
| B01 | Ámbar no se congela en el instante en que arranca un diálogo: puede seguir caminando e incluso disparar otra interacción durante uno o más cuadros. | `Core/DialogueBridge.cs:50` lanza `runner.StartDialogue(node)` sin esperarlo. En Yarn Spinner 3.2.8 ese método sólo llama a `Dialogue.Continue()` **al final** (`DialogueRunner.cs:1096`), después de `await YarnTask.WhenAll(...)` sobre el `OnDialogueStartedAsync()` de cada presentador. Hasta ese momento `Dialogue.IsActive` es `false`, así que `DialogueBridge.IsBusy` (`DialogueBridge.cs:17`) devuelve `false` y el bloqueo de `PlayerMovement.cs:39` no se aplica. Si algún presentador tiene fade-in, la ventana dura varios cuadros. | F05 | **Alta** | Punto 3 | Abierto |
| B02 | La cámara no sigue bien a Ámbar y el límite del mapa se calcula mal. | `Mapa0_Ofrenda.unity` tenía `OrthographicSize: 10` en la Cinemachine Camera mientras la Main Camera usa 5.625 (Pixel Perfect, 640×360 @ PPU 32). El `CinemachineConfiner2D` recorta usando el lente del vcam, así que asumía un encuadre de ~35.5 unidades de ancho contra un mapa de 36: la cámara quedaba clavada al centro. Además el `CinemachineBrain` imponía ese 10 a la Main Camera, peleándose con Pixel Perfect. | F03 | **Alta** | Punto 2 | **Corregido** |
| B03 | Pulsar Espacio o Enter en la pantalla de título puede iniciar partida nueva de inmediato, saltándose el menú. | `UI/MainMenu.cs:29`: en el mismo cuadro en que `GameInput.AnyKey.WasPressedThisFrame()` detecta la tecla se muestra el MenuPanel y se selecciona "Nueva partida". `InputSystemUIInputModule` usa Espacio y Enter como *Submit*; si procesa el submit después de ese cambio, activa el botón recién seleccionado. | F09 | Media | Punto 1 | Abierto |
| B04 | El prólogo puede no reproducirse nunca, sin error visible. | `Interaction/DialogueTrigger.cs:23` usa `Invoke(nameof(Fire), 0.3f)`. Si a los 0,3 s `DialogueBridge.Instance` sigue siendo `null`, `Fire()` sale sin hacer nada y no hay reintento. Como Ámbar aparece **dentro** del trigger del prólogo, `OnTriggerEnter2D` tampoco se dispara nunca. | F05 | Media | Punto 1 | Abierto |
| B05 | Un corte de luz o un cierre forzado durante el autoguardado deja la partida irrecuperable. | `Core/SaveSystem.cs:30` hace `File.WriteAllText` directamente sobre `partida.json`, y `GameManager` autoguarda en cada fragmento y en cada carga de escena. Una escritura interrumpida deja el JSON truncado y `Load()` (`:39`) devuelve `null`. | F10 | Media | Punto 7 | Abierto |
| B06 | Si algo bloquea al jugador y luego se cambia de escena, Ámbar queda congelada para siempre. | `Player/PlayerMovement.cs:27`: `Locked` es `static` y nadie lo reinicia en `GameManager.OnSceneLoaded` ni en `NewGame()`. Hoy es latente porque ningún script lo escribe; Con *Reload Domain* desactivado deja de ser latente: el valor cruza de una partida a la siguiente. Se reinicia ahora en un `RuntimeInitializeOnLoadMethod`. | F02 | Media | — | **Corregido** |
| B07 | Al hacer clic sobre un botón de la interfaz también se interactúa con el objeto del mundo que esté cerca. | `Core/GameInput.cs:39` incluye `<Mouse>/leftButton` en la acción *Interact*, y `Player/PlayerInteractor.cs:36` no consulta `EventSystem.current.IsPointerOverGameObject()`. En el Mapa 0 no se nota porque el HUD no tiene botones; aparecerá con el diario (Tab) y la pausa. | F04 | Baja | — | Abierto |
| B08 | `CandleLight.Instance` queda apuntando a un objeto destruido al cambiar de escena. | `Player/CandleLight.cs:20` y `:30`: se asigna en `Awake` y nunca se limpia en `OnDestroy`. Ningún script lo lee. Se limpia ahora en `OnDestroy` y al entrar a Play, junto con la lista estática `revealables`. | F07 | Baja | — | **Corregido** |
| B09 | `MemoryMap.Current` apunta a un objeto destruido mientras se está en el menú principal. | `Core/MemoryMap.cs:11`: se asigna en `Awake` y nunca se limpia. Los cinco lectores se salvaban porque comparan con `!= null` y Unity devuelve *fake null*, pero era frágil. Se limpia ahora en `OnDestroy`. | F06 | Baja | — | **Corregido** |
| B10 | Las banderas de "diálogo ya visto" se guardan mezcladas con los objetos revelados por la Luz de vela. | `Interaction/DialogueTrigger.cs` usa `GameManager.MarkRevealed/IsRevealed`, que escriben en `SaveData.revealed` (`Core/SaveSystem.cs:15`), la misma lista que usa `Revealable`. Funciona, pero en el guardado `M0_prologo` figura como si fuera un objeto revelado. | F10 | Baja | — | Abierto |
| B11 | Con los tilesets definitivos, la escena ya no se lee como noche. | La luz global está en 0.25 según `LEEME_SETUP.md` §4.2.2, valor pensado cuando los tiles eran cuadros de color oscuros. La duela y la piedra reales son mucho más claras y a 0.25 siguen viéndose iluminadas, restando fuerza al charco del farol. **No lo cambié**: el Documento de Diseño manda sobre el código. | F06 | Baja | — | Abierto (decisión de diseño) |

---

## Cómo queda la prueba de aceptación

| # | Punto | Estado previsible |
|---|---|---|
| 1 | Cualquier tecla → menú; Nueva partida carga la ofrenda y suena el prólogo | En riesgo por **B03** y **B04** |
| 2 | Camina en 4 direcciones, no atraviesa paredes, la cámara no sale del mapa | Debería pasar (**B02** corregido) |
| 3 | Aparece "[E] Examinar"; E/Espacio/clic abren diálogo; el jugador queda congelado | En riesgo por **B01** |
| 4 | Con F el farol crece y revela el sombrero | Debería pasar |
| 5 | Al reunir el delantal, la niebla del patio se disipa | Debería pasar |
| 6 | El contador llega a 4/4 y el pan dice "[E] Recordar" | Debería pasar |
| 7 | Al salir y elegir Continuar se conservan fragmentos, niebla y mapa | Debería pasar (**B05** sólo ante un corte) |

## Lo que esta revisión NO cubre

Es una revisión **de código**, no una partida jugada. No detecta problemas de sensación (velocidad de
Ámbar, radio de interacción, ritmo de los diálogos), de colocación (si un fragmento queda inalcanzable
por el trazado) ni de rendimiento. Eso sale al jugar, y esos hallazgos van también a la Buglist.

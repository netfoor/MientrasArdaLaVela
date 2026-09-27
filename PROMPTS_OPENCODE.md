# Prompts para el Sprint 1 (usar en orden, dentro de la carpeta del proyecto)

> Antes de cada prompt, OpenCode debe leer `AGENTS.md`.

> ## ✅ LOS SEIS PROMPTS YA SE EJECUTARON (septiembre 2026)
>
> Se conservan como registro de lo que se pidió. Abajo, bajo cada uno, qué salió.
> Estado general del proyecto: **`ESTADO.md`**.

---

## P1 · Revisión de compilación (F01) — después de instalar paquetes
```
Lee AGENTS.md. Revisa todos los scripts de Assets/_Proyecto/Scripts contra Unity 6 LTS, Input System,
Cinemachine 3 y la versión de Yarn Spinner instalada (revisa Packages/manifest.json y Packages/packages-lock.json
para saber la versión exacta). Corrige solo errores de API o compilación, sin cambiar el comportamiento.
Si la API de DialogueRunner difiere (StartDialogue, IsDialogueRunning, onDialogueComplete, Dialogue.NodeExists),
adapta únicamente DialogueBridge.cs. Lista cada cambio con archivo y motivo.
```

> **✅ Hecho.** Los 20 scripts compilan con 0 errores y 0 advertencias. Un solo archivo
> necesitó cambios, `Core/DialogueBridge.cs`, y fueron los tres que el prompt anticipaba:
> en Yarn Spinner 3.2.8 `StartDialogue` devuelve `YarnTask` (antes `void`), `YarnProject`
> es anulable (sin él `Dialogue.NodeExists` lanzaba excepción) y `onDialogueComplete`
> también es anulable.

## P2 · Errores de consola
```
Lee AGENTS.md. Este es el log de la consola de Unity al dar Play:
<pega aquí el log completo>
Encuentra la causa raíz y corrígela con el cambio mínimo. Indica si el problema es de código o de
configuración de la escena (en ese caso dame los pasos exactos en el Editor).
```

> **No hizo falta.** Nunca hubo errores de consola que diagnosticar: el proyecto compiló
> limpio desde el P1 y así sigue.

## P3 · Constructor automático de la escena del Mapa 0 (F06) — ahorra el montaje manual
```
Lee AGENTS.md y LEEME_SETUP.md (sección 4.2). Crea Assets/_Proyecto/Scripts/Editor/Mapa0Builder.cs
con un [MenuItem("Mientras arda la vela/Construir jerarquía Mapa 0")] que, en la escena abierta, cree
(si no existen) los GameObjects de la sección 4.2: Grid con los 4 Tilemaps y sus colliders, Global Light 2D,
Ámbar con Rigidbody2D, CapsuleCollider2D, PlayerMovement, PlayerInteractor, CandleLight y Farol (Light2D),
MemoryMap configurado, los 4 fragmentos con los IDs y nodos de la tabla, la niebla con OblivionFog, el Memento,
Lucía y los 3 DialogueTrigger. Usa sprites de placeholder (cuadros de color) si no hay arte.
Registra Undo y marca la escena como modificada. No toques el DialogueRunner: solo deja un comentario
de que debe arrastrarse el prefab de Yarn Spinner.
```

> **✅ Hecho**, y algo más de lo pedido.
> - `Editor/Mapa0Builder.cs` construye toda la sección 4.2, idempotente y con Undo agrupado.
> - Se añadió `Editor/Mapa0Verificador.cs`: ~100 comprobaciones contra el LEEME, incluida
>   la de que cada nodo Yarn referenciado exista de verdad en el Yarn Project.
> - Se añadió `Editor/Mapa0Captura.cs`: renderiza la escena a PNG sin abrir el editor.
> - **Se desobedeció una instrucción a propósito**: el prompt decía no tocar el
>   DialogueRunner y dejar un comentario. Yarn Spinner expone como público el método de su
>   propio menú, así que el constructor lo instancia, le asigna el Yarn Project y le añade
>   el `DialogueBridge`. Eso eliminó el único paso manual bloqueante.

## P4 · Animator de Ámbar (F02)
```
Lee AGENTS.md. Tengo la hoja de sprites de Ámbar exportada del generador LPC en
Assets/_Proyecto/Art/Personajes/ambar_walk.png (64×64 por cuadro, filas: arriba, izquierda, abajo, derecha;
9 cuadros por fila; el cuadro 0 es la pose de reposo). Crea un script de Editor que:
1) corte la hoja en sprites (PPU 32, pivote en los pies),
2) genere los AnimationClips Idle_Up/Left/Down/Right y Walk_Up/Left/Down/Right a 10 fps,
3) cree Ambar.controller con parámetros MoveX, MoveY, Speed y dos Blend Trees 2D (Idle y Walk)
como espera PlayerMovement.cs.
```

> **✅ Hecho.** `Editor/AmbarAnimator.cs` corta la hoja en 36 sprites con el pivote en los
> pies, genera los 8 clips a 10 fps y arma `Ambar.controller` con MoveX/MoveY/Speed y dos
> Blend Trees 2D direccionales. `AmbarAnimatorVerificador.cs`: 68 comprobaciones, 0 fallos.
>
> Diferencia con lo que suponía el prompt: la hoja del generador LPC es la **universal
> completa** (832×3456), no una hoja sólo de caminata. El bloque walk son las filas 9 a 12.
> `Editor/HojaLPC.cs` generaliza el corte y hace las 13 hojas de una vez.

## P5 · Pruebas del guardado (F10)
```
Lee AGENTS.md. Escribe pruebas EditMode (Unity Test Framework) para SaveSystem y GameManager:
guardar y cargar SaveData, CollectFragment no duplica IDs, CountFragments respeta el prefijo,
ResolveEcho dispara el evento una sola vez. Crea el asmdef de pruebas necesario.
```

> **✅ Hecho. 12 pruebas, 12 pasan.** En `Assets/_Proyecto/Tests/EditMode/`.
> Cubren lo pedido y algo más: guardar/cargar, archivo dañado, `CollectFragment` sin
> duplicados, `CountFragments` por prefijo, `ResolveEcho` y `CompleteMap` una sola vez,
> `MarkRevealed`, el autoguardado y `Ensure`. **Respaldan y restauran la partida real**,
> porque `SaveSystem` escribe en la misma ruta que el juego.
>
> Hicieron falta dos cambios estructurales: **tres asmdef** (un asmdef de pruebas no puede
> referenciar `Assembly-CSharp`) y un guardia `Application.isPlaying` en
> `GameManager.Awake`, porque `DontDestroyOnLoad` lanza excepción fuera de Play Mode.

## P6 · Cierre del sprint (Pruebas Alfa)
```
Lee AGENTS.md y la lista "Prueba de aceptación del Sprint 1" de LEEME_SETUP.md. Revisa el código buscando
casos que la rompan (suscripciones a eventos sin quitar, referencias nulas al cambiar de escena, doble
interacción en el mismo frame, diálogo que no libera al jugador). Entrega una tabla con: descripción,
descripción técnica (archivo/línea), FeatureID y severidad, lista para copiar a la hoja Buglist.
```

> **✅ Hecho.** La tabla está en **`Docs/BUGLIST_SPRINT1.md`**: 13 hallazgos con
> descripción técnica por archivo y línea, FeatureID y severidad, lista para la hoja
> Buglist. **10 corregidos**; quedan B05, B07 y B10, las tres de severidad baja.
>
> Los dos de severidad alta eran reales: el jugador no se congelaba al arrancar un diálogo
> (B01) y la Cinemachine Camera tenía un tamaño ortográfico que no correspondía con el de
> la cámara real, así que el Confiner recortaba mal y la cámara no seguía a Ámbar (B02).

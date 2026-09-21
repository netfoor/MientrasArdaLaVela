# Prompts para el Sprint 1 (usar en orden, dentro de la carpeta del proyecto)

> Antes de cada prompt, OpenCode debe leer `AGENTS.md`.

---

## P1 · Revisión de compilación (F01) — después de instalar paquetes
```
Lee AGENTS.md. Revisa todos los scripts de Assets/_Proyecto/Scripts contra Unity 6 LTS, Input System,
Cinemachine 3 y la versión de Yarn Spinner instalada (revisa Packages/manifest.json y Packages/packages-lock.json
para saber la versión exacta). Corrige solo errores de API o compilación, sin cambiar el comportamiento.
Si la API de DialogueRunner difiere (StartDialogue, IsDialogueRunning, onDialogueComplete, Dialogue.NodeExists),
adapta únicamente DialogueBridge.cs. Lista cada cambio con archivo y motivo.
```

## P2 · Errores de consola
```
Lee AGENTS.md. Este es el log de la consola de Unity al dar Play:
<pega aquí el log completo>
Encuentra la causa raíz y corrígela con el cambio mínimo. Indica si el problema es de código o de
configuración de la escena (en ese caso dame los pasos exactos en el Editor).
```

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

## P5 · Pruebas del guardado (F10)
```
Lee AGENTS.md. Escribe pruebas EditMode (Unity Test Framework) para SaveSystem y GameManager:
guardar y cargar SaveData, CollectFragment no duplica IDs, CountFragments respeta el prefijo,
ResolveEcho dispara el evento una sola vez. Crea el asmdef de pruebas necesario.
```

## P6 · Cierre del sprint (Pruebas Alfa)
```
Lee AGENTS.md y la lista "Prueba de aceptación del Sprint 1" de LEEME_SETUP.md. Revisa el código buscando
casos que la rompan (suscripciones a eventos sin quitar, referencias nulas al cambiar de escena, doble
interacción en el mismo frame, diálogo que no libera al jugador). Entrega una tabla con: descripción,
descripción técnica (archivo/línea), FeatureID y severidad, lista para copiar a la hoja Buglist.
```

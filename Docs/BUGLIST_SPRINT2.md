# Buglist — Sprint 2

| BugID | Descripción | Descripción técnica (archivo/línea) | FeatureID | Severidad | Estado |
|---|---|---|---|---|---|
| B14 | En el Recuerdo 1 no se reproduce ningún diálogo ni la cinemática; los fragmentos se reúnen en silencio. | `Recuerdo1_Panaderia.yarn` se importó, pero Yarn Spinner no recompiló el Yarn Project, así que sus nodos no existían. `DialogueBridge.Play` con un nodo inexistente ejecuta `onComplete` y el fragmento se registra sin diálogo. Ahora `ConstructorEscena.DialogueSystem` fuerza la reimportación del proyecto y hay un menú *Setup > Reimportar diálogos (Yarn)*. | F14 | Alta | Corregido y confirmado (2026-09-27) |
| B15 | Se atraviesan los muros (salón del Recuerdo 1 y, muy probablemente, también el Mapa 0). Sólo bloqueaban la niebla y los muebles. | `Mapa0Builder` y `Recuerdo1Builder` pedían los tiles `muro_arriba`/`muro_cara` sin collider (`TilesetLPC.Tile(clave, false)` pone `ColliderType.None`). Ahora se piden con `collider: true` y los dos verificadores comprueban que ningún tile de *Paredes* quede sin collider. | F06, F14 | Alta | Corregido y confirmado (2026-09-27) |
| B16 | El nombre del recuerdo en el HUD se partía en dos líneas y tapaba el contador. | `ConstructorEscena.HUD`: el texto medía 560 px. Ahora 900 px y sin salto de línea. | F14 | Baja | Corregido y confirmado (2026-09-27) |

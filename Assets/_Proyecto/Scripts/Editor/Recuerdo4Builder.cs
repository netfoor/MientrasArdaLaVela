using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static MientrasArdaLaVela.EditorTools.ConstructorEscena;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Recuerdo 4: "El campo de cempasúchil" (1955) — F20. Último recuerdo, sin Ecos.
    ///
    /// El Olvido se retira solo, franja por franja, conforme don Aurelio acepta la verdad:
    /// las huellas pequeñas abren la franja media → la vela de Refugio (oculta: la Luz de vela
    /// encuentra a la otra vela) abre la franja alta → cinemática: Refugio, de 12, encuentra a
    /// Aurelio, de 9, y le regala la canica (la da la propia escena) → se abre el árbol →
    /// la promesa bajo el pirul → memento final: la promesa. Su mosaico lleva al Epílogo.
    /// </summary>
    public static class Recuerdo4Builder
    {
        const string Ruta = "Assets/_Proyecto/Scenes/Recuerdo4_Campo.unity";
        const string RutaMosaico = "Assets/_Proyecto/Art/Mosaicos/mosaico_R4.png";
        const string Obj = "Assets/_Proyecto/Art/Objetos/";

        static readonly RectInt Campo = new RectInt(1, 1, 30, 18);

        static readonly Vector3 PosAmbar    = new Vector3(4.5f, 3.3f, 0f);
        static readonly Vector3 PosTizne    = new Vector3(3.3f, 3.1f, 0f);
        static readonly Vector3 PosHuellas  = new Vector3(7.5f, 5.5f, 0f);
        static readonly Vector3 PosVela     = new Vector3(24.5f, 9.5f, 0f);
        static readonly Vector3 PosAurelio9 = new Vector3(10.5f, 12.6f, 0f);
        static readonly Vector3 PosRefugio  = new Vector3(29.5f, 13f, 0f);
        static readonly Vector3 PosArbol    = new Vector3(16f, 17.1f, 0f);
        static readonly Vector3 PosPromesa  = new Vector3(18f, 15.6f, 0f);
        static readonly Vector3 PosMemento  = new Vector3(14.5f, 15.6f, 0f);

        [MenuItem("Mientras arda la vela/Construir Recuerdo 4 (campo)")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta(Ruta)) return;
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log("[Recuerdo4] Escena construida y guardada.");
        }

        public static void CrearEscenaYConstruir()
        {
            AbrirOCrear(Ruta);
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log($"[Recuerdo4] Escena guardada en {Ruta}");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();
            if (TilesetLPC.HayArte()) TilesetLPC.Preparar();

            ConstruirTilemaps();
            LuzGlobal(0.75f, new Color(1f, 0.74f, 0.55f));          // EXT. CAMPO — ATARDECER — 1955
            var ambar = Ambar(PosAmbar);
            ConstructorEscena.Tizne(ambar, PosTizne);
            Camara(ambar, new Rect(0f, 0f, 32f, 20f));
            ConstructorEscena.MemoryMap("R4", "R4_", "El campo de cempasúchil — 1955", 4, SceneNames.Epilogo);

            ConstruirPersonajes();
            var frag = Raiz("Fragmentos").transform;
            Fragmento(frag, "Frag_Huellas", PosHuellas, "R4_huellas", "R4_Frag_Huellas", false,
                      AssetDatabase.LoadAssetAtPath<Sprite>(Obj + "huellas.png"));
            Fragmento(frag, "Frag_Vela", PosVela, "R4_vela", "R4_Frag_Vela", true,
                      AssetDatabase.LoadAssetAtPath<Sprite>(Obj + "vela_cabo.png"));
            Fragmento(frag, "Frag_Promesa", PosPromesa, "R4_promesa", "R4_Frag_Promesa", false,
                      ConstructorEscena.Objeto("frag_promesa.png"));
            // Pétalos coleccionables (F24): repartidos por el campo, algunos entre las matas altas.
            Petalos("R4", new Vector3(28.5f, 3.5f, 0f), new Vector3(2.5f, 7.5f, 0f), new Vector3(22.5f, 5.5f, 0f), new Vector3(6.5f, 12.5f, 0f), new Vector3(24.5f, 17.5f, 0f));
            // "La canica regalada" (R4_canica) la entrega la cinemática del encuentro.

            // El Olvido: tres franjas que se retiran con cada hallazgo, y bruma ligera en las orillas.
            Niebla("Niebla Franja media", new Vector3(16f, 9.5f, 0f), new Vector2(30.4f, 3.2f), OblivionFog.Unlock.Fragment, "R4_huellas");
            Niebla("Niebla Franja alta", new Vector3(16f, 12.6f, 0f), new Vector2(30.4f, 3f), OblivionFog.Unlock.Fragment, "R4_vela");
            Niebla("Niebla Arbol", new Vector3(16f, 16.6f, 0f), new Vector2(30.4f, 5f), OblivionFog.Unlock.Fragment, "R4_canica");
            foreach (var go in new[] { GameObject.Find("Niebla Franja media"), GameObject.Find("Niebla Franja alta"), GameObject.Find("Niebla Arbol") })
                go.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.55f);   // olvido ligero

            ConstructorEscena.Memento("Memento_Promesa", PosMemento, ConstructorEscena.Objeto("memento_promesa.png"),
                                      "R4_Memento_Bloqueado", "R4_Memento_Activar", "R4_Mosaico_Resuelto",
                                      RutaMosaico, "La promesa");

            var triggers = Raiz("Triggers").transform;
            Trigger(triggers, "Trigger_Escena", PosAmbar, new Vector2(2f, 2f), "R4_Escena", "R4_escena", true);
            Trigger(triggers, "Trigger_Encuentro", new Vector3(12f, 12.5f, 0f), new Vector2(6f, 2.4f),
                    "R4_Encuentro", "R4_encuentro", false);

            ConstruirDecorado();
            HUD("El campo de cempasúchil — 1955");
            Mosaico();
            DialogueSystem();

            Undo.SetCurrentGroupName("Construir Recuerdo 4");
            Undo.CollapseUndoOperations(grupo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        static void ConstruirTilemaps()
        {
            var c = Tilemaps();
            // Tierra labrada propia (tools/arte/campo_f25.py): el empedrado del patio que se
            // usaba antes no es suelo de campo, y el Documento de Diseño pide el dorado del
            // atardecer de 1955. El surco cada cuatro filas le da dirección al sembradío.
            var tierra = TileObjeto("campo_tierra.png") ?? Tile("suelo_patio", new Color(0.55f, 0.43f, 0.27f));
            var surco  = TileObjeto("campo_tierra_surco.png") ?? tierra;
            // Barda de piedra de campo, no el sillar de castillo del tileset: esto es una
            // parcela a las afueras del pueblo, y el muro se ve en casi toda la vuelta del mapa.
            var cerca = TileObjeto("campo_barda_arriba.png", collider: true)
                        ?? Tile("muro_arriba", new Color(0.36f, 0.33f, 0.30f), collider: true);
            var cercaCara = TileObjeto("campo_barda.png", collider: true)
                        ?? Tile("muro_cara", new Color(0.36f, 0.33f, 0.30f), collider: true);
            var matas = new UnityEngine.Tilemaps.TileBase[8];
            for (int k = 0; k < matas.Length; k++) matas[k] = TileObjeto($"campo_mata_{k}.png");
            var brote = TileObjeto("campo_brote.png");

            var transitable = new HashSet<Vector2Int>();
            Undo.RegisterCompleteObjectUndo(c.Suelo, "Suelo");
            for (int x = Campo.xMin; x < Campo.xMax; x++)
                for (int y = Campo.yMin; y < Campo.yMax; y++)
                {
                    c.Suelo.SetTile(new Vector3Int(x, y, 0), y % 4 == 0 ? surco : tierra);
                    transitable.Add(new Vector2Int(x, y));
                }
            Muros(c.Paredes, transitable, null, cerca, cercaCara);   // cerca de piedra alrededor del campo

            // Sembradío: cuatro matas repartidas con ruido. Antes era (x + y) % 3, que dibuja
            // diagonales perfectas y se lee como papel tapiz en cuanto se ve el mapa entero.
            Undo.RegisterCompleteObjectUndo(c.Deco, "Deco");
            if (matas[0] == null) return;
            for (int x = Campo.xMin; x < Campo.xMax; x++)
                for (int y = Campo.yMin; y < Campo.yMax; y++)
                {
                    if (Vereda(x, y)) continue;
                    int r = Ruido(x, y);
                    if (r % 100 > 88) continue;                       // claros del campo
                    var pieza = r % 100 > 78 && brote != null ? brote : matas[r % matas.Length];
                    c.Deco.SetTile(new Vector3Int(x, y, 0), pieza);
                }
        }

        /// <summary>
        /// Las veredas por donde camina Ámbar y donde ocurre la cinemática: la de entrada, la
        /// que sube al norte y el claro del pirul. Sin ellas el jugador no vería por dónde ir.
        /// </summary>
        static bool Vereda(int x, int y)
            => y == 3 || y == 4 || x == 12 || x == 13 || (y >= 15 && x >= 13 && x <= 19);

        /// <summary>Ruido entero reproducible: mezcla x e y para que no queden líneas.</summary>
        static int Ruido(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        static void ConstruirPersonajes()
        {
            Npc("Aurelio9", PosAurelio9, "aurelio_9", "Aurelio9", "abajo", "R4_Aurelio9", "[E] Mirar");
            var refugio = Npc("Refugio12", PosRefugio, "cuco_12", "Refugio12", "izquierda");
            Set(refugio.GetComponent<CinematicActor>(), ("startHidden", true));
            // La vela que trae Refugio: aparece y se apaga con él (CinematicActor escala sus luces).
            LuzPuntual(refugio.transform, "Vela de Refugio", PosRefugio + new Vector3(0.35f, 0.8f, 0f),
                       new Color(1f, 0.8f, 0.45f), 1.2f, 3f);
        }

        static void ConstruirDecorado()
        {
            var p = Raiz("Decorado").transform;
            AdornoSprite(p, "Pirul", Obj + "arbol.png", PosArbol, OrdenNiebla - 3);
            var tronco = GameObject.Find("Pirul");
            if (tronco != null)
            {
                var col = Comp<BoxCollider2D>(tronco);
                col.size = new Vector2(0.6f, 0.5f);
                col.offset = new Vector2(0f, -1.2f);
            }
            // Matas altas de cempasúchil: "flores más altas que él".
            int k = 0;
            foreach (var (x, y) in new[] { (9.5f, 13.4f), (11.5f, 13.6f), (8.5f, 11.8f), (5.5f, 8.5f), (20.5f, 6.5f),
                                           (26.5f, 11.5f), (22.5f, 13.5f), (3.5f, 15.5f), (27.5f, 16.5f), (9.5f, 6.5f) })
                AdornoSprite(p, $"FlorAlta_{k}", Obj + $"flor_alta_{k++ % 2}.png", new Vector3(x, y, 0f), OrdenAmbar + 1);
        }
    }
}

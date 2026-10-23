using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MientrasArdaLaVela.EditorTools.ConstructorEscena;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Recuerdo 2: "La silla vacía" (1988) — F15. Tabla del nivel en el Documento de Diseño.
    ///
    /// Boda de Marisol en el patio, de noche, con papel picado. Recorrido:
    /// Rosa cuenta que invitó a Refugio (fragmento) → se disipa el Olvido de la mesa principal →
    /// tarjeta con su nombre en la silla vacía → con la tarjeta se desmiente el 1er Eco
    /// ("nadie lo invitó") y se abre el zaguán → pedazos del telegrama (ocultos, vela) → con el
    /// telegrama se desmiente el 2.º Eco ("nunca quiso volver") y se abre el jardín → el brindis
    /// interrumpido → memento: la caja de cerillos de La Espiga, con una esquina quemada.
    /// </summary>
    public static class Recuerdo2Builder
    {
        const string Ruta = "Assets/_Proyecto/Scenes/Recuerdo2_Boda.unity";
        const string RutaMosaico = "Assets/_Proyecto/Art/Mosaicos/mosaico_R2.png";

        static readonly RectInt Patio        = new RectInt(6, 3, 20, 12);
        static readonly RectInt Jardin       = new RectInt(1, 3, 4, 12);
        static readonly RectInt PuertaJardin = new RectInt(5, 8, 1, 1);
        static readonly RectInt Zaguan       = new RectInt(27, 6, 6, 4);
        static readonly RectInt PuertaZaguan = new RectInt(26, 7, 1, 1);

        static readonly Vector3 PosAmbar    = new Vector3(15.5f, 4.3f, 0f);
        static readonly Vector3 PosTizne    = new Vector3(14.3f, 4.1f, 0f);
        static readonly Vector3 PosAurelio  = new Vector3(15.5f, 8.3f, 0f);
        static readonly Vector3 PosRosa     = new Vector3(9.5f, 9.3f, 0f);
        static readonly Vector3 PosMarisol  = new Vector3(12.5f, 6.3f, 0f);
        static readonly Vector3 PosTarjeta  = new Vector3(16.5f, 12.4f, 0f);
        static readonly Vector3 PosMemento  = new Vector3(14.5f, 12.9f, 0f);
        static readonly Vector3 PosTelegrama = new Vector3(31.5f, 8.5f, 0f);
        static readonly Vector3 PosBrindis  = new Vector3(2.5f, 12.5f, 0f);
        static readonly Vector3 PosEco1     = new Vector3(26.5f, 7.1f, 0f);
        static readonly Vector3 PosEco2     = new Vector3(5.5f, 8.1f, 0f);

        static readonly Color ColPiedra = new Color(0.40f, 0.40f, 0.38f);
        static readonly Color ColMadera = new Color(0.44f, 0.34f, 0.26f);
        static readonly Color ColPared  = new Color(0.30f, 0.24f, 0.20f);
        static readonly Color ColDeco   = new Color(0.85f, 0.45f, 0.10f);

        [MenuItem("Mientras arda la vela/Construir Recuerdo 2 (boda)")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta(Ruta)) return;
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log("[Recuerdo2] Escena construida y guardada.");
        }

        public static void CrearEscenaYConstruir()
        {
            AbrirOCrear(Ruta);
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log($"[Recuerdo2] Escena guardada en {Ruta}");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();
            if (TilesetLPC.HayArte()) TilesetLPC.Preparar();

            ConstruirTilemaps();
            LuzGlobal(0.2f, new Color(0.62f, 0.68f, 0.95f));        // EXT. PATIO — NOCHE — 1988
            var ambar = Ambar(PosAmbar);
            ConstructorEscena.Tizne(ambar, PosTizne);
            var limites = new Rect(Jardin.xMin - 1, Patio.yMin - 1,
                                   Zaguan.xMax + 1 - (Jardin.xMin - 1), Patio.yMax + 1 - (Patio.yMin - 1));
            Camara(ambar, limites);
            ConstructorEscena.MemoryMap("R2", "R2_", "La silla vacía — 1988", 4, SceneNames.Recuerdo3);

            ConstruirPersonajes();
            var frag = Raiz("Fragmentos").transform;
            Fragmento(frag, "Frag_Tarjeta", PosTarjeta, "R2_tarjeta", "R2_Frag_Tarjeta", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_tarjeta.png"));
            Fragmento(frag, "Frag_Telegrama", PosTelegrama, "R2_telegrama", "R2_Frag_Telegrama", oculto: true,
                      sprite: ConstructorEscena.Objeto("frag_telegrama.png"));
            Fragmento(frag, "Frag_Brindis", PosBrindis, "R2_brindis", "R2_Frag_Brindis", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_copa.png"));
            // Pétalos coleccionables (F24): jardín, esquinas del patio y zaguán.
            Petalos("R2", new Vector3(1.5f, 4.5f, 0f), new Vector3(1.5f, 9.5f, 0f), new Vector3(25.5f, 13.5f, 0f), new Vector3(6.5f, 13.5f, 0f), new Vector3(32.5f, 6.5f, 0f));

            Niebla("Niebla Mesa principal", new Vector3(15.5f, 12.6f, 0f), new Vector2(10f, 4.8f),
                   OblivionFog.Unlock.Fragment, "R2_rosa");
            Eco("Eco_NadieLoInvito", PosEco1, "aurelio_42", "R2_eco1", "R2_tarjeta", "R2_Eco1_Falso", "R2_Eco1_Verdad");
            Niebla("Niebla Zaguan", new Vector3(30f, 8f, 0f), new Vector2(7f, 5f), OblivionFog.Unlock.Echo, "R2_eco1");
            Eco("Eco_NuncaQuisoVolver", PosEco2, "aurelio_42", "R2_eco2", "R2_telegrama", "R2_Eco2_Falso", "R2_Eco2_Verdad");

            ConstructorEscena.Memento("Memento_Cerillos", PosMemento, ConstructorEscena.Objeto("memento_cerillos.png"), "R2_Memento_Bloqueado", "R2_Memento_Activar",
                                      "R2_Mosaico_Resuelto", RutaMosaico, "La caja de cerillos de La Espiga");

            var triggers = Raiz("Triggers");
            Trigger(triggers.transform, "Trigger_Escena", PosAmbar, new Vector2(2f, 2f), "R2_Escena", "R2_escena", true);
            Trigger(triggers.transform, "Trigger_Zaguan", new Vector3(29.5f, 7.5f, 0f), new Vector2(3f, 3f),
                    "R2_Zaguan", "R2_zaguan", false);

            ConstruirDecorado();
            HUD("La silla vacía — 1988");
            Mosaico();
            DialogueSystem();

            Undo.SetCurrentGroupName("Construir Recuerdo 2");
            Undo.CollapseUndoOperations(grupo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        static void ConstruirTilemaps()
        {
            var c = Tilemaps();
            var piedra = Tile("suelo_patio", ColPiedra);
            var madera = Tile("suelo_casa", ColMadera);
            var muroArriba = Tile("muro_arriba", ColPared, collider: true);
            var muroCara = Tile("muro_cara", ColPared, collider: true);
            var petalos = Tile("petalos", ColDeco);
            var ralo = Tile("petalos_ralo", ColDeco);

            var transitable = new HashSet<Vector2Int>();
            Pintar(c.Suelo, Patio, piedra, transitable);
            Pintar(c.Suelo, Jardin, piedra, transitable);
            Pintar(c.Suelo, PuertaJardin, piedra, transitable);
            Pintar(c.Suelo, Zaguan, madera, transitable);
            Pintar(c.Suelo, PuertaZaguan, madera, transitable);
            Muros(c.Paredes, transitable, null, muroArriba, muroCara);

            // Cempasúchil en las orillas del jardín (sin collider).
            Undo.RegisterCompleteObjectUndo(c.Deco, "Deco");
            for (int y = Jardin.yMin; y < Jardin.yMax; y++)
            {
                c.Deco.SetTile(new Vector3Int(Jardin.xMin, y, 0), y % 2 == 0 ? petalos : ralo);
                c.Deco.SetTile(new Vector3Int(Jardin.xMax - 1, y, 0), y % 2 == 0 ? ralo : petalos);
            }
        }

        static void ConstruirPersonajes()
        {
            Npc("Aurelio42", PosAurelio, "aurelio_42", "Aurelio42", "arriba", "R2_Aurelio42", "[E] Mirar");
            // Provisional: la novia usa la hoja de Lucía de 19 años (no hay hoja de Marisol).
            Npc("Marisol", PosMarisol, "lucia_19", "Marisol", "abajo", "R2_Marisol", "[E] Mirar");
            Npc("Invitado", new Vector3(21.5f, 6.3f, 0f), "ernesto", "Invitado", "izquierda", "R2_Invitado",
                "[E] Escuchar", new Color(0.8f, 0.85f, 1f));
            Npc("Nino", new Vector3(22.5f, 9.3f, 0f), "lucia_9", "Nino", "abajo", "R2_Nino", "[E] Escuchar",
                new Color(1f, 0.92f, 0.85f));

            // Rosa: personaje de la escena y fragmento (su conversación con Aurelio).
            var rosa = Npc("Rosa", PosRosa, "rosa_40", "Rosa", "derecha");
            var destello = Hijo(rosa.transform, "Destello");
            destello.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            destello.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var srd = Comp<SpriteRenderer>(destello);
            srd.sprite = ConstructorEscena.Objeto("destello.png") ?? PlaceholderArt.Sprite("destello", Color.white, borde: false);
            srd.sortingOrder = OrdenObjeto + 1;
            Set(Comp<MemoryFragment>(rosa), ("fragmentId", "R2_rosa"), ("yarnNode", "R2_Frag_Rosa"),
                ("sparkle", destello), ("promptText", "[E] Escuchar a Rosa"));
        }

        static void ConstruirDecorado()
        {
            var p = Raiz("Decorado").transform;
            if (TilesetLPC.HayArte())
            {
                // Mesa principal y la silla vacía.
                Adorno(p, "MesaPrincipal1", "mesa", new Vector3(13.5f, 13.4f, 0f), solido: true);
                Adorno(p, "MesaPrincipal2", "mesa", new Vector3(15.5f, 13.4f, 0f), solido: true);
                Adorno(p, "MesaPrincipal3", "mesa", new Vector3(17.5f, 13.4f, 0f), solido: true);
                Adorno(p, "SillaVacia", "silla", new Vector3(16.5f, 12.4f, 0f), OrdenMueble);
                // Mesas redondas de los invitados, con sus sillas.
                foreach (var (x, y) in new[] { (9.5f, 12.4f), (22.5f, 12.4f), (19.5f, 5.4f), (8.5f, 5.0f) })
                {
                    Adorno(p, $"Mesa_{x}_{y}", "mesa", new Vector3(x, y, 0f), solido: true);
                    Adorno(p, $"SillaI_{x}_{y}", "silla", new Vector3(x - 1f, y + 0.1f, 0f));
                    Adorno(p, $"SillaD_{x}_{y}", "silla", new Vector3(x + 1f, y + 0.1f, 0f));
                }
                Adorno(p, "MesaBrindis", "mesa", new Vector3(2.5f, 12.4f, 0f), OrdenMueble);
                Adorno(p, "Maceta1", "maceta", new Vector3(6.5f, 3.6f, 0f), solido: true);
                Adorno(p, "Maceta2", "maceta", new Vector3(24.5f, 3.6f, 0f), solido: true);
            }

            // Papel picado: dos tiras sobre el patio, por encima de los personajes (cuelga).
            var colores = new[] { "rosa", "naranja", "verde", "azul", "morado" };
            foreach (float y in new[] { 14.6f, 10.9f })
                for (int x = Patio.xMin; x < Patio.xMax; x++)
                    AdornoSprite(p, $"PapelPicado_{x}_{y}", $"Assets/_Proyecto/Art/Objetos/papel_picado_{colores[(x + (int)y) % colores.Length]}.png",
                                 new Vector3(x + 0.5f, y, 0f), OrdenNiebla - 2);

            // Foquitos de la fiesta: luces cálidas a lo largo de las tiras.
            var luces = Raiz("Luces de la fiesta").transform;
            foreach (var x in new[] { 8f, 13f, 18f, 23f })
            {
                LuzPuntual(luces, $"Foco_{x}_alto", new Vector3(x, 13.5f, 0f), new Color(1f, 0.8f, 0.5f), 0.9f, 4.5f);
                LuzPuntual(luces, $"Foco_{x}_bajo", new Vector3(x, 8.5f, 0f), new Color(1f, 0.82f, 0.55f), 0.8f, 4.5f);
            }
            LuzPuntual(luces, "Farol_Jardin", new Vector3(3f, 10f, 0f), new Color(1f, 0.75f, 0.45f), 0.6f, 3.5f);
        }
    }
}

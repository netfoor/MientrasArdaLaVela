using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MientrasArdaLaVela.EditorTools.ConstructorEscena;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Recuerdo 1: "La panadería cerrada" (2015) — F14. Tabla del nivel en el Documento de Diseño.
    ///
    /// Recorrido: salón de venta (foto rota, Lucía, Aurelio de 69 y Marisol) → la pregunta de
    /// Lucía disipa la niebla de la trastienda → la lata de cartas está oculta (Luz de vela) →
    /// con las cartas se desmiente al Eco de la puerta → se disipa la niebla de la calle →
    /// la charola con dos piezas apartadas → memento: la otra mitad de la fotografía.
    ///
    /// Trazado en tiles (1 tile = 1 unidad = 32 px). Todo está en constantes para ajustarlo.
    /// </summary>
    public static class Recuerdo1Builder
    {
        const string Ruta = "Assets/_Proyecto/Scenes/Recuerdo1_Panaderia.unity";
        const string RutaMosaico = "Assets/_Proyecto/Art/Mosaicos/mosaico_R1.png";

        static readonly RectInt Salon      = new RectInt(2, 7, 16, 10);
        static readonly RectInt Trastienda = new RectInt(19, 7, 10, 10);
        static readonly RectInt PuertaTras = new RectInt(18, 9, 1, 2);
        static readonly RectInt PuertaCalle = new RectInt(9, 5, 1, 2);
        static readonly RectInt Calle      = new RectInt(1, 1, 28, 4);

        static readonly Vector3 PosAmbar     = new Vector3(14.5f, 9.3f, 0f);
        static readonly Vector3 PosTizne     = new Vector3(13.3f, 9.1f, 0f);
        static readonly Vector3 PosAurelio   = new Vector3(8.5f, 13.4f, 0f);
        static readonly Vector3 PosMarisol   = new Vector3(4.5f, 13.4f, 0f);
        static readonly Vector3 PosLucia     = new Vector3(12.5f, 9.6f, 0f);
        static readonly Vector3 PosFoto      = new Vector3(14.5f, 16.5f, 0f);
        static readonly Vector3 PosLata      = new Vector3(27.5f, 8.5f, 0f);
        static readonly Vector3 PosCharola   = new Vector3(21.5f, 2.5f, 0f);
        static readonly Vector3 PosMemento   = new Vector3(8.5f, 11.5f, 0f);
        static readonly Vector3 PosEco       = new Vector3(9.5f, 5.2f, 0f);

        static readonly Color ColLosa  = new Color(0.62f, 0.58f, 0.52f);
        static readonly Color ColMadera = new Color(0.44f, 0.34f, 0.26f);
        static readonly Color ColCalle = new Color(0.40f, 0.40f, 0.38f);
        static readonly Color ColPared = new Color(0.30f, 0.24f, 0.20f);
        static readonly Color ColMueble = new Color(0.52f, 0.36f, 0.22f);

        [MenuItem("Mientras arda la vela/Construir Recuerdo 1 (panadería)")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta(Ruta)) return;
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log("[Recuerdo1] Escena construida y guardada.");
        }

        /// <summary>Para batchmode: tools/unity.sh construir-recuerdo1.</summary>
        public static void CrearEscenaYConstruir()
        {
            AbrirOCrear(Ruta);
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log($"[Recuerdo1] Escena guardada en {Ruta}");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();
            if (TilesetLPC.HayArte()) TilesetLPC.Preparar();

            ConstruirTilemaps();
            LuzGlobal(0.78f, new Color(1f, 0.95f, 0.85f));          // día (INT. PANADERÍA — DÍA — 2015)
            var ambar = Ambar(PosAmbar);
            ConstructorEscena.Tizne(ambar, PosTizne);   // calificado: 'Tizne' también es un tipo
            var limites = new Rect(Calle.xMin - 1, Calle.yMin - 1,
                                   Trastienda.xMax + 1 - (Calle.xMin - 1), Salon.yMax + 1 - (Calle.yMin - 1));
            Camara(ambar, limites);
            ConstructorEscena.MemoryMap("R1", "R1_", "La panadería cerrada — 2015", 4, SceneNames.Recuerdo2);

            ConstruirPersonajes();
            ConstruirFragmentos();
            // Pétalos coleccionables (F24): salón, trastienda y calle.
            Petalos("R1", new Vector3(2.5f, 14.5f, 0f), new Vector3(16.5f, 12.5f, 0f), new Vector3(27.5f, 11.5f, 0f), new Vector3(20.5f, 8.5f, 0f), new Vector3(26.5f, 1.5f, 0f));
            Niebla("Niebla Trastienda", new Vector3(23.5f, 12f, 0f), new Vector2(11f, 10f),
                   OblivionFog.Unlock.Fragment, "R1_pregunta");
            Eco("Eco_NoTieneHermanos", PosEco, "aurelio_69", "R1_eco", "R1_lata", "R1_Eco_Falso", "R1_Eco_Verdad");
            Niebla("Niebla Calle", new Vector3(15f, 3f, 0f), new Vector2(28f, 4f), OblivionFog.Unlock.Echo, "R1_eco");
            ConstructorEscena.Memento("Memento_Fotografia", PosMemento, ConstructorEscena.Objeto("memento_foto.png"), "R1_Memento_Bloqueado", "R1_Memento_Activar",
                    "R1_Mosaico_Resuelto", RutaMosaico, "La fotografía de La Espiga");

            var triggers = Raiz("Triggers");
            Trigger(triggers.transform, "Trigger_Escena", PosAmbar, new Vector2(2f, 2f), "R1_Escena", "R1_escena", true);
            Trigger(triggers.transform, "Trigger_Trastienda", new Vector3(26f, 10f, 0f), new Vector2(4f, 4f),
                    "R1_Trastienda", "R1_trastienda", false);

            ConstruirDecorado();
            HUD("La panadería cerrada — 2015");
            Mosaico();
            DialogueSystem();

            Undo.SetCurrentGroupName("Construir Recuerdo 1");
            Undo.CollapseUndoOperations(grupo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        static void ConstruirTilemaps()
        {
            var c = Tilemaps();
            var losa = Tile("suelo_losa", ColLosa);
            var madera = Tile("suelo_casa", ColMadera);
            var calle = Tile("suelo_patio", ColCalle);
            var muroArriba = Tile("muro_arriba", ColPared, collider: true);
            var muroCara = Tile("muro_cara", ColPared, collider: true);
            var mostrador = Tile("mostrador", ColMueble, collider: true);
            var frente = Tile("mostrador_frente", ColMueble, collider: true);
            var estante = Tile("estante", ColMueble, collider: true);

            var transitable = new HashSet<Vector2Int>();
            Pintar(c.Suelo, Salon, losa, transitable);
            Pintar(c.Suelo, Trastienda, madera, transitable);
            Pintar(c.Suelo, PuertaTras, madera, transitable);
            Pintar(c.Suelo, PuertaCalle, losa, transitable);
            Pintar(c.Suelo, Calle, calle, transitable);

            // Muro divisorio entre salón y trastienda (con el vano de la puerta).
            var divisorio = new List<Vector2Int>();
            for (int y = Salon.yMin; y < Salon.yMax; y++) divisorio.Add(new Vector2Int(18, y));
            Muros(c.Paredes, transitable, divisorio, muroArriba, muroCara);

            // Mostrador en L del salón: cubierta arriba, frente abajo (los dos sólidos).
            Pintar(c.Muebles, new RectInt(4, 12, 8, 1), mostrador);
            Pintar(c.Muebles, new RectInt(4, 11, 8, 1), frente);
            // Repisas con charolas contra el muro del fondo.
            Pintar(c.Muebles, new RectInt(3, 16, 5, 1), estante);
            Pintar(c.Muebles, new RectInt(20, 16, 3, 1), estante);
        }

        static void ConstruirPersonajes()
        {
            Npc("Aurelio69", PosAurelio, "aurelio_69", "Aurelio69", "abajo", "R1_Aurelio69", "[E] Mirar");
            Npc("Marisol", PosMarisol, "marisol_45", "Marisol", "abajo", "R1_Marisol", "[E] Mirar");

            // Lucía es a la vez personaje de la escena y fragmento: su pregunta (R1_pregunta).
            var lucia = Npc("Lucia9", PosLucia, "lucia_9", "Lucia9", "arriba");
            var destello = Hijo(lucia.transform, "Destello");
            destello.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            destello.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var srd = Comp<SpriteRenderer>(destello);
            srd.sprite = ConstructorEscena.Objeto("destello.png") ?? PlaceholderArt.Sprite("destello", Color.white, borde: false);
            srd.sortingOrder = OrdenObjeto + 1;
            Set(Comp<MemoryFragment>(lucia), ("fragmentId", "R1_pregunta"), ("yarnNode", "R1_Frag_Pregunta"),
                ("sparkle", destello), ("promptText", "[E] Escuchar a Lucía"));
        }

        static void ConstruirFragmentos()
        {
            var padre = Raiz("Fragmentos").transform;
            Fragmento(padre, "Frag_Foto", PosFoto, "R1_foto", "R1_Frag_Foto", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_foto_rota.png"));
            Fragmento(padre, "Frag_Lata", PosLata, "R1_lata", "R1_Frag_Lata", oculto: true,
                      sprite: ConstructorEscena.Objeto("frag_lata.png"));
            Fragmento(padre, "Frag_Charola", PosCharola, "R1_charola", "R1_Frag_Charola", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_charola.png"));
        }

        static void ConstruirDecorado()
        {
            if (!TilesetLPC.HayArte()) return;
            var p = Raiz("Decorado").transform;
            Adorno(p, "Reloj", "reloj", new Vector3(10.5f, 16.5f, 0f));
            Adorno(p, "Vitrina", "vitrina", new Vector3(16.5f, 15.5f, 0f), solido: true);
            Adorno(p, "Frascos", "frascos", new Vector3(5.5f, 12.5f, 0f), OrdenMueble + 1);
            Adorno(p, "Horno", "horno", new Vector3(26.5f, 16.5f, 0f), solido: true);
            Adorno(p, "Mesa", "mesa", new Vector3(23.5f, 11.5f, 0f), solido: true);
            Adorno(p, "Costales", "pan", new Vector3(20.5f, 13.5f, 0f), solido: true);
            Adorno(p, "Cajas", "cajas", new Vector3(28.5f, 13.5f, 0f), solido: true);
            Adorno(p, "Banca", "mesa", new Vector3(21.5f, 2.5f, 0f), OrdenMueble);
            Adorno(p, "Maceta", "maceta", new Vector3(11.5f, 3.5f, 0f));
            Adorno(p, "Maceta2", "maceta", new Vector3(7.5f, 3.5f, 0f));
            // Mesita para clientes en el salón, que se veía vacío.
            Adorno(p, "MesaSalon", "mesa", new Vector3(4.5f, 8.5f, 0f), solido: true);
            Adorno(p, "Silla1", "silla", new Vector3(3.5f, 8.6f, 0f));
            Adorno(p, "Silla2", "silla", new Vector3(5.5f, 8.6f, 0f));
            Adorno(p, "PlantaSalon", "maceta", new Vector3(16.5f, 7.6f, 0f), solido: true);
        }
    }
}

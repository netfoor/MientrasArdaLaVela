using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MientrasArdaLaVela.EditorTools.ConstructorEscena;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Recuerdo 3: "La noche del horno" (31 de octubre de 1966) — F19. El nivel más difícil.
    ///
    /// Recorrido: entrada a oscuras, tres Ecos repiten «Cuco se quedó dormido» →
    /// la niebla del cuarto de los hermanos se abre con la Luz de vela (modo Candle) →
    /// la cama vacía de Aurelio (desmiente al 1.er Eco y abre el obrador) y la nota para Rosa
    /// (oculta, vela) → la leña de más en la boca del horno → con la nota y la leña se
    /// desmienten los dos Ecos del paso al fondo → REVELACIÓN (cinemática: don Ernesto con las
    /// manos quemadas, Refugio se culpa, Aurelio de 20 llega tarde); la propia escena entrega
    /// "la mirada de Refugio" → memento: la canica azul.
    /// </summary>
    public static class Recuerdo3Builder
    {
        const string Ruta = "Assets/_Proyecto/Scenes/Recuerdo3_Horno.unity";
        const string RutaMosaico = "Assets/_Proyecto/Art/Mosaicos/mosaico_R3.png";

        static readonly RectInt Entrada       = new RectInt(2, 2, 8, 5);
        static readonly RectInt PasilloCuarto = new RectInt(5, 7, 2, 2);
        static readonly RectInt Cuarto        = new RectInt(2, 9, 8, 7);
        static readonly RectInt PuertaObrador = new RectInt(10, 3, 2, 1);
        static readonly RectInt Frente        = new RectInt(12, 2, 16, 7);
        static readonly RectInt PasoFondo     = new RectInt(18, 9, 2, 1);
        static readonly RectInt Fondo         = new RectInt(12, 10, 16, 6);

        static readonly Vector3 PosAmbar   = new Vector3(4.5f, 4.0f, 0f);
        static readonly Vector3 PosTizne   = new Vector3(3.3f, 3.8f, 0f);
        static readonly Vector3 PosCama    = new Vector3(3.5f, 12.4f, 0f);
        static readonly Vector3 PosNota    = new Vector3(5.5f, 13.2f, 0f);
        static readonly Vector3 PosLena    = new Vector3(15.5f, 8.2f, 0f);
        static readonly Vector3 PosMemento = new Vector3(21.5f, 12.3f, 0f);
        static readonly Vector3 PosErnesto = new Vector3(15.5f, 12.3f, 0f);
        static readonly Vector3 PosRefugio = new Vector3(27.5f, 12.8f, 0f);   // entra por la puerta trasera
        static readonly Vector3 PosAurelio = new Vector3(27.5f, 14.3f, 0f);

        static readonly Color ColPiedra = new Color(0.36f, 0.36f, 0.34f);
        static readonly Color ColMadera = new Color(0.40f, 0.30f, 0.22f);
        static readonly Color ColPared  = new Color(0.26f, 0.21f, 0.19f);

        [MenuItem("Mientras arda la vela/Construir Recuerdo 3 (horno)")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta(Ruta)) return;
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log("[Recuerdo3] Escena construida y guardada.");
        }

        public static void CrearEscenaYConstruir()
        {
            AbrirOCrear(Ruta);
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log($"[Recuerdo3] Escena guardada en {Ruta}");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();
            if (TilesetLPC.HayArte()) TilesetLPC.Preparar();

            ConstruirTilemaps();
            LuzGlobal(0.12f, new Color(0.55f, 0.62f, 0.95f));        // INT. OBRADOR — NOCHE — 1966
            var ambar = Ambar(PosAmbar);
            ConstructorEscena.Tizne(ambar, PosTizne);
            Camara(ambar, new Rect(1f, 1f, 28f, 16f));
            ConstructorEscena.MemoryMap("R3", "R3_", "La noche del horno — 1966", 4, SceneNames.Recuerdo4);

            ConstruirPersonajes();
            var frag = Raiz("Fragmentos").transform;
            Fragmento(frag, "Frag_Cama", PosCama, "R3_cama", "R3_Frag_Cama", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_cama.png"));
            Fragmento(frag, "Frag_Nota", PosNota, "R3_nota", "R3_Frag_Nota", oculto: true,
                      sprite: ConstructorEscena.Objeto("frag_nota.png"));
            Fragmento(frag, "Frag_Lena", PosLena, "R3_lena", "R3_Frag_Lena", oculto: false,
                      sprite: ConstructorEscena.Objeto("lena.png"));
            // Pétalos coleccionables (F24): entrada, cuarto, obrador y fondo.
            Petalos("R3", new Vector3(8.5f, 2.5f, 0f), new Vector3(8.5f, 10.5f, 0f), new Vector3(27.5f, 2.5f, 0f), new Vector3(13.5f, 2.5f, 0f), new Vector3(12.5f, 11.5f, 0f));
            // El 4.º fragmento (R3_mirada, "la mirada de Refugio") lo entrega la cinemática.

            Niebla("Niebla Cuarto", new Vector3(6f, 12.5f, 0f), new Vector2(8.4f, 7.4f),
                   OblivionFog.Unlock.Candle, "R3_cuarto");
            Eco("Eco_Dormido1", new Vector3(10.5f, 3.05f, 0f), "cuco_23", "R3_eco1", "R3_cama", "R3_Eco_Falso", "R3_Eco1_Verdad");
            Niebla("Niebla Obrador", new Vector3(20f, 5.5f, 0f), new Vector2(16.4f, 7.4f),
                   OblivionFog.Unlock.Echo, "R3_eco1");
            Eco("Eco_Dormido2", new Vector3(18.5f, 9.05f, 0f), "ernesto", "R3_eco2", "R3_nota", "R3_Eco_Falso", "R3_Eco2_Verdad");
            Eco("Eco_Dormido3", new Vector3(19.5f, 9.05f, 0f), "aurelio_20", "R3_eco3", "R3_lena", "R3_Eco_Falso", "R3_Eco3_Verdad");
            Niebla("Niebla Fondo", new Vector3(20f, 13.15f, 0f), new Vector2(16.4f, 6.1f),
                   OblivionFog.Unlock.Echo, "R3_eco3");

            ConstructorEscena.Memento("Memento_Canica", PosMemento, ConstructorEscena.Objeto("memento_canica.png"), "R3_Memento_Bloqueado", "R3_Memento_Activar",
                                      "R3_Mosaico_Resuelto", RutaMosaico, "La canica azul de Refugio");

            var triggers = Raiz("Triggers").transform;
            Trigger(triggers, "Trigger_Escena", PosAmbar, new Vector2(2f, 2f), "R3_Escena", "R3_escena", true);
            Trigger(triggers, "Trigger_Cuarto", new Vector3(6f, 7.6f, 0f), new Vector2(2f, 1.6f), "R3_Cuarto", "R3_cuarto_tut", false);
            Trigger(triggers, "Trigger_Revelacion", new Vector3(19f, 10.8f, 0f), new Vector2(3f, 1.4f),
                    "R3_Revelacion", "R3_revelacion", false);

            ConstruirDecorado();
            HUD("La noche del horno — 1966");
            Mosaico();
            DialogueSystem();

            Undo.SetCurrentGroupName("Construir Recuerdo 3");
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
            var mostrador = Tile("mostrador", new Color(0.52f, 0.36f, 0.22f), collider: true);

            var transitable = new HashSet<Vector2Int>();
            Pintar(c.Suelo, Entrada, piedra, transitable);
            Pintar(c.Suelo, PasilloCuarto, madera, transitable);
            Pintar(c.Suelo, Cuarto, madera, transitable);
            Pintar(c.Suelo, PuertaObrador, piedra, transitable);
            Pintar(c.Suelo, Frente, piedra, transitable);
            Pintar(c.Suelo, PasoFondo, piedra, transitable);
            Pintar(c.Suelo, Fondo, piedra, transitable);
            Muros(c.Paredes, transitable, null, muroArriba, muroCara);

            // Mesas de amasar del obrador.
            Pintar(c.Muebles, new RectInt(21, 4, 5, 1), mostrador);
            Pintar(c.Muebles, new RectInt(22, 12, 4, 1), mostrador);
        }

        static void ConstruirPersonajes()
        {
            Npc("Ernesto", PosErnesto, "ernesto", "Ernesto", "arriba", "R3_Ernesto", "[E] Mirar");
            var refugio = Npc("Refugio23", PosRefugio, "cuco_23", "Refugio23", "izquierda");
            Set(refugio.GetComponent<CinematicActor>(), ("startHidden", true));
            var aurelio = Npc("Aurelio20", PosAurelio, "aurelio_20", "Aurelio20", "izquierda", "R3_Aurelio20", "[E] Mirar");
            Set(aurelio.GetComponent<CinematicActor>(), ("startHidden", true));
        }

        static void ConstruirDecorado()
        {
            var p = Raiz("Decorado").transform;
            if (TilesetLPC.HayArte())
            {
                // Dos camas: la de Aurelio (vacía) a la izquierda y la de Cuco a la derecha.
                foreach (var x0 in new[] { 3f, 7f })
                    for (int fila = 0; fila < 3; fila++)
                        for (int col = 0; col < 2; col++)
                            Adorno(p, $"Cama_{x0}_{fila}_{col}", $"cama_{fila * 2 + col + 1}",
                                   new Vector3(x0 + col, 15.5f - fila, 0f), OrdenMueble);
                Adorno(p, "Buro", "silla", new Vector3(5.5f, 14.4f, 0f));
                // El horno: la boca da al frente del obrador; el fuego arde en el fondo.
                Adorno(p, "BocaHorno1", "horno", new Vector3(14.5f, 9.5f, 0f), OrdenMueble);
                Adorno(p, "BocaHorno2", "horno", new Vector3(15.5f, 9.5f, 0f), OrdenMueble);
                Adorno(p, "Fuego1", "horno_fuego", new Vector3(13.5f, 15.5f, 0f), OrdenMueble);
                Adorno(p, "Fuego2", "horno_fuego", new Vector3(14.5f, 15.5f, 0f), OrdenMueble);
                Adorno(p, "Fuego3", "horno_fuego", new Vector3(15.5f, 15.5f, 0f), OrdenMueble);
                Adorno(p, "Costales1", "pan", new Vector3(26.5f, 7.5f, 0f), solido: true);
                Adorno(p, "Costales2", "pan", new Vector3(25.5f, 7.5f, 0f), solido: true);
                Adorno(p, "Cajas", "cajas", new Vector3(12.5f, 7.5f, 0f), solido: true);
                Adorno(p, "Estante", "estante", new Vector3(20.5f, 15.5f, 0f), OrdenMueble);
                Adorno(p, "Maceta", "maceta", new Vector3(8.5f, 5.6f, 0f), solido: true);

                // El obrador ocupa 16 x 7 tiles en cada mitad y tenía cuatro cosas: se veía
                // como una bodega vacía, no como el taller donde Aurelio amasaba de madrugada.
                Adorno(p, "MesaAmasado1", "mesa", new Vector3(19.5f, 5.4f, 0f), solido: true);
                Adorno(p, "MesaAmasado2", "mesa", new Vector3(21.5f, 5.4f, 0f), solido: true);
                Adorno(p, "FrascosObrador", "frascos", new Vector3(23.5f, 7.5f, 0f), OrdenMueble + 1);
                Adorno(p, "CajasObrador", "cajas", new Vector3(27.5f, 4.5f, 0f), solido: true);
                Adorno(p, "EstanteFondo", "estante", new Vector3(26.5f, 15.5f, 0f), OrdenMueble);
                Adorno(p, "CostalesFondo", "pan", new Vector3(12.5f, 11.5f, 0f), solido: true);
                Adorno(p, "LamparaObrador", "lampara", new Vector3(17.5f, 15.6f, 0f), OrdenObjeto);

                // La entrada de la casa, por donde llega Ámbar.
                Adorno(p, "MesaEntrada", "mesa", new Vector3(8.5f, 2.6f, 0f), solido: true);
                Adorno(p, "SillaEntrada", "silla", new Vector3(7.5f, 2.7f, 0f));
            }
            AdornoSprite(p, "Lena", "Assets/_Proyecto/Art/Objetos/lena.png", new Vector3(16.5f, 8.2f, 0f), OrdenMueble + 1);

            // Luces: luna en la entrada y el fuego del horno, que parpadea.
            var luces = Raiz("Luces").transform;
            LuzPuntual(luces, "Luna", new Vector3(5f, 4f, 0f), new Color(0.6f, 0.7f, 1f), 0.5f, 6f);
            foreach (var (x, y) in new[] { (14.5f, 14.5f), (17f, 12.5f), (14.5f, 9.8f) })
            {
                string n = $"Fuego_{x}_{y}";
                LuzPuntual(luces, n, new Vector3(x, y, 0f), new Color(1f, 0.5f, 0.2f), 1.1f, 5f);
                Comp<Parpadeo>(luces.Find(n).gameObject);
            }
        }
    }
}

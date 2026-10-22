using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Recorta de los tilesets de OpenGameArt sólo los tiles que usan los mapas y los
    /// convierte en assets Tile. Las hojas se cortan en una sola pasada por textura.
    ///
    /// Coordenadas: (columna, fila) contando desde la ESQUINA SUPERIOR IZQUIERDA de la hoja,
    /// que es como se leen los tilesets. Unity cuenta desde abajo, así que se convierten aquí.
    /// </summary>
    public static class TilesetLPC
    {
        const string Interior = "Assets/_Proyecto/Art/Tilesets/tileset_interior/LPC_house_interior/interior.png";
        const string Castillo = "Assets/_Proyecto/Art/Tilesets/tileset_interior/castle.png";
        const string Casa     = "Assets/_Proyecto/Art/Tilesets/tileset_interior/house_inside.png";
        const string Plantas  = "Assets/_Proyecto/Art/Tilesets/tileset_flores/lpc-flowers-plants-fungi-wood/plants.png";
        const string Carpeta  = "Assets/_Proyecto/Art/Tiles";
        const int Lado = 32;

        /// <summary>Tiles que necesita el Mapa 0: clave -> (hoja, columna, fila).</summary>
        static readonly Dictionary<string, (string hoja, int cx, int cy)> Catalogo = new()
        {
            ["suelo_casa"]   = (Interior,  0,  3),   // duela de madera
            ["suelo_patio"]  = (Interior,  1,  4),   // empedrado del patio
            ["muro_arriba"]  = (Castillo,  3,  2),   // piedra: cara superior del muro
            ["muro_cara"]    = (Castillo,  3,  3),   // piedra: cara que ve el jugador (muro norte)
            ["mueble"]       = (Interior,  0,  6),   // alacena
            ["petalos"]      = (Plantas,   0,  7),   // pétalos naranjas (cempasúchil)
            ["petalos_ralo"] = (Plantas,   1,  7),   // los mismos, más dispersos
            // --- Recuerdo 1: la panadería (F14) ---
            ["suelo_losa"]       = (Interior, 0, 4),  // losa ajedrezada del salón de venta
            ["mostrador"]        = (Interior, 5, 3),  // cubierta del mostrador
            ["mostrador_frente"] = (Interior, 5, 4),  // frente del mostrador
            ["estante"]          = (Interior, 2, 7),  // repisa con charolas y platos
        };

        /// <summary>Sprites sueltos (no tiles) que usan los objetos de la escena.</summary>
        static readonly Dictionary<string, (string hoja, int cx, int cy)> Objetos = new()
        {
            ["pan"]      = (Interior,  6,  6),   // pan de muerto (memento)
            ["velas"]    = (Castillo,  2, 14),   // candelabro de la ofrenda
            ["estufa"]   = (Casa,      1, 11),   // fogón encendido de la cocina
            ["maceta"]   = (Casa,      7,  7),   // planta del patio
            // --- Recuerdo 1: la panadería (F14) ---
            ["horno"]    = (Interior, 12,  5),   // horno de ladrillo apagándose
            ["reloj"]    = (Interior,  1, 10),   // reloj de pared
            ["lampara"]  = (Interior, 11,  0),   // lámpara colgante
            ["frascos"]  = (Interior,  7,  4),   // frascos sobre el mostrador
            ["mesa"]     = (Interior, 13,  2),   // mesa redonda de la trastienda
            ["cajas"]    = (Interior,  3,  8),   // cajas de la trastienda
            ["vitrina"]  = (Casa,     12,  1),   // vitrina de pan
            ["silla"]    = (Casa,      7,  4),
            // --- Recuerdo 3: el obrador y el cuarto de los hermanos (F19) ---
            ["horno_fuego"] = (Interior, 11, 4),   // horno encendido
            ["cama_1"] = (Casa, 14, 6), ["cama_2"] = (Casa, 15, 6),   // cabecera
            ["cama_3"] = (Casa, 14, 7), ["cama_4"] = (Casa, 15, 7),   // cobija
            ["cama_5"] = (Casa, 14, 8), ["cama_6"] = (Casa, 15, 8),   // pie
        };

        static Dictionary<string, Sprite> cache;

        /// <summary>Corta lo necesario de cada hoja. Idempotente.</summary>
        public static void Preparar()
        {
            cache = new Dictionary<string, Sprite>();
            var todo = Catalogo.Concat(Objetos).ToList();

            foreach (var hoja in todo.Select(e => e.Value.hoja).Distinct())
            {
                var deEstaHoja = todo.Where(e => e.Value.hoja == hoja).ToList();
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(hoja);
                if (tex == null) { Debug.LogError($"[Tiles] Falta la hoja {hoja}"); continue; }

                var imp = (TextureImporter)AssetImporter.GetAtPath(hoja);
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Multiple;
                imp.spritePixelsPerUnit = Lado;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.maxTextureSize = 4096;
                imp.SaveAndReimport();

                int alto = AssetDatabase.LoadAssetAtPath<Texture2D>(hoja).height;
                var factoria = new SpriteDataProviderFactories();
                factoria.Init();
                var prov = factoria.GetSpriteEditorDataProviderFromObject(imp);
                prov.InitSpriteEditorDataProvider();

                var rects = deEstaHoja.Select(e => new SpriteRect
                {
                    name = e.Key,
                    rect = new Rect(e.Value.cx * Lado, alto - (e.Value.cy + 1) * Lado, Lado, Lado),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    // Estable por nombre (antes GUID.Generate): volver a cortar una hoja para
                    // añadir tiles de otro mapa no cambia la identidad de los ya usados.
                    spriteID = IdEstable(e.Key),
                }).ToArray();
                prov.SetSpriteRects(rects);
                prov.Apply();
                imp.SaveAndReimport();

                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(hoja))
                    if (o is Sprite s && (Catalogo.ContainsKey(s.name) || Objetos.ContainsKey(s.name)))
                        cache[s.name] = s;
            }

            var faltan = todo.Select(e => e.Key).Where(k => !cache.ContainsKey(k)).ToList();
            if (faltan.Count > 0) Debug.LogError($"[Tiles] No se cortaron: {string.Join(", ", faltan)}");
            else Debug.Log($"[Tiles] {cache.Count} sprites recortados de {todo.Select(e => e.Value.hoja).Distinct().Count()} hojas.");
        }

        public static UnityEngine.Sprite Sprite(string clave)
        {
            if (cache == null) Preparar();
            return cache != null && cache.TryGetValue(clave, out var s) ? s : null;
        }

        /// <summary>Tile con el sprite indicado. Si no hay arte, devuelve null y el llamador decide.</summary>
        public static TileBase Tile(string clave, bool conCollider)
        {
            var sp = Sprite(clave);
            if (sp == null) return null;

            System.IO.Directory.CreateDirectory(Carpeta);
            string ruta = $"{Carpeta}/{clave}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(ruta);
            bool nuevo = tile == null;
            if (nuevo) tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sp;
            tile.colliderType = conCollider ? UnityEngine.Tilemaps.Tile.ColliderType.Grid
                                            : UnityEngine.Tilemaps.Tile.ColliderType.None;
            if (nuevo) AssetDatabase.CreateAsset(tile, ruta);
            else EditorUtility.SetDirty(tile);
            return tile;
        }

        static GUID IdEstable(string nombre)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("MientrasArdaLaVela/tile/" + nombre));
            return new GUID(string.Concat(bytes.Select(b => b.ToString("x2"))));
        }

        public static bool HayArte() => AssetDatabase.LoadAssetAtPath<Texture2D>(Interior) != null
                                     && AssetDatabase.LoadAssetAtPath<Texture2D>(Castillo) != null
                                     && AssetDatabase.LoadAssetAtPath<Texture2D>(Plantas) != null;
    }
}

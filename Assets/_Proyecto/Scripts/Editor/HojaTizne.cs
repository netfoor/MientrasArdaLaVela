using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Corta tizne_dog.png ("[LPC] Cats and Dogs", bluecarrot16) para Tizne (F11).
    ///
    /// La hoja mide 512×256 con cuadros de 32 px: cuatro variantes de color de 4 columnas cada
    /// una (3 cuadros de caminata + 1 extra). Filas desde arriba: 0 derecha, 1 arriba,
    /// 2 abajo, 3 izquierda. Se usa la variante negra (columnas 12 a 14), que es la que
    /// corresponde a un xoloitzcuintle. Los sprites quedan como "tizne_&lt;dir&gt;_&lt;n&gt;".
    /// </summary>
    public static class HojaTizne
    {
        public const string Ruta = "Assets/_Proyecto/Art/Personajes/tizne_dog.png";

        /// <summary>
        /// Variantes con accesorios (F23), en el orden que espera Tizne.variantes. Son hojas de
        /// 96×128 (sólo la variante negra, 3 cuadros × 4 filas) que genera tools/arte/tizne_accesorios.py.
        /// </summary>
        public static readonly string[] Variantes = { "collar", "sombrero", "ambos" };
        static readonly string[] Direcciones = { "up", "left", "down", "right" };
        static string RutaVariante(string v) => $"Assets/_Proyecto/Art/Personajes/tizne_{v}.png";
        const int Lado = 32;
        const int ColumnaBase = 12;                         // variante negra
        const float PivoteY = 3f / Lado;                    // las patas quedan a 3 px del borde

        // dirección → fila de la hoja (contando desde arriba)
        static readonly (string dir, int fila)[] Filas = { ("right", 0), ("up", 1), ("down", 2), ("left", 3) };

        [MenuItem("Mientras arda la vela/Cortar hoja de Tizne")]
        public static void CortarDesdeMenu()
        {
            var s = Cortar();
            Debug.Log(s != null ? $"[HojaTizne] {s.Count} sprites." : "[HojaTizne] No se pudo cortar.");
        }

        public static bool Cortada() => Cortada(Ruta, "tizne");
        static bool Cortada(string ruta, string prefijo)
            => AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().Any(x => x.name == $"{prefijo}_down_0");

        /// <summary>Devuelve los 3 cuadros de una dirección (up, left, down, right), cortando si hace falta.</summary>
        public static Sprite[] Cuadros(string dir) => Cuadros(Ruta, "tizne", ColumnaBase, dir);

        static Sprite[] Cuadros(string ruta, string prefijo, int columna, string dir)
        {
            if (!File.Exists(ruta)) return null;
            if (!Cortada(ruta, prefijo)) Cortar(ruta, prefijo, columna);
            var todos = AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().ToDictionary(x => x.name, x => x);
            var r = new Sprite[3];
            for (int i = 0; i < 3; i++) todos.TryGetValue($"{prefijo}_{dir}_{i}", out r[i]);
            return r.All(x => x != null) ? r : null;
        }

        /// <summary>
        /// Los 36 cuadros de las variantes con accesorios: [variante][dirección up,left,down,right][cuadro].
        /// Null si falta alguna hoja (Tizne se queda entonces sin accesorios).
        /// </summary>
        public static Sprite[] CuadrosVariantes()
        {
            var lista = new List<Sprite>();
            foreach (var v in Variantes)
                foreach (var dir in Direcciones)
                {
                    var c = Cuadros(RutaVariante(v), $"tizne_{v}", 0, dir);
                    if (c == null) { Debug.LogWarning($"[HojaTizne] Falta la variante '{v}' ({RutaVariante(v)})."); return null; }
                    lista.AddRange(c);
                }
            return lista.ToArray();
        }

        public static List<Sprite> Cortar() => Cortar(Ruta, "tizne", ColumnaBase);

        static List<Sprite> Cortar(string ruta, string prefijo, int columnaBase)
        {
            if (!File.Exists(ruta)) { Debug.LogError($"[HojaTizne] No existe {ruta}"); return null; }

            var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.spritePixelsPerUnit = 32;                  // un cuadro = 1 tile
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();

            int alto = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta).height;

            var factoria = new SpriteDataProviderFactories();
            factoria.Init();
            var prov = factoria.GetSpriteEditorDataProviderFromObject(imp);
            prov.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            foreach (var (dir, fila) in Filas)
                for (int c = 0; c < 3; c++)
                {
                    string nombre = $"{prefijo}_{dir}_{c}";
                    rects.Add(new SpriteRect
                    {
                        name = nombre,
                        rect = new Rect((columnaBase + c) * Lado, alto - (fila + 1) * Lado, Lado, Lado),
                        alignment = SpriteAlignment.Custom,
                        pivot = new Vector2(0.5f, PivoteY),
                        spriteID = IdEstable(nombre),
                    });
                }
            prov.SetSpriteRects(rects.ToArray());
            prov.Apply();
            imp.SaveAndReimport();

            return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().OrderBy(x => x.name).ToList();
        }

        /// <summary>GUID derivado del nombre, igual que HojaLPC: recortar no rompe referencias.</summary>
        static GUID IdEstable(string nombre)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("MientrasArdaLaVela/" + nombre));
            return new GUID(string.Concat(bytes.Select(b => b.ToString("x2"))));
        }
    }
}

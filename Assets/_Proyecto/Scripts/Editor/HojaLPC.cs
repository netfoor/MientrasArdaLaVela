using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Corta una hoja universal del generador LPC (832×3456, cuadros de 64 px) dejando sólo
    /// el bloque de caminata: filas 9 a 12 contando desde arriba (arriba, izquierda, abajo,
    /// derecha), 9 cuadros cada una, con el pivote en los pies.
    /// Los sprites quedan nombrados "&lt;prefijo&gt;_&lt;dirección&gt;_&lt;cuadro&gt;".
    /// </summary>
    public static class HojaLPC
    {
        public const int Lado = 64;
        public const int FilaWalk = 8;     // índice base 0 desde arriba
        public const int Cuadros = 9;
        const float PivoteY = 2f / Lado;   // los pies quedan a 2 px del borde inferior

        public static readonly string[] Direcciones = { "up", "left", "down", "right" };

        /// <summary>Corta la hoja y devuelve los sprites en orden (arriba, izquierda, abajo, derecha).</summary>
        public static List<Sprite> Cortar(string ruta, string prefijo)
        {
            if (!File.Exists(ruta)) { Debug.LogError($"[HojaLPC] No existe {ruta}"); return null; }

            var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.spritePixelsPerUnit = Lado / 2;      // 32 PPU: el cuadro de 64 px mide 2 unidades
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.maxTextureSize = 4096;
            imp.SaveAndReimport();

            int alto = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta).height;

            var factoria = new SpriteDataProviderFactories();
            factoria.Init();
            var prov = factoria.GetSpriteEditorDataProviderFromObject(imp);
            prov.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            for (int d = 0; d < Direcciones.Length; d++)
                for (int c = 0; c < Cuadros; c++)
                {
                    string nombre = $"{prefijo}_{Direcciones[d]}_{c}";
                    rects.Add(new SpriteRect
                    {
                        name = nombre,
                        // La hoja se cuenta desde arriba; Unity desde abajo.
                        rect = new Rect(c * Lado, alto - (FilaWalk + d + 1) * Lado, Lado, Lado),
                        alignment = SpriteAlignment.Custom,
                        pivot = new Vector2(0.5f, PivoteY),
                        spriteID = IdEstable(nombre),
                    });
                }
            prov.SetSpriteRects(rects.ToArray());
            prov.Apply();
            imp.SaveAndReimport();

            var porNombre = AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>()
                                         .ToDictionary(s => s.name, s => s);
            var orden = new List<Sprite>();
            foreach (var r in rects)
            {
                if (!porNombre.TryGetValue(r.name, out var s))
                {
                    Debug.LogError($"[HojaLPC] No se importó el sprite '{r.name}'.");
                    return null;
                }
                orden.Add(s);
            }
            return orden;
        }

        /// <summary>
        /// GUID derivado del nombre, no aleatorio: al volver a cortar la hoja los sprites
        /// conservan su identidad y no se rompen las referencias de clips ni escenas.
        /// </summary>
        static GUID IdEstable(string nombre)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("MientrasArdaLaVela/" + nombre));
            return new GUID(string.Concat(bytes.Select(b => b.ToString("x2"))));
        }

        /// <summary>Corta las 13 hojas de personaje. El prefijo sale del nombre del archivo sin "_walk".</summary>
        [MenuItem("Mientras arda la vela/Cortar todas las hojas de personaje")]
        public static void CortarTodas()
        {
            const string carpeta = "Assets/_Proyecto/Art/Personajes";
            int ok = 0;
            foreach (var ruta in Directory.GetFiles(carpeta, "*_walk.png").OrderBy(x => x))
            {
                string prefijo = Path.GetFileNameWithoutExtension(ruta).Replace("_walk", "");
                var sprites = Cortar(ruta.Replace('\\', '/'), prefijo);
                if (sprites != null) { ok++; Debug.Log($"  · {prefijo}: {sprites.Count} sprites"); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[HojaLPC] {ok} hojas cortadas.");
        }
    }
}

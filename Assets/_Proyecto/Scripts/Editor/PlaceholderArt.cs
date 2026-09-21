using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Genera sprites y tiles de relleno (cuadros de color) mientras no hay arte definitivo.
    /// Todo se importa con los ajustes de pixel art del LEEME §3: PPU 32, Point, sin compresión.
    /// </summary>
    public static class PlaceholderArt
    {
        public const string Carpeta = "Assets/_Proyecto/Art/Placeholders";

        /// <summary>Devuelve (creando si hace falta) un sprite cuadrado de 32×32 del color dado.</summary>
        public static UnityEngine.Sprite Sprite(string nombre, Color color, bool borde = true)
        {
            string ruta = $"{Carpeta}/{nombre}.png";
            var existente = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(ruta);
            if (existente != null) return existente;

            Directory.CreateDirectory(Carpeta);

            const int lado = 32;
            var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            var oscuro = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, color.a);
            for (int y = 0; y < lado; y++)
                for (int x = 0; x < lado; x++)
                {
                    bool esBorde = borde && (x == 0 || y == 0 || x == lado - 1 || y == lado - 1);
                    tex.SetPixel(x, y, esBorde ? oscuro : color);
                }
            tex.Apply();
            File.WriteAllBytes(ruta, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
            AjustarImportador(ruta);
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(ruta);
        }

        /// <summary>Devuelve (creando si hace falta) un Tile con el sprite del color dado.</summary>
        public static UnityEngine.Tilemaps.Tile Tile(string nombre, Color color)
        {
            string ruta = $"{Carpeta}/Tile_{nombre}.asset";
            var existente = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(ruta);
            if (existente != null) return existente;

            var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = Sprite(nombre, color);
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Sprite;
            AssetDatabase.CreateAsset(tile, ruta);
            return tile;
        }

        /// <summary>Ajustes de pixel art del LEEME §3 para cualquier textura del proyecto.</summary>
        public static void AjustarImportador(string ruta)
        {
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 32;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Aplica los ajustes de pixel art del LEEME §3 a toda textura que entre en
    /// Assets/_Proyecto/Art: PPU 32, Filter Mode Point, sin compresión, sin mipmaps.
    /// Sólo actúa en la PRIMERA importación, para no pisar los ajustes que se toquen
    /// a mano después (por ejemplo el corte en múltiples sprites de las hojas LPC).
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        const string Carpeta = "Assets/_Proyecto/Art/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Carpeta)) return;

            var imp = (TextureImporter)assetImporter;
            if (!imp.importSettingsMissing) return;   // ya tiene .meta: respetarlo

            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 32;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            // Las hojas LPC miden 832×3456: con el máximo por defecto (2048) Unity las
            // reescalaría y se rompería la rejilla de 64 px.
            imp.maxTextureSize = 4096;
        }
    }
}

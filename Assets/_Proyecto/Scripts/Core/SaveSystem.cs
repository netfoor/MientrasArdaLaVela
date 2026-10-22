using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MientrasArdaLaVela
{
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 3;
        public int version = CurrentVersion;
        public string currentScene = SceneNames.Ofrenda;
        public List<string> fragments = new List<string>();       // IDs de fragmentos reunidos
        public List<string> revealed = new List<string>();        // objetos revelados con la Luz de vela
        public List<string> seenDialogues = new List<string>();   // diálogos de una sola vez ya vistos (B10, v2)
        public List<string> resolvedEchoes = new List<string>();  // Ecos desmentidos
        public List<string> completedMaps = new List<string>();   // mapas terminados
        public List<string> petals = new List<string>();          // pétalos de cempasúchil reunidos (F24)
        public List<string> talked = new List<string>();          // personajes con los que ya se habló (logro "Todas las voces", v3)
        public int mosaicHints;                                   // pistas pedidas en los mosaicos (logro "sin pistas", F23)
    }

    /// <summary>
    /// Guardado local en JSON (Application.persistentDataPath).
    ///
    /// B05: la escritura es atómica. Se escribe primero en "partida.json.tmp" y después se
    /// sustituye el archivo real de un solo golpe, dejando la versión anterior como
    /// "partida.json.bak". Si el juego se cierra a mitad de la escritura, el archivo real
    /// no se toca; y si aun así quedara dañado, Load() recurre al respaldo.
    /// </summary>
    public static class SaveSystem
    {
        const string FileName = "partida.json";
        static string PathFile => Path.Combine(Application.persistentDataPath, FileName);
        static string PathTmp => PathFile + ".tmp";
        static string PathBak => PathFile + ".bak";

        public static bool HasSave => File.Exists(PathFile) || File.Exists(PathBak);

        public static void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(PathTmp, JsonUtility.ToJson(data, true));
                if (File.Exists(PathFile)) File.Replace(PathTmp, PathFile, PathBak);
                else File.Move(PathTmp, PathFile);
            }
            catch (Exception e) { Debug.LogError($"[SaveSystem] No se pudo guardar: {e.Message}"); }
        }

        public static SaveData Load()
        {
            var datos = Leer(PathFile);
            if (datos == null && File.Exists(PathBak))
            {
                Debug.LogWarning("[SaveSystem] Se usa el respaldo partida.json.bak.");
                datos = Leer(PathBak);
            }
            return datos == null ? null : Migrar(datos);
        }

        static SaveData Leer(string ruta)
        {
            if (!File.Exists(ruta)) return null;
            try
            {
                var datos = JsonUtility.FromJson<SaveData>(File.ReadAllText(ruta));
                if (datos == null) throw new Exception("archivo vacío");
                return datos;
            }
            catch (Exception e) { Debug.LogError($"[SaveSystem] Archivo dañado ({Path.GetFileName(ruta)}): {e.Message}"); return null; }
        }

        /// <summary>
        /// Actualiza partidas de versiones anteriores.
        /// v1 → v2 (B10): las banderas de diálogo ("M0_prologo", "M0_tut_vela"...) se guardaban
        /// en revealed. Los objetos revelados de verdad terminan en "_rev"; lo demás se mueve.
        /// </summary>
        static SaveData Migrar(SaveData d)
        {
            if (d.seenDialogues == null) d.seenDialogues = new List<string>();
            if (d.petals == null) d.petals = new List<string>();
            if (d.talked == null) d.talked = new List<string>();   // v2 → v3: lista nueva, empieza vacía
            if (d.version < 2)
            {
                for (int i = d.revealed.Count - 1; i >= 0; i--)
                {
                    if (d.revealed[i].EndsWith("_rev")) continue;
                    if (!d.seenDialogues.Contains(d.revealed[i])) d.seenDialogues.Add(d.revealed[i]);
                    d.revealed.RemoveAt(i);
                }
            }
            d.version = SaveData.CurrentVersion;
            return d;
        }

        public static void Delete()
        {
            foreach (var ruta in new[] { PathFile, PathTmp, PathBak })
                if (File.Exists(ruta)) File.Delete(ruta);
        }
    }
}

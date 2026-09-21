using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MientrasArdaLaVela
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string currentScene = SceneNames.Ofrenda;
        public List<string> fragments = new List<string>();       // IDs de fragmentos reunidos
        public List<string> revealed = new List<string>();        // objetos revelados con la Luz de vela
        public List<string> resolvedEchoes = new List<string>();  // Ecos desmentidos
        public List<string> completedMaps = new List<string>();   // mapas terminados
        public List<string> petals = new List<string>();          // pétalos (Sprint 3)
    }

    /// <summary>Guardado local en JSON (Application.persistentDataPath).</summary>
    public static class SaveSystem
    {
        const string FileName = "partida.json";
        static string PathFile => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(PathFile);

        public static void Save(SaveData data)
        {
            try { File.WriteAllText(PathFile, JsonUtility.ToJson(data, true)); }
            catch (Exception e) { Debug.LogError($"[SaveSystem] No se pudo guardar: {e.Message}"); }
        }

        public static SaveData Load()
        {
            if (!HasSave) return null;
            try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFile)); }
            catch (Exception e) { Debug.LogError($"[SaveSystem] Archivo dañado: {e.Message}"); return null; }
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(PathFile);
        }
    }
}

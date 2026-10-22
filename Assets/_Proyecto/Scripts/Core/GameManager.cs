using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Estado global de la partida. Persiste entre escenas y autoguarda
    /// al entrar a un mapa y al reunir fragmentos (Documento de Diseño: Guardar/Cargar).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData Data { get; private set; } = new SaveData();

        public event Action<string> FragmentCollected;
        public event Action<string> EchoResolved;
        public event Action<string> MapCompleted;
        public event Action<string> PetalCollected;

        /// <summary>Crea el GameManager si se abre una escena de juego directamente desde el editor.</summary>
        public static GameManager Ensure()
        {
            if (Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // DontDestroyOnLoad lanza InvalidOperationException fuera de Play Mode; el guardia
            // permite instanciar el GameManager en pruebas EditMode sin cambiar nada en juego.
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneNames.MainMenu) return;
            Data.currentScene = scene.name;
            Save(); // autoguardado al entrar a cada mapa
        }

        // ---------- Partida ----------
        public void NewGame()
        {
            SaveSystem.Delete();
            Data = new SaveData();
            SceneManager.LoadScene(SceneNames.Ofrenda);
        }

        public void ContinueGame()
        {
            Data = SaveSystem.Load() ?? new SaveData();
            SceneManager.LoadScene(string.IsNullOrEmpty(Data.currentScene) ? SceneNames.Ofrenda : Data.currentScene);
        }

        public void Save() => SaveSystem.Save(Data);

        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

        public void GoToMainMenu()
        {
            Save();
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        // ---------- Fragmentos ----------
        public bool HasFragment(string id) => !string.IsNullOrEmpty(id) && Data.fragments.Contains(id);

        public void CollectFragment(string id)
        {
            if (string.IsNullOrEmpty(id) || Data.fragments.Contains(id)) return;
            Data.fragments.Add(id);
            Save();
            FragmentCollected?.Invoke(id);
        }

        /// <summary>Cuenta fragmentos cuyo ID empieza con el prefijo del mapa (p. ej. "M0_").</summary>
        public int CountFragments(string mapPrefix)
        {
            int n = 0;
            foreach (var f in Data.fragments) if (f.StartsWith(mapPrefix)) n++;
            return n;
        }

        // ---------- Luz de vela ----------
        public bool IsRevealed(string id) => Data.revealed.Contains(id);
        public void MarkRevealed(string id)
        {
            if (Data.revealed.Contains(id)) return;
            Data.revealed.Add(id);
            Save();
        }

        // ---------- Diálogos de una sola vez (B10) ----------
        public bool HasSeenDialogue(string id) => !string.IsNullOrEmpty(id) && Data.seenDialogues.Contains(id);
        public void MarkDialogueSeen(string id)
        {
            if (string.IsNullOrEmpty(id) || Data.seenDialogues.Contains(id)) return;
            Data.seenDialogues.Add(id);
            Save();
        }

        // ---------- Ecos ----------
        public bool IsEchoResolved(string id) => Data.resolvedEchoes.Contains(id);
        public void ResolveEcho(string id)
        {
            if (Data.resolvedEchoes.Contains(id)) return;
            Data.resolvedEchoes.Add(id);
            Save();
            EchoResolved?.Invoke(id);
            Logros.Revisar(Data);
        }

        // ---------- Pétalos de cempasúchil (F24) ----------
        public bool HasPetal(string id) => !string.IsNullOrEmpty(id) && Data.petals.Contains(id);

        public void CollectPetal(string id)
        {
            if (string.IsNullOrEmpty(id) || Data.petals.Contains(id)) return;
            Data.petals.Add(id);
            Save();
            PetalCollected?.Invoke(id);
            Logros.Revisar(Data);
        }

        /// <summary>Pétalos reunidos de un mapa (prefijo "M0_", "R1_"...) o, con null, de todo el juego.</summary>
        public int CountPetals(string mapPrefix = null)
        {
            int n = 0;
            foreach (var p in Data.petals) if (mapPrefix == null || p.StartsWith(mapPrefix)) n++;
            return n;
        }

        // ---------- Personajes (logro "Todas las voces", F23) ----------
        public bool HasTalked(string node) => !string.IsNullOrEmpty(node) && Data.talked.Contains(node);
        public void MarkTalked(string node)
        {
            if (string.IsNullOrEmpty(node) || Data.talked.Contains(node)) return;
            Data.talked.Add(node);
            Save();
            Logros.Revisar(Data);
        }

        // ---------- Mosaicos (F12) ----------
        /// <summary>Cuenta una pista de Tizne dentro de un mosaico (logro "sin pistas").</summary>
        public void RegisterMosaicHint()
        {
            Data.mosaicHints++;
            Save();
        }

        // ---------- Mapas ----------
        public bool IsMapCompleted(string mapId) => Data.completedMaps.Contains(mapId);
        public void CompleteMap(string mapId)
        {
            if (Data.completedMaps.Contains(mapId)) return;
            Data.completedMaps.Add(mapId);
            Save();
            MapCompleted?.Invoke(mapId);
            Logros.Revisar(Data);   // F23: los logros de historia salen de los mapas completados
        }
    }
}

using System;
using UnityEngine;
using Yarn.Unity;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Punto único de acceso al DialogueRunner de Yarn Spinner.
    /// Congela al jugador mientras hay diálogo y permite esperar a que termine.
    /// Colocar en el mismo GameObject que el DialogueRunner (prefab "Dialogue System").
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public class DialogueBridge : MonoBehaviour
    {
        public static DialogueBridge Instance { get; private set; }
        public static bool IsBusy => Instance != null && Instance.runner.IsDialogueRunning;

        DialogueRunner runner;
        Action pendingOnComplete;

        void Awake()
        {
            Instance = this;
            runner = GetComponent<DialogueRunner>();
            // Yarn Spinner 3.x declara onDialogueComplete como UnityEvent anulable.
            if (runner.onDialogueComplete == null) runner.onDialogueComplete = new UnityEngine.Events.UnityEvent();
            runner.onDialogueComplete.AddListener(OnComplete);
        }

        void OnDestroy()
        {
            if (runner != null) runner.onDialogueComplete.RemoveListener(OnComplete);
            if (Instance == this) Instance = null;
        }

        /// <summary>Inicia un nodo de Yarn. onComplete se ejecuta al terminar.</summary>
        public bool Play(string node, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(node) || runner.IsDialogueRunning) return false;
            // En Yarn Spinner 3.x el Yarn Project es anulable; sin él, Dialogue.NodeExists lanzaría excepción.
            if (runner.YarnProject == null)
            {
                Debug.LogError("[Dialogue] El DialogueRunner no tiene asignado un Yarn Project.", this);
                onComplete?.Invoke();
                return false;
            }
            if (!runner.Dialogue.NodeExists(node)) { Debug.LogWarning($"[Dialogue] No existe el nodo '{node}'."); onComplete?.Invoke(); return false; }
            pendingOnComplete = onComplete;
            // StartDialogue devuelve YarnTask en 3.x (antes era void): se lanza sin esperar.
            _ = runner.StartDialogue(node);
            return true;
        }

        void OnComplete()
        {
            var cb = pendingOnComplete;
            pendingOnComplete = null;
            cb?.Invoke();
        }
    }
}

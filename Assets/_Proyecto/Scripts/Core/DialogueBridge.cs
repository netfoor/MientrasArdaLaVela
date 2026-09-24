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
        /// <summary>
        /// Incluye el hueco entre Play() y el arranque real del diálogo. En Yarn Spinner 3.x,
        /// StartDialogue es asíncrono y sólo llama a Dialogue.Continue() al final, tras esperar
        /// a los presentadores: hasta entonces IsDialogueRunning sigue en false y el jugador
        /// podía seguir caminando e incluso lanzar otra interacción.
        /// </summary>
        public static bool IsBusy => Instance != null &&
                                     (Instance.arrancando || Instance.runner.IsDialogueRunning);

        DialogueRunner runner;
        Action pendingOnComplete;
        bool arrancando;
        float esperandoDesde;

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
            arrancando = true;                 // se marca YA, de forma síncrona
            esperandoDesde = Time.unscaledTime;
            // StartDialogue devuelve YarnTask en 3.x (antes era void): se lanza sin esperar.
            _ = runner.StartDialogue(node);
            return true;
        }

        void Update()
        {
            if (!arrancando) return;
            // El diálogo ya arrancó de verdad: a partir de aquí manda IsDialogueRunning.
            if (runner.IsDialogueRunning) { arrancando = false; return; }
            // Red de seguridad: si StartDialogue se rindió sin avisar (por ejemplo, un Yarn
            // Project sin programa), sin esto el jugador quedaría congelado para siempre.
            if (Time.unscaledTime - esperandoDesde > 1f)
            {
                Debug.LogWarning("[Dialogue] El diálogo no arrancó en 1 s; se libera al jugador.");
                arrancando = false;
                OnComplete();
            }
        }

        void OnComplete()
        {
            arrancando = false;
            var cb = pendingOnComplete;
            pendingOnComplete = null;
            cb?.Invoke();
        }
    }
}

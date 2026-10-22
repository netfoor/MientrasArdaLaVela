using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Transición de recuerdo (F13): "FUNDIDO A BLANCO" del guion. La pantalla se funde a un
    /// blanco cálido de vela, se carga la escena siguiente y se vuelve a abrir.
    /// Se crea sola la primera vez que se usa y sobrevive a los cambios de escena.
    /// Si la escena todavía no existe en la lista de compilación (los recuerdos se construyen
    /// en el Sprint 2), se abre de nuevo sobre la misma escena y se avisa en la consola.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        static SceneTransition instance;
        public static bool Busy => instance != null && instance.ocupado;

        static readonly Color Blanco = new Color(1f, 0.96f, 0.86f, 1f);   // blanco de llama
        const float TiempoFundido = 1.1f;

        Image cortina;
        bool ocupado;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarEstaticos() => instance = null;   // Reload Domain está desactivado

        /// <summary>Funde a blanco, carga la escena y vuelve a abrir.</summary>
        public static void Go(string sceneName, Action ifMissing = null)
        {
            var t = Asegurar();
            if (t.ocupado) return;
            t.StartCoroutine(t.Rutina(sceneName, ifMissing));
        }

        static SceneTransition Asegurar()
        {
            if (instance != null) return instance;

            var go = new GameObject("SceneTransition");
            DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;                     // por encima del HUD, diálogo y mosaico
            go.AddComponent<GraphicRaycaster>();            // la cortina bloquea clics mientras dura

            var velo = new GameObject("Cortina", typeof(RectTransform));
            var rt = (RectTransform)velo.transform;
            rt.SetParent(go.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            instance = go.AddComponent<SceneTransition>();
            instance.cortina = velo.AddComponent<Image>();
            instance.cortina.color = new Color(Blanco.r, Blanco.g, Blanco.b, 0f);
            instance.cortina.raycastTarget = false;
            return instance;
        }

        IEnumerator Rutina(string escena, Action ifMissing)
        {
            ocupado = true;
            cortina.raycastTarget = true;
            Sonidos.Play("S08");   // la vela se aviva
            PlayerMovement.Locked = true;
            yield return Fundir(0f, 1f);

            bool existe = !string.IsNullOrEmpty(escena) && Application.CanStreamedLevelBeLoaded(escena);
            if (existe)
            {
                var op = SceneManager.LoadSceneAsync(escena);
                while (op != null && !op.isDone) yield return null;
                yield return new WaitForSecondsRealtime(0.3f);
            }
            else
            {
                Debug.LogWarning($"[Transición] La escena '{escena}' todavía no está en la lista de compilación; " +
                                 "se vuelve a la escena actual.");
                yield return new WaitForSecondsRealtime(0.5f);
            }

            PlayerMovement.Locked = false;
            yield return Fundir(1f, 0f);
            cortina.raycastTarget = false;
            ocupado = false;
            if (!existe) ifMissing?.Invoke();
        }

        IEnumerator Fundir(float desde, float hasta)
        {
            for (float t = 0f; t < TiempoFundido; t += Time.unscaledDeltaTime)
            {
                Alfa(Mathf.SmoothStep(desde, hasta, t / TiempoFundido));
                yield return null;
            }
            Alfa(hasta);
        }

        void Alfa(float a) => cortina.color = new Color(Blanco.r, Blanco.g, Blanco.b, a);
    }
}

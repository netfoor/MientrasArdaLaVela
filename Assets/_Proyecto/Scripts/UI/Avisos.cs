using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Avisos breves arriba al centro de la pantalla (F23/F24): logro desbloqueado, pétalo
    /// reunido y códigos secretos aceptados. Existe en todas las escenas y se construye sola.
    /// Los avisos se ponen en cola; usan tiempo real para verse también con el juego en pausa.
    /// </summary>
    public class Avisos : MonoBehaviour
    {
        static Avisos instancia;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => instancia = null;   // Reload Domain está desactivado

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (instancia != null) return;
            var go = new GameObject("Avisos");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<Avisos>();
        }

        /// <summary>Muestra un aviso (título en dorado y una línea de texto).</summary>
        public static void Mostrar(string titulo, string texto, float duracion = 3.2f)
        {
            if (instancia == null) Crear();
            instancia.cola.Enqueue((titulo, texto, duracion));
        }

        readonly Queue<(string titulo, string texto, float duracion)> cola = new Queue<(string, string, float)>();
        CanvasGroup grupo;
        TMP_Text txtTitulo, txtCuerpo;
        bool mostrando;
        GameManager suscrito;

        void Awake()
        {
            instancia = this;
            Construir();
            Logros.Desbloqueado += AlDesbloquear;
        }

        void OnDestroy()
        {
            Logros.Desbloqueado -= AlDesbloquear;
            if (suscrito != null) suscrito.PetalCollected -= AlReunirPetalo;
        }

        void AlDesbloquear(Logros.Logro l)
        {
            Sonidos.Play("S12", 0.95f);
            Mostrar("Logro desbloqueado", $"<b>{l.Titulo}</b>  ·  {l.Recompensa}", 4.5f);
        }

        void AlReunirPetalo(string id)
        {
            var gm = GameManager.Instance;
            int us = id.IndexOf('_');
            string prefijo = us > 0 ? id.Substring(0, us + 1) : "";
            Mostrar("Pétalo de cempasúchil",
                    $"{gm.CountPetals(prefijo)} de {Logros.PetalosPorMapa} en este recuerdo  ·  {gm.CountPetals()} de {Logros.TotalPetalos} en total",
                    2.6f);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm != suscrito)
            {
                if (suscrito != null) suscrito.PetalCollected -= AlReunirPetalo;
                gm.PetalCollected += AlReunirPetalo;
                suscrito = gm;
            }
            if (!mostrando && cola.Count > 0) StartCoroutine(Presentar(cola.Dequeue()));
        }

        IEnumerator Presentar((string titulo, string texto, float duracion) a)
        {
            mostrando = true;
            txtTitulo.text = a.titulo;
            txtCuerpo.text = a.texto;
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 0.35f) { grupo.alpha = k; yield return null; }
            grupo.alpha = 1f;
            for (float t = 0f; t < a.duracion; t += Time.unscaledDeltaTime) yield return null;
            for (float k = 1f; k > 0f; k -= Time.unscaledDeltaTime / 0.5f) { grupo.alpha = k; yield return null; }
            grupo.alpha = 0f;
            mostrando = false;
        }

        void Construir()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1150;      // encima de los créditos finales (1100)
            var escala = gameObject.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 1f;

            var tarjeta = new GameObject("Tarjeta", typeof(RectTransform)).GetComponent<RectTransform>();
            tarjeta.SetParent(transform, false);
            tarjeta.anchorMin = tarjeta.anchorMax = new Vector2(0.5f, 1f);
            tarjeta.pivot = new Vector2(0.5f, 1f);
            tarjeta.anchoredPosition = new Vector2(0f, -40f);
            tarjeta.sizeDelta = new Vector2(900f, 120f);
            var fondo = tarjeta.gameObject.AddComponent<Image>();
            fondo.color = new Color(0.10f, 0.07f, 0.12f, 0.9f);
            fondo.raycastTarget = false;
            grupo = tarjeta.gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;

            var borde = new GameObject("Borde", typeof(RectTransform)).GetComponent<RectTransform>();
            borde.SetParent(tarjeta, false);
            borde.anchorMin = new Vector2(0f, 0f); borde.anchorMax = new Vector2(0f, 1f);
            borde.pivot = new Vector2(0f, 0.5f);
            borde.sizeDelta = new Vector2(8f, 0f);
            var img = borde.gameObject.AddComponent<Image>();
            img.color = new Color(0.95f, 0.62f, 0.22f);
            img.raycastTarget = false;

            txtTitulo = Texto(tarjeta, 30f, new Vector2(0f, -14f), new Color(1f, 0.84f, 0.5f));
            txtCuerpo = Texto(tarjeta, 28f, new Vector2(0f, -60f), new Color(0.95f, 0.92f, 0.86f));
        }

        static TMP_Text Texto(RectTransform padre, float tam, Vector2 pos, Color color)
        {
            var rt = new GameObject("Texto", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(padre, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(860f, 46f);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize = tam;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.raycastTarget = false;
            return t;
        }
    }
}

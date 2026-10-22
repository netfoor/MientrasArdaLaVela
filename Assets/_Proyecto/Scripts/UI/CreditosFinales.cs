using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Créditos finales tras el Epílogo (F21): pantalla negra, texto que sube despacio con el
    /// tema principal (M01) y, al final, el resumen de la partida. Se crea sola con
    /// CreditosFinales.Mostrar() (comando de Yarn &lt;&lt;creditos&gt;&gt;). Al terminar vuelve al menú.
    /// La atribución completa de recursos está en Créditos del menú principal (CREDITOS.txt).
    /// </summary>
    public class CreditosFinales : MonoBehaviour
    {
        public static bool Activos { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => Activos = false;

        public static void Mostrar()
        {
            if (Activos) return;
            var go = new GameObject("Créditos finales");
            DontDestroyOnLoad(go);
            go.AddComponent<CreditosFinales>();
        }

        CanvasGroup grupo;
        RectTransform rodillo;
        TMP_Text pie;
        float alto = 3000f;

        void Awake()
        {
            Activos = true;
            var gm = GameManager.Ensure();
            gm.CompleteMap("EP");                // el juego queda terminado (logro "Dos velas", F23)
            PlayerMovement.Locked = true;
            Musica.Tocar("M01", 3f);
            Construir(gm);
            StartCoroutine(Rodar());
        }

        void OnDestroy() => Activos = false;

        void Construir(GameManager gm)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1100;          // encima de todo, incluso de la transición
            var escala = gameObject.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 1f;
            grupo = gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;

            var fondo = new GameObject("Fondo", typeof(RectTransform)).GetComponent<RectTransform>();
            fondo.SetParent(transform, false);
            fondo.anchorMin = Vector2.zero; fondo.anchorMax = Vector2.one; fondo.offsetMin = fondo.offsetMax = Vector2.zero;
            fondo.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.015f, 0.03f, 1f);

            rodillo = new GameObject("Rodillo", typeof(RectTransform)).GetComponent<RectTransform>();
            rodillo.SetParent(transform, false);
            rodillo.anchorMin = rodillo.anchorMax = new Vector2(0.5f, 0f);
            rodillo.pivot = new Vector2(0.5f, 1f);
            rodillo.sizeDelta = new Vector2(1400f, 3200f);
            rodillo.anchoredPosition = new Vector2(0f, 0f);
            var t = rodillo.gameObject.AddComponent<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.Top;
            t.fontSize = 34f;
            t.color = new Color(0.95f, 0.9f, 0.8f);
            t.text = Texto(gm);
            alto = t.GetPreferredValues(t.text, 1400f, 0f).y + 40f;
            rodillo.sizeDelta = new Vector2(1400f, alto);

            var rtPie = new GameObject("Pie", typeof(RectTransform)).GetComponent<RectTransform>();
            rtPie.SetParent(transform, false);
            rtPie.anchorMin = rtPie.anchorMax = new Vector2(0.5f, 0f);
            rtPie.anchoredPosition = new Vector2(0f, 60f);
            rtPie.sizeDelta = new Vector2(1200f, 60f);
            pie = rtPie.gameObject.AddComponent<TextMeshProUGUI>();
            pie.alignment = TextAlignmentOptions.Center;
            pie.fontSize = 30f;
            pie.color = new Color(1f, 0.84f, 0.5f, 0f);
            pie.text = "[E] Volver al menú";
        }

        static string Texto(GameManager gm)
        {
            int fragmentos = 0, total = 0, ecos = 0, totalEcos = 0;
            foreach (var r in Diario.Recuerdos)
            {
                foreach (var f in r.Fragmentos) { total++; if (gm.HasFragment(f.Id)) fragmentos++; }
                foreach (var e in r.Ecos) { totalEcos++; if (gm.IsEchoResolved(e.Id)) ecos++; }
            }
            var sb = new StringBuilder();
            sb.Append("<size=72><color=#FFD27F>Mientras arda la vela</color></size>\n\n\n");
            sb.Append("<size=30><i>Si uno se pierde, el otro lo busca con una vela.</i></size>\n\n\n\n");
            sb.Append("<size=40><b>Diseño, programación, arte y música</b></size>\nFortino Romero Mantilla\n\n\n");
            sb.Append("<b>Personajes</b>\nÁmbar, guía de ánimas\nTizne, su xoloitzcuintle\nDon Aurelio Cuautle\nRefugio «Cuco» Cuautle\nRosa · Marisol · Lucía · Don Ernesto\n\n\n");
            sb.Append("<b>Proceso de desarrollo</b>\nHuddle (Morales Urrutia et al., 2010)\n\n");
            sb.Append("<b>Ingeniería de Software</b>\nUniversidad Tecnológica de Puebla\nMtro. José Francisco Espinosa Garita\n\n\n");
            sb.Append("<b>Motor y herramientas</b>\nUnity 6 · Yarn Spinner (MIT)\n\n");
            sb.Append("<b>Personajes y tilesets</b>\nLiberated Pixel Cup (LPC) y sus artistas\nCC-BY-SA 3.0 / GPL 3.0 / OGA-BY\n<size=26>(atribución completa en Créditos del menú principal)</size>\n\n\n\n");
            sb.Append("<size=40><b>Tu recorrido</b></size>\n");
            sb.Append($"Fragmentos de memoria: {fragmentos} / {total}\n");
            sb.Append($"Ecos desmentidos: {ecos} / {totalEcos}\n");
            sb.Append($"Pétalos de cempasúchil: {gm.CountPetals()} / {Logros.TotalPetalos}\n");
            sb.Append(gm.Data.mosaicHints == 0 ? "Mosaicos resueltos sin pedir pistas a Tizne\n" : $"Pistas de Tizne en los mosaicos: {gm.Data.mosaicHints}\n");
            sb.Append($"Logros: {Logros.Cuantos} / {Logros.Todos.Length}\n");
            sb.Append("\n\n\n\n<size=44>Gracias por jugar.</size>\n\n<size=28>Día de Muertos, 2026</size>");
            return sb.ToString();
        }

        /// <summary>
        /// Escena adicional del logro "Camino de pétalos" (F24): los 25 pétalos reunidos forman
        /// un camino que cruza la pantalla, y un último texto cierra la historia de Lucía.
        /// </summary>
        IEnumerator EscenaPetalos()
        {
            // El texto de los créditos se retira para dejar sitio.
            var txt = rodillo.GetComponent<TMP_Text>();
            for (float k = 1f; k > 0f; k -= Time.unscaledDeltaTime / 1.2f) { txt.alpha = k; yield return null; }
            txt.alpha = 0f;

            var capa = new GameObject("Camino de pétalos", typeof(RectTransform)).GetComponent<RectTransform>();
            capa.SetParent(transform, false);
            capa.anchorMin = Vector2.zero; capa.anchorMax = Vector2.one; capa.offsetMin = capa.offsetMax = Vector2.zero;

            var sp = Resources.Load<Sprite>("UI/petalo");
            var petalos = new Image[Logros.TotalPetalos];
            for (int i = 0; i < petalos.Length; i++)
            {
                float t = i / (petalos.Length - 1f);
                // Un camino que serpentea de izquierda a derecha por la mitad baja de la pantalla.
                var pos = new Vector2(Mathf.Lerp(-820f, 820f, t), -120f + Mathf.Sin(t * Mathf.PI * 2.2f) * 110f);
                var rt = new GameObject($"Petalo{i + 1}", typeof(RectTransform)).GetComponent<RectTransform>();
                rt.SetParent(capa, false);
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(72f, 72f);
                rt.localRotation = Quaternion.Euler(0f, 0f, (i * 37) % 60 - 30);
                var img = rt.gameObject.AddComponent<Image>();
                if (sp != null) img.sprite = sp; else img.color = new Color(1f, 0.6f, 0.2f);
                img.preserveAspect = true;
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                petalos[i] = img;
            }
            // Aparecen uno por uno, como si alguien los fuera acomodando.
            for (int i = 0; i < petalos.Length; i++)
            {
                StartCoroutine(Aparecer(petalos[i], 0.6f));
                if (i % 5 == 4) Sonidos.Play("S16", 0.4f);
                for (float e = 0f; e < 0.18f; e += Time.unscaledDeltaTime) yield return null;
            }

            var rtTexto = new GameObject("Texto", typeof(RectTransform)).GetComponent<RectTransform>();
            rtTexto.SetParent(capa, false);
            rtTexto.anchoredPosition = new Vector2(0f, 250f);
            rtTexto.sizeDelta = new Vector2(1400f, 420f);
            var texto = rtTexto.gameObject.AddComponent<TextMeshProUGUI>();
            texto.alignment = TextAlignmentOptions.Center;
            texto.fontSize = 34f;
            texto.color = new Color(0.95f, 0.9f, 0.8f, 0f);
            texto.text =
                "<size=26><color=#FFD27F>Camino de pétalos</color></size>\n\n" +
                "A la mañana siguiente, Lucía salió a barrer el patio.\n" +
                "No encontró un solo pétalo fuera de su sitio.\n\n" +
                "Alguien los había vuelto a acomodar, uno por uno,\n" +
                "para que nadie se perdiera el año que viene.\n\n" +
                "<i>Junto a la puerta, unas huellas pequeñas de perro.</i>";
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 2f) { texto.alpha = k; yield return null; }
            for (float e = 0f; e < 7f; e += Time.unscaledDeltaTime)
            {
                if (GameInput.Interact.WasPressedThisFrame() && e > 1.5f) break;
                yield return null;
            }
        }

        static IEnumerator Aparecer(Image img, float dur)
        {
            var c = img.color;
            var rt = img.rectTransform;
            var fin = rt.anchoredPosition;
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / dur)
            {
                img.color = new Color(c.r, c.g, c.b, k);
                rt.anchoredPosition = fin + new Vector2(0f, (1f - k) * 30f);
                yield return null;
            }
            img.color = new Color(c.r, c.g, c.b, 1f);
            rt.anchoredPosition = fin;
        }

        IEnumerator Rodar()
        {
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 2.5f) { grupo.alpha = k; yield return null; }
            grupo.alpha = 1f;
            // Sube desde abajo de la pantalla hasta que el final queda centrado.
            // El borde superior del texto parte del pie de la pantalla; termina con la última línea centrada.
            float velocidad = 60f, fin = alto + 470f;
            for (float y = 0f; y < fin; y += Time.unscaledDeltaTime * velocidad)
            {
                rodillo.anchoredPosition = new Vector2(0f, y);
                if (GameInput.Interact.IsPressed()) y += Time.unscaledDeltaTime * velocidad * 4f;   // mantener E acelera
                yield return null;
            }
            if (Logros.Tiene(Logros.CaminoPetalos)) yield return EscenaPetalos();
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime) { pie.color = new Color(1f, 0.84f, 0.5f, k); yield return null; }
            while (!GameInput.Interact.WasPressedThisFrame()) yield return null;
            for (float k = 1f; k > 0f; k -= Time.unscaledDeltaTime / 1.5f) { grupo.alpha = k; yield return null; }
            PlayerMovement.Locked = false;
            Time.timeScale = 1f;
            GameManager.Instance.GoToMainMenu();
            Destroy(gameObject);
        }
    }
}

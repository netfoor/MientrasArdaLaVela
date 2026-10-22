using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Comandos de Yarn para escribir cinemáticas dentro de los mismos archivos de diálogo,
    /// sin plugins: los personajes caminan, miran, aparecen, la pantalla se funde o muestra
    /// una viñeta ilustrada, y entre comando y comando van las líneas de diálogo.
    ///
    ///   &lt;&lt;mover Aurelio69 6.5 11&gt;&gt;        camina y la escena espera a que llegue
    ///   &lt;&lt;caminar Lucia9 8 9&gt;&gt;            camina sin esperar (dos a la vez)
    ///   &lt;&lt;mirar Lucia9 arriba&gt;&gt;            arriba / abajo / izquierda / derecha
    ///   &lt;&lt;aparecer Eco1&gt;&gt;  &lt;&lt;desaparecer Eco1&gt;&gt;
    ///   &lt;&lt;esperar 1.5&gt;&gt;
    ///   &lt;&lt;fundido negro 1&gt;&gt;  &lt;&lt;fundido blanco 1&gt;&gt;  &lt;&lt;abrir 1&gt;&gt;
    ///   &lt;&lt;vineta horno_1966&gt;&gt;  &lt;&lt;quitar_vineta&gt;&gt;   (imágenes de la lista "vinetas")
    ///   &lt;&lt;camara Aurelio69 1&gt;&gt;        la cámara se desliza hasta el actor (espera 1 s)
    ///   &lt;&lt;camara_punto 9 13 1&gt;&gt;        ...o hasta un punto del mapa
    ///   &lt;&lt;camara_jugador 1&gt;&gt;           vuelve a Ámbar (también al terminar el diálogo)
    ///   &lt;&lt;sonido S15&gt;&gt;                  efecto de Audio/Resources/Audio
    ///   &lt;&lt;musica M08&gt;&gt;                  cambia la música de fondo
    ///   &lt;&lt;creditos&gt;&gt;                    fin del juego: créditos finales y resumen
    ///   &lt;&lt;fragmento R3_mirada&gt;&gt;          la escena misma entrega un fragmento
    ///
    /// Va en el mismo objeto que el DialogueRunner. El velo y las viñetas se dibujan en un
    /// canvas por DEBAJO de la caja de diálogo, para que el texto siga leyéndose encima.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public class CinematicCommands : MonoBehaviour
    {
        [Serializable]
        public struct Vineta
        {
            public string clave;
            public Sprite imagen;
        }

        [SerializeField] Vineta[] vinetas = new Vineta[0];
        [Tooltip("Debe quedar por debajo del canvas del diálogo y por encima del HUD.")]
        [SerializeField] int overlaySortingOrder = 90;

        Image velo;
        Image cuadro;

        void Awake()
        {
            var runner = GetComponent<DialogueRunner>();
            runner.AddCommandHandler("mover", (Func<string, float, float, IEnumerator>)Mover);
            runner.AddCommandHandler("caminar", (Action<string, float, float>)Caminar);
            runner.AddCommandHandler("mirar", (Action<string, string>)Mirar);
            runner.AddCommandHandler("aparecer", (Func<string, IEnumerator>)Aparecer);
            runner.AddCommandHandler("desaparecer", (Func<string, IEnumerator>)Desaparecer);
            runner.AddCommandHandler("esperar", (Func<float, IEnumerator>)Esperar);
            runner.AddCommandHandler("fundido", (Func<string, float, IEnumerator>)Fundido);
            runner.AddCommandHandler("abrir", (Func<float, IEnumerator>)Abrir);
            runner.AddCommandHandler("vineta", (Func<string, IEnumerator>)MostrarVineta);
            runner.AddCommandHandler("quitar_vineta", (Func<IEnumerator>)QuitarVineta);
            runner.AddCommandHandler("camara", (Func<string, float, IEnumerator>)Camara);
            runner.AddCommandHandler("camara_punto", (Func<float, float, float, IEnumerator>)CamaraPunto);
            runner.AddCommandHandler("camara_jugador", (Func<float, IEnumerator>)CamaraJugador);
            runner.AddCommandHandler("sonido", (Action<string>)(clave => Sonidos.Play(clave)));
            runner.AddCommandHandler("musica", (Action<string>)(clave => Musica.Tocar(clave)));
            runner.AddCommandHandler("creditos", (Action)(() => CreditosFinales.Mostrar()));
            runner.AddCommandHandler("fragmento", (Action<string>)(id =>
            {
                GameManager.Ensure().CollectFragment(id);
                Sonidos.Play("S03");
            }));
            // Por si una cinemática olvida devolver la cámara: al acabar el diálogo vuelve a Ámbar.
            if (runner.onDialogueComplete == null) runner.onDialogueComplete = new UnityEngine.Events.UnityEvent();
            runner.onDialogueComplete.AddListener(RestaurarCamara);
            CrearOverlay();
        }

        // ------------------------------------------------------------ cámara
        CinemachineCamera vcam;
        CinemachineFollow follow;
        Transform objetivoOriginal;
        Vector3 dampingOriginal;
        Transform foco;
        static readonly Vector3 DampingCine = new Vector3(1.4f, 1.4f, 0f);   // paneo lento y suave

        CinemachineCamera Vcam()
        {
            if (vcam == null)
            {
                vcam = FindFirstObjectByType<CinemachineCamera>();
                follow = vcam != null ? vcam.GetComponent<CinemachineFollow>() : null;
            }
            return vcam;
        }

        void Enfocar(Transform t)
        {
            var v = Vcam();
            if (v == null || t == null) return;
            if (objetivoOriginal == null)
            {
                objetivoOriginal = v.Target.TrackingTarget;
                if (follow != null) dampingOriginal = follow.TrackerSettings.PositionDamping;
            }
            if (follow != null) follow.TrackerSettings.PositionDamping = DampingCine;
            v.Target.TrackingTarget = t;
        }

        IEnumerator Camara(string id, float segundos)
        {
            var a = CinematicActor.Find(id);
            if (a != null) Enfocar(a.transform);
            if (segundos > 0f) yield return new WaitForSeconds(segundos);
        }

        IEnumerator CamaraPunto(float x, float y, float segundos)
        {
            if (foco == null) foco = new GameObject("Foco de cámara (cinemática)").transform;
            foco.position = new Vector3(x, y, 0f);
            Enfocar(foco);
            if (segundos > 0f) yield return new WaitForSeconds(segundos);
        }

        IEnumerator CamaraJugador(float segundos)
        {
            RestaurarCamara();
            if (segundos > 0f) yield return new WaitForSeconds(segundos);
        }

        void RestaurarCamara()
        {
            if (objetivoOriginal == null || Vcam() == null) return;
            vcam.Target.TrackingTarget = objetivoOriginal;
            objetivoOriginal = null;
            if (follow != null) StartCoroutine(DevolverDamping());
        }

        /// <summary>Mantiene el paneo suave mientras vuelve, y luego restaura el seguimiento normal.</summary>
        IEnumerator DevolverDamping()
        {
            yield return new WaitForSeconds(1.5f);
            if (follow != null && objetivoOriginal == null) follow.TrackerSettings.PositionDamping = dampingOriginal;
        }

        void OnDestroy()
        {
            if (foco != null) Destroy(foco.gameObject);
        }

        // ------------------------------------------------------------ actores
        IEnumerator Mover(string id, float x, float y)
        {
            var a = CinematicActor.Find(id);
            if (a != null) yield return a.MoveTo(new Vector2(x, y));
        }

        void Caminar(string id, float x, float y) => CinematicActor.Find(id)?.WalkTo(new Vector2(x, y));

        void Mirar(string id, string direccion) => CinematicActor.Find(id)?.Face(direccion);

        IEnumerator Aparecer(string id)
        {
            var a = CinematicActor.Find(id);
            if (a == null) yield break;
            a.gameObject.SetActive(true);
            yield return a.FadeTo(1f, 0.6f);
        }

        IEnumerator Desaparecer(string id)
        {
            var a = CinematicActor.Find(id);
            if (a == null) yield break;
            yield return a.FadeTo(0f, 0.6f);
        }

        IEnumerator Esperar(float segundos)
        {
            yield return new WaitForSeconds(segundos);
        }

        // ------------------------------------------------------------ pantalla
        IEnumerator Fundido(string color, float segundos)
        {
            var c = (color ?? "").ToLowerInvariant() == "blanco" ? new Color(1f, 0.96f, 0.86f) : Color.black;
            velo.color = new Color(c.r, c.g, c.b, velo.color.a);
            yield return Alfa(velo, 1f, segundos);
        }

        IEnumerator Abrir(float segundos)
        {
            yield return Alfa(velo, 0f, segundos);
        }

        IEnumerator MostrarVineta(string clave)
        {
            Sprite imagen = null;
            foreach (var v in vinetas) if (v.clave == clave) imagen = v.imagen;
            if (imagen == null) { Debug.LogWarning($"[Cinemática] No hay viñeta '{clave}'."); yield break; }
            if (cuadro.color.a > 0.01f) yield return Alfa(cuadro, 0f, 0.3f);
            cuadro.sprite = imagen;
            yield return Alfa(cuadro, 1f, 0.6f);
        }

        IEnumerator QuitarVineta()
        {
            yield return Alfa(cuadro, 0f, 0.6f);
        }

        static IEnumerator Alfa(Image img, float destino, float segundos)
        {
            float desde = img.color.a;
            for (float t = 0f; t < segundos; t += Time.deltaTime)
            {
                var c = img.color; c.a = Mathf.Lerp(desde, destino, t / segundos); img.color = c;
                yield return null;
            }
            var f = img.color; f.a = destino; img.color = f;
        }

        void CrearOverlay()
        {
            var go = new GameObject("Cinematica (overlay)");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = overlaySortingOrder;
            var escala = go.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 1f;

            velo = Capa(go.transform, "Velo", estirar: true);
            velo.color = new Color(0f, 0f, 0f, 0f);

            cuadro = Capa(go.transform, "Vineta", estirar: false);
            cuadro.preserveAspect = true;
            cuadro.color = new Color(1f, 1f, 1f, 0f);
            var rt = cuadro.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 110f);     // deja libre la caja de diálogo
            rt.sizeDelta = new Vector2(1280f, 720f);
        }

        static Image Capa(Transform padre, string nombre, bool estirar)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(padre, false);
            if (estirar)
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }
    }
}

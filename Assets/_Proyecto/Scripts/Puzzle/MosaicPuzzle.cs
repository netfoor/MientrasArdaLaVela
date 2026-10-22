using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Puzle de mosaico (F12): la imagen del memento partida en 4 × 4 piezas giradas; el
    /// jugador las gira hasta recomponerla (Figura 7 y Tabla 6 del Documento de Diseño).
    ///
    /// Controles: flechas / WASD / cruceta o el mouse mueven la selección; Q y E, clic
    /// izquierdo y derecho, o los bumpers giran; R reinicia; H pide la pista de Tizne (gira
    /// una pieza a su sitio, comparte los 20 s de espera); Esc sale sin resolver.
    ///
    /// La interfaz (canvas, fondo, tablero y textos) la crea Mapa0Builder; las 16 piezas se
    /// crean aquí al abrir, porque dependen de la imagen de cada memento.
    /// La mezcla es determinista por mapa: el mismo mosaico sale igual en cada partida.
    /// </summary>
    public class MosaicPuzzle : MonoBehaviour
    {
        public const int Lado = 4;
        public static MosaicPuzzle Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance.abierto;

        [Header("Interfaz (la asigna Mapa0Builder)")]
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform board;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text hintText;

        [Header("Ajustes")]
        [SerializeField] float pieceSize = 192f;           // 64 px de imagen × 3: escalado entero
        [SerializeField] float gap = 10f;
        [SerializeField] float border = 5f;
        [SerializeField] int minScrambled = 12;            // piezas que empiezan giradas (de 16)
        [SerializeField] float ownHintCooldown = 20f;      // sólo si no hay Tizne en la escena
        [SerializeField] Color frameIdle = new Color(0.16f, 0.12f, 0.18f, 1f);
        [SerializeField] Color frameCorrect = new Color(0.95f, 0.78f, 0.35f, 1f);
        [SerializeField] Color cursorColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] AudioSource rotateSfx;            // Sprint 3 (F22)
        [SerializeField] AudioSource solvedSfx;            // S07 "Mosaico completo"
        [SerializeField] AudioSource music;                // M07 "Mosaico" (en bucle mientras está abierto)

        [Header("Final: contemplar la imagen")]
        [SerializeField] float joinTime = 1.2f;            // las piezas se juntan y desaparecen los marcos
        [SerializeField] float zoom = 1.08f;               // la imagen se acerca despacio
        [SerializeField] float minContemplation = 2.5f;    // antes de ofrecer "[E] Continuar"
        [SerializeField] Color glowColor = new Color(1f, 0.9f, 0.65f, 1f);

        readonly MosaicPiece[] pieces = new MosaicPiece[Lado * Lado];
        readonly int[] initial = new int[Lado * Lado];
        readonly List<Sprite> creados = new List<Sprite>();
        RectTransform cursor;
        int selected;
        bool abierto, resolviendo, esperarSoltar;
        int frameApertura;
        Vector2Int ultimaDir;
        float siguienteRepeticion;
        float ownHintReadyAt;
        float avisoHasta;
        Action onSolved, onCancelled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarEstaticos() => Instance = null;   // Reload Domain está desactivado

        void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (abierto) PlayerMovement.Locked = false;
            LiberarSprites();
        }

        // ================================================================ abrir / cerrar
        /// <param name="image">Imagen completa del memento (sprite único).</param>
        /// <param name="title">Texto de cabecera, p. ej. "El pan de muerto de dos piezas".</param>
        /// <param name="seedKey">Clave de la mezcla; se usa el mapId para que sea estable.</param>
        public void Open(Sprite image, string title, string seedKey, Action solved, Action cancelled = null)
        {
            if (abierto) return;
            if (image == null || panel == null || board == null)
            {
                Debug.LogError("[Mosaico] Falta la imagen del memento o la interfaz del puzle.", this);
                solved?.Invoke();   // no bloquear el avance del juego por un error de montaje
                return;
            }

            onSolved = solved;
            onCancelled = cancelled;
            panel.SetActive(true);          // antes de crear piezas: las corrutinas necesitan objetos activos
            CrearPiezas();
            CortarImagen(image);
            Mezclar(seedKey);
            Reiniciar(animate: false);

            if (titleText != null) titleText.text = title;
            selected = 0;
            ActualizarCursor();

            abierto = true;
            resolviendo = false;
            esperarSoltar = true;           // si se entra con una dirección pulsada, no mover aún
            frameApertura = Time.frameCount; // la E que cerró el diálogo no debe girar una pieza
            PlayerMovement.Locked = true;
            board.localScale = Vector3.one;
            if (brillo != null) brillo.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
            if (music != null) { music.volume = 1f; music.Play(); }
            else Musica.Temporal("M07");   // pieza contemplativa del mosaico
            ActualizarEstado();
            ActualizarPista(forzar: true);
        }

        void Cerrar()
        {
            abierto = false;
            resolviendo = false;
            panel.SetActive(false);
            PlayerMovement.Locked = false;
            if (music != null) music.Stop();
            else Musica.Restaurar();
        }

        public void Cancel()
        {
            if (!abierto || resolviendo) return;
            Cerrar();
            var cb = onCancelled; onCancelled = null; onSolved = null;
            cb?.Invoke();
        }

        // ================================================================ bucle
        void Update()
        {
            if (!abierto || resolviendo || Time.frameCount == frameApertura) return;

            Navegar();
            if (GameInput.RotateLeft.WasPressedThisFrame()) Girar(selected, -1);
            if (GameInput.RotateRight.WasPressedThisFrame()) Girar(selected, +1);
            if (GameInput.ResetPuzzle.WasPressedThisFrame()) Reiniciar(animate: true);
            if (GameInput.Hint.WasPressedThisFrame()) Pista();
            if (GameInput.Pause.WasPressedThisFrame()) { Cancel(); return; }

            ActualizarPista(forzar: false);
        }

        void Navegar()
        {
            var m = GameInput.Move.ReadValue<Vector2>();
            var dir = Vector2Int.zero;
            if (m.sqrMagnitude > 0.25f)
            {
                if (Mathf.Abs(m.x) > Mathf.Abs(m.y)) dir.x = m.x > 0f ? 1 : -1;
                else dir.y = m.y > 0f ? 1 : -1;
            }

            if (dir == Vector2Int.zero) { esperarSoltar = false; ultimaDir = dir; return; }
            if (esperarSoltar) return;

            // Primera pulsación inmediata; si se mantiene, repite tras 0.35 s cada 0.12 s.
            if (dir != ultimaDir || Time.unscaledTime >= siguienteRepeticion)
            {
                siguienteRepeticion = Time.unscaledTime + (dir != ultimaDir ? 0.35f : 0.12f);
                ultimaDir = dir;
                int fila = selected / Lado - dir.y;   // arriba = fila anterior
                int col = selected % Lado + dir.x;
                fila = Mathf.Clamp(fila, 0, Lado - 1);
                col = Mathf.Clamp(col, 0, Lado - 1);
                Select(fila * Lado + col);
            }
        }

        // ================================================================ acciones
        public void Select(int index)
        {
            if (!abierto || resolviendo || index < 0 || index >= pieces.Length) return;
            selected = index;
            ActualizarCursor();
        }

        public void RotateFromPointer(int index, int dir)
        {
            if (!abierto || resolviendo || Time.frameCount == frameApertura) return;
            Select(index);
            Girar(index, dir);
        }

        void Girar(int index, int dir)
        {
            pieces[index].Rotate(dir);
            if (rotateSfx != null) rotateSfx.Play();
            else Sonidos.Play("S06", 0.9f);   // clic de madera
            ActualizarEstado();
            ComprobarResuelto();
        }

        void Reiniciar(bool animate)
        {
            for (int i = 0; i < pieces.Length; i++) pieces[i].SetRotation(initial[i], animate);
            ActualizarEstado();
        }

        void Pista()
        {
            int idx = !pieces[selected].IsCorrect ? selected : Array.FindIndex(pieces, p => !p.IsCorrect);
            if (idx < 0) return;

            bool concedida;
            if (Tizne.Instance != null) concedida = Tizne.Instance.TryUseHint();
            else
            {
                concedida = Time.time >= ownHintReadyAt;
                if (concedida) ownHintReadyAt = Time.time + ownHintCooldown;
            }
            if (!concedida) { avisoHasta = Time.unscaledTime + 1.5f; ActualizarPista(forzar: true); return; }

            if (GameManager.Instance != null) GameManager.Instance.RegisterMosaicHint();
            Select(idx);
            pieces[idx].SetRotation(0, animate: true);
            ActualizarEstado();
            ComprobarResuelto();
        }

        void ComprobarResuelto()
        {
            foreach (var p in pieces) if (!p.IsCorrect) return;
            StartCoroutine(Resolver());
        }

        /// <summary>
        /// Final del mosaico: las piezas se juntan sin marcos, la imagen se acerca y se ilumina
        /// un poco, y el jugador la contempla hasta que decide continuar (E / Espacio / clic).
        /// </summary>
        IEnumerator Resolver()
        {
            resolviendo = true;
            if (cursor != null) cursor.gameObject.SetActive(false);
            if (solvedSfx != null) solvedSfx.Play();
            else Sonidos.Play("S07");   // acorde brillante y campanilla
            if (statusText != null) statusText.text = "El recuerdo está completo.";
            if (hintText != null) hintText.text = "";

            // 1) Se juntan: sin separación ni marcos, como una sola imagen.
            var desde = new Vector2[pieces.Length];
            for (int i = 0; i < pieces.Length; i++) desde[i] = ((RectTransform)pieces[i].transform).anchoredPosition;
            for (float t = 0f; t < joinTime; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / joinTime);
                for (int i = 0; i < pieces.Length; i++)
                {
                    var rt = (RectTransform)pieces[i].transform;
                    rt.anchoredPosition = Vector2.Lerp(desde[i], PosicionCelda(i, 0f), k);
                    pieces[i].SetFrameColor(Color.Lerp(frameCorrect, new Color(frameCorrect.r, frameCorrect.g, frameCorrect.b, 0f), k));
                }
                yield return null;
            }

            // 2) Se acerca y se ilumina: sube el brillo cálido y vuelve a bajar a un resplandor leve.
            float duracion = 2.2f;
            for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
            {
                float k = t / duracion;
                board.localScale = Vector3.one * Mathf.Lerp(1f, zoom, Mathf.SmoothStep(0f, 1f, k));
                float a = k < 0.35f ? Mathf.Lerp(0f, 0.45f, k / 0.35f) : Mathf.Lerp(0.45f, 0.12f, (k - 0.35f) / 0.65f);
                if (brillo != null) brillo.color = new Color(glowColor.r, glowColor.g, glowColor.b, a);
                yield return null;
            }

            // 3) Contemplar: el jugador decide cuándo seguir.
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, minContemplation - duracion));
            if (hintText != null) hintText.text = "[E] Continuar";
            while (!(GameInput.Interact.WasPressedThisFrame() || GameInput.AnyKey.WasPressedThisFrame())) yield return null;

            if (music != null)
                for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime) { music.volume = 1f - t / 0.8f; yield return null; }
            Cerrar();
            var cb = onSolved; onSolved = null; onCancelled = null;
            cb?.Invoke();
        }

        // ================================================================ interfaz
        void ActualizarEstado()
        {
            int bien = 0;
            foreach (var p in pieces)
            {
                if (p.IsCorrect) bien++;
                p.SetFrameColor(p.IsCorrect ? frameCorrect : frameIdle);
            }
            if (statusText != null) statusText.text = $"Piezas en su lugar: {bien}/{pieces.Length}";
        }

        void ActualizarPista(bool forzar)
        {
            if (hintText == null) return;
            float resta = Tizne.Instance != null ? Tizne.Instance.CooldownRemaining
                                                 : Mathf.Max(0f, ownHintReadyAt - Time.time);
            string t = resta <= 0f
                ? "[H] Pista de Tizne"
                : $"Tizne descansa… {Mathf.CeilToInt(resta)} s";
            if (Time.unscaledTime < avisoHasta) t = "Tizne todavía no puede ayudar. " + t;
            if (forzar || hintText.text != t) hintText.text = t;
        }

        void ActualizarCursor()
        {
            if (cursor == null) return;
            cursor.gameObject.SetActive(true);
            cursor.anchoredPosition = PosicionCelda(selected);
            cursor.SetAsLastSibling();
        }

        Vector2 PosicionCelda(int i) => PosicionCelda(i, gap);

        Vector2 PosicionCelda(int i, float separacion)
        {
            float paso = pieceSize + separacion;
            int fila = i / Lado, col = i % Lado;
            return new Vector2((col - 1.5f) * paso, (1.5f - fila) * paso);
        }

        // ================================================================ construcción
        Image brillo;

        void CrearPiezas()
        {
            if (pieces[0] != null)
            {
                // Tras un final anterior las piezas quedaron juntas: vuelven a su rejilla.
                for (int i = 0; i < pieces.Length; i++) ((RectTransform)pieces[i].transform).anchoredPosition = PosicionCelda(i);
                return;
            }

            for (int i = 0; i < pieces.Length; i++)
            {
                var marco = NuevoRect($"Pieza_{i / Lado}_{i % Lado}", board, pieceSize + border * 2f);
                marco.anchoredPosition = PosicionCelda(i);
                var imgMarco = marco.gameObject.AddComponent<Image>();
                imgMarco.color = frameIdle;

                var dibujo = NuevoRect("Imagen", marco, pieceSize);
                var imgDibujo = dibujo.gameObject.AddComponent<Image>();
                imgDibujo.preserveAspect = true;

                var pieza = marco.gameObject.AddComponent<MosaicPiece>();
                pieza.Init(this, i, imgMarco, imgDibujo);
                pieces[i] = pieza;
            }

            // Resplandor del final, encima de las piezas y sin bloquear clics.
            var rtBrillo = NuevoRect("Brillo", board, Lado * pieceSize + 40f);
            brillo = rtBrillo.gameObject.AddComponent<Image>();
            brillo.raycastTarget = false;
            brillo.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);

            // Cursor de selección: un marco hueco hecho con cuatro barras.
            cursor = NuevoRect("Cursor", board, pieceSize + border * 2f + 12f);
            float lado = cursor.sizeDelta.x, grosor = 6f;
            Barra(cursor, new Vector2(0f, (lado - grosor) / 2f), new Vector2(lado, grosor));
            Barra(cursor, new Vector2(0f, -(lado - grosor) / 2f), new Vector2(lado, grosor));
            Barra(cursor, new Vector2(-(lado - grosor) / 2f, 0f), new Vector2(grosor, lado));
            Barra(cursor, new Vector2((lado - grosor) / 2f, 0f), new Vector2(grosor, lado));
        }

        void Barra(RectTransform padre, Vector2 pos, Vector2 tam)
        {
            var rt = NuevoRect("Barra", padre, 0f);
            rt.sizeDelta = tam;
            rt.anchoredPosition = pos;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = cursorColor;
            img.raycastTarget = false;   // el cursor no debe tapar los clics a las piezas
        }

        static RectTransform NuevoRect(string nombre, Transform padre, float lado)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(padre, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(lado, lado);
            return rt;
        }

        void CortarImagen(Sprite image)
        {
            LiberarSprites();
            var tex = image.texture;
            var r = image.textureRect;
            float w = r.width / Lado, h = r.height / Lado;
            for (int i = 0; i < pieces.Length; i++)
            {
                int fila = i / Lado, col = i % Lado;
                // La textura cuenta desde abajo; la fila 0 del tablero es la de arriba.
                var rect = new Rect(r.x + col * w, r.y + (Lado - 1 - fila) * h, w, h);
                var s = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), image.pixelsPerUnit);
                s.name = $"{image.name}_{fila}_{col}";
                creados.Add(s);
                pieces[i].SetSprite(s);
            }
        }

        void LiberarSprites()
        {
            foreach (var s in creados) if (s != null) Destroy(s);
            creados.Clear();
        }

        /// <summary>Mezcla estable: la misma clave produce siempre las mismas rotaciones.</summary>
        void Mezclar(string clave)
        {
            var rng = new System.Random(HashEstable(clave));
            for (int intento = 0; intento < 64; intento++)
            {
                int giradas = 0;
                for (int i = 0; i < initial.Length; i++)
                {
                    initial[i] = rng.Next(4);
                    if (initial[i] != 0) giradas++;
                }
                if (giradas >= Mathf.Min(minScrambled, initial.Length)) return;
            }
            // Muy improbable: completa a mano las que falten.
            for (int i = 0, n = 0; i < initial.Length && n < minScrambled; i++)
                if (initial[i] == 0) { initial[i] = 1 + (i % 3); n++; }
        }

        /// <summary>FNV-1a: string.GetHashCode no garantiza el mismo valor entre ejecuciones.</summary>
        static int HashEstable(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in s ?? "") { h ^= c; h *= 16777619; }
                return (int)h;
            }
        }
    }
}

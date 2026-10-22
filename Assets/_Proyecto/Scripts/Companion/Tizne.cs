using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Tizne, el xoloitzcuintle de Ámbar (F11).
    ///
    /// Sigue a Ámbar pisando el mismo camino que ella (una estela de "migas"), así no atraviesa
    /// paredes mientras la acompaña. Con H usa Olfatear (Tabla 22 del Documento de Diseño):
    /// corre hacia el fragmento pendiente más cercano, ladra y se queda señalándolo; después
    /// hay 20 s de espera. En el puzle de mosaico la misma pista gira una pieza a su sitio
    /// (lo pide MosaicPuzzle con TryUseHint, y comparte la espera).
    ///
    /// Al correr hacia la pista va en línea recta y SÍ puede cruzar la niebla y los muros: en la
    /// tradición el xoloitzcuintle acompaña a las almas, y el documento dice que "puede entrar a
    /// la niebla y olfatear los recuerdos". Por eso no lleva collider.
    /// </summary>
    public class Tizne : MonoBehaviour
    {
        public static Tizne Instance { get; private set; }

        [SerializeField] Transform target;                 // Ámbar
        [SerializeField] SpriteRenderer spriteRenderer;
        [Tooltip("3 cuadros por dirección, cortados de tizne_dog.png por HojaTizne.")]
        [SerializeField] Sprite[] walkUp, walkLeft, walkDown, walkRight;
        [SerializeField] float frameRate = 8f;
        [Tooltip("Accesorios (F23): collar, sombrero y ambos × up, left, down, right × 3 cuadros = 36 sprites.")]
        [SerializeField] Sprite[] variantes;

        [Header("Seguir a Ámbar")]
        [SerializeField] float followDistance = 1.1f;
        [SerializeField] float walkSpeed = 3.4f;           // Ámbar camina a 3.2
        [SerializeField] float catchUpDistance = 7f;       // más lejos que esto, reaparece junto a ella

        [Header("Olfatear (pista con H)")]
        [SerializeField] float cooldown = 20f;
        [SerializeField] float runSpeed = 6.5f;
        [SerializeField] float maxRun = 6f;                // no se aleja más que esto de su punto de salida
        [SerializeField] float stopBefore = 0.7f;          // se detiene antes de pisar el objeto
        [SerializeField] float pointTime = 2.2f;           // tiempo que se queda señalando
        [SerializeField] GameObject barkBubble;            // "¡Guau!" sobre la cabeza
        [SerializeField] AudioSource barkSfx;              // S: ladrido (Sprint 3, F22)

        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public bool IsReady => CooldownRemaining <= 0f;
        public bool IsSniffing => sniffing;
        public float Cooldown => cooldown;

        readonly List<Vector3> trail = new List<Vector3>();
        const float CrumbSpacing = 0.25f;
        const int MaxCrumbs = 64;

        float readyAt;
        bool sniffing;
        Vector2 facing = Vector2.down;
        float animClock;
        bool moving;
        Coroutine burbuja;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarEstaticos() => Instance = null;   // Reload Domain está desactivado

        void Awake()
        {
            Instance = this;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (barkBubble != null) barkBubble.SetActive(false);
            PonerAccesorios();
        }

        /// <summary>
        /// Collar alternativo (logro "Memoria propia") y sombrero de palma (código TIZNE).
        /// Sólo cambia qué cuadros se dibujan: la animación es la misma.
        /// </summary>
        void PonerAccesorios()
        {
            bool collar = Logros.CollarTizne, sombrero = Logros.SombreroTizne;
            int v = collar && sombrero ? 2 : sombrero ? 1 : collar ? 0 : -1;
            if (v < 0 || variantes == null || variantes.Length < 36) return;
            Sprite[] Tramo(int dir) { var r = new Sprite[3]; System.Array.Copy(variantes, v * 12 + dir * 3, r, 0, 3); return r; }
            walkUp = Tramo(0); walkLeft = Tramo(1); walkDown = Tramo(2); walkRight = Tramo(3);
            if (spriteRenderer != null && walkDown[0] != null) spriteRenderer.sprite = walkDown[0];
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (target == null) return;

            if (!sniffing && GameInput.Hint.WasPressedThisFrame() && PuedeOlfatear())
                StartCoroutine(Olfatear());

            if (!sniffing) Seguir();
            Animar();
        }

        bool PuedeOlfatear()
        {
            if (!IsReady || DialogueBridge.IsBusy || PlayerMovement.Locked) return false;
            if (MosaicPuzzle.IsOpen || SceneTransition.Busy) return false;   // en el puzle la pista la gestiona él
            return true;
        }

        // ------------------------------------------------------------------ seguir
        void Seguir()
        {
            // Estela: una miga cada 25 cm que recorre Ámbar.
            var pos = target.position;
            if (trail.Count == 0 || (pos - trail[trail.Count - 1]).sqrMagnitude > CrumbSpacing * CrumbSpacing)
            {
                trail.Add(pos);
                if (trail.Count > MaxCrumbs) trail.RemoveAt(0);
            }

            float distAmbar = Vector2.Distance(transform.position, pos);
            if (distAmbar > catchUpDistance)
            {
                // Se quedó muy atrás (cambio de escena, teletransporte): reaparece a su lado.
                transform.position = pos - (Vector3)(facing * followDistance);
                trail.Clear();
                moving = false;
                return;
            }

            moving = false;
            if (distAmbar <= followDistance || trail.Count == 0) return;

            // Avanza hacia la miga más antigua y la descarta al alcanzarla.
            var destino = trail[0];
            float paso = walkSpeed * (distAmbar > followDistance * 2.5f ? 1.4f : 1f) * Time.deltaTime;
            Mover(destino, paso);
            if ((transform.position - destino).sqrMagnitude < 0.02f) trail.RemoveAt(0);
        }

        void Mover(Vector3 destino, float paso)
        {
            var antes = transform.position;
            transform.position = Vector3.MoveTowards(antes, destino, paso);
            var delta = (Vector2)(transform.position - antes);
            if (delta.sqrMagnitude > 0.000001f)
            {
                moving = true;
                facing = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? new Vector2(Mathf.Sign(delta.x), 0f)
                    : new Vector2(0f, Mathf.Sign(delta.y));
            }
        }

        // ------------------------------------------------------------------ olfatear
        /// <summary>Lo más cercano que falte: un fragmento; si ya están todos, el memento.</summary>
        Transform BuscarPista()
        {
            Transform mejor = null;
            float mejorD = float.MaxValue;
            foreach (var f in MemoryFragment.All)
            {
                if (f == null || f.Collected || !f.gameObject.activeInHierarchy) continue;
                float d = (f.transform.position - transform.position).sqrMagnitude;
                if (d < mejorD) { mejorD = d; mejor = f.transform; }
            }
            if (mejor == null && Memento.Current != null && MemoryMap.Current != null && MemoryMap.Current.AllFragments)
                mejor = Memento.Current.transform;
            return mejor;
        }

        IEnumerator Olfatear()
        {
            sniffing = true;
            readyAt = Time.time + cooldown;
            var pista = BuscarPista();

            if (pista != null)
            {
                var salida = transform.position;
                var objetivo = pista.position;
                var dir = (Vector2)(objetivo - salida);
                float recorrido = Mathf.Min(Mathf.Max(0f, dir.magnitude - stopBefore), maxRun);
                var llegada = salida + (Vector3)(dir.normalized * recorrido);

                while ((transform.position - llegada).sqrMagnitude > 0.0025f)
                {
                    Mover(llegada, runSpeed * Time.deltaTime);
                    Animar();
                    yield return null;
                }
                moving = false;
                // Mira hacia la pista aunque se haya detenido antes de llegar.
                var hacia = (Vector2)(objetivo - transform.position);
                if (hacia.sqrMagnitude > 0.0001f)
                    facing = Mathf.Abs(hacia.x) > Mathf.Abs(hacia.y)
                        ? new Vector2(Mathf.Sign(hacia.x), 0f)
                        : new Vector2(0f, Mathf.Sign(hacia.y));
            }

            Ladrar(pointTime);
            yield return new WaitForSeconds(pointTime);

            // Vuelve con Ámbar: la estela vieja ya no sirve.
            trail.Clear();
            sniffing = false;
        }

        /// <summary>
        /// Pista dentro del puzle de mosaico. Devuelve false si Tizne aún está en espera.
        /// </summary>
        public bool TryUseHint()
        {
            if (!IsReady) return false;
            readyAt = Time.time + cooldown;
            Ladrar(1f);
            return true;
        }

        void Ladrar(float duracion)
        {
            // S09 de la Tabla 27. El campo barkSfx sigue por si alguna escena quiere su propia
            // fuente (un ladrido situado en el espacio); si no hay, suena el clip compartido.
            if (barkSfx != null) barkSfx.Play();
            else Sonidos.Play("S09", 0.5f, 0.06f);
            if (barkBubble == null) return;
            if (burbuja != null) StopCoroutine(burbuja);
            barkBubble.SetActive(true);
            burbuja = StartCoroutine(OcultarBurbuja(duracion));
        }

        IEnumerator OcultarBurbuja(float t)
        {
            yield return new WaitForSeconds(t);
            if (barkBubble != null) barkBubble.SetActive(false);
        }

        // ------------------------------------------------------------------ animación
        void Animar()
        {
            if (spriteRenderer == null) return;
            var cuadros = facing.y > 0.5f ? walkUp
                        : facing.y < -0.5f ? walkDown
                        : facing.x < 0f ? walkLeft : walkRight;
            if (cuadros == null || cuadros.Length == 0) return;

            if (moving) animClock += Time.deltaTime * frameRate * (sniffing ? 1.6f : 1f);
            else animClock = 0f;
            int i = moving ? Mathf.FloorToInt(animClock) % cuadros.Length : 0;
            if (cuadros[i] != null) spriteRenderer.sprite = cuadros[i];

            // Delante de Ámbar si está más abajo en pantalla; detrás si está más arriba.
            if (target != null)
            {
                var srAmbar = target.GetComponent<SpriteRenderer>();
                int baseOrden = srAmbar != null ? srAmbar.sortingOrder : 10;
                spriteRenderer.sortingOrder = transform.position.y < target.position.y ? baseOrden + 1 : baseOrden - 1;
            }
        }
    }
}

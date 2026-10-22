using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Personaje que las cinemáticas pueden mover (escenas "actuadas" en el motor, como en
    /// To the Moon). Lo controlan los comandos de Yarn de <see cref="CinematicCommands"/>:
    ///   &lt;&lt;mover Lucia9 6.5 9&gt;&gt;   &lt;&lt;mirar Lucia9 arriba&gt;&gt;   &lt;&lt;aparecer Eco1&gt;&gt;
    /// Usa las hojas LPC ya cortadas: 9 cuadros por dirección, el 0 es la pose de reposo.
    /// </summary>
    public class CinematicActor : MonoBehaviour
    {
        static readonly Dictionary<string, CinematicActor> actores = new Dictionary<string, CinematicActor>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarRegistro() => actores.Clear();   // Reload Domain está desactivado

        public static CinematicActor Find(string id)
        {
            if (id != null && actores.TryGetValue(id, out var a) && a != null) return a;
            Debug.LogWarning($"[Cinemática] No hay ningún actor con id '{id}' en la escena.");
            return null;
        }

        [SerializeField] string actorId = "";
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] up, left, down, right;
        [SerializeField] float speed = 2.2f;
        [SerializeField] float frameRate = 10f;
        [Tooltip("Empieza invisible; aparece con <<aparecer id>>.")]
        [SerializeField] bool startHidden = false;
        [Tooltip("Dirección inicial: arriba, abajo, izquierda o derecha.")]
        [SerializeField] string startFacing = "abajo";

        public string ActorId => actorId;
        float alfaBase = 1f;      // los espíritus son translúcidos: aparecer respeta su transparencia
        Light2D[] luces;          // p. ej. la vela de Refugio: aparece y se apaga con el actor
        float[] intensidades;
        Vector2 facing = Vector2.down;
        Coroutine enCurso;

        void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) alfaBase = spriteRenderer.color.a;
            luces = GetComponentsInChildren<Light2D>(true);
            intensidades = new float[luces.Length];
            for (int i = 0; i < luces.Length; i++) intensidades[i] = luces[i].intensity;
            if (!string.IsNullOrEmpty(actorId)) actores[actorId] = this;
            Face(startFacing);
            if (startHidden) SetAlpha(0f);
        }

        void OnDestroy()
        {
            if (!string.IsNullOrEmpty(actorId) && actores.TryGetValue(actorId, out var a) && a == this)
                actores.Remove(actorId);
        }

        // ------------------------------------------------------------ movimiento
        /// <summary>Camina hasta el punto (en unidades de mundo) y termina en reposo.</summary>
        public IEnumerator MoveTo(Vector2 destino)
        {
            float reloj = 0f;
            while (((Vector2)transform.position - destino).sqrMagnitude > 0.0004f)
            {
                var antes = (Vector2)transform.position;
                var nuevo = Vector2.MoveTowards(antes, destino, speed * Time.deltaTime);
                transform.position = new Vector3(nuevo.x, nuevo.y, transform.position.z);
                var delta = nuevo - antes;
                if (delta.sqrMagnitude > 0f)
                    facing = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                        ? new Vector2(Mathf.Sign(delta.x), 0f)
                        : new Vector2(0f, Mathf.Sign(delta.y));
                reloj += Time.deltaTime * frameRate;
                Pintar(1 + Mathf.FloorToInt(reloj) % 8);
                yield return null;
            }
            Pintar(0);
        }

        /// <summary>Como MoveTo pero sin que la cinemática espere (dos personajes a la vez).</summary>
        public void WalkTo(Vector2 destino)
        {
            if (enCurso != null) StopCoroutine(enCurso);
            enCurso = StartCoroutine(MoveTo(destino));
        }

        public void Face(string direccion)
        {
            switch ((direccion ?? "").ToLowerInvariant())
            {
                case "arriba": case "up": facing = Vector2.up; break;
                case "izquierda": case "left": facing = Vector2.left; break;
                case "derecha": case "right": facing = Vector2.right; break;
                default: facing = Vector2.down; break;
            }
            Pintar(0);
        }

        // ------------------------------------------------------------ aparecer
        public IEnumerator FadeTo(float alfa, float tiempo)
        {
            if (spriteRenderer == null) yield break;
            float desde = alfaBase > 0f ? spriteRenderer.color.a / alfaBase : 0f;
            for (float t = 0f; t < tiempo; t += Time.deltaTime)
            {
                SetAlpha(Mathf.Lerp(desde, alfa, t / tiempo));
                yield return null;
            }
            SetAlpha(alfa);
        }

        void SetAlpha(float a)
        {
            if (spriteRenderer == null) return;
            var c = spriteRenderer.color; c.a = a * alfaBase; spriteRenderer.color = c;
            if (luces != null) for (int i = 0; i < luces.Length; i++) luces[i].intensity = intensidades[i] * a;
        }

        void Pintar(int i)
        {
            var cuadros = facing.y > 0.5f ? up : facing.y < -0.5f ? down : facing.x < 0f ? left : right;
            if (spriteRenderer != null && cuadros != null && i < cuadros.Length && cuadros[i] != null)
                spriteRenderer.sprite = cuadros[i];
        }
    }
}

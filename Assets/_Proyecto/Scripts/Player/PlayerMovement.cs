using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Movimiento de Ámbar en vista cenital, 4 direcciones (F02).
    /// Animator esperado: parámetros float MoveX, MoveY, Speed
    /// (Blend Tree "Idle" usa MoveX/MoveY guardando la última dirección).
    /// Rigidbody2D: Dynamic, Gravity Scale 0, Freeze Rotation Z, Interpolate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float speed = 3.2f;          // unidades por segundo (1 unidad = 1 tile de 32 px)
        [SerializeField] Animator animator;

        static readonly int MoveX = Animator.StringToHash("MoveX");
        static readonly int MoveY = Animator.StringToHash("MoveY");
        static readonly int Speed = Animator.StringToHash("Speed");

        Rigidbody2D rb;
        Vector2 input;
        Vector2 lastDir = Vector2.down;

        public Vector2 FacingDirection => lastDir;
        /// <summary>Bloqueos externos (diálogo, puzle, pausa).</summary>
        public static bool Locked { get; set; }

        /// <summary>
        /// Con "Reload Domain" desactivado los estáticos sobreviven al salir de Play Mode.
        /// Sin esto, salir con el jugador bloqueado dejaría a Ámbar congelada en la
        /// siguiente partida, sin nada que la desbloquee.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ReiniciarEstado() => Locked = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            bool blocked = Locked || DialogueBridge.IsBusy;
            Vector2 raw = blocked ? Vector2.zero : GameInput.Move.ReadValue<Vector2>();

            // 4 direcciones: domina el eje con mayor magnitud
            if (Mathf.Abs(raw.x) > Mathf.Abs(raw.y)) raw.y = 0f; else raw.x = 0f;
            input = raw.sqrMagnitude > 0.01f ? raw.normalized : Vector2.zero;
            if (input != Vector2.zero) lastDir = input;

            if (animator != null)
            {
                animator.SetFloat(MoveX, lastDir.x);
                animator.SetFloat(MoveY, lastDir.y);
                animator.SetFloat(Speed, input.sqrMagnitude);
            }
        }

        void FixedUpdate()
        {
            rb.linearVelocity = input * speed;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Luz de vela de Ámbar (mantener F / gatillo derecho).
    /// Aumenta la Light2D del farol y revela los objetos Revealable dentro del radio (3 tiles).
    /// </summary>
    public class CandleLight : MonoBehaviour
    {
        [SerializeField] Light2D lanternLight;
        [SerializeField] float idleRadius = 1.5f;
        [SerializeField] float activeRadius = 3f;
        [SerializeField] float idleIntensity = 0.6f;
        [SerializeField] float activeIntensity = 1.2f;
        [SerializeField] float lerpSpeed = 6f;

        public static CandleLight Instance { get; private set; }
        public bool IsActive { get; private set; }
        public float CurrentRadius { get; private set; }

        static readonly List<Revealable> revealables = new List<Revealable>();
        public static void Register(Revealable r) { if (!revealables.Contains(r)) revealables.Add(r); }
        public static void Unregister(Revealable r) => revealables.Remove(r);

        void Awake()
        {
            Instance = this;
            CurrentRadius = idleRadius;
        }

        void Update()
        {
            IsActive = !DialogueBridge.IsBusy && !PlayerMovement.Locked && GameInput.Candle.IsPressed();
            float targetR = IsActive ? activeRadius : idleRadius;
            CurrentRadius = Mathf.Lerp(CurrentRadius, targetR, Time.deltaTime * lerpSpeed);

            if (lanternLight != null)
            {
                lanternLight.pointLightOuterRadius = CurrentRadius;
                lanternLight.intensity = Mathf.Lerp(lanternLight.intensity, IsActive ? activeIntensity : idleIntensity, Time.deltaTime * lerpSpeed);
            }

            if (!IsActive) return;
            for (int i = revealables.Count - 1; i >= 0; i--)
            {
                var r = revealables[i];
                if (r == null) { revealables.RemoveAt(i); continue; }
                if (Vector2.Distance(r.transform.position, transform.position) <= CurrentRadius) r.Reveal();
            }
        }
    }
}

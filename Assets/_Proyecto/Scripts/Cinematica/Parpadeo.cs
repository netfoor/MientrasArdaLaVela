using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MientrasArdaLaVela
{
    /// <summary>Hace parpadear una Light2D como fuego o vela (Recuerdo 3: el horno encendido).</summary>
    [RequireComponent(typeof(Light2D))]
    public class Parpadeo : MonoBehaviour
    {
        [SerializeField] float amplitud = 0.35f;
        [SerializeField] float velocidad = 7f;
        Light2D luz;
        float base_, semilla;

        void Awake()
        {
            luz = GetComponent<Light2D>();
            base_ = luz.intensity;
            semilla = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(semilla, Time.time * velocidad) - 0.5f;
            luz.intensity = Mathf.Max(0f, base_ * (1f + n * 2f * amplitud));
        }
    }
}

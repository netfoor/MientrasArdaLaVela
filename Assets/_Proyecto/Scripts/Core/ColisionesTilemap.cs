using UnityEngine;
using UnityEngine.Tilemaps;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// B17: se podían atravesar muros. Las escenas se construyen en batchmode y el
    /// CompositeCollider2D de "Paredes" se guardaba SIN geometría (m_CompositePaths vacío), así
    /// que en juego no había nada contra qué chocar. Este componente, en el Grid, obliga a los
    /// colliders de los tilemaps a reconstruirse al empezar la escena. Los constructores además
    /// generan la geometría antes de guardar (ConstructorEscena.GenerarColisiones).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class ColisionesTilemap : MonoBehaviour
    {
        void Awake() => Regenerar(gameObject);

        public static void Regenerar(GameObject raiz)
        {
            foreach (var tc in raiz.GetComponentsInChildren<TilemapCollider2D>(true)) tc.ProcessTilemapChanges();
            foreach (var cc in raiz.GetComponentsInChildren<CompositeCollider2D>(true)) cc.GenerateGeometry();
        }
    }
}

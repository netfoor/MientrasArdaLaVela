using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Una pieza del puzle de mosaico (F12). La crea MosaicPuzzle en tiempo de ejecución.
    /// Rotation cuenta cuartos de vuelta en sentido horario; 0 es la posición correcta.
    /// Clic izquierdo gira a la izquierda y clic derecho a la derecha (Tabla 6).
    /// </summary>
    public class MosaicPiece : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public int Index { get; private set; }
        public int Rotation { get; private set; }
        public bool IsCorrect => Rotation == 0;

        MosaicPuzzle owner;
        Image frame;
        Image picture;
        float angle;          // ángulo visual actual, en grados (negativo = horario)
        Coroutine anim;
        const float TiempoGiro = 0.12f;

        public void Init(MosaicPuzzle puzzle, int index, Image frameImage, Image pictureImage)
        {
            owner = puzzle;
            Index = index;
            frame = frameImage;
            picture = pictureImage;
        }

        public void SetSprite(Sprite s) => picture.sprite = s;
        public void SetFrameColor(Color c) => frame.color = c;

        /// <summary>Gira un cuarto de vuelta: +1 horario (derecha), -1 antihorario (izquierda).</summary>
        public void Rotate(int dir)
        {
            Rotation = Mod4(Rotation + dir);
            Animar(angle - 90f * dir);
        }

        /// <summary>Fija una rotación absoluta (reinicio y pista).</summary>
        public void SetRotation(int r, bool animate)
        {
            Rotation = Mod4(r);
            float destino = -90f * Rotation;
            if (!animate || !isActiveAndEnabled)
            {
                if (anim != null) { StopCoroutine(anim); anim = null; }
                angle = destino;
                Aplicar();
                return;
            }
            // Por el camino corto desde donde esté ahora.
            float delta = Mathf.DeltaAngle(angle, destino);
            Animar(angle + delta);
        }

        void Animar(float destino)
        {
            if (anim != null) StopCoroutine(anim);
            if (!isActiveAndEnabled) { angle = destino; Aplicar(); return; }
            anim = StartCoroutine(Girar(angle, destino));
        }

        IEnumerator Girar(float desde, float hasta)
        {
            for (float t = 0f; t < TiempoGiro; t += Time.unscaledDeltaTime)
            {
                angle = Mathf.Lerp(desde, hasta, t / TiempoGiro);
                Aplicar();
                yield return null;
            }
            angle = Mathf.Repeat(hasta, 360f) - 360f;   // mantiene el número acotado
            Aplicar();
            anim = null;
        }

        void Aplicar() => picture.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

        static int Mod4(int v) => ((v % 4) + 4) % 4;

        public void OnPointerEnter(PointerEventData eventData) => owner.Select(Index);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) owner.RotateFromPointer(Index, -1);
            else if (eventData.button == PointerEventData.InputButton.Right) owner.RotateFromPointer(Index, +1);
        }
    }
}

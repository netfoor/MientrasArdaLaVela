using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Sonido del texto del diálogo (S05 de la Tabla 27, F22): un tic corto por letra mientras
    /// la máquina de escribir de Yarn Spinner escribe la línea.
    ///
    /// Yarn Spinner 3.x permite engancharse a la escritura con un <see cref="ActionMarkupHandler"/>:
    /// el presentador llama a <see cref="OnCharacterWillAppear"/> por cada carácter visible. Lo
    /// monta <c>ConstructorEscena.DialogueSystem</c> en el objeto del presentador de líneas y lo
    /// registra en su lista de handlers.
    ///
    /// No suena en cada letra: una letra de cada dos, y nunca en espacios ni en signos, porque a
    /// 90 letras por segundo (velocidad "Rápida") el tic se vuelve un zumbido. Con la velocidad
    /// "Instantánea" (el código PANDEMUERTO) no suena nada: no hay escritura que acompañar.
    /// </summary>
    public class SonidoTexto : ActionMarkupHandler
    {
        const int CadaCuantas = 2;
        const float Volumen = 0.5f;
        const float Tono = 0.06f;

        int contador;

        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text) { }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text) => contador = 0;

        public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            if (Ajustes.VelocidadTexto >= Ajustes.NombresVelocidad.Length - 1) return YarnTask.CompletedTask;

            var texto = line.Text;
            if (currentCharacterIndex >= 0 && currentCharacterIndex < texto.Length)
            {
                char c = texto[currentCharacterIndex];
                if (char.IsWhiteSpace(c) || char.IsPunctuation(c)) return YarnTask.CompletedTask;
            }

            if (++contador % CadaCuantas == 0) Sonidos.Play("S05", Volumen, Tono);
            return YarnTask.CompletedTask;
        }

        public override void OnLineDisplayComplete() { }

        public override void OnLineWillDismiss() { }
    }
}

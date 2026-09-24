using UnityEngine;
using UnityEngine.InputSystem;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Acciones de entrada definidas por código (teclado, mouse y gamepad),
    /// según la tabla de Controles del Documento de Diseño.
    ///
    /// Las acciones se CONSTRUYEN DE CERO en cada entrada a Play Mode. Es a propósito:
    /// con "Enter Play Mode Options > Reload Domain" desactivado (que es como está este
    /// proyecto) los campos estáticos sobreviven de una partida a la siguiente, pero el
    /// Input System deshabilita todas las acciones al salir de Play. Si sólo se
    /// configuraran una vez, la segunda partida se quedaría sin controles.
    /// </summary>
    public static class GameInput
    {
        public static InputAction Move    { get; private set; }
        public static InputAction Interact{ get; private set; }
        public static InputAction Candle  { get; private set; }
        public static InputAction Hint    { get; private set; }
        public static InputAction Journal { get; private set; }
        public static InputAction Pause   { get; private set; }
        public static InputAction AnyKey  { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            Liberar();

            Move     = new InputAction("Move",     InputActionType.Value);
            Interact = new InputAction("Interact", InputActionType.Button);
            Candle   = new InputAction("Candle",   InputActionType.Button);
            Hint     = new InputAction("Hint",     InputActionType.Button);
            Journal  = new InputAction("Journal",  InputActionType.Button);
            Pause    = new InputAction("Pause",    InputActionType.Button);
            AnyKey   = new InputAction("AnyKey",   InputActionType.Button);

            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/leftStick");
            Move.AddBinding("<Gamepad>/dpad");

            Interact.AddBinding("<Keyboard>/e");
            Interact.AddBinding("<Keyboard>/space");
            Interact.AddBinding("<Mouse>/leftButton");
            Interact.AddBinding("<Gamepad>/buttonSouth");

            Candle.AddBinding("<Keyboard>/f");
            Candle.AddBinding("<Gamepad>/rightTrigger");

            Hint.AddBinding("<Keyboard>/h");
            Hint.AddBinding("<Gamepad>/buttonNorth");

            Journal.AddBinding("<Keyboard>/tab");
            Journal.AddBinding("<Gamepad>/select");

            Pause.AddBinding("<Keyboard>/escape");
            Pause.AddBinding("<Gamepad>/start");

            AnyKey.AddBinding("<Keyboard>/anyKey");
            AnyKey.AddBinding("<Mouse>/leftButton");
            AnyKey.AddBinding("<Gamepad>/buttonSouth");
            AnyKey.AddBinding("<Gamepad>/start");

            Move.Enable(); Interact.Enable(); Candle.Enable(); Hint.Enable();
            Journal.Enable(); Pause.Enable(); AnyKey.Enable();
        }

        /// <summary>Desecha las acciones de la partida anterior, si las hubiera.</summary>
        static void Liberar()
        {
            foreach (var a in new[] { Move, Interact, Candle, Hint, Journal, Pause, AnyKey })
            {
                if (a == null) continue;
                a.Disable();
                a.Dispose();
            }
        }
    }
}

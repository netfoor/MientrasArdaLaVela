using UnityEngine;
using UnityEngine.InputSystem;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Acciones de entrada definidas por código (teclado, mouse y gamepad),
    /// según la tabla de Controles del Documento de Diseño.
    /// </summary>
    public static class GameInput
    {
        public static readonly InputAction Move = new InputAction("Move", InputActionType.Value);
        public static readonly InputAction Interact = new InputAction("Interact", InputActionType.Button);
        public static readonly InputAction Candle = new InputAction("Candle", InputActionType.Button);
        public static readonly InputAction Hint = new InputAction("Hint", InputActionType.Button);
        public static readonly InputAction Journal = new InputAction("Journal", InputActionType.Button);
        public static readonly InputAction Pause = new InputAction("Pause", InputActionType.Button);
        public static readonly InputAction AnyKey = new InputAction("AnyKey", InputActionType.Button);

        static bool initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (initialized) return;
            initialized = true;

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
    }
}

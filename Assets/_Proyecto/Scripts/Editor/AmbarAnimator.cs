using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// P4 · Animator de Ámbar (F02).
    /// Corta la hoja LPC, genera los 8 AnimationClips y arma Ambar.controller con los
    /// parámetros MoveX, MoveY y Speed y dos Blend Trees 2D, tal como los espera PlayerMovement.
    ///
    /// La hoja del generador LPC es la universal completa (832×3456, cuadros de 64 px).
    /// La caminata ocupa las filas 9 a 12 contando desde arriba: arriba, izquierda, abajo y
    /// derecha, 9 cuadros cada una. El cuadro 0 de cada fila es la pose de reposo.
    /// </summary>
    public static class AmbarAnimator
    {
        const string RutaHoja = "Assets/_Proyecto/Art/Personajes/ambar_walk.png";
        const string CarpetaSalida = "Assets/_Proyecto/Animations/Ambar";
        const int Lado = 64;                 // tamaño del cuadro en px
        const int FilaWalk = 8;              // índice (desde arriba, base 0) de la primera fila de caminata
        const int Cuadros = 9;
        const float Fps = 10f;
        const float PivoteY = 2f / Lado;     // los pies quedan a 2 px del borde inferior

        // Orden de las filas en la hoja LPC y su dirección en el Blend Tree
        static readonly (string nombre, Vector2 dir)[] Direcciones =
        {
            ("Up",    new Vector2( 0f,  1f)),
            ("Left",  new Vector2(-1f,  0f)),
            ("Down",  new Vector2( 0f, -1f)),
            ("Right", new Vector2( 1f,  0f)),
        };

        [MenuItem("Mientras arda la vela/Generar Animator de Ámbar")]
        public static void Generar()
        {
            if (!File.Exists(RutaHoja))
            {
                Debug.LogError($"[Ámbar] No se encontró la hoja en {RutaHoja}.");
                return;
            }

            Directory.CreateDirectory(CarpetaSalida);
            var sprites = CortarHoja();
            if (sprites == null) return;

            var clips = new Dictionary<string, AnimationClip>();
            for (int d = 0; d < Direcciones.Length; d++)
            {
                var (nombre, _) = Direcciones[d];
                // Reposo: sólo el cuadro 0.
                clips[$"Idle_{nombre}"] = CrearClip($"Idle_{nombre}", new[] { sprites[d * Cuadros] }, repetir: false);
                // Caminata: los cuadros 1..8.
                var paso = new Sprite[Cuadros - 1];
                for (int i = 1; i < Cuadros; i++) paso[i - 1] = sprites[d * Cuadros + i];
                clips[$"Walk_{nombre}"] = CrearClip($"Walk_{nombre}", paso, repetir: true);
            }

            CrearController(clips);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Ámbar] Listo: {sprites.Count} sprites, {clips.Count} clips y Ambar.controller en {CarpetaSalida}.");
        }

        // ---------------- corte de la hoja ----------------
        // La lógica vive en HojaLPC, que corta cualquier hoja del generador LPC.
        static List<Sprite> CortarHoja() => HojaLPC.Cortar(RutaHoja, "ambar");

        // ---------------- clips ----------------
        static AnimationClip CrearClip(string nombre, Sprite[] cuadros, bool repetir)
        {
            string ruta = $"{CarpetaSalida}/{nombre}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, ruta);
            }
            clip.frameRate = Fps;

            var enlace = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",                       // el SpriteRenderer va en el propio objeto Ámbar
                propertyName = "m_Sprite",
            };
            var llaves = new ObjectReferenceKeyframe[cuadros.Length];
            for (int i = 0; i < cuadros.Length; i++)
                llaves[i] = new ObjectReferenceKeyframe { time = i / Fps, value = cuadros[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, enlace, llaves);

            var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
            ajustes.loopTime = repetir;
            AnimationUtility.SetAnimationClipSettings(clip, ajustes);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        // ---------------- controller ----------------
        static void CrearController(Dictionary<string, AnimationClip> clips)
        {
            string ruta = $"{CarpetaSalida}/Ambar.controller";
            if (File.Exists(ruta)) AssetDatabase.DeleteAsset(ruta);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ruta);

            ctrl.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var maquina = ctrl.layers[0].stateMachine;

            var idle = ArbolDireccional(ctrl, "Idle", clips, "Idle_");
            var walk = ArbolDireccional(ctrl, "Walk", clips, "Walk_");

            maquina.defaultState = idle;

            // Idle -> Walk cuando hay movimiento; Walk -> Idle cuando se detiene.
            var aWalk = idle.AddTransition(walk);
            aWalk.hasExitTime = false;
            aWalk.duration = 0f;
            aWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var aIdle = walk.AddTransition(idle);
            aIdle.hasExitTime = false;
            aIdle.duration = 0f;
            aIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            EditorUtility.SetDirty(ctrl);
        }

        static AnimatorState ArbolDireccional(AnimatorController ctrl, string nombre,
                                              Dictionary<string, AnimationClip> clips, string prefijo)
        {
            var estado = ctrl.CreateBlendTreeInController(nombre, out BlendTree arbol);
            arbol.name = nombre;
            arbol.blendType = BlendTreeType.SimpleDirectional2D;
            arbol.blendParameter = "MoveX";
            arbol.blendParameterY = "MoveY";
            foreach (var (dir, vector) in Direcciones)
                arbol.AddChild(clips[$"{prefijo}{dir}"], vector);
            return estado;
        }
    }
}

using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>Comprueba que el Animator de Ámbar (P4/F02) coincide con lo que espera PlayerMovement.</summary>
    public static class AmbarAnimatorVerificador
    {
        const string Carpeta = "Assets/_Proyecto/Animations/Ambar";
        static int fallos;

        [MenuItem("Mientras arda la vela/Verificar Animator de Ámbar")]
        public static void Verificar()
        {
            fallos = 0;
            var dirs = new[] { "Up", "Left", "Down", "Right" };

            // --- Clips ---
            foreach (var d in dirs)
            {
                RevisarClip($"Idle_{d}", 1, repetir: false);
                RevisarClip($"Walk_{d}", 8, repetir: true);
            }

            // --- Controller ---
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{Carpeta}/Ambar.controller");
            if (!Ok("Ambar.controller existe", ctrl != null)) { Fin(); return; }

            foreach (var p in new[] { "MoveX", "MoveY", "Speed" })
                Ok($"Parámetro float '{p}'",
                   ctrl.parameters.Any(x => x.name == p && x.type == AnimatorControllerParameterType.Float));

            var maquina = ctrl.layers[0].stateMachine;
            foreach (var nombre in new[] { "Idle", "Walk" })
            {
                var st = maquina.states.FirstOrDefault(s => s.state.name == nombre).state;
                if (!Ok($"Estado '{nombre}'", st != null)) continue;
                var arbol = st.motion as BlendTree;
                if (!Ok($"  '{nombre}' es un Blend Tree", arbol != null)) continue;
                Ok($"  '{nombre}' es 2D direccional", arbol.blendType == BlendTreeType.SimpleDirectional2D);
                Ok($"  '{nombre}' usa MoveX/MoveY",
                   arbol.blendParameter == "MoveX" && arbol.blendParameterY == "MoveY");
                Ok($"  '{nombre}' tiene las 4 direcciones", arbol.children.Length == 4,
                   $"{arbol.children.Length}");
                foreach (var d in dirs)
                    Ok($"  '{nombre}' incluye {nombre}_{d}",
                       arbol.children.Any(c => c.motion != null && c.motion.name == $"{nombre}_{d}"));
            }

            Ok("El estado por defecto es Idle", maquina.defaultState != null && maquina.defaultState.name == "Idle");

            var idle = maquina.states.First(s => s.state.name == "Idle").state;
            var walk = maquina.states.First(s => s.state.name == "Walk").state;
            var iw = idle.transitions.FirstOrDefault(t => t.destinationState == walk);
            Ok("Transición Idle→Walk con Speed > 0.1",
                iw != null && !iw.hasExitTime &&
                iw.conditions.Any(c => c.parameter == "Speed" && c.mode == AnimatorConditionMode.Greater
                                       && Mathf.Approximately(c.threshold, 0.1f)));
            var wi = walk.transitions.FirstOrDefault(t => t.destinationState == idle);
            Ok("Transición Walk→Idle con Speed < 0.1",
                wi != null && !wi.hasExitTime &&
                wi.conditions.Any(c => c.parameter == "Speed" && c.mode == AnimatorConditionMode.Less
                                       && Mathf.Approximately(c.threshold, 0.1f)));

            // --- Sprites ---
            var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/_Proyecto/Art/Personajes/ambar_walk.png")
                                       .OfType<Sprite>().ToArray();
            Ok("36 sprites cortados de la hoja", sprites.Length == 36, $"{sprites.Length}");
            if (sprites.Length > 0)
            {
                var s = sprites[0];
                Ok("PPU 32", Mathf.Approximately(s.pixelsPerUnit, 32f), $"{s.pixelsPerUnit}");
                Ok("Cuadro de 64×64", s.rect.width == 64 && s.rect.height == 64);
                float pivY = s.pivot.y / s.rect.height;
                Ok("Pivote a los pies (Y ≈ 0.031)", pivY < 0.1f, $"{pivY:F4}");
            }
            Fin();
        }

        static void RevisarClip(string nombre, int cuadros, bool repetir)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{Carpeta}/{nombre}.anim");
            if (!Ok($"Clip '{nombre}'", clip != null)) return;
            Ok($"  '{nombre}' a 10 fps", Mathf.Approximately(clip.frameRate, 10f), $"{clip.frameRate}");
            var enlaces = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            if (!Ok($"  '{nombre}' anima m_Sprite",
                    enlaces.Any(b => b.propertyName == "m_Sprite" && b.type == typeof(SpriteRenderer)))) return;
            var curva = AnimationUtility.GetObjectReferenceCurve(clip, enlaces.First(b => b.propertyName == "m_Sprite"));
            Ok($"  '{nombre}' con {cuadros} cuadros", curva.Length == cuadros, $"{curva.Length}");
            Ok($"  '{nombre}' loop = {repetir}",
               AnimationUtility.GetAnimationClipSettings(clip).loopTime == repetir);
        }

        static bool Ok(string desc, bool cond, string detalle = null)
        {
            if (cond) Debug.Log($"  ok   · {desc}");
            else { fallos++; Debug.LogError($"FALLA  · {desc}{(detalle != null ? $" (valor: '{detalle}')" : "")}"); }
            return cond;
        }

        static void Fin() => Debug.Log($"### ANIMATOR ÁMBAR: {fallos} fallos ###");
    }
}

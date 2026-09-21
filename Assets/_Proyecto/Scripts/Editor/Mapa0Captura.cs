using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>Renderiza la escena del Mapa 0 a PNG para revisarla sin abrir el editor.</summary>
    public static class Mapa0Captura
    {
        [MenuItem("Mientras arda la vela/Capturar Mapa 0")]
        public static void Capturar()
        {
            const string ruta = "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity";
            if (EditorSceneManager.GetActiveScene().path != ruta)
                EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            string destino = System.Environment.GetEnvironmentVariable("CAPTURA_DIR") ?? "/tmp";
            Directory.CreateDirectory(destino);

            // 1) Vista completa del trazado, con la luz global subida para poder inspeccionarlo.
            var luz = GameObject.Find("Global Light 2D")?.GetComponent<Light2D>();
            float original = luz != null ? luz.intensity : 0f;
            if (luz != null) { luz.intensity = 1f; luz.color = Color.white; }
            Render(new Vector3(18f, 11f, -10f), 12.5f, 1600, 900, $"{destino}/mapa0_trazado.png");
            if (luz != null) { luz.intensity = original; luz.color = new Color(0.65f, 0.70f, 0.95f); }

            // 2) Lo que ve el jugador: con la Main Camera real (Pixel Perfect + luces 2D).
            var ambar = GameObject.Find("Ambar");
            var pos = ambar != null ? ambar.transform.position : new Vector3(9.5f, 15.5f, 0f);
            RenderConMainCamera(pos, $"{destino}/mapa0_juego.png");

            Debug.Log($"[Captura] PNG escritos en {destino}");
        }


        /// <summary>Usa la Main Camera de la escena, para que apliquen las luces 2D y el Pixel Perfect.</summary>
        static void RenderConMainCamera(Vector3 centro, string archivo)
        {
            var cam = GameObject.Find("Main Camera")?.GetComponent<Camera>();
            if (cam == null) { Debug.LogError("[Captura] No hay Main Camera."); return; }

            var posPrevia = cam.transform.position;
            var rtPrevia = cam.targetTexture;
            cam.transform.position = new Vector3(centro.x, centro.y, -10f);

            const int ancho = 1280, alto = 720;
            RefrescarLuces2D();
            var rt = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            var previa = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(ancho, alto, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, ancho, alto), 0, 0);
            tex.Apply();
            RenderTexture.active = previa;
            File.WriteAllBytes(archivo, tex.EncodeToPNG());

            cam.targetTexture = rtPrevia;
            cam.transform.position = posPrevia;
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
        }


        /// <summary>
        /// Light2D construye su malla y sus bounds en LateUpdate(). En batchmode no hay bucle de
        /// juego, así que un cam.Render() suelto dejaría las luces Point con bounds vacíos y el
        /// culling las descartaría. Aquí se hace a mano lo mismo que haría LateUpdate.
        /// Solo afecta a esta herramienta de captura: en el editor y en Play Mode no hace falta.
        /// </summary>
        static void RefrescarLuces2D()
        {
            var tipo = typeof(Light2D);
            var updateMesh = tipo.GetMethod("UpdateMesh",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateSphere = tipo.GetMethod("UpdateBoundingSphere",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (updateMesh == null || updateSphere == null)
            {
                Debug.LogWarning("[Captura] No se encontraron UpdateMesh/UpdateBoundingSphere en Light2D.");
                return;
            }
            foreach (var luz in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            {
                if (luz.lightType == Light2D.LightType.Global) continue;
                updateMesh.Invoke(luz, new object[] { true });
                updateSphere.Invoke(luz, null);
            }
        }

        static void Render(Vector3 pos, float tam, int ancho, int alto, string archivo)
        {
            var go = new GameObject("__captura");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = tam;
            cam.transform.position = pos;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            RefrescarLuces2D();
            var rt = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            cam.targetTexture = rt;
            cam.Render();

            var previa = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(ancho, alto, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, ancho, alto), 0, 0);
            tex.Apply();
            RenderTexture.active = previa;

            File.WriteAllBytes(archivo, tex.EncodeToPNG());

            cam.targetTexture = null;
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }
}

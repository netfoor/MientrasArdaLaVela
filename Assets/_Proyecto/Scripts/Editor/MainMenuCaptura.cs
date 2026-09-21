using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>Renderiza los paneles del MainMenu a PNG para revisarlos sin abrir el editor.</summary>
    public static class MainMenuCaptura
    {
        [MenuItem("Mientras arda la vela/Capturar MainMenu")]
        public static void Capturar()
        {
            const string ruta = "Assets/_Proyecto/Scenes/MainMenu.unity";
            if (EditorSceneManager.GetActiveScene().path != ruta)
                EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            string destino = System.Environment.GetEnvironmentVariable("CAPTURA_DIR") ?? "/tmp";
            Directory.CreateDirectory(destino);

            var paneles = new[] { "TitlePanel", "MenuPanel", "CreditsPanel" };
            foreach (var nombre in paneles)
            {
                foreach (var p in paneles)
                    GameObject.Find("Canvas")?.transform.Find(p)?.gameObject.SetActive(p == nombre);
                GameObject.Find("Canvas")?.transform.Find("OptionsPanel")?.gameObject.SetActive(false);
                Render($"{destino}/menu_{nombre.ToLower()}.png");
            }

            // Restaurar el estado inicial
            foreach (var p in paneles)
                GameObject.Find("Canvas")?.transform.Find(p)?.gameObject.SetActive(p == "TitlePanel");

            Debug.Log($"[Captura] PNG del menú escritos en {destino}");
        }

        static void Render(string archivo)
        {
            var canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            var cam = GameObject.Find("Main Camera")?.GetComponent<Camera>();
            if (canvas == null || cam == null) { Debug.LogError("[Captura] Falta Canvas o Main Camera."); return; }

            // Un Canvas en Overlay no se puede volcar a una RenderTexture; se pasa a Camera un momento.
            var modoPrevio = canvas.renderMode;
            var camPrevia = canvas.worldCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();

            const int ancho = 1280, alto = 720;
            var rt = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32);
            var rtPrevia = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            var activaPrevia = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(ancho, alto, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, ancho, alto), 0, 0);
            tex.Apply();
            RenderTexture.active = activaPrevia;
            File.WriteAllBytes(archivo, tex.EncodeToPNG());

            cam.targetTexture = rtPrevia;
            canvas.renderMode = modoPrevio;
            canvas.worldCamera = camPrevia;
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
        }
    }
}

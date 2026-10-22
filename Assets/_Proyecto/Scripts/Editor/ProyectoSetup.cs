using System.IO;
using UnityEditor;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Tareas de montaje del proyecto (F01) que se pueden ejecutar desde el menú
    /// o en batchmode con -executeMethod.
    /// </summary>
    public static class ProyectoSetup
    {
        const string RutaYarnProject = "Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject";

        [MenuItem("Mientras arda la vela/Setup/Importar recursos de TextMeshPro")]
        public static void ImportarTMPEssentials()
        {
            // TMP_Settings sólo existe después de importar los Essential Resources.
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
            {
                Debug.Log("[Setup] Los recursos de TextMeshPro ya estaban importados.");
                return;
            }

            // TMP_PackageResourceImporter.ImportResources importa de forma sincrónica
            // (AssetDatabase.ImportPackage es asíncrono y no termina en batchmode).
            Debug.Log("[Setup] Importando TMP Essential Resources...");
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
        }

        /// <summary>Recompila el Yarn Project: úsalo si un .yarn nuevo no aparece en el juego.</summary>
        [MenuItem("Mientras arda la vela/Setup/Reimportar diálogos (Yarn)")]
        public static void ReimportarDialogos()
        {
            AssetDatabase.ImportAsset(RutaYarnProject, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            var yp = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(RutaYarnProject);
            Debug.Log(yp != null ? $"[Yarn] Proyecto recompilado: {System.Linq.Enumerable.Count(yp.NodeNames)} nodos." : "[Yarn] No se encontró el Yarn Project.");
        }

        [MenuItem("Mientras arda la vela/Setup/Crear Yarn Project")]
        public static void CrearYarnProject()
        {
            if (File.Exists(RutaYarnProject))
            {
                Debug.Log("[Setup] El Yarn Project ya existe.");
                return;
            }

            var proyecto = new Yarn.Compiler.Project
            {
                // Toma todos los .yarn de Assets/_Proyecto/Dialogue y subcarpetas.
                SourceFilePatterns = new[] { "**/*.yarn" },
                // Igual que Unity: ignora carpetas terminadas en ~ y las muestras.
                ExcludeFilePatterns = new[] { "**/*~/*", "./Samples/Yarn Spinner*/*" },
                BaseLanguage = "es-MX",
            };

            Directory.CreateDirectory(Path.GetDirectoryName(RutaYarnProject));
            proyecto.SaveToFile(RutaYarnProject);
            AssetDatabase.ImportAsset(RutaYarnProject);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Yarn Project creado en {RutaYarnProject}");
        }

        /// <summary>Comprobación rápida del estado del proyecto (F01).</summary>
        [MenuItem("Mientras arda la vela/Setup/Verificar proyecto")]
        public static void Verificar()
        {
            Debug.Log($"[Verificar] TMP Settings: {(AssetDatabase.FindAssets("t:TMP_Settings").Length > 0 ? "OK" : "FALTA")}");

            var yp = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(RutaYarnProject);
            if (yp == null)
            {
                Debug.LogError("[Verificar] Yarn Project: FALTA");
            }
            else
            {
                var nodos = yp.NodeNames;
                Debug.Log($"[Verificar] Yarn Project: OK, {nodos.Length} nodos -> {string.Join(", ", nodos)}");
            }

            Debug.Log($"[Verificar] Capa Interactable: {(LayerMask.NameToLayer("Interactable") >= 0 ? "OK (indice " + LayerMask.NameToLayer("Interactable") + ")" : "FALTA")}");
            Debug.Log($"[Verificar] Escenas en Build: {EditorBuildSettings.scenes.Length}");
        }

        /// <summary>Todo el arranque en una sola llamada (para batchmode).</summary>
        public static void ArranqueCompleto()
        {
            ImportarTMPEssentials();
            CrearYarnProject();
            AssetDatabase.Refresh();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Compilación del juego (F26). La plataforma de entrega es PC Windows 10/11, según el
    /// Documento de Diseño; la de Linux está aquí para poder probar el build en esta máquina.
    ///
    /// Las escenas salen de la lista de Build Profiles (File > Build Profiles), no de una lista
    /// propia: así no hay dos sitios que puedan discrepar. MainMenu tiene que ser la primera.
    ///
    /// NO se toca `productName`: la carpeta de la partida guardada
    /// (`Application.persistentDataPath`) se forma con él, y cambiarlo dejaría huérfanas las
    /// partidas que ya existen.
    /// </summary>
    public static class Build
    {
        [MenuItem("Mientras arda la vela/Compilar/Windows (64 bits)")]
        public static void Windows() => Compilar(BuildTarget.StandaloneWindows64, "Windows", "MientrasArdaLaVela.exe");

        [MenuItem("Mientras arda la vela/Compilar/Linux (64 bits)")]
        public static void Linux() => Compilar(BuildTarget.StandaloneLinux64, "Linux", "MientrasArdaLaVela.x86_64");

        static void Compilar(BuildTarget destino, string carpeta, string ejecutable)
        {
            var escenas = new List<string>();
            foreach (var e in EditorBuildSettings.scenes)
                if (e.enabled) escenas.Add(e.path);

            if (escenas.Count == 0) { Fallar("la lista de escenas del build está vacía."); return; }
            if (!escenas[0].EndsWith("MainMenu.unity"))
            {
                Fallar($"la primera escena del build es {escenas[0]}; tiene que ser MainMenu " +
                       "(arrancar en otra deja al GameManager sin partida y 'Continuar' no funciona).");
                return;
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, destino))
            {
                Fallar($"falta el módulo de {carpeta} en este editor. " +
                       "Unity Hub > Installs > 6000.6.0f1 > Add modules.");
                return;
            }

            var salida = Path.Combine(Directory.GetCurrentDirectory(), "Builds", carpeta, ejecutable);
            Directory.CreateDirectory(Path.GetDirectoryName(salida));

            var informe = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = escenas.ToArray(),
                locationPathName = salida,
                target = destino,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            });

            var r = informe.summary;
            if (r.result != BuildResult.Succeeded)
            {
                Fallar($"el build de {carpeta} terminó como {r.result} con {r.totalErrors} errores.");
                return;
            }

            Debug.Log($"### BUILD {carpeta.ToUpperInvariant()}: {r.totalSize / (1024 * 1024)} MB en " +
                      $"{r.totalTime.TotalSeconds:F0} s · {escenas.Count} escenas ###\n{salida}");
        }

        static void Fallar(string motivo)
        {
            Debug.LogError($"FALLA build: {motivo}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}

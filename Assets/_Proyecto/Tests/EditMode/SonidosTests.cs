using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// El catálogo de audio (F22) contra la Tabla 27 del Documento de Diseño. Nace de un fallo
    /// real: Sonidos.Play no avisa cuando falta un clip (por diseño: es mejor el silencio que una
    /// excepción), así que S01, S05, S09, S10 y S14 estuvieron invocados y mudos sin que nada lo
    /// delatara. Estas dos pruebas son el aviso que faltaba.
    /// </summary>
    public class SonidosTests
    {
        static string Audio => Path.Combine(Application.dataPath, "_Proyecto/Audio/Resources");

        static bool Existe(string clave)
        {
            var carpeta = Path.Combine(Audio, clave[0] == 'M' ? "Musica" : "Audio");
            if (!Directory.Exists(carpeta)) return false;
            foreach (var f in Directory.GetFiles(carpeta))
                if (Path.GetFileNameWithoutExtension(f) == clave && Path.GetExtension(f) != ".meta") return true;
            return false;
        }

        [Test]
        public void Las_ocho_piezas_y_los_catorce_efectos_de_la_Tabla_27_tienen_clip()
        {
            var faltan = new List<string>();
            for (int i = 1; i <= 8; i++) if (!Existe($"M{i:00}")) faltan.Add($"M{i:00}");
            for (int i = 1; i <= 14; i++) if (!Existe($"S{i:00}")) faltan.Add($"S{i:00}");
            CollectionAssert.IsEmpty(faltan, "Sin clip en Audio/Resources: " + string.Join(", ", faltan));
        }

        [Test]
        public void Todo_sonido_que_el_juego_invoca_existe()
        {
            var raiz = Path.Combine(Application.dataPath, "_Proyecto");
            // Sonidos.Play("S03"), Musica.Tocar("M01"), Musica.Temporal("M07")...
            var enCodigo = new Regex(@"(?:Play|Tocar|Temporal|Clip)\(""([SM]\d\d)""");
            // <<sonido S08>>, <<musica M08>>
            var enYarn = new Regex(@"<<(?:sonido|musica)\s+([SM]\d\d)\s*>>");

            var invocados = new SortedSet<string>();
            void Buscar(string carpeta, string patron, Regex re)
            {
                foreach (var archivo in Directory.GetFiles(Path.Combine(raiz, carpeta), patron, SearchOption.AllDirectories))
                {
                    if (archivo.Contains("/Tests/")) continue;
                    foreach (Match m in re.Matches(File.ReadAllText(archivo))) invocados.Add(m.Groups[1].Value);
                }
            }
            Buscar("Scripts", "*.cs", enCodigo);
            Buscar("Dialogue", "*.yarn", enYarn);

            Assert.IsNotEmpty(invocados, "No se encontró ninguna llamada de audio: ¿cambió el patrón?");
            var faltan = new List<string>();
            foreach (var clave in invocados) if (!Existe(clave)) faltan.Add(clave);
            CollectionAssert.IsEmpty(faltan, "Invocados sin clip: " + string.Join(", ", faltan));
        }
    }
}

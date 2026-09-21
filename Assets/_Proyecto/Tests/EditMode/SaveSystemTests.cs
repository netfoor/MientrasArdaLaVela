using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// Pruebas del guardado en JSON (F10).
    ///
    /// SaveSystem escribe en Application.persistentDataPath, que es la MISMA ruta que usa
    /// la partida real. Para no borrarla, se respalda el archivo antes de la tanda y se
    /// restaura al final.
    /// </summary>
    public class SaveSystemTests
    {
        static string Archivo => Path.Combine(Application.persistentDataPath, "partida.json");
        static string Respaldo => Path.Combine(Application.persistentDataPath, "partida.json.respaldo-pruebas");

        [OneTimeSetUp]
        public void RespaldarPartidaReal()
        {
            if (File.Exists(Archivo)) File.Copy(Archivo, Respaldo, true);
        }

        [OneTimeTearDown]
        public void RestaurarPartidaReal()
        {
            if (File.Exists(Respaldo))
            {
                File.Copy(Respaldo, Archivo, true);
                File.Delete(Respaldo);
            }
            else if (File.Exists(Archivo))
            {
                File.Delete(Archivo);
            }
        }

        [SetUp]
        public void Limpiar() => SaveSystem.Delete();

        [Test]
        public void Sin_archivo_HasSave_es_falso_y_Load_devuelve_null()
        {
            Assert.IsFalse(SaveSystem.HasSave);
            Assert.IsNull(SaveSystem.Load());
        }

        [Test]
        public void Guardar_y_cargar_conserva_todos_los_campos()
        {
            var datos = new SaveData
            {
                currentScene = SceneNames.Ofrenda,
                fragments = { "M0_foto", "M0_delantal" },
                revealed = { "M0_sombrero_rev", "M0_prologo" },
                resolvedEchoes = { "M0_eco" },
                completedMaps = { "M0" },
            };

            SaveSystem.Save(datos);
            Assert.IsTrue(SaveSystem.HasSave, "El archivo de partida debería existir tras guardar.");

            var leidos = SaveSystem.Load();
            Assert.IsNotNull(leidos);
            Assert.AreEqual(datos.version, leidos.version);
            Assert.AreEqual(SceneNames.Ofrenda, leidos.currentScene);
            CollectionAssert.AreEqual(datos.fragments, leidos.fragments);
            CollectionAssert.AreEqual(datos.revealed, leidos.revealed);
            CollectionAssert.AreEqual(datos.resolvedEchoes, leidos.resolvedEchoes);
            CollectionAssert.AreEqual(datos.completedMaps, leidos.completedMaps);
        }

        [Test]
        public void Delete_borra_el_archivo()
        {
            SaveSystem.Save(new SaveData());
            Assert.IsTrue(SaveSystem.HasSave);
            SaveSystem.Delete();
            Assert.IsFalse(SaveSystem.HasSave);
        }

        [Test]
        public void Un_archivo_dañado_no_revienta_y_devuelve_null()
        {
            File.WriteAllText(Archivo, "{ esto no es JSON válido ");
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[SaveSystem\] Archivo dañado"));
            Assert.IsNull(SaveSystem.Load());
        }
    }
}

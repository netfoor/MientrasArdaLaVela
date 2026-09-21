using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// Pruebas del estado global de la partida (F10): fragmentos, ecos y mapas.
    /// Igual que en SaveSystemTests, se respalda la partida real porque GameManager
    /// autoguarda en cada cambio.
    /// </summary>
    public class GameManagerTests
    {
        static string Archivo => Path.Combine(Application.persistentDataPath, "partida.json");
        static string Respaldo => Path.Combine(Application.persistentDataPath, "partida.json.respaldo-gm");

        GameObject objeto;

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
        public void Crear()
        {
            SaveSystem.Delete();
            objeto = new GameObject("GameManager (pruebas)");
            var gm = objeto.AddComponent<GameManager>();
            Invocar(gm, "Awake");
            Assert.IsNotNull(GameManager.Instance, "GameManager.Instance debería quedar asignado en Awake.");
        }

        [TearDown]
        public void Destruir()
        {
            if (objeto != null)
            {
                var gm = objeto.GetComponent<GameManager>();
                if (gm != null) Invocar(gm, "OnDestroy");
                Object.DestroyImmediate(objeto);
            }
            SaveSystem.Delete();
            Assert.IsTrue(GameManager.Instance == null, "El singleton debe quedar limpio entre pruebas.");
        }

        /// <summary>
        /// En EditMode Unity no ejecuta los mensajes del ciclo de vida (Awake/OnDestroy):
        /// sólo corren en Play Mode o en componentes marcados [ExecuteAlways]. Para probar
        /// GameManager sin convertirlo en [ExecuteAlways] —lo que cambiaría su comportamiento
        /// en el editor— se invocan a mano y así el singleton queda igual que en juego.
        /// </summary>
        static void Invocar(GameManager gm, string metodo)
        {
            typeof(GameManager)
                .GetMethod(metodo, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(gm, null);
        }

        // ---------- Fragmentos ----------
        [Test]
        public void CollectFragment_no_duplica_ids()
        {
            var gm = GameManager.Instance;
            int avisos = 0;
            gm.FragmentCollected += _ => avisos++;

            gm.CollectFragment("M0_foto");
            gm.CollectFragment("M0_foto");
            gm.CollectFragment("M0_foto");

            Assert.AreEqual(1, gm.Data.fragments.Count, "El mismo fragmento no debe guardarse dos veces.");
            Assert.AreEqual(1, avisos, "El evento FragmentCollected debe dispararse una sola vez.");
            Assert.IsTrue(gm.HasFragment("M0_foto"));
        }

        [Test]
        public void CollectFragment_ignora_ids_vacios()
        {
            var gm = GameManager.Instance;
            gm.CollectFragment("");
            gm.CollectFragment(null);
            Assert.AreEqual(0, gm.Data.fragments.Count);
        }

        [Test]
        public void CountFragments_respeta_el_prefijo_del_mapa()
        {
            var gm = GameManager.Instance;
            gm.CollectFragment("M0_foto");
            gm.CollectFragment("M0_delantal");
            gm.CollectFragment("M0_petalos");
            gm.CollectFragment("R1_charola");

            Assert.AreEqual(3, gm.CountFragments("M0_"), "Sólo los del mapa 0.");
            Assert.AreEqual(1, gm.CountFragments("R1_"), "Sólo los del recuerdo 1.");
            Assert.AreEqual(0, gm.CountFragments("R2_"));
            Assert.AreEqual(4, gm.Data.fragments.Count);
        }

        // ---------- Ecos ----------
        [Test]
        public void ResolveEcho_dispara_el_evento_una_sola_vez()
        {
            var gm = GameManager.Instance;
            int avisos = 0;
            gm.EchoResolved += _ => avisos++;

            gm.ResolveEcho("M0_eco");
            gm.ResolveEcho("M0_eco");

            Assert.AreEqual(1, avisos, "EchoResolved no debe repetirse para el mismo eco.");
            Assert.AreEqual(1, gm.Data.resolvedEchoes.Count);
            Assert.IsTrue(gm.IsEchoResolved("M0_eco"));
        }

        // ---------- Mapas ----------
        [Test]
        public void CompleteMap_dispara_el_evento_una_sola_vez()
        {
            var gm = GameManager.Instance;
            int avisos = 0;
            gm.MapCompleted += _ => avisos++;

            gm.CompleteMap("M0");
            gm.CompleteMap("M0");

            Assert.AreEqual(1, avisos);
            Assert.IsTrue(gm.IsMapCompleted("M0"));
            Assert.IsFalse(gm.IsMapCompleted("R1"));
        }

        // ---------- Revelados (Luz de vela) ----------
        [Test]
        public void MarkRevealed_no_duplica()
        {
            var gm = GameManager.Instance;
            gm.MarkRevealed("M0_sombrero_rev");
            gm.MarkRevealed("M0_sombrero_rev");

            Assert.AreEqual(1, gm.Data.revealed.Count);
            Assert.IsTrue(gm.IsRevealed("M0_sombrero_rev"));
        }

        // ---------- Persistencia ----------
        [Test]
        public void Reunir_un_fragmento_autoguarda_en_disco()
        {
            GameManager.Instance.CollectFragment("M0_foto");

            var enDisco = SaveSystem.Load();
            Assert.IsNotNull(enDisco, "CollectFragment debe autoguardar (Documento de Diseño: Guardar/Cargar).");
            CollectionAssert.Contains(enDisco.fragments, "M0_foto");
        }

        [Test]
        public void Ensure_no_crea_un_segundo_GameManager()
        {
            var primero = GameManager.Instance;
            var segundo = GameManager.Ensure();
            Assert.AreSame(primero, segundo, "Ensure debe devolver la instancia que ya existe.");
        }
    }
}

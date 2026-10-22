using System.Linq;
using NUnit.Framework;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// Pruebas de los logros (F23) y de los pétalos (F24). Cumplidos es una función pura sobre
    /// la partida; Revisar y Desbloquear se prueban sin tocar PlayerPrefs (en EditMode los
    /// logros sólo viven en memoria).
    /// </summary>
    public class LogrosTests
    {
        [SetUp] public void Limpiar() => Logros.OlvidarEnMemoria();
        [TearDown] public void Dejar() => Logros.OlvidarEnMemoria();

        static SaveData PartidaCompleta()
        {
            var d = new SaveData();
            d.completedMaps.AddRange(new[] { "M0", "R1", "R2", "R3", "R4", "EP" });
            return d;
        }

        [Test]
        public void HayNueveLogrosConIdsUnicos()
        {
            Assert.AreEqual(9, Logros.Todos.Length);
            Assert.AreEqual(9, Logros.Todos.Select(l => l.Id).Distinct().Count());
        }

        [Test]
        public void PartidaNueva_NoCumpleNingunLogro()
        {
            Assert.IsEmpty(Logros.Cumplidos(new SaveData()));
        }

        [Test]
        public void LogrosDeHistoria_SalenDeLosMapasCompletados()
        {
            var d = new SaveData();
            d.completedMaps.Add("M0");
            d.completedMaps.Add("R2");
            CollectionAssert.AreEquivalent(new[] { Logros.PrimeraLuz, Logros.SillaVacia }, Logros.Cumplidos(d));
        }

        [Test]
        public void MemoriaPropia_SoloSinPistas()
        {
            var d = PartidaCompleta();
            Assert.Contains(Logros.MemoriaPropia, Logros.Cumplidos(d));
            d.mosaicHints = 1;
            Assert.IsFalse(Logros.Cumplidos(d).Contains(Logros.MemoriaPropia));
        }

        [Test]
        public void CaminoDePetalos_PideLos25Distintos()
        {
            var d = new SaveData();
            foreach (var m in Logros.MapasConPetalos)
                for (int i = 1; i <= Logros.PetalosPorMapa; i++) d.petals.Add($"{m}_petalo{i}");
            Assert.AreEqual(25, Logros.TotalPetalos);
            Assert.Contains(Logros.CaminoPetalos, Logros.Cumplidos(d));
            d.petals.RemoveAt(0);
            d.petals.Add(d.petals[0]);   // un repetido no cuenta
            Assert.IsFalse(Logros.Cumplidos(d).Contains(Logros.CaminoPetalos));
        }

        [Test]
        public void TodasLasVoces_PidePersonajesYEcos()
        {
            var d = new SaveData();
            d.talked.AddRange(Logros.Voces);
            Assert.IsFalse(Logros.Cumplidos(d).Contains(Logros.TodasLasVoces), "Faltan los Ecos.");
            d.resolvedEchoes.AddRange(Diario.Recuerdos.SelectMany(r => r.Ecos).Select(e => e.Id));
            Assert.Contains(Logros.TodasLasVoces, Logros.Cumplidos(d));
        }

        [Test]
        public void Revisar_DesbloqueaUnaSolaVezYAvisa()
        {
            int avisos = 0;
            void Contar(Logros.Logro l) => avisos++;
            Logros.Desbloqueado += Contar;
            try
            {
                var d = new SaveData();
                d.completedMaps.Add("M0");
                Assert.AreEqual(1, Logros.Revisar(d));
                Assert.AreEqual(0, Logros.Revisar(d), "Un logro ya obtenido no se desbloquea dos veces.");
                Assert.IsTrue(Logros.Tiene(Logros.PrimeraLuz));
                Assert.AreEqual(1, avisos);
            }
            finally { Logros.Desbloqueado -= Contar; }
        }

        [Test]
        public void CodigoDesconocido_NoHaceNada()
        {
            Assert.IsNull(Logros.ProbarCodigo("HOLAMUNDO"));
        }
    }
}

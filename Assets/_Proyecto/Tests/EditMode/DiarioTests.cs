using System.Collections.Generic;
using NUnit.Framework;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>El catálogo del Diario (F16) debe ser coherente con las convenciones de IDs.</summary>
    public class DiarioTests
    {
        [Test]
        public void Cada_fragmento_y_eco_usa_el_prefijo_de_su_recuerdo_y_no_se_repite()
        {
            var vistos = new HashSet<string>();
            foreach (var r in Diario.Recuerdos)
            {
                foreach (var f in r.Fragmentos)
                {
                    StringAssert.StartsWith(r.MapId + "_", f.Id);
                    Assert.IsTrue(vistos.Add(f.Id), $"ID repetido en el Diario: {f.Id}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(f.Titulo), f.Id);
                }
                foreach (var e in r.Ecos) StringAssert.StartsWith(r.MapId + "_", e.Id);
                Assert.LessOrEqual(r.Fragmentos.Length, 4, r.MapId);
            }
        }

        [Test]
        public void Los_cinco_mapas_estan_en_orden()
        {
            var ids = new List<string>();
            foreach (var r in Diario.Recuerdos) ids.Add(r.MapId);
            CollectionAssert.AreEqual(new[] { "M0", "R1", "R2", "R3", "R4" }, ids);
        }
    }
}

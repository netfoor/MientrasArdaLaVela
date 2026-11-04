using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// Que los diálogos se puedan avanzar en una compilación, no sólo dentro del editor (F05).
    ///
    /// Nace de un fallo real encontrado en el build del 2026-11-04: el juego se quedaba clavado
    /// en la primera línea y ninguna tecla respondía. El <c>LineAdvancer</c> del prefab de Yarn
    /// Spinner está en modo KeyCodes, pero el componente que lee el teclado lo añade
    /// <c>OnValidate</c>, que sólo existe en el editor; como el prefab vive en el paquete y es
    /// de sólo lectura, nunca queda guardado. En el editor se añadía al vuelo y todo parecía
    /// correcto. Por eso el avance lo pone <see cref="AvanceDialogo"/>, y estas pruebas vigilan
    /// que siga en pie: sin él, el juego compilado es injugable.
    /// </summary>
    public class AvanceDialogoTests
    {
        [Test]
        public void AvanceDialogo_se_crea_solo_al_cargar_la_escena()
        {
            var crear = typeof(AvanceDialogo).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.GetCustomAttributes<RuntimeInitializeOnLoadMethodAttribute>()
                    .Any(a => a.loadType == RuntimeInitializeLoadType.AfterSceneLoad));

            Assert.NotNull(crear, "AvanceDialogo debe crearse solo tras cargar la escena: si hay "
                                + "que montarlo a mano en cada escena, la primera que se olvide "
                                + "deja el juego compilado sin forma de avanzar los diálogos.");
        }

        [Test]
        public void AvanceDialogo_reinicia_su_estatico()
        {
            // Reload Domain está desactivado: sin este reinicio, el singleton sobrevive al salir
            // de Play Mode y en la siguiente partida nadie vuelve a crear el objeto.
            var reinicio = typeof(AvanceDialogo).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.GetCustomAttributes<RuntimeInitializeOnLoadMethodAttribute>()
                    .Any(a => a.loadType == RuntimeInitializeLoadType.SubsystemRegistration));

            Assert.NotNull(reinicio, "AvanceDialogo guarda una instancia estática y debe reiniciarla "
                                   + "en SubsystemRegistration (Reload Domain está desactivado).");
        }

        [Test]
        public void El_avance_usa_la_misma_tecla_que_interactuar()
        {
            // Init y Liberar son privados y los llama Unity al arrancar; en EditMode hay que
            // invocarlos a mano para que las acciones existan durante la prueba.
            static MethodInfo Metodo(string nombre) =>
                typeof(GameInput).GetMethod(nombre, BindingFlags.Static | BindingFlags.NonPublic);

            Metodo("Init").Invoke(null, null);
            try
            {
                var enlaces = GameInput.Interact.bindings.Select(b => b.path).ToArray();
                CollectionAssert.Contains(enlaces, "<Keyboard>/space", "Espacio debe avanzar el diálogo.");
                CollectionAssert.Contains(enlaces, "<Keyboard>/e", "E debe avanzar el diálogo.");
                CollectionAssert.Contains(enlaces, "<Mouse>/leftButton", "El clic debe avanzar el diálogo.");
            }
            finally
            {
                Metodo("Liberar")?.Invoke(null, null);
            }
        }
    }
}

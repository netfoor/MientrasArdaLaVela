using NUnit.Framework;
using UnityEngine;

namespace MientrasArdaLaVela.Tests
{
    /// <summary>
    /// Los tres códigos secretos de la Tabla 26 del Documento de Diseño (F23).
    ///
    /// Son lo único del juego que nunca se había comprobado: hay que escribirlos a mano en el
    /// Menú principal y nadie lo había hecho. Intenté probarlos sobre la compilación, tecleando
    /// con eventos del Input System, y no llegó ni un carácter a onTextInput, así que esa vía
    /// no vale. Aquí se prueba la parte que sí se puede aislar —Logros.ProbarCodigo, que es
    /// quien decide y aplica el efecto—; lo que queda fuera es la suscripción del menú a
    /// Keyboard.onTextInput, que son las dos líneas de MainMenu.OnEnable/OnDisable.
    /// </summary>
    public class CodigosTests
    {
        const string Galeria = "codigo_galeria";
        bool sombrero, lectura;
        int galeria;

        [SetUp]
        public void Guardar()
        {
            galeria = PlayerPrefs.GetInt(Galeria, 0);
            sombrero = Logros.SombreroTizne;
            lectura = Logros.ModoLectura;
            PlayerPrefs.SetInt(Galeria, 0);
            Logros.SombreroTizne = false;
            Logros.ModoLectura = false;
        }

        [TearDown]
        public void Restaurar()
        {
            PlayerPrefs.SetInt(Galeria, galeria);
            Logros.SombreroTizne = sombrero;
            Logros.ModoLectura = lectura;
            PlayerPrefs.Save();
        }

        [Test]
        public void CEMPASUCHIL_abre_la_galeria()
        {
            Assert.NotNull(Logros.ProbarCodigo("CEMPASUCHIL"), "el código debería aceptarse");
            Assert.AreEqual(1, PlayerPrefs.GetInt(Galeria, 0), "la Galería de bocetos debe quedar abierta");
        }

        [Test]
        public void TIZNE_pone_y_quita_el_sombrero()
        {
            Logros.ProbarCodigo("TIZNE");
            Assert.IsTrue(Logros.SombreroTizne, "Tizne debería ponerse el sombrero");
            Logros.ProbarCodigo("TIZNE");
            Assert.IsFalse(Logros.SombreroTizne, "y quitárselo al repetir el código");
        }

        [Test]
        public void PANDEMUERTO_activa_el_modo_lectura()
        {
            Logros.ProbarCodigo("PANDEMUERTO");
            Assert.IsTrue(Logros.ModoLectura, "el modo lectura debería activarse");
            Assert.AreEqual(Ajustes.NombresVelocidad.Length - 1, Ajustes.VelocidadTexto,
                            "y poner el texto en la velocidad instantánea");
        }

        [Test]
        public void Un_texto_cualquiera_no_activa_nada()
        {
            Assert.IsNull(Logros.ProbarCodigo("CALAVERA"));
            Assert.IsNull(Logros.ProbarCodigo(""));
            Assert.IsNull(Logros.ProbarCodigo(null));
        }

        [Test]
        public void PANDEMUERTO_no_dispara_tambien_TIZNE()
        {
            // Termina en "...TO", no en "TIZNE", pero el guardián está puesto a propósito:
            // si se comparara por "contiene", un código activaría al otro.
            Logros.ProbarCodigo("PANDEMUERTO");
            Assert.IsFalse(Logros.SombreroTizne, "PANDEMUERTO no debe tocar el sombrero de Tizne");
        }

        [Test]
        public void El_codigo_se_reconoce_al_final_de_lo_tecleado()
        {
            // El menú guarda los últimos 16 caracteres: si alguien se equivoca y sigue
            // escribiendo, el código debe valer igual.
            Assert.NotNull(Logros.ProbarCodigo("XYZCEMPASUCHIL"), "debe leerse al final de lo tecleado");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Logros (F23, Tabla 24 del Documento de Diseño) y códigos secretos (Tabla 25).
    ///
    /// Los logros son del jugador, no de una partida: se guardan en PlayerPrefs y siguen
    /// ahí aunque se empiece una partida nueva. Las condiciones se calculan a partir de la
    /// partida (Cumplidos) y GameManager llama a Revisar después de cada avance.
    ///
    /// Fuera de Play Mode (pruebas EditMode) los logros sólo se guardan en memoria, para que
    /// correr las pruebas no desbloquee nada en el equipo del jugador.
    /// </summary>
    public static class Logros
    {
        public class Logro
        {
            public string Id, Titulo, Condicion, Recompensa;
            public Logro(string id, string titulo, string condicion, string recompensa)
            { Id = id; Titulo = titulo; Condicion = condicion; Recompensa = recompensa; }
        }

        public const string PrimeraLuz = "primera_luz", PanDosPiezas = "pan_dos_piezas", SillaVacia = "silla_vacia",
                            BajoLaCeniza = "bajo_la_ceniza", LaPromesa = "la_promesa", DosVelas = "dos_velas",
                            CaminoPetalos = "camino_petalos", MemoriaPropia = "memoria_propia", TodasLasVoces = "todas_las_voces";

        public static readonly Logro[] Todos =
        {
            new Logro(PrimeraLuz,    "Primera luz",       "Completar el tutorial en la ofrenda.",                 "Medalla"),
            new Logro(PanDosPiezas,  "Pan de dos piezas", "Completar el Recuerdo 1.",                             "Medalla"),
            new Logro(SillaVacia,    "La silla vacía",    "Completar el Recuerdo 2.",                             "Medalla"),
            new Logro(BajoLaCeniza,  "Bajo la ceniza",    "Completar el Recuerdo 3.",                             "Medalla"),
            new Logro(LaPromesa,     "La promesa",        "Completar el Recuerdo 4.",                             "Medalla"),
            new Logro(DosVelas,      "Dos velas",         "Terminar el juego.",                                   "Medalla y Galería de bocetos"),
            new Logro(CaminoPetalos, "Camino de pétalos", "Reunir los 25 pétalos de cempasúchil.",                "Escena adicional en los créditos"),
            new Logro(MemoriaPropia, "Memoria propia",    "Resolver los mosaicos sin pedir pistas a Tizne.",      "Collar alternativo para Tizne"),
            new Logro(TodasLasVoces, "Todas las voces",   "Hablar con todos los personajes y desmentir todos los Ecos.", "Nota especial en el Diario"),
        };

        /// <summary>Mapa que completa cada logro de historia.</summary>
        static readonly (string mapa, string logro)[] PorMapa =
        {
            ("M0", PrimeraLuz), ("R1", PanDosPiezas), ("R2", SillaVacia), ("R3", BajoLaCeniza), ("R4", LaPromesa), ("EP", DosVelas),
        };

        /// <summary>Nodos de los personajes con los que se puede hablar ("Todas las voces").</summary>
        public static readonly string[] Voces =
        {
            "M0_Lucia",
            "R1_Aurelio69", "R1_Marisol",
            "R2_Aurelio42", "R2_Marisol", "R2_Invitado", "R2_Nino",
            "R3_Ernesto", "R3_Aurelio20",
            "R4_Aurelio9",
        };

        // ---------------------------------------------------------------- pétalos (F24)
        public const int PetalosPorMapa = 5;
        public static readonly string[] MapasConPetalos = { "M0", "R1", "R2", "R3", "R4" };
        public static int TotalPetalos => PetalosPorMapa * MapasConPetalos.Length;   // 25

        // ---------------------------------------------------------------- condiciones
        /// <summary>Logros cuyas condiciones cumple la partida (función pura, se prueba en EditMode).</summary>
        public static List<string> Cumplidos(SaveData d)
        {
            var r = new List<string>();
            if (d == null) return r;
            foreach (var (mapa, logro) in PorMapa)
                if (d.completedMaps.Contains(mapa)) r.Add(logro);

            if (d.petals != null && d.petals.Distinct().Count() >= TotalPetalos) r.Add(CaminoPetalos);

            bool cuatro = new[] { "R1", "R2", "R3", "R4" }.All(d.completedMaps.Contains);
            if (cuatro && d.mosaicHints == 0) r.Add(MemoriaPropia);

            var ecos = Diario.Recuerdos.SelectMany(x => x.Ecos).Select(e => e.Id);
            if (d.talked != null && Voces.All(d.talked.Contains) && ecos.All(d.resolvedEchoes.Contains)) r.Add(TodasLasVoces);
            return r;
        }

        // ---------------------------------------------------------------- persistencia
        const string Clave = "logros";
        static HashSet<string> obtenidos;

        public static event Action<Logro> Desbloqueado;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() { obtenidos = null; Desbloqueado = null; }   // Reload Domain está desactivado

        static HashSet<string> Obtenidos
        {
            get
            {
                if (obtenidos == null)
                {
                    obtenidos = new HashSet<string>();
                    if (Application.isPlaying)
                        foreach (var id in PlayerPrefs.GetString(Clave, "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                            obtenidos.Add(id);
                }
                return obtenidos;
            }
        }

        public static bool Tiene(string id) => Obtenidos.Contains(id);
        public static int Cuantos => Todos.Count(l => Tiene(l.Id));
        public static Logro Buscar(string id) => Todos.FirstOrDefault(l => l.Id == id);

        /// <summary>Desbloquea los logros que la partida ya cumple. Devuelve cuántos fueron nuevos.</summary>
        public static int Revisar(SaveData d)
        {
            int nuevos = 0;
            foreach (var id in Cumplidos(d))
                if (Desbloquear(id)) nuevos++;
            return nuevos;
        }

        public static bool Desbloquear(string id)
        {
            var logro = Buscar(id);
            if (logro == null || !Obtenidos.Add(id)) return false;
            if (Application.isPlaying)
            {
                PlayerPrefs.SetString(Clave, string.Join(",", Obtenidos));
                PlayerPrefs.Save();
            }
            Desbloqueado?.Invoke(logro);
            return true;
        }

        /// <summary>Sólo para pruebas: olvida los logros en memoria.</summary>
        public static void OlvidarEnMemoria() => obtenidos = new HashSet<string>();

        // ---------------------------------------------------------------- recompensas y códigos
        public static bool GaleriaAbierta => Tiene(DosVelas) || PlayerPrefs.GetInt("codigo_galeria", 0) == 1;
        public static bool CollarTizne => Tiene(MemoriaPropia);

        public static bool SombreroTizne
        {
            get => PlayerPrefs.GetInt("codigo_sombrero", 0) == 1;
            set { PlayerPrefs.SetInt("codigo_sombrero", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool ModoLectura
        {
            get => PlayerPrefs.GetInt("codigo_lectura", 0) == 1;
            set
            {
                if (value == ModoLectura) return;
                if (value) PlayerPrefs.SetInt("vel_texto_previa", Ajustes.VelocidadTexto);
                PlayerPrefs.SetInt("codigo_lectura", value ? 1 : 0);
                Ajustes.VelocidadTexto = value ? Ajustes.NombresVelocidad.Length - 1 : PlayerPrefs.GetInt("vel_texto_previa", 1);
            }
        }

        public static readonly string[] Codigos = { "CEMPASUCHIL", "TIZNE", "PANDEMUERTO" };

        /// <summary>
        /// Busca un código al final de lo que se lleva tecleado en el menú principal. Si lo
        /// encuentra lo aplica y devuelve (título, mensaje); si no, null.
        /// </summary>
        public static (string titulo, string mensaje)? ProbarCodigo(string tecleado)
        {
            string t = (tecleado ?? "").ToUpperInvariant();
            foreach (var c in Codigos)
            {
                if (!t.EndsWith(c)) continue;
                // "PANDEMUERTO" no debe activar también "TIZNE" y viceversa: sólo cuenta el código completo.
                switch (c)
                {
                    case "CEMPASUCHIL":
                        PlayerPrefs.SetInt("codigo_galeria", 1); PlayerPrefs.Save();
                        return ("Código aceptado", "La Galería de bocetos está abierta.");
                    case "TIZNE":
                        SombreroTizne = !SombreroTizne;
                        return ("Código aceptado", SombreroTizne ? "Tizne se pone su sombrero de palma." : "Tizne guarda su sombrero.");
                    case "PANDEMUERTO":
                        ModoLectura = !ModoLectura;
                        return ("Código aceptado", ModoLectura ? "Modo lectura: el texto aparece completo al instante."
                                                               : "Modo lectura desactivado.");
                }
            }
            return null;
        }
    }
}

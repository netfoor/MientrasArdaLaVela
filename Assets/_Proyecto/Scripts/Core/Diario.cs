using System.Collections.Generic;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Contenido del Diario de recuerdos (F16): qué escribe Ámbar de cada recuerdo, de cada
    /// fragmento y de cada Eco desmentido. El diario sólo muestra lo que el jugador ya descubrió.
    /// Al construir un recuerdo nuevo, añade aquí su entrada.
    /// </summary>
    public static class Diario
    {
        public class Fragmento
        {
            public string Id, Titulo, Nota;
            public Fragmento(string id, string titulo, string nota) { Id = id; Titulo = titulo; Nota = nota; }
        }

        public class Eco
        {
            public string Id, Mentira, Verdad;
            public Eco(string id, string mentira, string verdad) { Id = id; Mentira = mentira; Verdad = verdad; }
        }

        public class Recuerdo
        {
            public string MapId, Escena, Titulo, Resumen;
            public Fragmento[] Fragmentos;
            public Eco[] Ecos;
        }

        public static readonly List<Recuerdo> Recuerdos = new List<Recuerdo>
        {
            new Recuerdo
            {
                MapId = "M0", Escena = SceneNames.Ofrenda, Titulo = "La ofrenda — 2025",
                Resumen = "Don Aurelio llegó siguiendo los pétalos, pero no recuerda para qué. Horneaba siempre dos piezas de pan: una para él y otra para alguien que olvidó.",
                Fragmentos = new[]
                {
                    new Fragmento("M0_foto", "La fotografía", "Aurelio joven, el primer día a cargo del horno. Se ve orgulloso... y solo."),
                    new Fragmento("M0_delantal", "El delantal", "Cincuenta años de anís y leña. Nunca quiso ver las manos de su padre."),
                    new Fragmento("M0_petalos", "El camino que se divide", "Lucía pone un solo camino de pétalos. Este año hay dos."),
                    new Fragmento("M0_sombrero", "Un sombrero de niño", "De palma, pequeño. De alguien que lo cuidaba del sol en el campo."),
                },
                Ecos = new Eco[0],
            },
            new Recuerdo
            {
                MapId = "R1", Escena = SceneNames.Recuerdo1, Titulo = "La panadería cerrada — 2015",
                Resumen = "El último día de La Espiga. Aurelio le dio las llaves a Marisol y Lucía preguntó por el tío Cuco.",
                Fragmentos = new[]
                {
                    new Fragmento("R1_foto", "La foto cortada", "Cortada con tijeras. Del otro lado: un hombro y el ala de un sombrero de palma."),
                    new Fragmento("R1_pregunta", "La pregunta de Lucía", "«Tenías un hermano. El tío Cuco.» Aurelio no contestó."),
                    new Fragmento("R1_lata", "Las cartas sin abrir", "Una lata llena de cartas de Refugio Cuautle. Llegaban cada noviembre. Ninguna abierta."),
                    new Fragmento("R1_charola", "Dos piezas apartadas", "Treinta años dejando dos panes junto a la puerta, por si alguien pasaba."),
                },
                Ecos = new[] { new Eco("R1_eco", "«Aurelio no tiene hermanos.»", "Su hermano le escribió durante años.") },
            },
            new Recuerdo
            {
                MapId = "R2", Escena = SceneNames.Recuerdo2, Titulo = "La silla vacía — 1988",
                Resumen = "La boda de Marisol. Rosa invitó a Refugio en secreto; llegó un telegrama y Aurelio lo rompió.",
                Fragmentos = new[]
                {
                    new Fragmento("R2_rosa", "Lo que dijo Rosa", "«Es el tío de tu hija. Ya pasaron veinte años.»"),
                    new Fragmento("R2_tarjeta", "La tarjeta de la silla vacía", "«Refugio», con la letra de Rosa. A la derecha de Aurelio, donde se sienta la familia."),
                    new Fragmento("R2_telegrama", "El telegrama roto", "«Llego mañana. Cuco.» Aurelio repite «por su culpa» como una oración aprendida."),
                    new Fragmento("R2_brindis", "El brindis interrumpido", "«Por los que están y por los que faltan...» Aurelio tiró la copa."),
                },
                Ecos = new[]
                {
                    new Eco("R2_eco1", "«Nadie invitó a Refugio.»", "Rosa lo invitó y le guardó un lugar."),
                    new Eco("R2_eco2", "«Refugio nunca quiso volver.»", "Escribió: «Llego mañana».")
                },
            },
            new Recuerdo
            {
                MapId = "R3", Escena = SceneNames.Recuerdo3, Titulo = "La noche del horno — 1966",
                Resumen = "El 31 de octubre de 1966 Aurelio retacó el horno de leña y se fue a ver a Rosa. El horno se incendió y don Ernesto se quemó las manos. Refugio dijo «fui yo» para cubrir a su hermano, y su padre lo corrió de la casa. Aurelio llegó tarde, lo vio irse y no dijo nada.",
                Fragmentos = new[]
                {
                    new Fragmento("R3_cama", "La cama tendida", "La de Cuco estaba deshecha. La de Aurelio, tendida: nadie durmió en ella."),
                    new Fragmento("R3_nota", "La nota para Rosa", "«Te veo en el atrio a las once.» Aurelio no estaba en casa."),
                    new Fragmento("R3_lena", "La leña de más", "El horno retacado para que aguantara solo. Lo dejó así y se fue."),
                    new Fragmento("R3_mirada", "La mirada de Refugio", "Miró hacia la cama vacía de su hermano... y dijo «fui yo»."),
                },
                Ecos = new[]
                {
                    new Eco("R3_eco1", "«Cuco se quedó dormido.»", "Cuco sí estaba en su cama; Aurelio no."),
                    new Eco("R3_eco2", "«Cuco se quedó dormido.»", "Aurelio salió a ver a Rosa."),
                    new Eco("R3_eco3", "«Cuco se quedó dormido.»", "Aurelio dejó el horno retacado de leña."),
                },
            },
            new Recuerdo
            {
                MapId = "R4", Escena = SceneNames.Recuerdo4, Titulo = "El campo de cempasúchil — 1955",
                Resumen = "A los nueve años, Aurelio se perdió en el campo. Refugio salió a buscarlo solo, con una vela, le regaló su canica azul y le hizo una promesa: si uno se pierde, el otro lo busca con una vela.",
                Fragmentos = new[]
                {
                    new Fragmento("R4_huellas", "Huellas pequeñas", "De huarache de niño. Van y vienen: alguien que no sabía a dónde ir."),
                    new Fragmento("R4_vela", "La vela de Refugio", "Cera fresca sobre una piedra. Se detuvo a volver a encenderla."),
                    new Fragmento("R4_canica", "La canica regalada", "«Te regalo mi canica azul.» Aurelio la guardó sesenta años."),
                    new Fragmento("R4_promesa", "La promesa", "«Si uno se pierde, el otro lo busca con una vela.»"),
                },
                Ecos = new Eco[0],   // en este recuerdo no hay Ecos: el Olvido se retira solo
            },
        };
    }
}

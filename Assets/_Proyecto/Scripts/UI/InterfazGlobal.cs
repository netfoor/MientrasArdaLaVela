using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Interfaz que existe en todas las escenas de juego y se construye sola en tiempo de
    /// ejecución (no depende de los constructores de escena):
    ///   · Menú de pausa (Esc) — Reanudar, Diario, Opciones, Salir al menú (F17)
    ///   · Diario de recuerdos (Tab) — una página por recuerdo visitado (F16)
    ///   · Opciones — música, efectos, velocidad del texto, pantalla completa, controles (F17)
    ///   · Logros y Galería de bocetos (F23), también desde el menú principal
    ///   · La vela de progreso, arriba a la derecha: se consume 25 % por recuerdo (F18)
    /// Mientras hay un menú abierto el juego se congela (Time.timeScale = 0).
    /// </summary>
    public class InterfazGlobal : MonoBehaviour
    {
        public static InterfazGlobal Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => Instancia = null;   // Reload Domain está desactivado

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (Instancia != null) return;
            var go = new GameObject("Interfaz global");
            DontDestroyOnLoad(go);
            go.AddComponent<InterfazGlobal>();
        }

        enum Pantalla { Ninguna, Pausa, Diario, Opciones, Logros, Galeria }

        static readonly Color Tinta = new Color(0.16f, 0.11f, 0.09f);
        static readonly Color Papel = new Color(0.93f, 0.88f, 0.76f, 0.98f);
        static readonly Color Oro = new Color(1f, 0.84f, 0.5f);
        static readonly Color BotonNormal = new Color(0.20f, 0.15f, 0.22f, 0.95f);
        static readonly Color BotonResaltado = new Color(0.55f, 0.36f, 0.18f, 1f);

        Pantalla actual = Pantalla.Ninguna;
        Pantalla vuelveA = Pantalla.Ninguna;   // de dónde se abrieron Opciones o Logros (pausa o menú principal)
        bool diarioDesdePausa;
        Action alCerrarExterno;
        bool congelado, bloqueoPrevio;
        float escalaPrevia = 1f;
        bool bloqueadoAntes;
        int ultimaDireccion;

        GameObject velo, pausa, diario, opciones, logros, galeria;
        Button primeroPausa, cerrarDiario, cerrarLogros, cerrarGaleria;
        TMP_Text logrosTexto, galeriaPie;
        Image galeriaImagen;
        int boceto;
        TMP_Text diarioTexto, diarioPie, txtVelocidad, txtPantalla;
        Slider sMusica, sEfectos;
        readonly List<Diario.Recuerdo> paginas = new List<Diario.Recuerdo>();
        int pagina;

        GameObject velaRaiz;
        Image cera;
        RectTransform llama;
        float ceraMostrada = 1f;
        GameManager suscrito;

        bool EnJuego => SceneManager.GetActiveScene().name != SceneNames.MainMenu;

        // ================================================================ ciclo de vida
        void Awake()
        {
            Instancia = this;
            ConstruirMenus();
            ConstruirVela();
            SceneManager.sceneLoaded += AlCargarEscena;
            AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            if (suscrito != null) suscrito.MapCompleted -= AlCompletarMapa;
            if (Instancia == this) Instancia = null;
        }

        void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            CerrarTodo();
            Ajustes.Aplicar();
            AsegurarEventSystem();
            velaRaiz.SetActive(EnJuego);
            ceraMostrada = CeraObjetivo();
        }

        void Update()
        {
            SuscribirGameManager();
            bool bloqueado = DialogueBridge.IsBusy || MosaicPuzzle.IsOpen || SceneTransition.Busy;

            if (actual == Pantalla.Ninguna)
            {
                // "bloqueadoAntes": el Esc que cierra el mosaico no debe abrir la pausa en el mismo cuadro.
                if (EnJuego && !bloqueado && !bloqueadoAntes)
                {
                    if (GameInput.Pause.WasPressedThisFrame()) Abrir(Pantalla.Pausa);
                    else if (GameInput.Journal.WasPressedThisFrame()) { diarioDesdePausa = false; Abrir(Pantalla.Diario); }
                }
            }
            else
            {
                if (GameInput.Pause.WasPressedThisFrame()) Atras();
                else if (actual == Pantalla.Diario && GameInput.Journal.WasPressedThisFrame()) Atras();
                else if (actual == Pantalla.Diario) Navegar(CambiarPagina);
                else if (actual == Pantalla.Galeria) Navegar(CambiarBoceto);
            }
            bloqueadoAntes = bloqueado;
            AnimarVela();
        }

        // ================================================================ abrir / cerrar
        void Abrir(Pantalla p)
        {
            if (actual == Pantalla.Ninguna && EnJuego) Congelar();
            actual = p;
            velo.SetActive(true);
            pausa.SetActive(p == Pantalla.Pausa);
            diario.SetActive(p == Pantalla.Diario);
            opciones.SetActive(p == Pantalla.Opciones);
            logros.SetActive(p == Pantalla.Logros);
            galeria.SetActive(p == Pantalla.Galeria);

            GameObject foco = null;
            if (p == Pantalla.Pausa) foco = primeroPausa.gameObject;
            if (p == Pantalla.Opciones) { RefrescarOpciones(); foco = sMusica.gameObject; }   // navegación desde el 1.er deslizador
            if (p == Pantalla.Diario) { PrepararDiario(); foco = cerrarDiario.gameObject; }
            if (p == Pantalla.Logros) { PintarLogros(); foco = cerrarLogros.gameObject; }
            if (p == Pantalla.Galeria) { boceto = 0; PintarBoceto(); foco = cerrarGaleria.gameObject; }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(foco);
        }

        void Atras()
        {
            switch (actual)
            {
                case Pantalla.Opciones:
                case Pantalla.Logros:
                case Pantalla.Galeria:
                    if (vuelveA == Pantalla.Pausa) Abrir(Pantalla.Pausa);
                    else { CerrarTodo(); var cb = alCerrarExterno; alCerrarExterno = null; cb?.Invoke(); }
                    break;
                case Pantalla.Diario:
                    if (diarioDesdePausa) Abrir(Pantalla.Pausa); else CerrarTodo();
                    break;
                default:
                    CerrarTodo();
                    break;
            }
        }

        void CerrarTodo()
        {
            actual = Pantalla.Ninguna;
            if (velo == null) return;
            velo.SetActive(false);
            pausa.SetActive(false);
            diario.SetActive(false);
            opciones.SetActive(false);
            logros.SetActive(false);
            galeria.SetActive(false);
            Descongelar();
        }

        void Congelar()
        {
            if (congelado) return;
            congelado = true;
            escalaPrevia = Time.timeScale;
            bloqueoPrevio = PlayerMovement.Locked;
            Time.timeScale = 0f;
            PlayerMovement.Locked = true;
        }

        void Descongelar()
        {
            if (!congelado) return;
            congelado = false;
            Time.timeScale = escalaPrevia <= 0f ? 1f : escalaPrevia;
            PlayerMovement.Locked = bloqueoPrevio;
        }

        /// <summary>Opciones desde el menú principal. alCerrar se llama al pulsar Volver.</summary>
        public static void AbrirOpciones(Action alCerrar) => AbrirDesdeMenu(Pantalla.Opciones, alCerrar);
        /// <summary>Logros desde el menú principal (F23).</summary>
        public static void AbrirLogros(Action alCerrar) => AbrirDesdeMenu(Pantalla.Logros, alCerrar);
        /// <summary>Galería de bocetos desde el menú principal (recompensa de "Dos velas" o código CEMPASUCHIL).</summary>
        public static void AbrirGaleria(Action alCerrar) => AbrirDesdeMenu(Pantalla.Galeria, alCerrar);

        static void AbrirDesdeMenu(Pantalla p, Action alCerrar)
        {
            if (Instancia == null) Crear();
            Instancia.alCerrarExterno = alCerrar;
            Instancia.vuelveA = Pantalla.Ninguna;
            Instancia.Abrir(p);
        }

        // ================================================================ acciones de la pausa
        void Reanudar() => CerrarTodo();
        void DiarioDesdePausa() { diarioDesdePausa = true; Abrir(Pantalla.Diario); }
        void OpcionesDesdePausa() { vuelveA = Pantalla.Pausa; Abrir(Pantalla.Opciones); }
        void LogrosDesdePausa() { vuelveA = Pantalla.Pausa; Abrir(Pantalla.Logros); }

        void SalirAlMenu()
        {
            CerrarTodo();
            Time.timeScale = 1f;
            PlayerMovement.Locked = false;
            GameManager.Ensure().GoToMainMenu();   // guarda antes de salir
        }

        // ================================================================ diario
        void PrepararDiario()
        {
            paginas.Clear();
            var gm = GameManager.Ensure();
            string escenaActual = SceneManager.GetActiveScene().name;
            pagina = 0;
            foreach (var r in Diario.Recuerdos)
            {
                bool visible = r.MapId == "M0" || r.Escena == escenaActual || gm.IsMapCompleted(r.MapId)
                               || gm.CountFragments(r.MapId + "_") > 0;
                if (!visible) continue;
                if (r.Escena == escenaActual) pagina = paginas.Count;
                paginas.Add(r);
            }
            notaEspecial = Logros.Tiene(Logros.TodasLasVoces);
            PintarPagina();
        }

        bool notaEspecial;
        int TotalPaginas => paginas.Count + (notaEspecial ? 1 : 0);

        void Navegar(Action<int> cambiar)
        {
            var m = GameInput.Move.ReadValue<Vector2>();
            int dir = m.x > 0.5f ? 1 : m.x < -0.5f ? -1 : 0;
            if (dir != 0 && dir != ultimaDireccion) cambiar(dir);
            ultimaDireccion = dir;
        }

        void CambiarPagina(int dir)
        {
            if (TotalPaginas == 0) return;
            pagina = Mathf.Clamp(pagina + dir, 0, TotalPaginas - 1);
            PintarPagina();
        }

        const string NotaDeAmbar =
            "<size=46><b>Nota de Ámbar</b></size>\n<color=#6B4A1E><i>Escrita en la última hoja, con letra apretada</i></color>\n\n" +
            "Esta vez escuché a todos. A la niña que se durmió junto al altar, al panadero de cada edad, " +
            "a Marisol en la panadería y en la boda, al invitado que no se acordaba de su propio chiste, " +
            "al niño que corría entre las mesas, a Don Ernesto junto al horno.\n\n" +
            "Y también a los Ecos. Cada uno repetía una mentira pequeña, de esas que uno se cuenta " +
            "para no extrañar tanto. Ninguno mentía por maldad: mentían para que doliera menos.\n\n" +
            "Guiar almas no es enseñarles el camino. Es quedarse a oír lo que tienen que decir " +
            "hasta que ellas mismas se acuerdan de a dónde iban.\n\n" +
            "Tizne dice que ya lo sabía. Tizne siempre dice eso.";

        void PintarPagina()
        {
            if (TotalPaginas == 0) { diarioTexto.text = ""; return; }
            diarioPie.text = $"<  Página {pagina + 1} de {TotalPaginas}  >        (flechas para pasar página · Tab o Esc: cerrar)";
            if (pagina >= paginas.Count) { diarioTexto.text = NotaDeAmbar; return; }
            var r = paginas[pagina];
            var gm = GameManager.Ensure();
            bool completo = gm.IsMapCompleted(r.MapId);
            var sb = new StringBuilder();
            sb.Append($"<size=46><b>{r.Titulo}</b></size>\n");
            sb.Append(completo ? "<color=#6B4A1E><i>Recuerdo completo</i></color>\n\n" : "<color=#8A6A40><i>Todavía hay niebla aquí...</i></color>\n\n");
            if (completo && !string.IsNullOrEmpty(r.Resumen)) sb.Append(r.Resumen).Append("\n\n");

            int encontrados = 0;
            foreach (var f in r.Fragmentos) if (gm.HasFragment(f.Id)) encontrados++;
            sb.Append($"<b>Fragmentos {encontrados}/{Mathf.Max(4, r.Fragmentos.Length)}</b>\n");
            foreach (var f in r.Fragmentos)
                sb.Append(gm.HasFragment(f.Id) ? $"•  <b>{f.Titulo}</b>. {f.Nota}\n" : "•  <color=#9A8A70>???</color>\n");

            bool cabecera = false;
            foreach (var e in r.Ecos)
            {
                if (!gm.IsEchoResolved(e.Id)) continue;
                if (!cabecera) { sb.Append("\n<b>Ecos desmentidos</b>\n"); cabecera = true; }
                sb.Append($"<s>{e.Mentira}</s>  {e.Verdad}\n");
            }
            if (Array.IndexOf(Logros.MapasConPetalos, r.MapId) >= 0)
                sb.Append($"\n<color=#B8641A><b>Pétalos de cempasúchil {gm.CountPetals(r.MapId + "_")}/{Logros.PetalosPorMapa}</b></color>");
            diarioTexto.text = sb.ToString();
        }

        // ================================================================ logros (F23)
        void PintarLogros()
        {
            var gm = GameManager.Instance;
            var sb = new StringBuilder();
            sb.Append($"<b>{Logros.Cuantos} de {Logros.Todos.Length} logros</b>");
            if (gm != null) sb.Append($"        Pétalos de esta partida: {gm.CountPetals()}/{Logros.TotalPetalos}");
            sb.Append("\n\n");
            foreach (var l in Logros.Todos)
            {
                if (Logros.Tiene(l.Id))
                    sb.Append($"<color=#B8641A>•</color>  <b>{l.Titulo}</b>. {l.Condicion}  <color=#8A6A40><i>{l.Recompensa}</i></color>\n");
                else
                    sb.Append($"<color=#9A8A70>•  <b>{l.Titulo}</b>. {l.Condicion}</color>\n");
            }
            var activos = new List<string>();
            if (Logros.SombreroTizne) activos.Add("sombrero de palma");
            if (Logros.ModoLectura) activos.Add("modo lectura");
            if (PlayerPrefs.GetInt("codigo_galeria", 0) == 1) activos.Add("galería");
            if (activos.Count > 0) sb.Append($"\n<color=#8A6A40><i>Códigos activos: {string.Join(", ", activos)}</i></color>");
            logrosTexto.text = sb.ToString();
        }

        // ================================================================ galería de bocetos (F23)
        static readonly (string archivo, string titulo)[] Bocetos =
        {
            ("boceto_M0", "La ofrenda: el pan de dos piezas"),
            ("boceto_R1", "Panadería La Espiga, 1998"),
            ("boceto_R2", "La boda: la caja de cerillos"),
            ("boceto_R3", "El horno, 1976: la canica azul"),
            ("boceto_R4", "El campo de cempasúchil, 1965"),
        };

        void CambiarBoceto(int dir)
        {
            boceto = Mathf.Clamp(boceto + dir, 0, Bocetos.Length - 1);
            PintarBoceto();
        }

        void PintarBoceto()
        {
            var (archivo, titulo) = Bocetos[boceto];
            var sp = Resources.Load<Sprite>("Galeria/" + archivo);
            galeriaImagen.sprite = sp;
            galeriaImagen.color = sp != null ? Color.white : new Color(1f, 1f, 1f, 0.1f);
            galeriaPie.text = $"<  {titulo}  ·  {boceto + 1} de {Bocetos.Length}  >";
        }

        // ================================================================ opciones
        void RefrescarOpciones()
        {
            sMusica.SetValueWithoutNotify(Ajustes.VolumenMusica);
            sEfectos.SetValueWithoutNotify(Ajustes.VolumenEfectos);
            txtVelocidad.text = $"Velocidad del texto: {Ajustes.NombresVelocidad[Ajustes.VelocidadTexto]}";
            txtPantalla.text = $"Pantalla completa: {(Ajustes.PantallaCompleta ? "Sí" : "No")}";
        }

        void CiclarVelocidad()
        {
            Ajustes.VelocidadTexto = (Ajustes.VelocidadTexto + 1) % Ajustes.NombresVelocidad.Length;
            RefrescarOpciones();
        }

        void AlternarPantalla()
        {
            Ajustes.PantallaCompleta = !Ajustes.PantallaCompleta;
            RefrescarOpciones();
        }

        // ================================================================ vela (F18)
        float CeraObjetivo()
        {
            var gm = GameManager.Instance;
            if (gm == null) return 1f;
            int n = 0;
            foreach (var id in new[] { "R1", "R2", "R3", "R4" }) if (gm.IsMapCompleted(id)) n++;
            return Mathf.Max(0.05f, 1f - 0.25f * n);
        }

        void SuscribirGameManager()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm == suscrito) return;
            if (suscrito != null) suscrito.MapCompleted -= AlCompletarMapa;
            gm.MapCompleted += AlCompletarMapa;
            suscrito = gm;
            ceraMostrada = CeraObjetivo();
        }

        void AlCompletarMapa(string id) { /* AnimarVela baja la cera poco a poco hasta el nuevo nivel */ }

        void AnimarVela()
        {
            if (!velaRaiz.activeSelf) return;
            ceraMostrada = Mathf.MoveTowards(ceraMostrada, CeraObjetivo(), Time.unscaledDeltaTime * 0.08f);
            cera.fillAmount = ceraMostrada;
            const float baseY = 26f, alto = 150f;
            float t = Time.unscaledTime;
            llama.anchoredPosition = new Vector2(0f, baseY + alto * ceraMostrada + 4f);
            float parpadeo = 1f + 0.08f * (Mathf.PerlinNoise(t * 3.1f, 0.3f) - 0.5f) * 2f;
            llama.localScale = new Vector3(1f / Mathf.Sqrt(parpadeo), parpadeo, 1f);
        }

        // ================================================================ construcción
        void ConstruirMenus()
        {
            var canvas = NuevoCanvas("Menus", 200);

            velo = Rect("Velo", canvas.transform, Vector2.zero, Vector2.zero, estirar: true).gameObject;
            velo.AddComponent<Image>().color = new Color(0.03f, 0.02f, 0.05f, 0.72f);

            // --- Pausa ---
            pausa = Panel(canvas.transform, "Pausa", new Vector2(640f, 700f), new Color(0.10f, 0.08f, 0.12f, 0.96f));
            Texto(pausa.transform, "Pausa", 60f, new Vector2(0f, 265f), new Vector2(560f, 90f), Oro, TextAlignmentOptions.Center);
            primeroPausa = Boton(pausa.transform, "Reanudar", new Vector2(0f, 150f), Reanudar);
            Boton(pausa.transform, "Diario de recuerdos", new Vector2(0f, 60f), DiarioDesdePausa);
            Boton(pausa.transform, "Logros", new Vector2(0f, -30f), LogrosDesdePausa);
            Boton(pausa.transform, "Opciones", new Vector2(0f, -120f), OpcionesDesdePausa);
            Boton(pausa.transform, "Salir al menú", new Vector2(0f, -210f), SalirAlMenu);

            // --- Diario ---
            diario = Panel(canvas.transform, "Diario", new Vector2(1320f, 900f), Papel);
            Texto(diario.transform, "Diario de recuerdos", 38f, new Vector2(0f, 395f), new Vector2(1200f, 60f),
                  new Color(0.45f, 0.30f, 0.16f), TextAlignmentOptions.Center);
            diarioTexto = Texto(diario.transform, "", 30f, new Vector2(0f, -20f), new Vector2(1180f, 700f), Tinta, TextAlignmentOptions.TopLeft);
            diarioPie = Texto(diario.transform, "", 26f, new Vector2(0f, -405f), new Vector2(1200f, 50f),
                              new Color(0.45f, 0.30f, 0.16f), TextAlignmentOptions.Center);
            Boton(diario.transform, "<", new Vector2(-600f, -405f), () => CambiarPagina(-1), ancho: 70f);
            Boton(diario.transform, ">", new Vector2(600f, -405f), () => CambiarPagina(1), ancho: 70f);
            cerrarDiario = Boton(diario.transform, "Cerrar", new Vector2(520f, 395f), Atras, ancho: 180f);

            // --- Opciones ---
            opciones = Panel(canvas.transform, "Opciones", new Vector2(1100f, 860f), new Color(0.10f, 0.08f, 0.12f, 0.96f));
            Texto(opciones.transform, "Opciones", 56f, new Vector2(0f, 360f), new Vector2(900f, 80f), Oro, TextAlignmentOptions.Center);
            Texto(opciones.transform, "Música", 32f, new Vector2(-300f, 260f), new Vector2(300f, 50f), Color.white, TextAlignmentOptions.Left);
            sMusica = Deslizador(opciones.transform, new Vector2(150f, 260f), v => Ajustes.VolumenMusica = v);
            Texto(opciones.transform, "Efectos", 32f, new Vector2(-300f, 190f), new Vector2(300f, 50f), Color.white, TextAlignmentOptions.Left);
            sEfectos = Deslizador(opciones.transform, new Vector2(150f, 190f), v => { Ajustes.VolumenEfectos = v; Sonidos.Play("S02", 0.9f); });
            var bVel = Boton(opciones.transform, "", new Vector2(0f, 105f), CiclarVelocidad, ancho: 620f);
            txtVelocidad = bVel.GetComponentInChildren<TMP_Text>();
            var bPan = Boton(opciones.transform, "", new Vector2(0f, 20f), AlternarPantalla, ancho: 620f);
            txtPantalla = bPan.GetComponentInChildren<TMP_Text>();
            Texto(opciones.transform,
                  "<b>Controles</b>\n" +
                  "Mover: W A S D / flechas / stick      Interactuar: E, Espacio o clic / A\n" +
                  "Luz de vela: mantener F / gatillo derecho      Olfatear (Tizne): H / Y\n" +
                  "Diario: Tab / Select      Pausa: Esc / Start\n" +
                  "Mosaico: Q / E o clic izq. / der. para girar · R reiniciar · H pista",
                  24f, new Vector2(0f, -170f), new Vector2(1000f, 250f), new Color(0.85f, 0.82f, 0.78f), TextAlignmentOptions.Center);
            Boton(opciones.transform, "Volver", new Vector2(0f, -360f), Atras);

            // --- Logros ---
            logros = Panel(canvas.transform, "Logros", new Vector2(1400f, 900f), Papel);
            Texto(logros.transform, "Logros", 44f, new Vector2(0f, 395f), new Vector2(1200f, 60f),
                  new Color(0.45f, 0.30f, 0.16f), TextAlignmentOptions.Center);
            logrosTexto = Texto(logros.transform, "", 28f, new Vector2(0f, -30f), new Vector2(1300f, 760f), Tinta, TextAlignmentOptions.TopLeft);
            cerrarLogros = Boton(logros.transform, "Volver", new Vector2(560f, 395f), Atras, ancho: 200f);

            // --- Galería de bocetos ---
            galeria = Panel(canvas.transform, "Galeria", new Vector2(1000f, 960f), new Color(0.10f, 0.08f, 0.12f, 0.97f));
            Texto(galeria.transform, "Galería de bocetos", 44f, new Vector2(0f, 425f), new Vector2(900f, 60f), Oro, TextAlignmentOptions.Center);
            var rtImg = Rect("Boceto", galeria.transform, new Vector2(0f, 10f), new Vector2(760f, 760f));
            galeriaImagen = rtImg.gameObject.AddComponent<Image>();
            galeriaImagen.preserveAspect = true;
            galeriaImagen.raycastTarget = false;
            galeriaPie = Texto(galeria.transform, "", 28f, new Vector2(0f, -420f), new Vector2(760f, 50f),
                               new Color(0.85f, 0.82f, 0.78f), TextAlignmentOptions.Center);
            Boton(galeria.transform, "<", new Vector2(-440f, -420f), () => CambiarBoceto(-1), ancho: 70f);
            Boton(galeria.transform, ">", new Vector2(440f, -420f), () => CambiarBoceto(1), ancho: 70f);
            cerrarGaleria = Boton(galeria.transform, "Volver", new Vector2(360f, 425f), Atras, ancho: 200f);

            velo.SetActive(false); pausa.SetActive(false); diario.SetActive(false); opciones.SetActive(false);
            logros.SetActive(false); galeria.SetActive(false);
        }

        void ConstruirVela()
        {
            var canvas = NuevoCanvas("Vela", 5);
            velaRaiz = Rect("Vela de progreso", canvas.transform, new Vector2(-90f, -40f), new Vector2(80f, 230f)).gameObject;
            var rt = (RectTransform)velaRaiz.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(-90f, -40f);
            rt.sizeDelta = new Vector2(80f, 230f);

            var plato = Rect("Plato", rt, new Vector2(0f, 10f), new Vector2(76f, 16f), abajo: true);
            plato.gameObject.AddComponent<Image>().color = new Color(0.55f, 0.42f, 0.30f);

            var hueco = Rect("Hueco", rt, new Vector2(0f, 26f), new Vector2(36f, 150f), abajo: true);
            hueco.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            var rtCera = Rect("Cera", rt, new Vector2(0f, 26f), new Vector2(36f, 150f), abajo: true);
            cera = rtCera.gameObject.AddComponent<Image>();
            cera.color = new Color(0.97f, 0.93f, 0.82f);
            cera.sprite = BlancoSprite();
            cera.type = Image.Type.Filled;
            cera.fillMethod = Image.FillMethod.Vertical;
            cera.fillOrigin = (int)Image.OriginVertical.Bottom;

            llama = Rect("Llama", rt, new Vector2(0f, 180f), new Vector2(28f, 42f), abajo: true);
            llama.pivot = new Vector2(0.5f, 0f);
            var img = llama.gameObject.AddComponent<Image>();
            var sp = Resources.Load<Sprite>("UI/llama");
            if (sp != null) img.sprite = sp; else img.color = new Color(1f, 0.75f, 0.3f);
            img.preserveAspect = true;
        }

        static Sprite blanco;
        static Sprite BlancoSprite()
        {
            if (blanco == null)
            {
                var tex = new Texture2D(2, 2);
                tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                tex.Apply();
                blanco = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            }
            return blanco;
        }

        // ================================================================ utilidades de UI
        Canvas NuevoCanvas(string nombre, int orden)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = orden;
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920f, 1080f);
            s.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        static RectTransform Rect(string nombre, Transform padre, Vector2 pos, Vector2 tam, bool estirar = false, bool abajo = false)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(padre, false);
            if (estirar)
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                return rt;
            }
            rt.anchorMin = rt.anchorMax = abajo ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            rt.pivot = abajo ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            return rt;
        }

        static GameObject Panel(Transform padre, string nombre, Vector2 tam, Color color)
        {
            var rt = Rect(nombre, padre, Vector2.zero, tam);
            rt.gameObject.AddComponent<Image>().color = color;
            return rt.gameObject;
        }

        static TMP_Text Texto(Transform padre, string texto, float tam, Vector2 pos, Vector2 tamRect, Color color,
                              TextAlignmentOptions alin)
        {
            var rt = Rect("Texto", padre, pos, tamRect);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = texto;
            t.fontSize = tam;
            t.color = color;
            t.alignment = alin;
            t.raycastTarget = false;
            return t;
        }

        static Button Boton(Transform padre, string texto, Vector2 pos, UnityAction accion, float ancho = 460f)
        {
            var rt = Rect("Boton " + texto, padre, pos, new Vector2(ancho, 72f));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Color.white;
            var b = rt.gameObject.AddComponent<Button>();
            var colores = b.colors;
            colores.normalColor = BotonNormal;
            colores.highlightedColor = BotonResaltado;
            colores.selectedColor = BotonResaltado;
            colores.pressedColor = Oro;
            colores.colorMultiplier = 1f;
            b.colors = colores;
            b.onClick.AddListener(accion);
            var t = Texto(rt, texto, 34f, Vector2.zero, new Vector2(ancho - 20f, 64f), Color.white, TextAlignmentOptions.Center);
            t.name = "Etiqueta";
            return b;
        }

        static Slider Deslizador(Transform padre, Vector2 pos, UnityAction<float> alCambiar)
        {
            var rt = Rect("Deslizador", padre, pos, new Vector2(520f, 34f));
            var fondo = Rect("Fondo", rt, Vector2.zero, Vector2.zero, estirar: true);
            fondo.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.22f, 0.28f);

            var area = Rect("Relleno", rt, Vector2.zero, Vector2.zero, estirar: true);
            var relleno = Rect("Barra", area, Vector2.zero, Vector2.zero, estirar: true);
            relleno.gameObject.AddComponent<Image>().color = Oro;

            var areaAsa = Rect("AreaAsa", rt, Vector2.zero, Vector2.zero, estirar: true);
            var asa = Rect("Asa", areaAsa, Vector2.zero, Vector2.zero, estirar: true);
            asa.anchorMin = new Vector2(0f, 0f); asa.anchorMax = new Vector2(0f, 1f);
            asa.sizeDelta = new Vector2(22f, 18f);
            var imgAsa = asa.gameObject.AddComponent<Image>();
            imgAsa.color = Color.white;

            var s = rt.gameObject.AddComponent<Slider>();
            s.fillRect = relleno;
            s.handleRect = asa;
            s.targetGraphic = imgAsa;
            s.direction = Slider.Direction.LeftToRight;
            s.minValue = 0f; s.maxValue = 1f;
            var colores = s.colors;
            colores.normalColor = new Color(0.85f, 0.85f, 0.85f);
            colores.selectedColor = Oro;
            colores.highlightedColor = Oro;
            s.colors = colores;
            s.onValueChanged.AddListener(alCambiar);
            return s;
        }

        static void AsegurarEventSystem()
        {
            if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
    }
}

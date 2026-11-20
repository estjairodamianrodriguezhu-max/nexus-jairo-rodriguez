using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ProyectoNEXUS;

namespace NEXUS.WindowsForms;

public sealed class Nexus3Form : Form
{
    // Double buffering avoids clearing the background between animation frames.
    private sealed class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }
    }
    readonly Color bg = Color.FromArgb(6, 12, 27);
    readonly Color surface = Color.FromArgb(16, 31, 53);
    readonly Color cyan = Color.FromArgb(57, 222, 244);
    readonly Color muted = Color.FromArgb(167, 191, 214);
    readonly System.Windows.Forms.Timer animation = new() { Interval = 65 };
    readonly Random rng = new();
    Panel page = null!;
    Panel? animatedHero;
    ProgressBar? splashBar;
    Label clock = null!;
    Label status = null!;
    TextBox? nameBox;
    NumericUpDown? ageBox;
    Explorador? player;
    Anomalia anomaly = new("Interferencia Cuántica IRIS", 40);
    readonly NexusSave save = new();
    int frame;
    int progress;
    readonly System.Collections.Generic.List<string> history = new();
    int exploredMissions;
    int scannedSectors;
    bool animationsEnabled = true;
    void Log(string message)
    {
        history.Insert(0, DateTime.Now.ToString("HH:mm") + "  •  " + message);
        if (history.Count > 25) history.RemoveAt(history.Count - 1);
        Persist();
    }

    void Persist()
    {
        if (player == null) return;
        save.PlayerName = player.Nombre;
        save.Age = player.Edad;
        save.Energy = player.Energia;
        save.Stability = player.Estabilidad;
        save.Interference = anomaly.NivelInterferencia;
        save.Missions = exploredMissions;
        save.Scans = scannedSectors;
        save.Log = new System.Collections.Generic.List<string>(history);
        try { NexusProgress.Save(save); }
        catch (Exception ex) { status.Text = "⚠ No se pudo guardar la partida: " + ex.Message; }
    }

    void Award(int xp, string reason)
    {
        save.Experience += xp;
        foreach (var achievement in NexusProgress.NewAchievements(save))
        {
            history.Insert(0, "★ LOGRO DESBLOQUEADO: " + achievement);
            MessageBox.Show(achievement, "NEXUS // LOGRO DESBLOQUEADO");
        }
        Log(reason + "  (+" + xp + " XP)");
    }

    void Restore()
    {
        try
        {
            var loaded = NexusProgress.Load();
            if (loaded == null) return;
            player = new Explorador(loaded.PlayerName, loaded.Age, loaded.Energy, loaded.Stability);
            anomaly = new Anomalia("Interferencia Cuántica IRIS", Math.Max(1, loaded.Interference));
            if (loaded.Interference == 0) anomaly.ReducirInterferencia(1);
            save.Experience = loaded.Experience;
            save.Interventions = loaded.Interventions;
            save.RechargeCount = loaded.RechargeCount;
            save.ScannedCells = loaded.ScannedCells;
            save.Achievements = loaded.Achievements;
            exploredMissions = loaded.Missions;
            scannedSectors = loaded.Scans;
            history.AddRange(loaded.Log);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo recuperar la partida guardada. Los archivos anteriores no se modificarán.\n" + ex.Message,
                "NEXUS // Guardado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    readonly string[] worlds = { "IRIS-07  •  VALLE DE CRISTAL", "IRIS-12  •  SECTOR ABISAL" };
    readonly string[] maps = { "..◆...????|.⌂..⚠..◆??|..P..●...?|~~....⚠...|~~~..◆..⌂.", "~~~~..◆.??|~⌂.~~⚠.??|..P.~~●...|.⚠...~~~◆.|..◆...~~⌂." };

    public Nexus3Form()
    {
        Text = "NEXUS 4.0  //  SISTEMA IRIS  //  MISIONES + GUARDADO";
        MinimumSize = new Size(950, 660);
        Size = new Size(1250, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = bg;
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true;
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (progress < 100) return;
            if (e.Control && e.KeyCode == Keys.D1) { Home(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D2) { Profile(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D3) { Explore(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D4) { Anomalies(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D5) { Cartography(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.L) { History(); e.SuppressKeyPress = true; }
        };
        animation.Tick += (_, _) =>
        {
            frame++;
            if (clock != null && !clock.IsDisposed) clock.Text = DateTime.Now.ToString("dd MMM yyyy  •  HH:mm:ss").ToUpperInvariant();
            if (animationsEnabled && animatedHero != null && !animatedHero.IsDisposed) animatedHero.Invalidate();
            if (progress < 100)
            {
                progress = Math.Min(100, progress + rng.Next(2, 6));
                if (splashBar != null && !splashBar.IsDisposed) splashBar.Value = progress;
                if (progress == 100) { Build(); Home(); }
            }
        };
        Shown += (_, _) => { Restore(); Splash(); animation.Start(); };
        FormClosing += (_, _) => Persist();
    }

    Label Label(string text, int size = 11, bool bold = false, Color? color = null)
        => new() { Text = text, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = color ?? Color.White, BackColor = Color.Transparent, AutoSize = false };

    Button Button(string title, Action action, bool primary = false)
    {
        var b = new Button { Text = title, Dock = DockStyle.Top, Height = 49, Margin = new Padding(0, 0, 0, 11), Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold), BackColor = primary ? cyan : Color.FromArgb(28, 49, 74), ForeColor = primary ? bg : Color.White };
        b.FlatAppearance.BorderSize = primary ? 0 : 1;
        b.FlatAppearance.BorderColor = Color.FromArgb(50, 97, 128);
        b.MouseEnter += (_, _) => b.BackColor = primary ? Color.FromArgb(139, 245, 255) : Color.FromArgb(43, 75, 104);
        b.MouseLeave += (_, _) => b.BackColor = primary ? cyan : Color.FromArgb(28, 49, 74);
        b.Click += (_, _) => action();
        return b;
    }

    Panel Card(int height)
    {
        var p = new Panel { Height = height, Dock = DockStyle.Top, BackColor = surface, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 14) };
        p.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(46, 95, 123)); e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
        return p;
    }

    void Splash()
    {
        Controls.Clear();
        var wrap = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, BackColor = bg, Padding = new Padding(80) };
        wrap.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
        wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        wrap.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        var title = Label("N  E  X  U  S", 44, true, cyan); title.TextAlign = ContentAlignment.MiddleCenter; title.Dock = DockStyle.Fill;
        var sub = Label("CONECTANDO CON LA RED DIMENSIONAL IRIS...", 12, false, muted); sub.TextAlign = ContentAlignment.MiddleCenter; sub.Dock = DockStyle.Fill;
        var bar = new ProgressBar { Maximum = 100, Value = progress, Dock = DockStyle.Fill, Margin = new Padding(120, 10, 120, 10) };
        splashBar = bar;
        wrap.Controls.Add(new Panel(), 0, 0); wrap.Controls.Add(title, 0, 1); wrap.Controls.Add(sub, 0, 2); wrap.Controls.Add(bar, 0, 3);
        Controls.Add(wrap);
    }

    void Build()
    {
        Controls.Clear();
        var nav = new Panel { Dock = DockStyle.Left, Width = 252, BackColor = Color.FromArgb(10, 22, 40), Padding = new Padding(15, 23, 15, 15) };
        var top = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(12, 26, 45), Padding = new Padding(25, 20, 20, 8) };
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.FromArgb(12, 26, 45), Padding = new Padding(17, 7, 0, 0) };
        page = new Panel { Dock = DockStyle.Fill, BackColor = bg, AutoScroll = true, Padding = new Padding(30, 22, 30, 25) };
        Controls.Add(page); Controls.Add(bottom); Controls.Add(top); Controls.Add(nav);
        var logo = Label("N E X U S", 24, true, cyan); logo.Dock = DockStyle.Top; logo.Height = 60;
        var caption = Label("SISTEMA DIMENSIONAL  /  V4.0", 9, false, muted); caption.Dock = DockStyle.Top; caption.Height = 62;
        var menu = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        (string, Action)[] actions = { ("⌂   PANEL PRINCIPAL", Home), ("◈   PERFIL DEL CADETE", Profile), ("◇   EXPLORACIÓN", Explore), ("⚠   ANOMALÍAS IRIS", Anomalies), ("▦   CARTOGRAFÍA", Cartography), ("✦   MISIONES Y LOGROS", Missions), ("⚡   RECARGAR ENERGÍA", Recharge), ("☷   BITÁCORA", History), ("◉   ANIMACIONES", ToggleAnimations), ("ⓘ   ACERCA DE", About) };
        int y = 0;
        foreach (var (name, action) in actions) { var b = Button(name, action); b.Dock = DockStyle.None; b.SetBounds(0, y, 218, 47); menu.Controls.Add(b); y += 59; }
        nav.Controls.Add(menu); nav.Controls.Add(caption); nav.Controls.Add(logo);
        var heading = Label("CENTRO DE OPERACIONES   /   IRIS", 13, true); heading.Dock = DockStyle.Left; heading.Width = 470;
        clock = Label("", 10, false, muted); clock.Dock = DockStyle.Right; clock.Width = 235; clock.TextAlign = ContentAlignment.MiddleRight;
        top.Controls.Add(clock); top.Controls.Add(heading);
        status = Label("● SISTEMA EN LÍNEA   •   SIMULACIÓN LOCAL   •   PROTOCOLO IRIS", 9, false, cyan); status.Dock = DockStyle.Fill;
        bottom.Controls.Add(status);
    }

    void Screen(string title, string subtitle)
    {
        page.Controls.Clear(); animatedHero = null;
        var h = Label(title.ToUpperInvariant(), 25, true); h.Dock = DockStyle.Top; h.Height = 55;
        var s = Label(subtitle, 11, false, muted); s.Dock = DockStyle.Top; s.Height = 44;
        page.Controls.Add(s); page.Controls.Add(h);
    }
    void Add(Control c) { c.Dock = DockStyle.Top; page.Controls.Add(c); page.Controls.SetChildIndex(c, 0); }

    void Info(string title, string body, int height = 115)
    {
        var card = Card(height);
        var h = Label(title, 14, true, cyan); h.Dock = DockStyle.Top; h.Height = 39;
        var b = Label(body, 11); b.Dock = DockStyle.Fill;
        card.Controls.Add(b); card.Controls.Add(h); Add(card);
    }

    void Hero(string title, string description)
    {
        var hero = new BufferedPanel { Dock = DockStyle.Top, Height = 177, BackColor = surface };
        hero.Paint += (_, e) =>
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using var gradient = new LinearGradientBrush(hero.ClientRectangle, Color.FromArgb(17, 52, 83), Color.FromArgb(8, 19, 39), LinearGradientMode.Horizontal);
            g.FillRectangle(gradient, hero.ClientRectangle);
            using var lines = new Pen(Color.FromArgb(25, 65, 105));
            for (int x = 0; x < hero.Width; x += 40) g.DrawLine(lines, x, 0, x, hero.Height);
            for (int y = 0; y < hero.Height; y += 40) g.DrawLine(lines, 0, y, hero.Width, y);
            int cx = Math.Max(135, hero.Width - 135), cy = hero.Height / 2;
            using var orbit = new Pen(Color.FromArgb(115, 57, 222, 244), 2);
            for (int i = 0; i < 3; i++) g.DrawEllipse(orbit, cx - 35 - i * 20, cy - 35 - i * 20, 70 + 40 * i, 70 + 40 * i);
            using var dot = new SolidBrush(cyan);
            double angle = (animationsEnabled ? frame : 0) * .04;
            g.FillEllipse(dot, cx + (int)(75 * Math.Cos(angle)) - 6, cy + (int)(75 * Math.Sin(angle)) - 6, 12, 12);
            using var font = new Font("Segoe UI", 23, FontStyle.Bold);
            using var smaller = new Font("Segoe UI", 10);
            g.DrawString(title, font, Brushes.White, new RectangleF(25, 33, Math.Max(170, hero.Width - 270), 55));
            using var brush = new SolidBrush(muted);
            g.DrawString(description, smaller, brush, new RectangleF(28, 95, Math.Max(170, hero.Width - 270), 60));
        };
        animatedHero = hero; Add(hero);
    }

    void Meter(string title, int value)
    {
        var c = Card(78);
        var l = Label(title + "     " + value + "%", 11, true, cyan); l.Dock = DockStyle.Top; l.Height = 30;
        var bar = new ProgressBar { Dock = DockStyle.Top, Height = 15, Maximum = 100, Value = Math.Clamp(value, 0, 100) };
        c.Controls.Add(bar); c.Controls.Add(l); Add(c);
    }

    void Home()
    {
        Screen("Panel principal", "NEXUS 4.0  •  Misiones, niveles y guardado automático.");
        Hero("PROTOCOLO NEXUS", "RED IRIS OPERATIVA   /   ACCESO DIMENSIONAL HABILITADO");
        Info("ESTADO DE LA MISIÓN", player == null ? "No hay explorador registrado. Crea un perfil para comenzar." :
            "CADETE: " + player.Nombre + "    EDAD: " + player.Edad + "\nENERGÍA: " + player.Energia + "%    ESTABILIDAD: " + player.Estabilidad + "%    INTERFERENCIA: " + anomaly.NivelInterferencia + "%", 125);
        Info("UNIVERSO IRIS", "02 realidades: Valle de Cristal y Sector Abisal.\nExplora, analiza anomalías y mantén tu energía vital.", 125);
        Add(Button("VER MISIONES Y LOGROS  ★", Missions, true));
        Info("ESTADÍSTICAS DE MISIÓN", "Inmersiones realizadas: " + exploredMissions + "     Sectores escaneados: " + scannedSectors +
            "\nNivel: " + NexusProgress.Level(save.Experience) + "     Experiencia: " + save.Experience + " XP", 112);
        Meter("PROGRESO DEL SIGUIENTE NIVEL", NexusProgress.LevelProgress(save.Experience));
        Add(Button(player == null ? "CREAR PERFIL DE EXPLORADOR" : "INICIAR EXPLORACIÓN", player == null ? Profile : Explore, true));
        if (player != null) { Meter("ENERGÍA VITAL", player.Energia); Meter("ESTABILIDAD", player.Estabilidad); }
    }

    void Profile()
    {
        Screen("Perfil del cadete", "Configura tu identidad para acceder a la simulación.");
        if (player != null)
        {
            Info("EXPLORADOR ACTIVO", "Nombre: " + player.Nombre + "\nEdad: " + player.Edad +
                "     Nivel: " + NexusProgress.Level(save.Experience) + "     XP: " + save.Experience, 118);
            Meter("ENERGÍA VITAL", player.Energia); Meter("ESTABILIDAD", player.Estabilidad);
            Add(Button("CONTINUAR EXPLORACIÓN", Explore, true)); return;
        }
        var card = Card(255);
        var l = Label("NOMBRE DEL EXPLORADOR", 11, true, cyan); l.SetBounds(22, 18, 380, 30);
        nameBox = new TextBox { Font = new Font("Segoe UI", 13), BackColor = Color.FromArgb(29, 49, 70), ForeColor = Color.White };
        nameBox.SetBounds(22, 52, 380, 36);
        var age = Label("EDAD", 11, true, cyan); age.SetBounds(22, 103, 250, 30);
        ageBox = new NumericUpDown { Minimum = 18, Maximum = 80, Value = 20, Font = new Font("Segoe UI", 12), BackColor = Color.FromArgb(29, 49, 70), ForeColor = Color.White };
        ageBox.SetBounds(22, 139, 130, 33);
        var activateProfileButton = Button("ACTIVAR PERFIL", () => {
            if (nameBox!.Text.Trim().Length < 2) { MessageBox.Show("Escribe un nombre de al menos dos caracteres."); return; }
            player = new Explorador(nameBox.Text.Trim(), (int)ageBox!.Value, 80, 90);
            status.Text = "● EXPLORADOR CONECTADO   •   " + player.Nombre.ToUpperInvariant(); Log("Cadete " + player.Nombre + " registrado"); Home();
        }, true);
        activateProfileButton.Dock = DockStyle.None; activateProfileButton.SetBounds(22, 190, 260, 44);
        card.Controls.AddRange(new Control[] { l, nameBox, age, ageBox, activateProfileButton }); Add(card);
    }

    bool RequirePlayer()
    {
        if (player != null) return true;
        MessageBox.Show("Primero registra tu perfil de explorador.", "NEXUS");
        Profile(); return false;
    }

    void Explore()
    {
        if (!RequirePlayer()) return;
        Screen("Exploración dimensional", "Elige una realidad para iniciar una inmersión.");
        Hero("PORTALES DIMENSIONALES", "SECTORES IRIS-07 E IRIS-12   /   DOS REALIDADES DISPONIBLES");
        Info("RECURSOS DEL CADETE", "Energía: " + player!.Energia + "%     Estabilidad: " + player.Estabilidad +
            "%\nCada inmersión consume 15 de energía y 5 de estabilidad.", 118);
        for (int i = worlds.Length - 1; i >= 0; i--)
        {
            int selected = i;
            var c = Card(118);
            var title = Label(worlds[i], 15, true, cyan); title.Dock = DockStyle.Top; title.Height = 42;
            var btn = Button("INICIAR INMERSIÓN", () => {
                if (player!.Energia < 20) { MessageBox.Show("Energía insuficiente. Recarga antes de explorar."); return; }
                player.ModificarEnergia(-15); player.ModificarEstabilidad(-5);
                exploredMissions++; Award(20, "Inmersión en " + worlds[selected]);
                MessageBox.Show("Inmersión completada en " + worlds[selected] + "\nEnergía: " + player.Energia + "%\nEstabilidad: " + player.Estabilidad + "%", "NEXUS");
                Explore();
            }, true);
            c.Controls.Add(btn); c.Controls.Add(title); Add(c);
        }
    }

    void Anomalies()
    {
        if (!RequirePlayer()) return;
        Screen("Anomalías IRIS", "Monitoreo de interferencia cuántica.");
        Meter("INTERFERENCIA CUÁNTICA", anomaly.NivelInterferencia);
        Info("ANÁLISIS DE RIESGO", "Interferencia actual: " + anomaly.NivelInterferencia +
            "%\nEnergía disponible: " + player!.Energia + "%\nIntervenir consume 10 de energía y reduce 15 de interferencia.", 155);
        Add(Button("ANALIZAR E INTERVENIR", () => {
            if (anomaly.NivelInterferencia == 0) { MessageBox.Show("Anomalía neutralizada."); return; }
            if (player.Energia < 15) { MessageBox.Show("Energía insuficiente."); return; }
            player.ModificarEnergia(-10); anomaly.ReducirInterferencia(15);
            save.Interventions++;
            Award(15, "Interferencia reducida a " + anomaly.NivelInterferencia + "%");
            Anomalies();
        }, true));
    }

    void Cartography()
    {
        Screen("Cartografía dimensional", "Explora mapas tácticos con escaneo de sectores.");
        Hero("ATLAS DIMENSIONAL", "MAPAS INTERACTIVOS   /   ESCÁNER DE RIESGOS");
        Add(Button("IRIS-07  /  VALLE DE CRISTAL", () => OpenMap(0), true));
        Add(Button("IRIS-12  /  SECTOR ABISAL", () => OpenMap(1), true));
        Info("LEYENDA", "P Explorador   ◆ Cristal   ⚠ Riesgo   ⌂ Base\n≈ Agua   ? Zona desconocida   ● Punto de interés", 120);
    }

    void OpenMap(int index)
    {
        Screen("Mapa " + worlds[index], "Selecciona una casilla para escanear el sector.");
        var rows = maps[index].Split('|');
        var card = Card(355);
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 10, RowCount = 5, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
        for (int c = 0; c < 10; c++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        for (int r = 0; r < 5; r++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        int known = 0;
        for (int r = 0; r < 5; r++) for (int c = 0; c < 10; c++)
        {
            char symbol = c < rows[r].Length ? rows[r][c] : '?';
            if (symbol != '?') known++;
            int row = r, col = c;
            var tile = new Button {
                Text = symbol == '.' ? "·" : symbol == '~' ? "≈" : symbol.ToString(),
                Dock = DockStyle.Fill, Margin = new Padding(2), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Symbol", 16, FontStyle.Bold),
                BackColor = symbol == '~' ? Color.FromArgb(20, 60, 100) : symbol == '?' ? Color.FromArgb(16, 26, 43) : Color.FromArgb(26, 49, 68),
                ForeColor = symbol == '⚠' ? Color.OrangeRed : symbol == '◆' ? cyan : Color.White
            };
            tile.FlatAppearance.BorderSize = 0;
            tile.Click += (_, _) => {
                string key = index + ":" + row + ":" + col;
                if (save.ScannedCells.Add(key)) { scannedSectors++; Award(5, "Nuevo sector escaneado: " + key); }
                MessageBox.Show("Sector [" + (row + 1) + "," + (col + 1) + "]\n" +
                (symbol == '?' ? "Zona no cartografiada" : symbol == '⚠' ? "Riesgo detectado" : symbol == '◆' ? "Cristal energético" :
                symbol == 'P' ? "Posición del explorador" : symbol == '⌂' ? "Base de operaciones" : symbol == '●' ? "Punto de interés" :
                symbol == '~' ? "Zona acuática" : "Terreno estable"), "Escáner IRIS"); };
            grid.Controls.Add(tile, c, r);
        }
        card.Controls.Add(grid); Add(card);
        Info("COBERTURA CARTOGRÁFICA", (known * 2) + "% del territorio registrado. Selecciona un sector para obtener información.", 105);
        Add(Button("VOLVER A CARTOGRAFÍA", Cartography));
    }

    void Recharge()
    {
        if (!RequirePlayer()) return;
        player!.RecargarEnergia(30);
        save.RechargeCount++;
        Log("Recarga de energía completada: " + player.Energia + "%");
        MessageBox.Show("Recarga completada. Energía actual: " + player.Energia + "%", "NEXUS");
        Home();
    }

    void Missions()
    {
        Screen("Misiones y logros", "Objetivos, recompensas y progreso del explorador.");
        Hero("CENTRO DE MISIONES", "COMPLETA OBJETIVOS   /   OBTÉN EXPERIENCIA Y SUBE DE NIVEL");
        Info("RANGO DEL EXPLORADOR", "Nivel " + NexusProgress.Level(save.Experience) + "   •   " +
            save.Experience + " XP acumulados\nLogros desbloqueados: " + save.Achievements.Count + " / 4", 120);
        Meter("PROGRESO AL SIGUIENTE NIVEL", NexusProgress.LevelProgress(save.Experience));
        var missions = NexusProgress.Missions(save);
        for (int i = missions.Length - 1; i >= 0; i--)
        {
            var m = missions[i];
            bool completed = save.Achievements.Contains(m.id);
            Info((completed ? "★ COMPLETADA  /  " : "◇ EN CURSO  /  ") + m.title,
                m.detail + "\nProgreso: " + Math.Min(m.current, m.goal) + "/" + m.goal +
                "     Recompensa: " + m.reward + " XP", 112);
        }
    }

    void ToggleAnimations()
    {
        animationsEnabled = !animationsEnabled;
        Log("Animaciones " + (animationsEnabled ? "activadas" : "pausadas"));
        if (animatedHero != null && !animatedHero.IsDisposed) animatedHero.Invalidate();
        MessageBox.Show(animationsEnabled ? "Animaciones activadas." : "Animaciones pausadas.", "Preferencias NEXUS");
    }

    void History()
    {
        Screen("Bitácora de misiones", "Registro local de las acciones de esta sesión.");
        Info("RESUMEN DE OPERACIONES", "Inmersiones: " + exploredMissions + "     Escaneos: " + scannedSectors +
            "\nInterferencia: " + anomaly.NivelInterferencia + "%", 118);
        var card = Card(360);
        var log = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.None,
            BackColor = surface, ForeColor = Color.White, Font = new Font("Consolas", 11),
            Text = history.Count == 0 ? "No hay acciones registradas todavía." : string.Join(Environment.NewLine + Environment.NewLine, history) };
        card.Controls.Add(log); Add(card);
        Add(Button("COPIAR BITÁCORA", () => {
            if (history.Count == 0) { MessageBox.Show("La bitácora está vacía."); return; }
            Clipboard.SetText(string.Join(Environment.NewLine, history));
            MessageBox.Show("Bitácora copiada al portapapeles.");
        }, true));
    }

    void About()
    {
        Screen("Acerca de NEXUS", "Edición 3.0 de la interfaz dimensional.");
        Hero("NEXUS 3.0", "CENTRO DE CONTROL IRIS   /   INTERFAZ DE EXPLORACIÓN");
        Info("ARQUITECTURA", "Windows Forms en C# y .NET 10. Reutiliza las entidades originales Explorador y Anomalia.\nGuardado automático en AppData local. La aplicación de consola original permanece sin modificaciones. Atajos: Ctrl+1 a Ctrl+5 para navegar, Ctrl+L para la bitácora.", 145);
    }
}

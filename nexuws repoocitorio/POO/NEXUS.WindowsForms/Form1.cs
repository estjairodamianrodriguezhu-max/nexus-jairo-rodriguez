using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ProyectoNEXUS;

namespace NEXUS.WindowsForms
{
    public partial class FrmMenuPrincipal : Form
    {
        private readonly Color bg = Color.FromArgb(7, 13, 28);
        private readonly Color panel = Color.FromArgb(16, 28, 49);
        private readonly Color cyan = Color.FromArgb(60, 225, 245);
        private readonly Color muted = Color.FromArgb(150, 174, 198);
        private readonly System.Windows.Forms.Timer introTimer = new() { Interval = 45 };
        private readonly System.Windows.Forms.Timer pulseTimer = new() { Interval = 80 };
        private readonly Random random = new();
        private Panel content = null!;
        private Label status = null!;
        private Label clock = null!;
        private Label energyLabel = null!;
        private Label stabilityLabel = null!;
        private Label anomalyLabel = null!;
        private ProgressBar energyBar = null!;
        private ProgressBar stabilityBar = null!;
        private TextBox nameInput = null!;
        private NumericUpDown ageInput = null!;
        private Explorador? cadete;
        private Anomalia anomaly = new("Interferencia Cuántica IRIS", 40);
        private int loadProgress;
        private int tick;
        private string currentScreen = "Inicio";
        private readonly string[] dimensions = { "IRIS-07  |  Valle de Cristal", "IRIS-12  |  Sector Abisal" };
        private readonly string[][] maps =
        {
            new[] { "..◆...????", ".⌂..⚠..◆??", "..P..●...?", "~~....⚠...", "~~~..◆..⌂." },
            new[] { "~~~~..◆.??", "~⌂.~~⚠.??", "..P.~~●...", ".⚠...~~~◆.", "..◆...~~⌂." }
        };

        public FrmMenuPrincipal()
        {
            InitializeComponent();
            Controls.Clear();
            Text = "NEXUS // Centro de Control Dimensional";
            MinimumSize = new Size(900, 600);
            Size = new Size(1180, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bg;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            introTimer.Tick += (_, _) => {
                loadProgress = Math.Min(100, loadProgress + random.Next(2, 7));
                if (loadProgress == 100) { introTimer.Stop(); BuildShell(); ShowHome(); }
                else ShowSplash();
            };
            pulseTimer.Tick += (_, _) => {
                tick++;
                if (clock != null && !clock.IsDisposed) clock.Text = DateTime.Now.ToString("dd MMM yyyy  •  HH:mm:ss").ToUpperInvariant();
            };
            Shown += (_, _) => { ShowSplash(); introTimer.Start(); pulseTimer.Start(); };
        }

        private Label TextLabel(string text, int size, Color color, bool bold = false)
            => new() { Text = text, ForeColor = color, AutoSize = true, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent };

        private Button ActionButton(string text, Action action, bool primary = false)
        {
            var button = new Button {
                Text = text, Height = 48, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = primary ? bg : Color.White, BackColor = primary ? cyan : Color.FromArgb(27, 46, 70),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 0, 12)
            };
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(51, 95, 126);
            button.MouseEnter += (_, _) => button.BackColor = primary ? Color.FromArgb(137, 245, 255) : Color.FromArgb(38, 66, 95);
            button.MouseLeave += (_, _) => button.BackColor = primary ? cyan : Color.FromArgb(27, 46, 70);
            button.Click += (_, _) => action();
            return button;
        }

        private Panel Card(int height = 0)
        {
            var p = new Panel { BackColor = panel, Dock = DockStyle.Top, Padding = new Padding(22), Margin = new Padding(0, 0, 0, 16) };
            if (height > 0) p.Height = height;
            p.Paint += (_, e) => {
                using var pen = new Pen(Color.FromArgb(39, 83, 112));
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        private void ShowSplash()
        {
            Controls.Clear();
            var splash = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, BackColor = bg, Padding = new Padding(60) };
            splash.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            splash.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            splash.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            splash.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            splash.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            var title = TextLabel("N E X U S", 46, cyan, true); title.Dock = DockStyle.Fill; title.TextAlign = ContentAlignment.MiddleCenter;
            var sub = TextLabel("INICIALIZANDO PROTOCOLO DE EXPLORACIÓN DIMENSIONAL", 12, muted); sub.Dock = DockStyle.Fill; sub.TextAlign = ContentAlignment.MiddleCenter;
            var progress = new ProgressBar { Dock = DockStyle.Fill, Maximum = 100, Value = loadProgress, Style = ProgressBarStyle.Continuous, Margin = new Padding(140, 8, 140, 8) };
            splash.Controls.Add(new Panel(), 0, 0); splash.Controls.Add(title, 0, 1);
            splash.Controls.Add(sub, 0, 2); splash.Controls.Add(progress, 0, 3);
            splash.Controls.Add(new Panel(), 0, 4);
            Controls.Add(splash);
        }

        private void BuildShell()
        {
            Controls.Clear();
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 245, BackColor = Color.FromArgb(11, 22, 40), Padding = new Padding(16, 28, 16, 12) };
            var header = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.FromArgb(12, 24, 43), Padding = new Padding(28, 20, 24, 12) };
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 35, BackColor = Color.FromArgb(12, 24, 43), Padding = new Padding(20, 7, 20, 0) };
            content = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(32, 25, 32, 30), BackColor = bg };
            Controls.Add(content); Controls.Add(footer); Controls.Add(header); Controls.Add(sidebar);
            var brand = TextLabel("N E X U S", 24, cyan, true); brand.Dock = DockStyle.Top; brand.Height = 65; brand.AutoSize = false;
            var subtitle = TextLabel("CONTROL DIMENSIONAL  /  V2", 9, muted); subtitle.Dock = DockStyle.Top; subtitle.Height = 50; subtitle.AutoSize = false;
            sidebar.Controls.Add(BuildNav());
            sidebar.Controls.Add(subtitle); sidebar.Controls.Add(brand);
            var head = TextLabel("CENTRO DE OPERACIONES   /   IRIS", 13, Color.White, true);
            head.Dock = DockStyle.Left; head.AutoSize = false; head.Width = 480;
            clock = TextLabel("", 10, muted); clock.Dock = DockStyle.Right; clock.AutoSize = false; clock.Width = 245; clock.TextAlign = ContentAlignment.MiddleRight;
            header.Controls.Add(clock); header.Controls.Add(head);
            status = TextLabel("● SISTEMA EN LÍNEA     •     SIMULACIÓN LOCAL     •     DATOS EN MEMORIA", 9, cyan);
            footer.Controls.Add(status);
        }

        private Panel BuildNav()
        {
            var nav = new Panel { Dock = DockStyle.Fill };
            var buttons = new (string, Action)[] {
                ("⌂   PANEL PRINCIPAL", ShowHome),
                ("◈   PERFIL DEL CADETE", ShowProfile),
                ("◇   EXPLORAR REALIDADES", ShowExplore),
                ("⚠   ANOMALÍAS IRIS", ShowAnomaly),
                ("▦   CARTOGRAFÍA", ShowMaps),
                ("⚡   RECARGAR ENERGÍA", Recharge),
                ("ⓘ   ACERCA DE NEXUS", ShowAbout)
            };
            int y = 8;
            foreach (var (name, action) in buttons) {
                var b = ActionButton(name, action); b.Dock = DockStyle.None; b.SetBounds(0, y, 210, 48);
                nav.Controls.Add(b); y += 59;
            }
            return nav;
        }

        private void Screen(string title, string subtitle)
        {
            currentScreen = title;
            content.Controls.Clear();
            var subtitleLabel = TextLabel(subtitle, 11, muted);
            subtitleLabel.Dock = DockStyle.Top; subtitleLabel.Height = 42; subtitleLabel.AutoSize = false;
            var titleLabel = TextLabel(title.ToUpperInvariant(), 26, Color.White, true);
            titleLabel.Dock = DockStyle.Top; titleLabel.Height = 58; titleLabel.AutoSize = false;
            content.Controls.Add(subtitleLabel);
            content.Controls.Add(titleLabel);
        }

        private void AddTop(Control control)
        {
            control.Dock = DockStyle.Top;
            content.Controls.Add(control);
            content.Controls.SetChildIndex(control, 0);
        }

        private void AddSection(string heading, string body, int height = 125)
        {
            var c = Card(height);
            var h = TextLabel(heading, 14, cyan, true); h.Dock = DockStyle.Top; h.Height = 37; h.AutoSize = false;
            var b = TextLabel(body, 11, Color.FromArgb(216, 230, 243)); b.Dock = DockStyle.Fill; b.AutoSize = false;
            c.Controls.Add(b); c.Controls.Add(h); AddTop(c);
        }

        private void ShowHome()
        {
            Screen("Panel principal", "Tu centro de mando para explorar el universo NEXUS.");
            AddSection("ESTADO DE LA MISIÓN", cadete == null
                ? "Aún no hay un cadete registrado. Crea tu perfil para iniciar la exploración."
                : $"CADETE: {cadete.Nombre}     EDAD: {cadete.Edad}\nENERGÍA: {cadete.Energia}%     ESTABILIDAD: {cadete.Estabilidad}%     INTERFERENCIA: {anomaly.NivelInterferencia}%", 135);
            AddSection("UNIVERSO IRIS", "02 realidades dimensionales disponibles: Valle de Cristal y Sector Abisal.\nInvestiga anomalías, conserva energía y descubre los mapas cartografiados.", 125);
            var start = ActionButton(cadete == null ? "CREAR PERFIL DE EXPLORADOR" : "INICIAR EXPLORACIÓN", cadete == null ? ShowProfile : ShowExplore, true);
            AddTop(start);
        }

        private void ShowProfile()
        {
            Screen("Perfil del cadete", "Configura tu identidad para ingresar al programa IRIS.");
            if (cadete != null) {
                AddSection("EXPLORADOR ACTIVO", $"Nombre: {cadete.Nombre}     |     Edad: {cadete.Edad}\nEnergía: {cadete.Energia}%     |     Estabilidad: {cadete.Estabilidad}%", 130);
                AddTop(ActionButton("CONTINUAR EXPLORACIÓN", ShowExplore, true));
                return;
            }
            var form = Card(260);
            var label = TextLabel("NOMBRE DEL EXPLORADOR", 11, cyan, true); label.SetBounds(22, 18, 350, 30);
            nameInput = new TextBox { Font = new Font("Segoe UI", 13), BackColor = Color.FromArgb(28, 44, 65), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            nameInput.SetBounds(22, 55, 390, 36); nameInput.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            var ageLabel = TextLabel("EDAD DEL EXPLORADOR", 11, cyan, true); ageLabel.SetBounds(22, 108, 350, 28);
            ageInput = new NumericUpDown { Minimum = 18, Maximum = 80, Value = 20, Font = new Font("Segoe UI", 13), BackColor = Color.FromArgb(28, 44, 65), ForeColor = Color.White };
            ageInput.SetBounds(22, 145, 140, 34);
            var register = ActionButton("ACTIVAR PERFIL", Register, true); register.Dock = DockStyle.None; register.SetBounds(22, 195, 250, 45);
            form.Controls.AddRange(new Control[] { label, nameInput, ageLabel, ageInput, register }); AddTop(form);
        }

        private void Register()
        {
            string name = nameInput.Text.Trim();
            if (name.Length < 2) { MessageBox.Show("Escribe un nombre de al menos dos caracteres.", "Validación NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            cadete = new Explorador(name, (int)ageInput.Value, 80, 90);
            status.Text = "● PERFIL ACTIVADO   •   " + cadete.Nombre.ToUpperInvariant();
            ShowHome();
        }

        private bool NeedCadet()
        {
            if (cadete != null) return false;
            MessageBox.Show("Primero registra tu perfil de explorador.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ShowProfile(); return true;
        }

        private void ShowExplore()
        {
            if (NeedCadet()) return;
            Screen("Exploración dimensional", "Selecciona una realidad y realiza una inmersión.");
            AddSection("RECURSOS DEL CADETE", $"Energía disponible: {cadete!.Energia}%     •     Estabilidad: {cadete.Estabilidad}%\nCada inmersión consume 15 puntos de energía y 5 de estabilidad.", 118);
            for (int i = dimensions.Length - 1; i >= 0; i--) {
                int index = i;
                var card = Card(125);
                var heading = TextLabel(dimensions[i], 15, cyan, true); heading.Dock = DockStyle.Top; heading.Height = 45; heading.AutoSize = false;
                var btn = ActionButton("INICIAR INMERSIÓN", () => Explore(index), true);
                card.Controls.Add(btn); card.Controls.Add(heading); AddTop(card);
            }
        }

        private void Explore(int index)
        {
            if (NeedCadet()) return;
            if (cadete!.Energia < 20) { MessageBox.Show("Necesitas al menos 20% de energía para explorar. Usa la opción de recarga.", "Energía insuficiente"); return; }
            cadete.ModificarEnergia(-15); cadete.ModificarEstabilidad(-5);
            MessageBox.Show($"Inmersión completada en {dimensions[index]}.\nEnergía restante: {cadete.Energia}%\nEstabilidad: {cadete.Estabilidad}%", "Exploración exitosa");
            ShowExplore();
        }

        private void ShowAnomaly()
        {
            if (NeedCadet()) return;
            Screen("Anomalías IRIS", "Monitorea y reduce la interferencia cuántica.");
            AddSection("INTERFERENCIA CUÁNTICA IRIS", $"Nivel de interferencia: {anomaly.NivelInterferencia}%\nEnergía del cadete: {cadete!.Energia}%\nCada intervención consume 10 de energía y reduce la interferencia en 15 puntos.", 155);
            AddTop(ActionButton("ANALIZAR E INTERVENIR", Intervene, true));
        }

        private void Intervene()
        {
            if (NeedCadet()) return;
            if (cadete!.Energia < 15) { MessageBox.Show("Energía insuficiente: se requiere al menos 15%.", "NEXUS"); return; }
            if (anomaly.NivelInterferencia == 0) { MessageBox.Show("La anomalía ya está neutralizada.", "NEXUS"); return; }
            cadete.ModificarEnergia(-10);
            anomaly.ReducirInterferencia(15);
            ShowAnomaly();
        }

        private void ShowMaps()
        {
            Screen("Cartografía dimensional", "Mapas tácticos registrados en la red IRIS.");
            for (int i = maps.Length - 1; i >= 0; i--) {
                int idx = i;
                AddTop(ActionButton("ABRIR MAPA  /  " + dimensions[i], () => OpenMap(idx), true));
            }
            AddSection("LEYENDA", "P = Explorador    ◆ = Cristal    ⚠ = Riesgo    ⌂ = Base\n~ = Agua    ? = Zona desconocida    ● = Punto de interés", 120);
        }

        private void OpenMap(int index)
        {
            Screen("Mapa " + dimensions[index].Split('|')[0].Trim(), dimensions[index]);
            var grid = Card(340);
            var display = new Label {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 23, FontStyle.Bold), ForeColor = cyan,
                Text = string.Join(Environment.NewLine, maps[index]).Replace(".", "·").Replace("~", "≈"),
                BackColor = panel
            };
            grid.Controls.Add(display); AddTop(grid);
            int known = 0, total = 0;
            foreach (string row in maps[index]) foreach (char ch in row) { total++; if (ch != '?') known++; }
            AddSection("COBERTURA CARTOGRÁFICA", $"{100.0 * known / total:0.0}% del territorio registrado.\nLos símbolos muestran las zonas conocidas y las áreas pendientes de explorar.", 115);
            AddTop(ActionButton("VOLVER A CARTOGRAFÍA", ShowMaps));
        }

        private void Recharge()
        {
            if (NeedCadet()) return;
            cadete!.RecargarEnergia(30);
            MessageBox.Show($"Recarga completada. Energía actual: {cadete.Energia}%.", "NEXUS");
            ShowHome();
        }

        private void ShowAbout()
        {
            Screen("Acerca de NEXUS", "Plataforma de exploración dimensional.");
            AddSection("NEXUS // EDICIÓN INTERFAZ", "Aplicación Windows Forms con navegación interactiva, exploración dimensional, cartografía, perfil de explorador y gestión de anomalías.", 140);
            AddSection("ARQUITECTURA", "Interfaz en C# / Windows Forms. Las entidades Explorador y Anomalia se reutilizan desde el proyecto original.\nLa simulación de consola permanece intacta.", 140);
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void button3_Click(object sender, EventArgs e) { }
        private void btnNombre_Click(object sender, EventArgs e) { ShowProfile(); }
    }
}

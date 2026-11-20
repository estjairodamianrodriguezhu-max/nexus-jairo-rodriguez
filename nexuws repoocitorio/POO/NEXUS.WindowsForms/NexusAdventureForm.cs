using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NEXUS.WindowsForms;

// A keyboard-controlled, top-down 2D level. World coordinates are tile-based.
public sealed class NexusAdventureForm : Form
{
    private sealed class GameSurface : Control
    {
        public GameSurface()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }
    }

    private readonly string[] rows;
    private readonly int world;
    private readonly Func<int, int, char, bool> interact;
    private readonly Func<string> hud;
    private readonly GameSurface surface = new() { Dock = DockStyle.Fill };
    private readonly Label info = new()
    {
        Dock = DockStyle.Bottom, Height = 65, BackColor = Color.FromArgb(10, 22, 39),
        ForeColor = Color.FromArgb(195, 232, 244), Font = new Font("Segoe UI", 11),
        TextAlign = ContentAlignment.MiddleCenter
    };
    private int px = 2, py = 2;
    private int tick;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 50 };
    private const int Tile = 76;
    private int viewColumns = 13;
    private int viewRows = 8;
    private bool showMiniMap = true;

    public NexusAdventureForm(int world, string map, Func<int, int, char, bool> interact, Func<string> hud)
    {
        this.world = world;
        this.interact = interact;
        this.hud = hud;
        rows = map.Split('|');
        Text = "NEXUS 6.0  //  EXPANSIÓN DIMENSIONAL  //  IRIS-" + (world == 0 ? "07" : "12");
        Size = new Size(1220, 820);
        MinimumSize = new Size(820, 550);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(5, 12, 26);
        KeyPreview = true;
        Controls.Add(surface);
        Controls.Add(info);
        surface.Paint += Draw;
        timer.Tick += (_, _) => { tick++; surface.Invalidate(); };
        Shown += (_, _) => { timer.Start(); UpdateInfo("Explora el mapa. Puedes ajustar el zoom con + y -."); };
        FormClosed += (_, _) => timer.Dispose();
        KeyDown += OnKey;
    }

    private char At(int x, int y)
    {
        if (x < 0 || y < 0 || y >= rows.Length || x >= rows[y].Length) return '#';
        return rows[y][x];
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        int dx = 0, dy = 0;
        switch (e.KeyCode)
        {
            case Keys.W: case Keys.Up: dy = -1; break;
            case Keys.S: case Keys.Down: dy = 1; break;
            case Keys.A: case Keys.Left: dx = -1; break;
            case Keys.D: case Keys.Right: dx = 1; break;
            case Keys.E: case Keys.Space: Interact(); e.SuppressKeyPress = true; return;
            case Keys.M: showMiniMap = !showMiniMap; surface.Invalidate(); UpdateInfo(showMiniMap ? "Minimapa activado." : "Minimapa oculto."); return;
            case Keys.Oemplus: case Keys.Add: ChangeZoom(-1); return;
            case Keys.OemMinus: case Keys.Subtract: ChangeZoom(1); return;
            case Keys.Escape: Close(); return;
            default: return;
        }
        e.SuppressKeyPress = true;
        int nx = px + dx, ny = py + dy;
        char target = At(nx, ny);
        if (target == '#' || target == '~')
        {
            UpdateInfo("Terreno bloqueado: busca otro camino.");
            return;
        }
        px = nx; py = ny;
        UpdateInfo(target == '◆' ? "Cristal encontrado. Pulsa E para recogerlo." :
            target == '⚠' ? "¡Riesgo detectado! Pulsa E para estabilizarlo." :
            "Explorando sector IRIS. E = interactuar, ESC = salir.");
        surface.Invalidate();
    }

    private void ChangeZoom(int direction)
    {
        viewColumns = Math.Clamp(viewColumns + direction * 2, 9, 19);
        viewRows = Math.Clamp(viewRows + direction, 6, 12);
        UpdateInfo("Zoom actualizado.");
        surface.Invalidate();
    }

    private void Interact()
    {
        char current = At(px, py);
        if (current != '◆' && current != '⚠')
        {
            UpdateInfo("Aquí no hay recursos ni riesgos. Acércate a un símbolo brillante.");
            return;
        }
        if (interact(px, py, current))
        {
            var chars = rows[py].ToCharArray();
            chars[px] = '.';
            rows[py] = new string(chars);
            UpdateInfo(current == '◆' ? "Cristales recolectados. ¡Buen trabajo!" :
                "Riesgo estabilizado. Zona segura.");
        }
        surface.Invalidate();
    }

    private void UpdateInfo(string message) => info.Text =
        message + Environment.NewLine + hud() + "    |    Posición: " + (px + 1) + "," + (py + 1) +
        "    |    WASD: mover   E: interactuar   +/-: zoom   M: mapa   ESC: salir";


    private void DrawMiniMap(Graphics g)
    {
        const int cell = 4;
        int left = Math.Max(8, surface.ClientSize.Width - NexusWorld.Width * cell - 22);
        int top = 12;
        using var backdrop = new SolidBrush(Color.FromArgb(220, 5, 18, 34));
        using var land = new SolidBrush(Color.FromArgb(43, 105, 105));
        using var water = new SolidBrush(Color.FromArgb(24, 70, 145));
        using var hazard = new SolidBrush(Color.FromArgb(238, 102, 82));
        using var gem = new SolidBrush(Color.FromArgb(80, 236, 251));
        g.FillRectangle(backdrop, left - 8, top - 8, NexusWorld.Width * cell + 16, NexusWorld.Height * cell + 30);
        for (int y = 0; y < NexusWorld.Height; y++)
        for (int x = 0; x < NexusWorld.Width; x++)
        {
            char c = At(x, y);
            g.FillRectangle(c == '~' ? water : c == '⚠' ? hazard : c == '◆' ? gem : land,
                left + x * cell, top + y * cell, cell, cell);
        }
        g.FillEllipse(Brushes.White, left + px * cell - 3, top + py * cell - 3, 10, 10);
        using var font = new Font("Segoe UI", 9, FontStyle.Bold);
        g.DrawString("IRIS  /  MAPA 40 × 30", font, Brushes.White, left, top + NexusWorld.Height * cell + 3);
    }

    private void Draw(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(5, 12, 26));
        int cols = viewColumns, count = viewRows;
        int startX = Math.Clamp(px - cols / 2, 0, Math.Max(0, NexusWorld.Width - cols));
        int startY = Math.Clamp(py - count / 2, 0, Math.Max(0, NexusWorld.Height - count));
        float scale = Math.Min((surface.ClientSize.Width - 24f) / (cols * Tile),
            (surface.ClientSize.Height - 24f) / (count * Tile));
        if (scale <= 0) return;
        float ox = (surface.ClientSize.Width - cols * Tile * scale) / 2f;
        float oy = (surface.ClientSize.Height - count * Tile * scale) / 2f;
        g.TranslateTransform(ox, oy);
        g.ScaleTransform(scale, scale);

        using var ground = new SolidBrush(world == 0 ? Color.FromArgb(24, 55, 60) : Color.FromArgb(28, 36, 67));
        using var alternate = new SolidBrush(world == 0 ? Color.FromArgb(31, 66, 65) : Color.FromArgb(36, 43, 77));
        using var water = new SolidBrush(Color.FromArgb(17, 70, 116));
        using var unknown = new SolidBrush(Color.FromArgb(12, 22, 36));
        using var border = new Pen(Color.FromArgb(55, 106, 130), 1);
        using var waterLine = new Pen(Color.FromArgb(63, 162, 214), 2);
        using var rock = new SolidBrush(Color.FromArgb(66, 87, 102));
        using var crystal = new SolidBrush(Color.FromArgb(60, 239, 255));
        using var danger = new SolidBrush(Color.FromArgb(247, 92, 70));
        using var baseBrush = new SolidBrush(Color.FromArgb(120, 196, 145));
        using var text = new Font("Segoe UI", 21, FontStyle.Bold);
        using var small = new Font("Segoe UI", 10, FontStyle.Bold);

        for (int y = 0; y < count; y++)
        for (int x = 0; x < cols; x++)
        {
            int wx = startX + x, wy = startY + y;
            char c = At(wx, wy);
            int left = x * Tile, top = y * Tile;
            var rect = new Rectangle(left, top, Tile, Tile);
            g.FillRectangle(c == '~' ? water : c == '?' ? unknown : (x + y) % 2 == 0 ? ground : alternate, rect);
            g.DrawRectangle(border, rect);
            if (c == '#') { using var obstacle = new SolidBrush(Color.FromArgb(78, 93, 108)); g.FillPolygon(obstacle, new Point[] { new(left+7, top+60), new(left+22, top+12), new(left+55, top+8), new(left+69, top+58) }); }
            // Fixed geometric details create a textured terrain without external image assets.
            int seed = (wx * 37 + wy * 53 + world * 19) % 37;
            if (c != '~' && c != '?')
            {
                g.FillEllipse(rock, left + 8 + seed % 15, top + 10 + seed % 19, 7, 5);
                g.FillEllipse(rock, left + 40 + seed % 9, top + 49, 11, 6);
                if (seed % 5 == 0) { using var foliage = new SolidBrush(world == 0 ? Color.FromArgb(38, 142, 114) : Color.FromArgb(70, 88, 157)); g.FillEllipse(foliage, left + 14, top + 33, 21, 17); }
            }
            if (c == '~')
            {
                for (int i = 0; i < 3; i++)
                {
                    int yy = top + 17 + i * 19 + (int)(2 * Math.Sin(tick * .07 + x + i));
                    g.DrawArc(waterLine, left + 8, yy, 53, 12, 0, 180);
                }
            }
            if (c == '◆')
            {
                Point[] gem = { new(left + 38, top + 11), new(left + 58, top + 36),
                    new(left + 38, top + 62), new(left + 18, top + 36) };
                g.FillPolygon(crystal, gem);
                g.DrawPolygon(Pens.White, gem);
            }
            if (c == '⚠')
            {
                Point[] triangle = { new(left + 38, top + 10), new(left + 65, top + 59), new(left + 11, top + 59) };
                g.FillPolygon(danger, triangle);
                g.DrawString("!", text, Brushes.White, left + 30, top + 22);
            }
            if (c == '⌂')
            {
                g.FillRectangle(baseBrush, left + 18, top + 30, 40, 29);
                g.DrawString("⌂", text, Brushes.White, left + 23, top + 10);
            }
            if (c == '●') g.FillEllipse(Brushes.Gold, left + 25, top + 25, 26, 26);
            if (c == '?') g.DrawString("?", text, Brushes.DimGray, left + 27, top + 18);
        }

        // Draw the explorer on top of terrain, with a simple animated glow.
        int cx = (px - startX) * Tile + Tile / 2, cy = (py - startY) * Tile + Tile / 2;
        int pulse = 3 + (int)(3 * Math.Sin(tick * .13));
        using var aura = new Pen(Color.FromArgb(130, 59, 226, 248), 3);
        g.DrawEllipse(aura, cx - 27 - pulse, cy - 27 - pulse, 54 + pulse * 2, 54 + pulse * 2);
        using var suit = new SolidBrush(Color.FromArgb(38, 177, 231));
        g.FillEllipse(Brushes.WhiteSmoke, cx - 12, cy - 22, 24, 24);
        g.FillEllipse(suit, cx - 17, cy - 1, 34, 28);
        g.FillRectangle(Brushes.DarkSlateBlue, cx - 9, cy - 16, 18, 7);
        g.DrawString("EXPLORADOR", small, Brushes.White, cx - 39, cy + 28);
        g.ResetTransform();
        if (showMiniMap) DrawMiniMap(g);
    }
}

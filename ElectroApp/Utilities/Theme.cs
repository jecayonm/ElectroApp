using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ElectroApp.Utilities
{
    public static class Theme
    {
        // Paleta de colores (tono profesional)
        public static readonly Color Primary = Color.FromArgb(33, 150, 243);       // Azul 500
        public static readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);   // Azul 700
        public static readonly Color Accent = Color.FromArgb(255, 193, 7);         // Amber 600
        public static readonly Color Bg = Color.FromArgb(245, 247, 250);           // Gris muy claro
        public static readonly Color AltBg = Color.FromArgb(236, 239, 241);        // Gris claro
        public static readonly Color Fore = Color.FromArgb(33, 33, 33);            // Texto principal
        public static readonly Color Muted = Color.FromArgb(97, 97, 97);           // Texto secundario
        public static readonly Color Border = Color.FromArgb(189, 189, 189);
        public static readonly Font UiFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        private static ToolStripProfessionalRenderer _renderer;

        public static void ApplyGlobals()
        {
            // Renderer para ToolStrip/MenuStrip/StatusStrip
            if (_renderer == null)
            {
                _renderer = new ToolStripProfessionalRenderer(new ThemeColorTable())
                {
                    RoundedEdges = false
                };
            }
            ToolStripManager.Renderer = _renderer;
            ToolStripManager.VisualStylesEnabled = true;
        }

        public static void Apply(Form form)
        {
            if (form == null || form.IsDisposed) return;

            form.Font = UiFont;
            form.BackColor = Bg;
            form.ForeColor = Fore;

            ApplyToControlTree(form);
        }

        private static void ApplyToControlTree(Control root)
        {
            if (root == null) return;

            // Controles específicos
            if (root is DataGridView dgv)
                StyleDataGridView(dgv);
            else if (root is ToolStrip ts)
                StyleToolStrip(ts);
            else if (root is StatusStrip ss)
                StyleStatusStrip(ss);
            else if (root is MenuStrip ms)
                StyleMenuStrip(ms);
            else if (root is Button btn)
                StyleButton(btn);
            else if (root is Label lb)
                lb.ForeColor = Fore;
            else if (root is TextBoxBase tb)
                StyleTextBox(tb);
            else if (root is ComboBox cb)
                StyleCombo(cb);

            foreach (Control c in root.Controls)
            {
                // Evitar colorear internamente elementos de ToolStrip (control hostea items propios)
                if (!(root is ToolStrip))
                {
                    c.BackColor = (c is Panel || c is GroupBox) ? AltBg : c.BackColor;
                    c.ForeColor = Fore;
                }
                ApplyToControlTree(c);
            }
        }

        public static void StyleDataGridView(DataGridView grid)
        {
            if (grid == null) return;
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Bg;
            grid.GridColor = Border;
            grid.BorderStyle = BorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryDark;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryDark;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Fore;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(227, 242, 253); // azul muy claro
            grid.DefaultCellStyle.SelectionForeColor = Fore;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // Doble buffer para mejorar scroll (vía reflección para .NET Framework)
            try
            {
                var pi = typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                pi?.SetValue(grid, true, null);
            }
            catch { }
        }

        public static void StyleToolStrip(ToolStrip ts)
        {
            ts.Renderer = _renderer ?? (_renderer = new ToolStripProfessionalRenderer(new ThemeColorTable()));
            ts.GripStyle = ToolStripGripStyle.Hidden;
            ts.BackColor = Primary;
            ts.ForeColor = Color.White;
            ts.Padding = new Padding(4, 4, 4, 4);
        }

        public static void StyleMenuStrip(MenuStrip ms)
        {
            ms.Renderer = _renderer ?? (_renderer = new ToolStripProfessionalRenderer(new ThemeColorTable()));
            ms.BackColor = Primary;
            ms.ForeColor = Color.White;
            ms.Padding = new Padding(6, 4, 6, 4);
        }

        public static void StyleStatusStrip(StatusStrip ss)
        {
            ss.Renderer = _renderer ?? (_renderer = new ToolStripProfessionalRenderer(new ThemeColorTable()));
            ss.BackColor = PrimaryDark;
            ss.ForeColor = Color.White;
        }

        public static void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = PrimaryDark;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(227, 242, 253);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(187, 222, 251);
            btn.BackColor = Color.White;
            btn.ForeColor = Fore;
        }

        public static void StyleTextBox(TextBoxBase tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Color.White;
            tb.ForeColor = Fore;
        }

        public static void StyleCombo(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Standard;
            cb.BackColor = Color.White;
            cb.ForeColor = Fore;
        }

        private class ThemeColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => Primary;
            public override Color ToolStripGradientMiddle => Primary;
            public override Color ToolStripGradientEnd => Primary;
            public override Color MenuStripGradientBegin => Primary;
            public override Color MenuStripGradientEnd => Primary;
            public override Color StatusStripGradientBegin => PrimaryDark;
            public override Color StatusStripGradientEnd => PrimaryDark;
            public override Color ImageMarginGradientBegin => Primary;
            public override Color ImageMarginGradientMiddle => Primary;
            public override Color ImageMarginGradientEnd => Primary;
            public override Color ToolStripBorder => PrimaryDark;
            public override Color ButtonSelectedHighlight => Color.FromArgb(227, 242, 253);
            public override Color ButtonSelectedBorder => PrimaryDark;
            public override Color ButtonPressedHighlight => Color.FromArgb(187, 222, 251);
        }
    }
}

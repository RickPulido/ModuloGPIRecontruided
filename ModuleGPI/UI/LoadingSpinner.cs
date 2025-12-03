using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Control de loading spinner animado
    /// </summary>
    public class LoadingSpinner : Control
    {
        private Timer _timer;
        private int _angle = 0;
        private Color _color = Color.FromArgb(0, 120, 215);
        private string _text = "Cargando...";

        public string LoadingText
        {
            get => _text;
            set { _text = value; Invalidate(); }
        }

        public Color SpinnerColor
        {
            get => _color;
            set { _color = value; Invalidate(); }
        }

        public LoadingSpinner()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            this.Size = new Size(150, 150);
            this.BackColor = Color.White;

            _timer = new Timer { Interval = 50 };
            _timer.Tick += (s, e) => { _angle = (_angle + 15) % 360; Invalidate(); };
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int centerX = this.Width / 2;
            int centerY = this.Height / 2 - 20;
            int radius = 30;

            for (int i = 0; i < 12; i++)
            {
                int alpha = (int)(255 * (1 - (i / 12.0)));
                using (var pen = new Pen(Color.FromArgb(alpha, _color), 4))
                {
                    double angle = (_angle + i * 30) * Math.PI / 180;
                    int x1 = centerX + (int)(radius * Math.Cos(angle));
                    int y1 = centerY + (int)(radius * Math.Sin(angle));
                    int x2 = centerX + (int)((radius - 10) * Math.Cos(angle));
                    int y2 = centerY + (int)((radius - 10) * Math.Sin(angle));
                    e.Graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            using (var font = new Font("Segoe UI", 10F))
            using (var brush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                var textSize = e.Graphics.MeasureString(_text, font);
                e.Graphics.DrawString(_text, font, brush,
                    centerX - textSize.Width / 2,
                    centerY + radius + 15);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
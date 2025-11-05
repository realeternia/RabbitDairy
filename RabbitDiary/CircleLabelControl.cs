using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeControl
{
    public class CircleLabelControl : Control
    {
        // 文本颜色（使用 Brush 方便直接赋值 Brushes.White 等）
        public Brush TextColor { get; set; } = Brushes.Black;

        public CircleLabelControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.Size = new Size(35, 35);
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Arial", 10.5f, FontStyle.Regular);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 绘制圆形背景
            using (Brush b = new SolidBrush(this.BackColor))
            {
                g.FillEllipse(b, 0, 0, this.Width - 1, this.Height - 1);
            }

            // 绘制边框
            using (Pen p = new Pen(Color.LightGray))
            {
                g.DrawEllipse(p, 0, 0, this.Width - 1, this.Height - 1);
            }

            // 绘制文本，居中
            string text = this.Text ?? "";
            SizeF textSize = g.MeasureString(text, this.Font);
            float tx = (this.Width - textSize.Width) / 2f;
            float ty = (this.Height - textSize.Height) / 2f;
            g.DrawString(text, this.Font, TextColor, tx, ty);
        }

        // 简单地让控件可被 TabFocus（如果需要）
        protected override bool IsInputKey(Keys keyData) => true;

        // 鼠标移入时显示手形光标
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Cursor = Cursors.Hand;
        }

        // 鼠标移出时恢复默认光标
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Cursor = Cursors.Default;
        }
    }
}

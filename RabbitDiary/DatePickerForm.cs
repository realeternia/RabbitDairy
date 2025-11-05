using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TimeControl
{
    public class DatePickerForm : Form
    {
        // 控件
        private Panel pnlDate;
        private Label lbTimeShow;
        private Label lbLeft;
        private Label lbRight;
        // 新增：上一年/下一年按钮（用户要求的 labelRight2 / labelLeft2）
        private Label lbRight2; // 表示“前一年”（点击后年 -1）
        private Label lbLeft2;  // 表示“后一年”（点击后年 +1）

        // 逻辑数据
        private CircleLabelControl selectCircLabel = null;
        private DateTime selectDate = DateTime.Now;
        // 默认显示月视图
        private bool isWeekMode = false;
        private Dictionary<Point, DateTime> timeDic = new Dictionary<Point, DateTime>();

        // 文本指示代替图片（可替换为 ImageLocation）
        private string showText = "展开";
        private Label labelLeft2;
        private Label labelRight2;
        private string foldText = "收起";

        // 新增：可用初始日期构造
        public DatePickerForm()
        {
            // Designer 会使用无参构造器加载表单；只初始化组件，避免在设计时执行运行时逻辑（如数据构建、事件触发等）。
            InitializeComponent();
            // 不在此处调用 initControl()，以防设计器执行运行时代码导致 CodeDom 解析错误。
            // 仅在运行时执行完整的初始化逻辑，设计时跳过可能引发 Designer 错误的代码。
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                initControl();
            }
            else
            {
                // 在设计器中仍可设置一些最小的可视化信息，避免空引用。
                if (lbTimeShow != null)
                    lbTimeShow.Text = selectDate.ToString("yyyy | MM.dd");
            }
        }

        public DatePickerForm(DateTime initialDate)
        {
            selectDate = initialDate;
            InitializeComponent();

            // 仅在运行时执行完整的初始化逻辑，设计时跳过可能引发 Designer 错误的代码。
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                initControl();
            }
            else
            {
                // 在设计器中仍可设置一些最小的可视化信息，避免空引用。
                if (lbTimeShow != null)
                    lbTimeShow.Text = selectDate.ToString("yyyy | MM.dd");
            }
        }

        private void InitializeComponent()
        {
            this.lbTimeShow = new System.Windows.Forms.Label();
            this.lbLeft = new System.Windows.Forms.Label();
            this.lbRight = new System.Windows.Forms.Label();
            this.lbRight2 = new System.Windows.Forms.Label();
            this.lbLeft2 = new System.Windows.Forms.Label();
            this.pnlDate = new System.Windows.Forms.Panel();
            this.labelLeft2 = new System.Windows.Forms.Label();
            this.labelRight2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbTimeShow
            // 
            this.lbTimeShow.AutoSize = true;
            this.lbTimeShow.Font = new System.Drawing.Font("Arial", 12F);
            this.lbTimeShow.Location = new System.Drawing.Point(12, 8);
            this.lbTimeShow.Name = "lbTimeShow";
            this.lbTimeShow.Size = new System.Drawing.Size(0, 18);
            this.lbTimeShow.TabIndex = 0;
            // 
            // lbLeft
            // 
            this.lbLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbLeft.Location = new System.Drawing.Point(12, 34);
            this.lbLeft.Name = "lbLeft";
            this.lbLeft.Size = new System.Drawing.Size(16, 20);
            this.lbLeft.TabIndex = 2;
            this.lbLeft.Text = "<";
            // 
            // lbRight
            // 
            this.lbRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbRight.Location = new System.Drawing.Point(40, 34);
            this.lbRight.Name = "lbRight";
            this.lbRight.Size = new System.Drawing.Size(16, 20);
            this.lbRight.TabIndex = 3;
            this.lbRight.Text = ">";
            // 
            // lbRight2
            // 
            this.lbRight2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbRight2.Location = new System.Drawing.Point(68, 34);
            this.lbRight2.Name = "lbRight2";
            this.lbRight2.Size = new System.Drawing.Size(24, 20);
            this.lbRight2.TabIndex = 5;
            this.lbRight2.Text = "<<";
            // 
            // lbLeft2
            // 
            this.lbLeft2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbLeft2.Location = new System.Drawing.Point(100, 34);
            this.lbLeft2.Name = "lbLeft2";
            this.lbLeft2.Size = new System.Drawing.Size(24, 20);
            this.lbLeft2.TabIndex = 6;
            this.lbLeft2.Text = ">>";
            // 
            // pnlDate
            // 
            this.pnlDate.Location = new System.Drawing.Point(12, 60);
            this.pnlDate.Name = "pnlDate";
            this.pnlDate.Size = new System.Drawing.Size(334, 207);
            this.pnlDate.TabIndex = 4;
            // 
            // labelLeft2
            // 
            this.labelLeft2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLeft2.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelLeft2.Location = new System.Drawing.Point(13, 37);
            this.labelLeft2.Name = "labelLeft2";
            this.labelLeft2.Size = new System.Drawing.Size(30, 20);
            this.labelLeft2.TabIndex = 5;
            this.labelLeft2.Text = "<<";
            // 
            // labelRight2
            // 
            this.labelRight2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelRight2.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelRight2.Location = new System.Drawing.Point(47, 37);
            this.labelRight2.Name = "labelRight2";
            this.labelRight2.Size = new System.Drawing.Size(30, 20);
            this.labelRight2.TabIndex = 6;
            this.labelRight2.Text = ">>";
            // 
            // DatePickerForm
            // 
            this.ClientSize = new System.Drawing.Size(354, 279);
            this.Controls.Add(this.lbTimeShow);
            this.Controls.Add(this.lbLeft);
            this.Controls.Add(this.lbRight);
            this.Controls.Add(this.lbRight2);
            this.Controls.Add(this.lbLeft2);
            this.Controls.Add(this.pnlDate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DatePickerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // 具名 Paint 处理器，避免在 InitializeComponent 中使用匿名方法导致 Designer 解析失败
        private void PicMode_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(isWeekMode ? showText : foldText, this.Font, Brushes.Black, 0, 0);
        }

        private void initControl()
        {
            // 显示当前选中日期（或当前时间）的年月/日（界面上会更新为选中项）
            lbTimeShow.Text = DateTime.Now.ToString("yyyy | MM.dd");
            lbLeft.Click += label3_Click;
            lbRight.Click += label4_Click;
            // 绑定新增的上一年/下一年事件
            lbRight2.Click += labelPrevYear_Click; // 前一年
            lbLeft2.Click += labelNextYear_Click;  // 后一年

            // 初始显示
            DateTime dateNow = DateTime.Now;
            if (isWeekMode)
            {
                // 周视图：先清空再绘制当前周（setDate 现在仅追加一行）
                this.pnlDate.Controls.Clear();
                timeDic = new Dictionary<Point, DateTime>();
                DayOfWeek week = dateNow.DayOfWeek;
                setDate(dateNow, week, 0);
            }
            else
            {
                // 月视图：传入当月第一天，getDateValue 会在循环前清空并逐行调用 setDate
                DateTime firstOfMonth = new DateTime(dateNow.Year, dateNow.Month, 1);
                DayOfWeek firstDow = firstOfMonth.DayOfWeek;
                getDateValue(firstOfMonth, firstDow);
            }

            // 选中当前日期（如果存在）
            foreach (var point in timeDic)
            {
                if (point.Value == Convert.ToDateTime(selectDate.ToString("yyyy-MM-dd")))
                {
                    var ctrl = this.pnlDate.Controls.Cast<Control>().FirstOrDefault(c => c.Location == point.Key) as CircleLabelControl;
                    if (ctrl != null) { setSelectCircLabel(ctrl); break; }
                }
            }
        }

        private void setDate(DateTime date, DayOfWeek week, int y /*, 可选 targetMonth */ , int? targetMonth = null)
        {
            DateTime dateTimeDic = DateTime.Now;
            int cells = 7; // 一周7天（只负责添加当前行，不再清空控件）
            for (int i = 0; i < cells; i++)
            {
                CircleLabelControl circleLabel = new CircleLabelControl();
                circleLabel.Location = new Point(6 + i * 47, 2 + y * 36);
                Brush backColor = Brushes.WhiteSmoke;
                Brush textColor = Brushes.Black;
                circleLabel.Text = getDateString(i, ref backColor, ref textColor, date, week, y, ref dateTimeDic);

                // 如果传入了 targetMonth，则仅显示属于该月的日期；否则显示全部（用于周视图）
                if (targetMonth.HasValue && dateTimeDic.Month != targetMonth.Value)
                {
                    // 跳过不属于目标月的日期（不创建控件）
                    continue;
                }

                circleLabel.BackColor = (backColor as SolidBrush)?.Color ?? Color.WhiteSmoke;
                circleLabel.TextColor = textColor;
                circleLabel.Name = $"circleLabel_{i}_{y}";
                circleLabel.Size = new Size(35, 35);
                circleLabel.Font = new Font("Arial", 10.5f, FontStyle.Regular);
                circleLabel.Click += circleLabel_Click;
                this.pnlDate.Controls.Add(circleLabel);
                timeDic[circleLabel.Location] = dateTimeDic;
            }
        }

        private string getDateString(int i, ref Brush backColor, ref Brush textColor, DateTime dateNow, DayOfWeek week, int y, ref DateTime dateTimeDic)
        {
            string date = string.Empty;
            int offset = 0;
            switch (week)
            {
                case DayOfWeek.Sunday: offset = -6 + i; break;
                case DayOfWeek.Monday: offset = 0 + i; break;
                case DayOfWeek.Tuesday: offset = -1 + i; break;
                case DayOfWeek.Wednesday: offset = -2 + i; break;
                case DayOfWeek.Thursday: offset = -3 + i; break;
                case DayOfWeek.Friday: offset = -4 + i; break;
                case DayOfWeek.Saturday: offset = -5 + i; break;
            }
            DateTime d = dateNow.AddDays(offset);
            dateTimeDic = Convert.ToDateTime(d.ToString("yyyy-MM-dd"));

            // 周末（周六/周日）字体为红色，其它为黑色
            if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
            {
                textColor = Brushes.Red;
            }
            else
            {
                textColor = Brushes.Black;
            }

            // 今天显示为 "今"（颜色仍受周末规则影响）
            if (dateTimeDic.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
            {
                date = "今";
            }
            else
            {
                date = d.ToString("dd");
            }
            // 在月视图时可根据是否同月变灰（示例里只实现周视图默认）
            return date;
        }

        private DateTime getSelectDateTime()
        {
            DateTime date = DateTime.Now;
            if (selectCircLabel != null && timeDic.TryGetValue(selectCircLabel.Location, out DateTime dt))
            {
                date = dt;
            }
            selectDate = date;
            return date;
        }

        private void setSelectCircLabel(CircleLabelControl circleLabelControl)
        {
            if (circleLabelControl == null) return;
            selectCircLabel = circleLabelControl;
            selectDate = getSelectDateTime();
            lbTimeShow.Text = selectDate.ToString("yyyy | MM") + "." + selectDate.ToString("dd");

            // 如果为灰色且处于月视图，可切换月（示例保留为不切换）
            // 重置所有控件样式
            foreach (Control control in this.pnlDate.Controls)
            {
                if (control is CircleLabelControl circle && circle != circleLabelControl)
                {
                    circle.BackColor = Brushes.WhiteSmoke is Brush ? (circle.BackColor = Color.WhiteSmoke) : circle.BackColor;
                  //  circle.TextColor = circle.Text == "今" ? Brushes.Blue : Brushes.Black;
                    circle.Font = new Font("Arial", 10.5f, FontStyle.Regular);
                }
            }

            // 高亮选中
            circleLabelControl.BackColor = Brushes.RoyalBlue is Brush ? Color.RoyalBlue : circleLabelControl.BackColor;
            circleLabelControl.TextColor = Brushes.White;
            circleLabelControl.Font = new Font("Arial", 10.5f, FontStyle.Bold);
            circleLabelControl.Invalidate();
        }

        private void setPicture()
        {
            // 切换展开/收起（示例：切换为月视图时展开）
            if (!isWeekMode)
            {
                // 展开为月视图
                DateTime dateTime = Convert.ToDateTime(getSelectDateTime().ToString("yyyy-MM-01"));
                DayOfWeek dayOfWeek = dateTime.DayOfWeek;
                getDateValue(dateTime, dayOfWeek);
                return;
            }
            // 收起回到周视图
            this.pnlDate.Controls.Clear();
            this.pnlDate.Size = new Size(291, 26);
            DateTime dateNow = selectDate;
            DayOfWeek week = dateNow.DayOfWeek;
            timeDic = new Dictionary<Point, DateTime>();
            setDate(dateNow, week, 0);
            // 选中当前日期
            foreach (var point in timeDic)
            {
                if (point.Value == Convert.ToDateTime(selectDate.ToString("yyyy-MM-dd")))
                {
                    var ctrl = this.pnlDate.Controls.Cast<Control>().FirstOrDefault(c => c.Location == point.Key) as CircleLabelControl;
                    if (ctrl != null) { setSelectCircLabel(ctrl); break; }
                }
            }
        }

        private void getDateValue(DateTime dateTime, DayOfWeek dayOfWeek)
        {
            int daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            int num = 0;
            if (daysInMonth <= 30 && dayOfWeek == DayOfWeek.Sunday) num = 6;
            if (daysInMonth <= 30 && dayOfWeek > DayOfWeek.Sunday) num = 5;
            if (daysInMonth > 30 && (dayOfWeek > DayOfWeek.Sunday && dayOfWeek <= DayOfWeek.Friday)) num = 5;
            if (daysInMonth > 30 && (dayOfWeek == DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Friday)) num = 6;
            this.pnlDate.Size = new Size(356, 3 + num * 36 + 10);
            this.pnlDate.Controls.Clear();
            timeDic = new Dictionary<Point, DateTime>();
            DateTime start = dateTime;
            for (int j = 0; j < num; j++)
            {
                if (j > 0)
                {
                    start = start.AddDays(7);
                    dayOfWeek = start.DayOfWeek;
                }
                // 传入目标月份，setDate 将只渲染属于该月的日期（剔除上月/下月的日期）
                setDate(start, dayOfWeek, j, dateTime.Month);
            }
            foreach (var point in timeDic)
            {
                if (point.Value == Convert.ToDateTime(selectDate.ToString("yyyy-MM-dd")))
                {
                    var ctrl = this.pnlDate.Controls.Cast<Control>().FirstOrDefault(c => c.Location == point.Key) as CircleLabelControl;
                    if (ctrl != null) { setSelectCircLabel(ctrl); break; }
                }
            }
        }

        // 新增：公开选中的日期，调用方可读取
        public DateTime SelectedDate => selectDate;

        // 新增：静态辅助方法，方便在按钮事件中调用并获得选中日期（未选返回 null）
        public static DateTime? ShowPicker(IWin32Window owner, DateTime? initialDate = null)
        {
            using (var f = new DatePickerForm(initialDate ?? DateTime.Now))
            {
                var dr = f.ShowDialog(owner);
                if (dr == DialogResult.OK)
                    return f.SelectedDate;
                return null;
            }
        }

        #region 事件
        private void circleLabel_Click(object sender, EventArgs e)
        {
            CircleLabelControl circleLabelControl = sender as CircleLabelControl;
            setSelectCircLabel(circleLabelControl);

            // 用户点击后直接关闭并返回结果给调用者
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 新增：前一年（lbRight2）点击处理
        private void labelPrevYear_Click(object sender, EventArgs e)
        {
            DateTime dateTime = getSelectDateTime();
            // 跳到上一年，定位到当年同月的第一天用于月视图绘制
            dateTime = new DateTime(dateTime.Year - 1, dateTime.Month, 1);
            selectDate = dateTime;
            DayOfWeek monthWeek = dateTime.DayOfWeek;
            timeDic = new Dictionary<Point, DateTime>();
            this.pnlDate.Controls.Clear();
            getDateValue(dateTime, monthWeek);
        }

        // 新增：下一年（lbLeft2）点击处理
        private void labelNextYear_Click(object sender, EventArgs e)
        {
            DateTime dateTime = getSelectDateTime();
            // 跳到下一年，定位到当年同月的第一天用于月视图绘制
            dateTime = new DateTime(dateTime.Year + 1, dateTime.Month, 1);
            selectDate = dateTime;
            DayOfWeek monthWeek = dateTime.DayOfWeek;
            timeDic = new Dictionary<Point, DateTime>();
            this.pnlDate.Controls.Clear();
            getDateValue(dateTime, monthWeek);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            DateTime dateTime = getSelectDateTime();
            if (!isWeekMode)
            {
                dateTime = Convert.ToDateTime(dateTime.AddMonths(-1).ToString("yyyy-MM-01"));
                selectDate = dateTime;
                DayOfWeek monthWeek = dateTime.DayOfWeek;
                timeDic = new Dictionary<Point, DateTime>();
                this.pnlDate.Controls.Clear();
                getDateValue(dateTime, monthWeek);
                return;
            }
            dateTime = dateTime.AddDays(-7);
            DayOfWeek week = dateTime.DayOfWeek;
            timeDic = new Dictionary<Point, DateTime>();
            this.pnlDate.Controls.Clear();
            setDate(dateTime, week, 0);
            // 选中第一个格
            var firstKey = timeDic.FirstOrDefault().Key;
            var ctrl = this.pnlDate.Controls.Cast<Control>().FirstOrDefault(c => c.Location == firstKey) as CircleLabelControl;
            if (ctrl != null) setSelectCircLabel(ctrl);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            DateTime dateTime = getSelectDateTime();
            if (!isWeekMode)
            {
                dateTime = Convert.ToDateTime(dateTime.AddMonths(1).ToString("yyyy-MM-01"));
                selectDate = dateTime;
                DayOfWeek monthWeek = dateTime.DayOfWeek;
                timeDic = new Dictionary<Point, DateTime>();
                this.pnlDate.Controls.Clear();
                getDateValue(dateTime, monthWeek);
                return;
            }
            dateTime = dateTime.AddDays(7);
            DayOfWeek week = dateTime.DayOfWeek;
            timeDic = new Dictionary<Point, DateTime>();
            this.pnlDate.Controls.Clear();
            setDate(dateTime, week, 0);
            var firstKey = timeDic.FirstOrDefault().Key;
            var ctrl = this.pnlDate.Controls.Cast<Control>().FirstOrDefault(c => c.Location == firstKey) as CircleLabelControl;
            if (ctrl != null) setSelectCircLabel(ctrl);
        }
        #endregion
    }
}

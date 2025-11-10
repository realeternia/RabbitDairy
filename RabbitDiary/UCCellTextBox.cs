using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeControl;
using System.IO;

namespace RabbitDiary
{
    public partial class UCCellTextBox : UserControl
    {
        public UCCellTextBox()
        {
            InitializeComponent();

            customTextBox1.LabelPageInfo = labelPageInfo;

            // 订阅日期变化事件
            customTextBox1.OnChangeDate += CustomTextBox1_OnChangeDate;

            // 初始化完成后触发一次，加载当天（或当前设置的）日记
            customTextBox1.ChangeDate(DateTime.Now);
        }

        // 新增：当 CustomTextBox 报告日期切换时，从文件加载对应日期的内容
        private void CustomTextBox1_OnChangeDate(object sender, CustomTextBox.DateChangedEventArgs e)
        {
            try
            {
                string baseDir = Path.Combine(Application.StartupPath, "Diary");
                string monthDir = Path.Combine(baseDir, e.Date.ToString("yyyy-MM"));
                string filePath = Path.Combine(monthDir, e.Date.ToString("yyyy-MM-dd") + ".txt");

                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath, Encoding.UTF8);
                    customTextBox1.DiaryBody = content;
                }
                else
                {
                    customTextBox1.DiaryBody = string.Empty;
                }
                // 刷新统计显示
                customTextBox1.UpdatePageInfo();
                customTextBox1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取日记失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDatePick_Click(object sender, EventArgs e)
        {
            // 使用 DatePickerForm 的静态辅助方法获取用户选择
            var picked = TimeControl.DatePickerForm.ShowPicker(this, customTextBox1.CurrentDate);
            if (picked.HasValue)
            {
                // 切换日期并触发 OnChangeDate，订阅者会负责加载文件
                customTextBox1.ChangeDate(picked.Value);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // 获取要写入的文本（允许为空）
            string text = customTextBox1.DiaryBody ?? string.Empty;

            try
            {
                // 基础目录（可按需修改为其它路径）
                string baseDir = Path.Combine(Application.StartupPath, "Diary");

                // 使用 customTextBox1 当前选中日期作为文件归属
                DateTime date = customTextBox1.CurrentDate;

                // 按月分目录： yyyy-MM
                string monthDir = Path.Combine(baseDir, date.ToString("yyyy-MM"));
                Directory.CreateDirectory(monthDir);

                // 文件名： yyyy-MM-dd.txt
                string filePath = Path.Combine(monthDir, date.ToString("yyyy-MM-dd") + ".txt");

                // 覆盖写入，使用 UTF8
                File.WriteAllText(filePath, text, Encoding.UTF8);

                MessageBox.Show($"已保存: {filePath}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

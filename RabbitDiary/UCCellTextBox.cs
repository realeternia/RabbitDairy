using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitDiary
{
    public partial class UCCellTextBox : UserControl
    {
        public UCCellTextBox()
        {
            InitializeComponent();

            customTextBox1.LabelPageInfo = labelPageInfo;
        }

    }
}

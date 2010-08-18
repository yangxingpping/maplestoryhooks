using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapleStoryHooks
{
    public partial class ProcessDialog : Form
    {
        public ProcessDialog()
        {
            InitializeComponent();
        }

        public string ProcessName { get { return textBox1.Text;  } }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }   
    }
}

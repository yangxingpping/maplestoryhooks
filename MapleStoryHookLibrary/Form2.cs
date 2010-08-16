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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void addToTreeViewReal(PacketSegment segment)
        {

        }

        public delegate void addToTreeViewDelegate(PacketSegment segment);

        public void addToTable(PacketSegment segment)
        {
            try
            {
                this.Invoke(new addToTreeViewDelegate(addToTreeViewReal), segment);

            }
            catch (Exception e)
            {
                Main.Interface.WriteConsole(e.Message);
            }
        }
    } 
}

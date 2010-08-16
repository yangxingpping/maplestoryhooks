using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace MapleStoryHooks
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //listBox2.DataSource = packetDict.Keys;
        }

       


        public void addToTreeViewReal(PacketSegment segment)
        {
            if (!treeView1.Nodes.ContainsKey(segment.Id.ToString()))
            {
                treeView1.Nodes.Add(segment.Id.ToString(), segment.Direction + "(" + segment.Id + ")");

            }
            TreeNode node = treeView1.Nodes[segment.Id.ToString()];
            node.Nodes.Add(segment.Type + ": " + segment.ToString());
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


        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
        }
    }

}
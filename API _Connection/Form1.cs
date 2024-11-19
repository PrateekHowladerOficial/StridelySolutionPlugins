using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using Tekla.Structures;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.Model.UI;
using Component = Tekla.Structures.Model.Component;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Tekla.Structures.Solid;
using System.Collections;
using static Tekla.Structures.Model.Position;

namespace API__Connection
{
    public partial class Form1 : Form
    {
       
       
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
          

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Visible = true;
        }
    }
}

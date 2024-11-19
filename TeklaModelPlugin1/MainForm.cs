using System;
using System.Windows;
using Tekla.Structures.Model.Operations;

namespace TeklaModelPlugin1
{
    public partial class MainForm : Tekla.Structures.Dialog.PluginFormBase
    {
        public MainForm()
        {
            InitializeComponent();
        }

        

       
        



        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("both sides");
            comboBox1.Items.Add("Front");
            comboBox1.Items.Add("Back");
            comboBox1.SelectedIndex = 1;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Up");
            comboBox2.Items.Add("Down");
            comboBox2.SelectedIndex = 0;
          
            pictureBox4.Image = imageList1.Images[1];
            comboBox3.Items.Clear();
            comboBox3.Items.Add("Outside");
            comboBox3.Items.Add("Inside");
            comboBox3.SelectedIndex = 0;
            pictureBox5.Image = imageList2.Images[0];
            comboBox5.Items.Clear();
            comboBox5.Items.Add("Middle");
            comboBox5.Items.Add("Outter edge");
            comboBox5.Items.Add("Inner edge");
            comboBox5.SelectedIndex = 0;    
        }
        protected override string LoadValuesPath(string fileName)
        {
            SetAttributeValue(textBox1, 10);
            SetAttributeValue(textBox2, "IS2062");
            SetAttributeValue(textBox3, "batten plate");
            SetAttributeValue(textBox4, "foo");
            SetAttributeValue(textBox5, 300);
            SetAttributeValue(textBox6, 5);
            SetAttributeValue(textBox7, 300);
            SetAttributeValue(textBox8, 0);
            SetAttributeValue(textBox9, 0);
            SetAttributeValue(comboBox2, 0);
            SetAttributeValue(comboBox1, 1);
            SetAttributeValue(comboBox3, 0);
            
            SetAttributeValue(comboBox5, 1);
            return base.LoadValuesPath(fileName);
        }

      

        private void okApplyModifyGetOnOffCancel1_Load(object sender, EventArgs e)
        {

        }
        private void okApplyModifyGetOnOffCancel1_ApplyClicked(object sender, EventArgs e)
        {
            this.Apply();
        }
        private void okApplyModifyGetOnOffCancel1_CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okApplyModifyGetOnOffCancel1_GetClicked(object sender, EventArgs e)
        {
            this.Get();
        }

        private void okApplyModifyGetOnOffCancel1_ModifyClicked(object sender, EventArgs e)
        {
            this.Modify();

        }

        private void okApplyModifyGetOnOffCancel1_OkClicked(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void okApplyModifyGetOnOffCancel1_OnOffClicked(object sender, EventArgs e)
        {
            this.ToggleSelection();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox6.Image = imageList3.Images[comboBox5.SelectedIndex];
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox6.Text.Length <= 0)
            {
               
                Operation.DisplayPrompt("The number of plates cannot be less than '0'!");
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text.Length <= 0)
            {
                
                Operation.DisplayPrompt("The Width of plates cannot be less than '0'!");
            }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (textBox7.Text.Length <= 0)
            {
                
                Operation.DisplayPrompt("The Distance Between Plates cannot be less than '0'!");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length <= 0)
            {

                Operation.DisplayPrompt("The thickness of Plates cannot be less than '0'!");
            }
        }

        

        private void materialCatalog1_Load(object sender, EventArgs e)
        {
            
        }
        private void materialCatalog1_select(object sender, EventArgs e)
        {
            textBox2.Text = materialCatalog1.SelectedMaterial.ToString();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox4.Image = imageList1.Images[comboBox1.SelectedIndex];
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox5.Image = imageList2.Images[comboBox3.SelectedIndex];
        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            pictureBox3.Image = imageList4.Images[comboBox2.SelectedIndex];
        }
    }
}
using RenderData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Type17_ClipToStiffener
{
    public partial class MainForm : Tekla.Structures.Dialog.PluginFormBase
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OkApplyModifyGetOnOffCancel_OkClicked(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void OkApplyModifyGetOnOffCancel_ApplyClicked(object sender, EventArgs e)
        {
            this.Apply();
        }

        private void OkApplyModifyGetOnOffCancel_ModifyClicked(object sender, EventArgs e)
        {
            this.Modify();
        }

        private void OkApplyModifyGetOnOffCancel_GetClicked(object sender, EventArgs e)
        {
            this.Get();
        }

        private void OkApplyModifyGetOnOffCancel_OnOffClicked(object sender, EventArgs e)
        {
            this.ToggleSelection();
        }

        private void OkApplyModifyGetOnOffCancel_CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ParametersTabPage_Click(object sender, EventArgs e)
        {

        }

      
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            
           
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            
           
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                
                foreach (ComboBox item in new List<ComboBox> { comboBox5,comboBox4 ,comboBox6, comboBox7, comboBox8, comboBox9, comboBox10 })
                {
                    item.Items.Add("Yes");
                    item.Items.Add("no");
                }

                comboBox5.SelectedIndex = 0;
                comboBox6.SelectedIndex = 0;
                comboBox7.SelectedIndex = 1;
                comboBox8.SelectedIndex = 1;
                comboBox9.SelectedIndex = 0;
                comboBox10.SelectedIndex = 1;

                comboBox2.SelectedIndex = 0;
                comboBox3.SelectedIndex = 0;
                comboBox4.SelectedIndex = 0;
               
            }
            
            catch (Exception e1 )
            {
                MessageBox.Show(e1.ToString());
            }
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void textBox20_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialCatalog1_Load_1(object sender, EventArgs e)
        {

            
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
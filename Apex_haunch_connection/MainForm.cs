using System;
using System.Collections.Generic;

using ComboBox = System.Windows.Forms.ComboBox;

namespace Apex_haunch_connection
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

        private void OkApplyModifyGetOnOffCancel_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            foreach (ComboBox item in new List<ComboBox> { comboBox5, comboBox4, comboBox6, comboBox7, comboBox8, comboBox9, comboBox10 })
            {
                item.Items.Clear();
                item.Items.Add("Yes");
                item.Items.Add("no");
            }

            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            comboBox7.SelectedIndex = 1;
            comboBox8.SelectedIndex = 1;
            comboBox9.SelectedIndex = 0;
            comboBox10.SelectedIndex = 1;

            comboBox4.SelectedIndex = 0;
            comboBox1.Items.Add("Top");
            comboBox1.Items.Add("Middle");
            comboBox1.SelectedIndex = 0;
            pictureBox6.Image = imageList1.Images[comboBox1.SelectedIndex];

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void materialCatalog1_Load(object sender, EventArgs e)
        {
            
        }
        private void materialCatalog1_select(object sender, EventArgs e)
        {
           // textBox10.SelectedText = materialCatalog1.SelectedMaterial.ToString();
            SetAttributeValue(textBox10, materialCatalog1.SelectedMaterial.ToString());
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox6.Image = imageList1.Images[comboBox1.SelectedIndex];
        }
    }
}
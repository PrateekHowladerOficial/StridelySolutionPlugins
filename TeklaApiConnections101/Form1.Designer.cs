namespace TeklaApiConnections101
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_EndPlate = new System.Windows.Forms.Button();
            this.btn_SingleGirt = new System.Windows.Forms.Button();
            this.btn_DoubleGirt = new System.Windows.Forms.Button();
            this.btn_KneeConn = new System.Windows.Forms.Button();
            this.btn_SpliceConn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_EndPlate
            // 
            this.btn_EndPlate.Location = new System.Drawing.Point(54, 53);
            this.btn_EndPlate.Name = "btn_EndPlate";
            this.btn_EndPlate.Size = new System.Drawing.Size(167, 23);
            this.btn_EndPlate.TabIndex = 0;
            this.btn_EndPlate.Text = "End Plate Connection";
            this.btn_EndPlate.UseVisualStyleBackColor = true;
            this.btn_EndPlate.Click += new System.EventHandler(this.btn_EndPlate_Click);
            // 
            // btn_SingleGirt
            // 
            this.btn_SingleGirt.Location = new System.Drawing.Point(54, 101);
            this.btn_SingleGirt.Name = "btn_SingleGirt";
            this.btn_SingleGirt.Size = new System.Drawing.Size(167, 23);
            this.btn_SingleGirt.TabIndex = 1;
            this.btn_SingleGirt.Text = "Single Girt Connection";
            this.btn_SingleGirt.UseVisualStyleBackColor = true;
            this.btn_SingleGirt.Click += new System.EventHandler(this.btn_SingleGirt_Click);
            // 
            // btn_DoubleGirt
            // 
            this.btn_DoubleGirt.Location = new System.Drawing.Point(54, 148);
            this.btn_DoubleGirt.Name = "btn_DoubleGirt";
            this.btn_DoubleGirt.Size = new System.Drawing.Size(167, 23);
            this.btn_DoubleGirt.TabIndex = 2;
            this.btn_DoubleGirt.Text = "Double Girt Connection";
            this.btn_DoubleGirt.UseVisualStyleBackColor = true;
            this.btn_DoubleGirt.Click += new System.EventHandler(this.btn_DoubleGirt_Click);
            // 
            // btn_KneeConn
            // 
            this.btn_KneeConn.Location = new System.Drawing.Point(54, 198);
            this.btn_KneeConn.Name = "btn_KneeConn";
            this.btn_KneeConn.Size = new System.Drawing.Size(167, 23);
            this.btn_KneeConn.TabIndex = 3;
            this.btn_KneeConn.Text = "Knee Connection";
            this.btn_KneeConn.UseVisualStyleBackColor = true;
            this.btn_KneeConn.Click += new System.EventHandler(this.btn_KneeConn_Click);
            // 
            // btn_SpliceConn
            // 
            this.btn_SpliceConn.Location = new System.Drawing.Point(54, 249);
            this.btn_SpliceConn.Name = "btn_SpliceConn";
            this.btn_SpliceConn.Size = new System.Drawing.Size(167, 23);
            this.btn_SpliceConn.TabIndex = 4;
            this.btn_SpliceConn.Text = "Splice Connection";
            this.btn_SpliceConn.UseVisualStyleBackColor = true;
            this.btn_SpliceConn.Click += new System.EventHandler(this.btn_SpliceConn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 319);
            this.Controls.Add(this.btn_SpliceConn);
            this.Controls.Add(this.btn_KneeConn);
            this.Controls.Add(this.btn_DoubleGirt);
            this.Controls.Add(this.btn_SingleGirt);
            this.Controls.Add(this.btn_EndPlate);
            this.Name = "Form1";
            this.Text = "Tekla Connections";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_EndPlate;
        private System.Windows.Forms.Button btn_SingleGirt;
        private System.Windows.Forms.Button btn_DoubleGirt;
        private System.Windows.Forms.Button btn_KneeConn;
        private System.Windows.Forms.Button btn_SpliceConn;
    }
}


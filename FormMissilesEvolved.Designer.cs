namespace MissilesEvolved
{
    partial class FormMissilesEvolved
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timerSimulate = new System.Windows.Forms.Timer(this.components);
            this.numericUpDownWindStrength = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxConstantWind = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.numericUpDownMaxDeviation = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWindStrength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDeviation)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 462);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // timerSimulate
            // 
            this.timerSimulate.Interval = 20;
            // 
            // numericUpDownWindStrength
            // 
            this.numericUpDownWindStrength.DecimalPlaces = 1;
            this.numericUpDownWindStrength.Location = new System.Drawing.Point(95, 22);
            this.numericUpDownWindStrength.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownWindStrength.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            -2147483648});
            this.numericUpDownWindStrength.Name = "numericUpDownWindStrength";
            this.numericUpDownWindStrength.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownWindStrength.TabIndex = 1;
            this.toolTip1.SetToolTip(this.numericUpDownWindStrength, "Maximum strength of the wind.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Max strength:";
            // 
            // checkBoxConstantWind
            // 
            this.checkBoxConstantWind.AutoSize = true;
            this.checkBoxConstantWind.Location = new System.Drawing.Point(12, 93);
            this.checkBoxConstantWind.Name = "checkBoxConstantWind";
            this.checkBoxConstantWind.Size = new System.Drawing.Size(146, 19);
            this.checkBoxConstantWind.TabIndex = 3;
            this.checkBoxConstantWind.Text = "Wind blows constantly";
            this.toolTip1.SetToolTip(this.checkBoxConstantWind, "Wind blows constantly, uncheck for random.");
            this.checkBoxConstantWind.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(543, 459);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "P - Pause | F - Frames/second";
            // 
            // numericUpDownMaxDeviation
            // 
            this.numericUpDownMaxDeviation.DecimalPlaces = 1;
            this.numericUpDownMaxDeviation.Location = new System.Drawing.Point(95, 57);
            this.numericUpDownMaxDeviation.Maximum = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
            this.numericUpDownMaxDeviation.Name = "numericUpDownMaxDeviation";
            this.numericUpDownMaxDeviation.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownMaxDeviation.TabIndex = 5;
            this.toolTip1.SetToolTip(this.numericUpDownMaxDeviation, "The max amount the angle of the wind shifts.");
            this.numericUpDownMaxDeviation.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDownMaxDeviation);
            this.groupBox1.Controls.Add(this.numericUpDownWindStrength);
            this.groupBox1.Controls.Add(this.checkBoxConstantWind);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(537, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(211, 124);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Wind";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Max deviation:";
            // 
            // FormMissilesEvolved
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(761, 490);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMissilesEvolved";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Missiles Evolved";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWindStrength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDeviation)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pictureBox1;
        private System.Windows.Forms.Timer timerSimulate;
        private NumericUpDown numericUpDownWindStrength;
        private Label label1;
        private CheckBox checkBoxConstantWind;
        private Label label2;
        private ToolTip toolTip1;
        private GroupBox groupBox1;
        private Label label3;
        private NumericUpDown numericUpDownMaxDeviation;
    }
}
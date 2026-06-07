using System;
using System.Windows.Forms;

namespace WarningApp
{
    public partial class SettingForm : Form
    {
        private MainForm mainForm = null!;

        public SettingForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.intervalNumericUpDown.Value = mainForm.IntervalMinutes;
            this.restNumericUpDown.Value = mainForm.RestMinutes;
        }

        private void saveButton_Click(object? sender, EventArgs e)
        {
            mainForm.IntervalMinutes = (int)intervalNumericUpDown.Value;
            mainForm.RestMinutes = (int)restNumericUpDown.Value;
            mainForm.SaveSettings();
            mainForm.ResetTimer();
            this.Close();
        }

        private System.ComponentModel.IContainer? components;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.intervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.restNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.saveButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.intervalNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.restNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "休息间隔：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "休息时长：";
            // 
            // intervalNumericUpDown
            // 
            this.intervalNumericUpDown.Location = new System.Drawing.Point(83, 21);
            this.intervalNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.intervalNumericUpDown.Name = "intervalNumericUpDown";
            this.intervalNumericUpDown.Size = new System.Drawing.Size(65, 25);
            this.intervalNumericUpDown.TabIndex = 2;
            this.intervalNumericUpDown.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // restNumericUpDown
            // 
            this.restNumericUpDown.Location = new System.Drawing.Point(83, 54);
            this.restNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.restNumericUpDown.Name = "restNumericUpDown";
            this.restNumericUpDown.Size = new System.Drawing.Size(65, 25);
            this.restNumericUpDown.TabIndex = 3;
            this.restNumericUpDown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(60, 96);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 30);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "保存";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(154, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "分钟";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(154, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "分钟";
            // 
            // SettingForm
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 138);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.restNumericUpDown);
            this.Controls.Add(this.intervalNumericUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置";
            ((System.ComponentModel.ISupportInitialize)(this.intervalNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.restNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label label1 = null!;
        private Label label2 = null!;
        private NumericUpDown intervalNumericUpDown = null!;
        private NumericUpDown restNumericUpDown = null!;
        private Button saveButton = null!;
        private Label label3 = null!;
        private Label label4 = null!;
    }
}
using System.ComponentModel;

namespace RPCC
{
    partial class FocusForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FocusForm));
            this.Run_slow = new System.Windows.Forms.Label();
            this.Run_slow_numericUpDown = new System.Windows.Forms.NumericUpDown();
            this.Run_fast_numericUpDown = new System.Windows.Forms.NumericUpDown();
            this.Run_fast = new System.Windows.Forms.Label();
            this.Focus_pos_label = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button_stop = new System.Windows.Forms.Button();
            this.label_setF = new System.Windows.Forms.Label();
            this.label_setS = new System.Windows.Forms.Label();
            this.numericUpDown_setF = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_setS = new System.Windows.Forms.NumericUpDown();
            this.label_setZero = new System.Windows.Forms.Label();
            this.numericUpDown_setZero = new System.Windows.Forms.NumericUpDown();
            this.checkBox_AutoFocus = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.Run_slow_numericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Run_fast_numericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setZero)).BeginInit();
            this.SuspendLayout();
            // 
            // Run_slow
            // 
            this.Run_slow.AutoSize = true;
            this.Run_slow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Run_slow.Location = new System.Drawing.Point(12, 31);
            this.Run_slow.Name = "Run_slow";
            this.Run_slow.Size = new System.Drawing.Size(81, 13);
            this.Run_slow.TabIndex = 0;
            this.Run_slow.Text = "Run slow +-234";
            // 
            // Run_slow_numericUpDown
            // 
            this.Run_slow_numericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Run_slow_numericUpDown.Location = new System.Drawing.Point(130, 29);
            this.Run_slow_numericUpDown.Maximum = new decimal(new int[] {
            234,
            0,
            0,
            0});
            this.Run_slow_numericUpDown.Minimum = new decimal(new int[] {
            234,
            0,
            0,
            -2147483648});
            this.Run_slow_numericUpDown.Name = "Run_slow_numericUpDown";
            this.Run_slow_numericUpDown.Size = new System.Drawing.Size(80, 20);
            this.Run_slow_numericUpDown.TabIndex = 1;
            this.Run_slow_numericUpDown.ValueChanged += new System.EventHandler(this.Run_slow_numericUpDown_ValueChanged);
            // 
            // Run_fast_numericUpDown
            // 
            this.Run_fast_numericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Run_fast_numericUpDown.Location = new System.Drawing.Point(130, 61);
            this.Run_fast_numericUpDown.Maximum = new decimal(new int[] {
            234,
            0,
            0,
            0});
            this.Run_fast_numericUpDown.Minimum = new decimal(new int[] {
            234,
            0,
            0,
            -2147483648});
            this.Run_fast_numericUpDown.Name = "Run_fast_numericUpDown";
            this.Run_fast_numericUpDown.Size = new System.Drawing.Size(80, 20);
            this.Run_fast_numericUpDown.TabIndex = 3;
            this.Run_fast_numericUpDown.ValueChanged += new System.EventHandler(this.Run_fast_numericUpDown_ValueChanged);
            // 
            // Run_fast
            // 
            this.Run_fast.AutoSize = true;
            this.Run_fast.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Run_fast.Location = new System.Drawing.Point(12, 60);
            this.Run_fast.Name = "Run_fast";
            this.Run_fast.Size = new System.Drawing.Size(77, 13);
            this.Run_fast.TabIndex = 2;
            this.Run_fast.Text = "Run fast +-234";
            // 
            // Focus_pos_label
            // 
            this.Focus_pos_label.AutoSize = true;
            this.Focus_pos_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Focus_pos_label.Location = new System.Drawing.Point(11, 92);
            this.Focus_pos_label.Name = "Focus_pos_label";
            this.Focus_pos_label.Size = new System.Drawing.Size(78, 13);
            this.Focus_pos_label.TabIndex = 4;
            this.Focus_pos_label.Text = "Focus position:";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_stop.Location = new System.Drawing.Point(15, 126);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(198, 29);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label_setF
            // 
            this.label_setF.AutoSize = true;
            this.label_setF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_setF.Location = new System.Drawing.Point(270, 31);
            this.label_setF.Name = "label_setF";
            this.label_setF.Size = new System.Drawing.Size(131, 13);
            this.label_setF.TabIndex = 6;
            this.label_setF.Text = "Set speed fast (steps/sec)";
            // 
            // label_setS
            // 
            this.label_setS.AutoSize = true;
            this.label_setS.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_setS.Location = new System.Drawing.Point(270, 63);
            this.label_setS.Name = "label_setS";
            this.label_setS.Size = new System.Drawing.Size(135, 13);
            this.label_setS.TabIndex = 7;
            this.label_setS.Text = "Set speed slow (steps/sec)";
            // 
            // numericUpDown_setF
            // 
            this.numericUpDown_setF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDown_setF.Location = new System.Drawing.Point(445, 29);
            this.numericUpDown_setF.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_setF.Name = "numericUpDown_setF";
            this.numericUpDown_setF.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown_setF.TabIndex = 8;
            this.numericUpDown_setF.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.numericUpDown_setF.ValueChanged += new System.EventHandler(this.numericUpDown_setF_ValueChanged);
            // 
            // numericUpDown_setS
            // 
            this.numericUpDown_setS.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDown_setS.Location = new System.Drawing.Point(445, 61);
            this.numericUpDown_setS.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_setS.Name = "numericUpDown_setS";
            this.numericUpDown_setS.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown_setS.TabIndex = 9;
            this.numericUpDown_setS.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDown_setS.ValueChanged += new System.EventHandler(this.numericUpDown_setS_ValueChanged);
            // 
            // label_setZero
            // 
            this.label_setZero.AutoSize = true;
            this.label_setZero.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_setZero.Location = new System.Drawing.Point(270, 92);
            this.label_setZero.Name = "label_setZero";
            this.label_setZero.Size = new System.Drawing.Size(85, 13);
            this.label_setZero.TabIndex = 10;
            this.label_setZero.Text = "Set zero position";
            // 
            // numericUpDown_setZero
            // 
            this.numericUpDown_setZero.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDown_setZero.Location = new System.Drawing.Point(445, 92);
            this.numericUpDown_setZero.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDown_setZero.Name = "numericUpDown_setZero";
            this.numericUpDown_setZero.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown_setZero.TabIndex = 11;
            this.numericUpDown_setZero.ValueChanged += new System.EventHandler(this.numericUpDown_setZero_ValueChanged);
            // 
            // checkBox_AutoFocus
            // 
            this.checkBox_AutoFocus.AutoSize = true;
            this.checkBox_AutoFocus.Checked = true;
            this.checkBox_AutoFocus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoFocus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBox_AutoFocus.Location = new System.Drawing.Point(273, 133);
            this.checkBox_AutoFocus.Name = "checkBox_AutoFocus";
            this.checkBox_AutoFocus.Size = new System.Drawing.Size(77, 17);
            this.checkBox_AutoFocus.TabIndex = 12;
            this.checkBox_AutoFocus.Text = "Auto focus";
            this.checkBox_AutoFocus.UseVisualStyleBackColor = true;
            this.checkBox_AutoFocus.CheckedChanged += new System.EventHandler(this.checkBox_AutoFocus_CheckedChanged);
            // 
            // FocusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 211);
            this.Controls.Add(this.checkBox_AutoFocus);
            this.Controls.Add(this.numericUpDown_setZero);
            this.Controls.Add(this.label_setZero);
            this.Controls.Add(this.numericUpDown_setS);
            this.Controls.Add(this.numericUpDown_setF);
            this.Controls.Add(this.label_setS);
            this.Controls.Add(this.label_setF);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.Focus_pos_label);
            this.Controls.Add(this.Run_fast_numericUpDown);
            this.Controls.Add(this.Run_fast);
            this.Controls.Add(this.Run_slow_numericUpDown);
            this.Controls.Add(this.Run_slow);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FocusForm";
            this.Text = "Focus";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Focus_Form_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Run_slow_numericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Run_fast_numericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setZero)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Run_slow;
        private System.Windows.Forms.NumericUpDown Run_slow_numericUpDown;
        private System.Windows.Forms.NumericUpDown Run_fast_numericUpDown;
        private System.Windows.Forms.Label Run_fast;
        private System.Windows.Forms.Label Focus_pos_label;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Label label_setF;
        private System.Windows.Forms.Label label_setS;
        private System.Windows.Forms.NumericUpDown numericUpDown_setF;
        private System.Windows.Forms.NumericUpDown numericUpDown_setS;
        private System.Windows.Forms.Label label_setZero;
        private System.Windows.Forms.NumericUpDown numericUpDown_setZero;
        private System.Windows.Forms.CheckBox checkBox_AutoFocus;
    }
}
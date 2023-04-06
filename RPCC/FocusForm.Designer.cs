﻿using System.ComponentModel;

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
            this.endswitch = new System.Windows.Forms.Label();
            this.numericUpDown_setDefoc = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_goZenith = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.Run_slow_numericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Run_fast_numericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setZero)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setDefoc)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Run_slow
            // 
            this.Run_slow.AutoSize = true;
            this.Run_slow.Location = new System.Drawing.Point(11, 131);
            this.Run_slow.Name = "Run_slow";
            this.Run_slow.Size = new System.Drawing.Size(127, 20);
            this.Run_slow.TabIndex = 0;
            this.Run_slow.Text = "Run slow (steps)";
            // 
            // Run_slow_numericUpDown
            // 
            this.Run_slow_numericUpDown.Enabled = false;
            this.Run_slow_numericUpDown.Location = new System.Drawing.Point(265, 129);
            this.Run_slow_numericUpDown.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.Run_slow_numericUpDown.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            -2147483648});
            this.Run_slow_numericUpDown.Name = "Run_slow_numericUpDown";
            this.Run_slow_numericUpDown.Size = new System.Drawing.Size(120, 26);
            this.Run_slow_numericUpDown.TabIndex = 1;
            this.Run_slow_numericUpDown.ValueChanged += new System.EventHandler(this.Run_slow_numericUpDown_ValueChanged);
            // 
            // Run_fast_numericUpDown
            // 
            this.Run_fast_numericUpDown.Enabled = false;
            this.Run_fast_numericUpDown.Location = new System.Drawing.Point(265, 161);
            this.Run_fast_numericUpDown.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.Run_fast_numericUpDown.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            -2147483648});
            this.Run_fast_numericUpDown.Name = "Run_fast_numericUpDown";
            this.Run_fast_numericUpDown.Size = new System.Drawing.Size(120, 26);
            this.Run_fast_numericUpDown.TabIndex = 3;
            this.Run_fast_numericUpDown.ValueChanged += new System.EventHandler(this.Run_fast_numericUpDown_ValueChanged);
            // 
            // Run_fast
            // 
            this.Run_fast.AutoSize = true;
            this.Run_fast.Location = new System.Drawing.Point(11, 161);
            this.Run_fast.Name = "Run_fast";
            this.Run_fast.Size = new System.Drawing.Size(123, 20);
            this.Run_fast.TabIndex = 2;
            this.Run_fast.Text = "Run fast (steps)";
            // 
            // Focus_pos_label
            // 
            this.Focus_pos_label.AutoSize = true;
            this.Focus_pos_label.Location = new System.Drawing.Point(16, 22);
            this.Focus_pos_label.Name = "Focus_pos_label";
            this.Focus_pos_label.Size = new System.Drawing.Size(116, 20);
            this.Focus_pos_label.TabIndex = 4;
            this.Focus_pos_label.Text = "Focus position:";
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(20, 97);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(314, 29);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label_setF
            // 
            this.label_setF.AutoSize = true;
            this.label_setF.Location = new System.Drawing.Point(11, 35);
            this.label_setF.Name = "label_setF";
            this.label_setF.Size = new System.Drawing.Size(195, 20);
            this.label_setF.TabIndex = 6;
            this.label_setF.Text = "Set speed fast (steps/sec)";
            // 
            // label_setS
            // 
            this.label_setS.AutoSize = true;
            this.label_setS.Location = new System.Drawing.Point(11, 67);
            this.label_setS.Name = "label_setS";
            this.label_setS.Size = new System.Drawing.Size(199, 20);
            this.label_setS.TabIndex = 7;
            this.label_setS.Text = "Set speed slow (steps/sec)";
            // 
            // numericUpDown_setF
            // 
            this.numericUpDown_setF.Enabled = false;
            this.numericUpDown_setF.Location = new System.Drawing.Point(265, 34);
            this.numericUpDown_setF.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_setF.Name = "numericUpDown_setF";
            this.numericUpDown_setF.Size = new System.Drawing.Size(120, 26);
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
            this.numericUpDown_setS.Enabled = false;
            this.numericUpDown_setS.Location = new System.Drawing.Point(265, 65);
            this.numericUpDown_setS.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_setS.Name = "numericUpDown_setS";
            this.numericUpDown_setS.Size = new System.Drawing.Size(120, 26);
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
            this.label_setZero.Location = new System.Drawing.Point(11, 99);
            this.label_setZero.Name = "label_setZero";
            this.label_setZero.Size = new System.Drawing.Size(128, 20);
            this.label_setZero.TabIndex = 10;
            this.label_setZero.Text = "Set zero position";
            // 
            // numericUpDown_setZero
            // 
            this.numericUpDown_setZero.Enabled = false;
            this.numericUpDown_setZero.Location = new System.Drawing.Point(265, 97);
            this.numericUpDown_setZero.Maximum = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            this.numericUpDown_setZero.Minimum = new decimal(new int[] {
            1500,
            0,
            0,
            -2147483648});
            this.numericUpDown_setZero.Name = "numericUpDown_setZero";
            this.numericUpDown_setZero.Size = new System.Drawing.Size(120, 26);
            this.numericUpDown_setZero.TabIndex = 11;
            this.numericUpDown_setZero.ValueChanged += new System.EventHandler(this.numericUpDown_setZero_ValueChanged);
            // 
            // checkBox_AutoFocus
            // 
            this.checkBox_AutoFocus.AutoSize = true;
            this.checkBox_AutoFocus.Checked = true;
            this.checkBox_AutoFocus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoFocus.Location = new System.Drawing.Point(24, 67);
            this.checkBox_AutoFocus.Name = "checkBox_AutoFocus";
            this.checkBox_AutoFocus.Size = new System.Drawing.Size(105, 24);
            this.checkBox_AutoFocus.TabIndex = 12;
            this.checkBox_AutoFocus.Text = "Auto focus";
            this.checkBox_AutoFocus.UseVisualStyleBackColor = true;
            this.checkBox_AutoFocus.CheckedChanged += new System.EventHandler(this.checkBox_AutoFocus_CheckedChanged);
            // 
            // endswitch
            // 
            this.endswitch.AutoSize = true;
            this.endswitch.Location = new System.Drawing.Point(16, 42);
            this.endswitch.Name = "endswitch";
            this.endswitch.Size = new System.Drawing.Size(137, 20);
            this.endswitch.TabIndex = 13;
            this.endswitch.Text = "Endswitch: unjoint";
            // 
            // numericUpDown_setDefoc
            // 
            this.numericUpDown_setDefoc.Location = new System.Drawing.Point(214, 35);
            this.numericUpDown_setDefoc.Maximum = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            this.numericUpDown_setDefoc.Minimum = new decimal(new int[] {
            1500,
            0,
            0,
            -2147483648});
            this.numericUpDown_setDefoc.Name = "numericUpDown_setDefoc";
            this.numericUpDown_setDefoc.Size = new System.Drawing.Size(120, 26);
            this.numericUpDown_setDefoc.TabIndex = 15;
            this.numericUpDown_setDefoc.ValueChanged += new System.EventHandler(this.numericUpDown_setDefoc_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 20);
            this.label1.TabIndex = 14;
            this.label1.Text = "Set defocus (steps)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_goZenith);
            this.groupBox1.Controls.Add(this.numericUpDown_setDefoc);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button_stop);
            this.groupBox1.Controls.Add(this.checkBox_AutoFocus);
            this.groupBox1.Location = new System.Drawing.Point(422, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(352, 133);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Auto Focus";
            // 
            // checkBox_goZenith
            // 
            this.checkBox_goZenith.AutoSize = true;
            this.checkBox_goZenith.Location = new System.Drawing.Point(135, 67);
            this.checkBox_goZenith.Name = "checkBox_goZenith";
            this.checkBox_goZenith.Size = new System.Drawing.Size(137, 24);
            this.checkBox_goZenith.TabIndex = 16;
            this.checkBox_goZenith.Text = "Focus at zenith";
            this.checkBox_goZenith.UseVisualStyleBackColor = true;
            this.checkBox_goZenith.CheckedChanged += new System.EventHandler(this.checkBox_goZenith_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numericUpDown_setS);
            this.groupBox2.Controls.Add(this.numericUpDown_setZero);
            this.groupBox2.Controls.Add(this.label_setF);
            this.groupBox2.Controls.Add(this.label_setZero);
            this.groupBox2.Controls.Add(this.Run_fast_numericUpDown);
            this.groupBox2.Controls.Add(this.label_setS);
            this.groupBox2.Controls.Add(this.Run_fast);
            this.groupBox2.Controls.Add(this.numericUpDown_setF);
            this.groupBox2.Controls.Add(this.Run_slow_numericUpDown);
            this.groupBox2.Controls.Add(this.Run_slow);
            this.groupBox2.Location = new System.Drawing.Point(16, 24);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(400, 202);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Focus Settings";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Focus_pos_label);
            this.groupBox3.Controls.Add(this.endswitch);
            this.groupBox3.Location = new System.Drawing.Point(422, 156);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(352, 70);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Info";
            // 
            // FocusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 241);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FocusForm";
            this.Text = "Focus";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FocusForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Run_slow_numericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Run_fast_numericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setZero)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_setDefoc)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Label endswitch;
        private System.Windows.Forms.NumericUpDown numericUpDown_setDefoc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBox_goZenith;
    }
}
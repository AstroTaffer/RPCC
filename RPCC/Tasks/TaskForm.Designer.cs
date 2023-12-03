using System.ComponentModel;

namespace RPCC.Tasks
{
    partial class TaskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskForm));
            this.groupBoxCoords = new System.Windows.Forms.GroupBox();
            this.comboBoxObjectType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelTaskN = new System.Windows.Forms.Label();
            this.textBoxObserver = new System.Windows.Forms.TextBox();
            this.labelObserver = new System.Windows.Forms.Label();
            this.textBoxObject = new System.Windows.Forms.TextBox();
            this.labelObject = new System.Windows.Forms.Label();
            this.textBoxDateTime = new System.Windows.Forms.TextBox();
            this.labelTimeStart = new System.Windows.Forms.Label();
            this.textBoxCoords = new System.Windows.Forms.TextBox();
            this.labelCoords = new System.Windows.Forms.Label();
            this.groupBoxFrame = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numericUpDown_ybin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_xbin = new System.Windows.Forms.NumericUpDown();
            this.labelYbin = new System.Windows.Forms.Label();
            this.labelXbin = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxDuration = new System.Windows.Forms.TextBox();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelFrameType = new System.Windows.Forms.Label();
            this.comboBoxFrameType = new System.Windows.Forms.ComboBox();
            this.comboBoxExp = new System.Windows.Forms.ComboBox();
            this.labelExp = new System.Windows.Forms.Label();
            this.textBoxExpN = new System.Windows.Forms.TextBox();
            this.labelExpN = new System.Windows.Forms.Label();
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.checkBoxFili = new System.Windows.Forms.CheckBox();
            this.checkBoxFilr = new System.Windows.Forms.CheckBox();
            this.checkBoxFilg = new System.Windows.Forms.CheckBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.groupBoxCoords.SuspendLayout();
            this.groupBoxFrame.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ybin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_xbin)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBoxFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCoords
            // 
            this.groupBoxCoords.Controls.Add(this.comboBoxObjectType);
            this.groupBoxCoords.Controls.Add(this.label1);
            this.groupBoxCoords.Controls.Add(this.labelTaskN);
            this.groupBoxCoords.Controls.Add(this.textBoxObserver);
            this.groupBoxCoords.Controls.Add(this.labelObserver);
            this.groupBoxCoords.Controls.Add(this.textBoxObject);
            this.groupBoxCoords.Controls.Add(this.labelObject);
            this.groupBoxCoords.Controls.Add(this.textBoxDateTime);
            this.groupBoxCoords.Controls.Add(this.labelTimeStart);
            this.groupBoxCoords.Controls.Add(this.textBoxCoords);
            this.groupBoxCoords.Controls.Add(this.labelCoords);
            this.groupBoxCoords.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBoxCoords.Location = new System.Drawing.Point(12, 12);
            this.groupBoxCoords.Name = "groupBoxCoords";
            this.groupBoxCoords.Size = new System.Drawing.Size(433, 219);
            this.groupBoxCoords.TabIndex = 0;
            this.groupBoxCoords.TabStop = false;
            this.groupBoxCoords.Text = "CoordinatesJ2000, Time and Info";
            // 
            // comboBoxObjectType
            // 
            this.comboBoxObjectType.FormattingEnabled = true;
            this.comboBoxObjectType.Items.AddRange(new object[] { "Exoplanet", "Brown dwarf", "White dwarf", "Nova", "Super nova", "Planet", "Asteroid", "Satellite", "Variable star", "Star cluster", "Nebula", "Galaxy", "Alert", "Other" });
            this.comboBoxObjectType.Location = new System.Drawing.Point(6, 181);
            this.comboBoxObjectType.Name = "comboBoxObjectType";
            this.comboBoxObjectType.Size = new System.Drawing.Size(202, 28);
            this.comboBoxObjectType.TabIndex = 31;
            this.comboBoxObjectType.Text = "Exoplanet";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 23);
            this.label1.TabIndex = 9;
            this.label1.Text = "Object type";
            // 
            // labelTaskN
            // 
            this.labelTaskN.Location = new System.Drawing.Point(6, 22);
            this.labelTaskN.Name = "labelTaskN";
            this.labelTaskN.Size = new System.Drawing.Size(140, 23);
            this.labelTaskN.TabIndex = 8;
            this.labelTaskN.Text = "Task №";
            // 
            // textBoxObserver
            // 
            this.textBoxObserver.Location = new System.Drawing.Point(219, 126);
            this.textBoxObserver.Name = "textBoxObserver";
            this.textBoxObserver.Size = new System.Drawing.Size(203, 26);
            this.textBoxObserver.TabIndex = 7;
            // 
            // labelObserver
            // 
            this.labelObserver.Location = new System.Drawing.Point(219, 100);
            this.labelObserver.Name = "labelObserver";
            this.labelObserver.Size = new System.Drawing.Size(116, 23);
            this.labelObserver.TabIndex = 6;
            this.labelObserver.Text = "Observer";
            // 
            // textBoxObject
            // 
            this.textBoxObject.Location = new System.Drawing.Point(6, 126);
            this.textBoxObject.Name = "textBoxObject";
            this.textBoxObject.Size = new System.Drawing.Size(201, 26);
            this.textBoxObject.TabIndex = 5;
            // 
            // labelObject
            // 
            this.labelObject.Location = new System.Drawing.Point(6, 100);
            this.labelObject.Name = "labelObject";
            this.labelObject.Size = new System.Drawing.Size(140, 23);
            this.labelObject.TabIndex = 4;
            this.labelObject.Text = "Object";
            // 
            // textBoxDateTime
            // 
            this.textBoxDateTime.Location = new System.Drawing.Point(221, 71);
            this.textBoxDateTime.Name = "textBoxDateTime";
            this.textBoxDateTime.Size = new System.Drawing.Size(202, 26);
            this.textBoxDateTime.TabIndex = 3;
            // 
            // labelTimeStart
            // 
            this.labelTimeStart.Location = new System.Drawing.Point(221, 45);
            this.labelTimeStart.Name = "labelTimeStart";
            this.labelTimeStart.Size = new System.Drawing.Size(115, 23);
            this.labelTimeStart.TabIndex = 2;
            this.labelTimeStart.Text = "DateTime UTC";
            // 
            // textBoxCoords
            // 
            this.textBoxCoords.Location = new System.Drawing.Point(7, 71);
            this.textBoxCoords.Name = "textBoxCoords";
            this.textBoxCoords.Size = new System.Drawing.Size(201, 26);
            this.textBoxCoords.TabIndex = 1;
            // 
            // labelCoords
            // 
            this.labelCoords.Location = new System.Drawing.Point(8, 45);
            this.labelCoords.Name = "labelCoords";
            this.labelCoords.Size = new System.Drawing.Size(207, 23);
            this.labelCoords.TabIndex = 0;
            this.labelCoords.Text = "Ra (h:m:s.ss) Dec (d:m:s.ss)";
            // 
            // groupBoxFrame
            // 
            this.groupBoxFrame.Controls.Add(this.groupBox3);
            this.groupBoxFrame.Controls.Add(this.groupBox2);
            this.groupBoxFrame.Controls.Add(this.groupBoxFilters);
            this.groupBoxFrame.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBoxFrame.Location = new System.Drawing.Point(12, 237);
            this.groupBoxFrame.Name = "groupBoxFrame";
            this.groupBoxFrame.Size = new System.Drawing.Size(433, 200);
            this.groupBoxFrame.TabIndex = 1;
            this.groupBoxFrame.TabStop = false;
            this.groupBoxFrame.Text = "Frames";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericUpDown_ybin);
            this.groupBox3.Controls.Add(this.numericUpDown_xbin);
            this.groupBox3.Controls.Add(this.labelYbin);
            this.groupBox3.Controls.Add(this.labelXbin);
            this.groupBox3.Location = new System.Drawing.Point(247, 25);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(102, 160);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Binning";
            // 
            // numericUpDown_ybin
            // 
            this.numericUpDown_ybin.InterceptArrowKeys = false;
            this.numericUpDown_ybin.Location = new System.Drawing.Point(11, 117);
            this.numericUpDown_ybin.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            this.numericUpDown_ybin.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDown_ybin.Name = "numericUpDown_ybin";
            this.numericUpDown_ybin.Size = new System.Drawing.Size(78, 26);
            this.numericUpDown_ybin.TabIndex = 36;
            this.numericUpDown_ybin.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // numericUpDown_xbin
            // 
            this.numericUpDown_xbin.InterceptArrowKeys = false;
            this.numericUpDown_xbin.Location = new System.Drawing.Point(11, 49);
            this.numericUpDown_xbin.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            this.numericUpDown_xbin.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDown_xbin.Name = "numericUpDown_xbin";
            this.numericUpDown_xbin.Size = new System.Drawing.Size(78, 26);
            this.numericUpDown_xbin.TabIndex = 35;
            this.numericUpDown_xbin.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // labelYbin
            // 
            this.labelYbin.Location = new System.Drawing.Point(13, 92);
            this.labelYbin.Name = "labelYbin";
            this.labelYbin.Size = new System.Drawing.Size(65, 23);
            this.labelYbin.TabIndex = 34;
            this.labelYbin.Text = "Ybin";
            // 
            // labelXbin
            // 
            this.labelXbin.Location = new System.Drawing.Point(13, 25);
            this.labelXbin.Name = "labelXbin";
            this.labelXbin.Size = new System.Drawing.Size(65, 23);
            this.labelXbin.TabIndex = 32;
            this.labelXbin.Text = "Xbin";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxDuration);
            this.groupBox2.Controls.Add(this.labelDuration);
            this.groupBox2.Controls.Add(this.labelFrameType);
            this.groupBox2.Controls.Add(this.comboBoxFrameType);
            this.groupBox2.Controls.Add(this.comboBoxExp);
            this.groupBox2.Controls.Add(this.labelExp);
            this.groupBox2.Controls.Add(this.textBoxExpN);
            this.groupBox2.Controls.Add(this.labelExpN);
            this.groupBox2.Location = new System.Drawing.Point(8, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(233, 160);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Exposures";
            // 
            // textBoxDuration
            // 
            this.textBoxDuration.Location = new System.Drawing.Point(14, 118);
            this.textBoxDuration.Name = "textBoxDuration";
            this.textBoxDuration.Size = new System.Drawing.Size(93, 26);
            this.textBoxDuration.TabIndex = 30;
            // 
            // labelDuration
            // 
            this.labelDuration.Location = new System.Drawing.Point(14, 91);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(93, 23);
            this.labelDuration.TabIndex = 29;
            this.labelDuration.Text = "Duration (h)";
            // 
            // labelFrameType
            // 
            this.labelFrameType.Location = new System.Drawing.Point(113, 91);
            this.labelFrameType.Name = "labelFrameType";
            this.labelFrameType.Size = new System.Drawing.Size(107, 23);
            this.labelFrameType.TabIndex = 28;
            this.labelFrameType.Text = "Frame Type";
            // 
            // comboBoxFrameType
            // 
            this.comboBoxFrameType.FormattingEnabled = true;
            this.comboBoxFrameType.Items.AddRange(new object[] { "Object", "Dark", "Flat", "Test", "Bias" });
            this.comboBoxFrameType.Location = new System.Drawing.Point(113, 117);
            this.comboBoxFrameType.Name = "comboBoxFrameType";
            this.comboBoxFrameType.Size = new System.Drawing.Size(107, 28);
            this.comboBoxFrameType.TabIndex = 27;
            this.comboBoxFrameType.Text = "Object";
            // 
            // comboBoxExp
            // 
            this.comboBoxExp.FormattingEnabled = true;
            this.comboBoxExp.Items.AddRange(new object[] { "2", "5", "10", "15", "20", "30", "50", "80", "120", "180" });
            this.comboBoxExp.Location = new System.Drawing.Point(113, 49);
            this.comboBoxExp.Name = "comboBoxExp";
            this.comboBoxExp.Size = new System.Drawing.Size(107, 28);
            this.comboBoxExp.TabIndex = 15;
            this.comboBoxExp.Text = "2";
            // 
            // labelExp
            // 
            this.labelExp.Location = new System.Drawing.Point(115, 23);
            this.labelExp.Name = "labelExp";
            this.labelExp.Size = new System.Drawing.Size(92, 23);
            this.labelExp.TabIndex = 14;
            this.labelExp.Text = "In seconds";
            // 
            // textBoxExpN
            // 
            this.textBoxExpN.Location = new System.Drawing.Point(14, 50);
            this.textBoxExpN.Name = "textBoxExpN";
            this.textBoxExpN.Size = new System.Drawing.Size(93, 26);
            this.textBoxExpN.TabIndex = 13;
            this.textBoxExpN.Text = "2";
            // 
            // labelExpN
            // 
            this.labelExpN.Location = new System.Drawing.Point(14, 23);
            this.labelExpN.Name = "labelExpN";
            this.labelExpN.Size = new System.Drawing.Size(65, 23);
            this.labelExpN.TabIndex = 12;
            this.labelExpN.Text = "Number";
            // 
            // groupBoxFilters
            // 
            this.groupBoxFilters.Controls.Add(this.checkBoxFili);
            this.groupBoxFilters.Controls.Add(this.checkBoxFilr);
            this.groupBoxFilters.Controls.Add(this.checkBoxFilg);
            this.groupBoxFilters.Location = new System.Drawing.Point(355, 25);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Size = new System.Drawing.Size(67, 160);
            this.groupBoxFilters.TabIndex = 24;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filters";
            // 
            // checkBoxFili
            // 
            this.checkBoxFili.Checked = true;
            this.checkBoxFili.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFili.Location = new System.Drawing.Point(15, 123);
            this.checkBoxFili.Name = "checkBoxFili";
            this.checkBoxFili.Size = new System.Drawing.Size(43, 23);
            this.checkBoxFili.TabIndex = 29;
            this.checkBoxFili.Text = "i";
            this.checkBoxFili.UseVisualStyleBackColor = true;
            // 
            // checkBoxFilr
            // 
            this.checkBoxFilr.Checked = true;
            this.checkBoxFilr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFilr.Location = new System.Drawing.Point(15, 75);
            this.checkBoxFilr.Name = "checkBoxFilr";
            this.checkBoxFilr.Size = new System.Drawing.Size(43, 23);
            this.checkBoxFilr.TabIndex = 28;
            this.checkBoxFilr.Text = "r";
            this.checkBoxFilr.UseVisualStyleBackColor = true;
            // 
            // checkBoxFilg
            // 
            this.checkBoxFilg.Checked = true;
            this.checkBoxFilg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFilg.Location = new System.Drawing.Point(15, 25);
            this.checkBoxFilg.Name = "checkBoxFilg";
            this.checkBoxFilg.Size = new System.Drawing.Size(43, 32);
            this.checkBoxFilg.TabIndex = 27;
            this.checkBoxFilg.Text = "g";
            this.checkBoxFilg.UseVisualStyleBackColor = true;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(12, 443);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(86, 31);
            this.buttonAdd.TabIndex = 2;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(104, 443);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 31);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(359, 443);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(86, 31);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "Reject";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // TaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 486);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.groupBoxFrame);
            this.Controls.Add(this.groupBoxCoords);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TaskForm";
            this.Text = "TaskForm";
            this.groupBoxCoords.ResumeLayout(false);
            this.groupBoxCoords.PerformLayout();
            this.groupBoxFrame.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ybin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_xbin)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxFilters.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ComboBox comboBoxObjectType;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.NumericUpDown numericUpDown_xbin;
        private System.Windows.Forms.NumericUpDown numericUpDown_ybin;

        private System.Windows.Forms.TextBox textBoxDuration;
        private System.Windows.Forms.Label labelDuration;

        private System.Windows.Forms.GroupBox groupBox3;

        private System.Windows.Forms.Button buttonDelete;

        private System.Windows.Forms.Button buttonCancel;

        private System.Windows.Forms.ComboBox comboBoxFrameType;
        private System.Windows.Forms.Button buttonAdd;

        private System.Windows.Forms.GroupBox groupBox2;

        private System.Windows.Forms.GroupBox groupBoxFilters;

        private System.Windows.Forms.CheckBox checkBoxFilg;
        private System.Windows.Forms.CheckBox checkBoxFilr;
        private System.Windows.Forms.CheckBox checkBoxFili;

        private System.Windows.Forms.Label labelYbin;

        private System.Windows.Forms.Label labelFrameType;

        private System.Windows.Forms.Label labelXbin;

        private System.Windows.Forms.ComboBox comboBoxExp;

        private System.Windows.Forms.Label labelExp;

        private System.Windows.Forms.TextBox textBoxExpN;
        private System.Windows.Forms.Label labelExpN;

        private System.Windows.Forms.Label labelTaskN;

        private System.Windows.Forms.TextBox textBoxObject;
        private System.Windows.Forms.Label labelObject;
        private System.Windows.Forms.TextBox textBoxObserver;
        private System.Windows.Forms.Label labelObserver;

        private System.Windows.Forms.TextBox textBoxDateTime;
        private System.Windows.Forms.Label labelTimeStart;

        private System.Windows.Forms.TextBox textBoxCoords;

        private System.Windows.Forms.Label labelCoords;

        private System.Windows.Forms.GroupBox groupBoxCoords;
        private System.Windows.Forms.GroupBox groupBoxFrame;

        #endregion
    }
}
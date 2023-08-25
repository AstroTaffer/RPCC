using System;
using System.Globalization;
using System.Windows.Forms;

namespace RPCC.Tasks
{
    public partial class TaskForm : Form
    {
        private bool IsNewTask;
        private ObservationTask task;
        private int RowIndex;
        
        public TaskForm(bool isNewTask, int rowIndex = 0)
        {
            InitializeComponent();
            RowIndex = rowIndex;
            textBoxDateTime.Text = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            IsNewTask = isNewTask;
            if(IsNewTask)
            {
                task.TaskNumber = Tasker.GetTasksLen();
                labelTaskN.Text = $@"Task №{task.TaskNumber}";
            }
            else
            {
                
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Are you sure you want to add this task?", @"confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            task.RaDec = textBoxCoords.Text;
            
            task.TimeAdd = DateTime.UtcNow;
            task.TimeStart = DateTime.Parse(textBoxDateTime.Text);
            task.Exp = short.Parse(comboBoxExp.Text);
            task.AllFrames = short.Parse(textBoxExpN.Text);
            task.Duration = task.Exp * task.AllFrames / 60f;
            task.TimeEnd = task.TimeStart.AddHours(task.Duration);
            
            task.Object = textBoxObject.Text;
            task.Observer = textBoxObserver.Text;
            task.Status = 0;
            task.Xbin = short.Parse(textBoxXbin.Text);
            task.XSubframeStart = short.Parse(textBoxXstart.Text);
            task.XSubframeEnd = short.Parse(textBoxXend.Text);
            task.Ybin = short.Parse(textBoxYbin.Text);
            task.YSubframeStart = short.Parse(textBoxYstart.Text);
            task.YSubframeEnd = short.Parse(textBoxYend.Text);
            var fil = "";
            if (checkBoxFilg.Checked) fil += "g";
            if (checkBoxFilr.Checked) fil += "r";
            if (checkBoxFili.Checked) fil += "i";
            task.Filters = fil;
            task.FrameType = comboBoxFrameType.Text;
            Tasker.AddTask(task);
            buttonCancel_Click(sender, e);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (IsNewTask)
            {
                buttonCancel_Click(sender, e);
                return;
            }

            if (MessageBox.Show(@"Are you sure you want to delete this task?", @"confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            Tasker.DeleteTask(RowIndex);
            buttonCancel_Click(sender, e);
        }
    }
}
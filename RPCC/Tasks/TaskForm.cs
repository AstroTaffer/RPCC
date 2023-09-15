using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RPCC.Utils;

namespace RPCC.Tasks
{
    public partial class TaskForm : Form
    {
        private readonly bool _isNewTask;
        private readonly int _rowIndex;
        private readonly ObservationTask _task = new ObservationTask();

        public TaskForm(bool isNewTask, int rowIndex = 0)
        {
            InitializeComponent();
            _rowIndex = rowIndex;
            textBoxDateTime.Text = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            _isNewTask = isNewTask;
            if (_isNewTask)
            {
                _task.TaskNumber = Tasker.GetTasksLen();
            }
            else
            {
                _task = Tasker.GetTaskByRowIndex(_rowIndex);

                textBoxCoords.Text = _task.RaDec;
                textBoxObject.Text = _task.Object;
                textBoxObserver.Text = _task.Observer;
                textBoxXbin.Text = _task.Xbin.ToString();
                textBoxYbin.Text = _task.Ybin.ToString();
                textBoxDateTime.Text = _task.TimeStart.ToString(CultureInfo.CurrentCulture);
                textBoxExpN.Text = _task.AllFrames.ToString(CultureInfo.CurrentCulture);

                comboBoxExp.Text = _task.Exp.ToString(CultureInfo.CurrentCulture);
                textBoxDuration.Text = _task.Duration.ToString(CultureInfo.CurrentCulture);
                comboBoxFrameType.Text = _task.FrameType;
                var s = _task.Filters.Split(' ');
                if (s.Contains("g")) checkBoxFilg.Checked = true;
                if (s.Contains("r")) checkBoxFilr.Checked = true;
                if (s.Contains("i")) checkBoxFili.Checked = true;
            }

            labelTaskN.Text = $@"Task №{_task.TaskNumber}";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Are you sure you want to add this task?", @"confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            AddTask(sender, e);
        }

        private void AddTask(object sender, EventArgs e)
        {
            var fil = "";
            if (checkBoxFilg.Checked) fil += "g ";
            if (checkBoxFilr.Checked) fil += "r ";
            if (checkBoxFili.Checked) fil += "i";

            if (textBoxCoords.Text == "" || textBoxDateTime.Text == "" || comboBoxExp.Text == "" ||
                comboBoxFrameType.Text == "" || fil == "")
            {
                MessageBox.Show(@"Blank data, can't add task", @"OK", MessageBoxButtons.OK);
                return;
            }

            _task.ComputeRaDec(textBoxCoords.Text);
            _task.TimeAdd = DateTime.UtcNow;
            _task.TimeStart = DateTime.Parse(textBoxDateTime.Text);
//todo libsofa
            // if (!CoordinatesManager.CheckElevateLimit(_task.Ra, _task.Dec, _task.TimeStart))
            // {
            //     MessageBox.Show(@"Target under elevation limit", @"OK", MessageBoxButtons.OK);
            //     return;
            // }
            
            _task.Exp = short.Parse(comboBoxExp.Text);

            if (textBoxDuration.Text == "")
            {
                _task.AllFrames = short.Parse(textBoxExpN.Text);
                _task.Duration = _task.Exp * _task.AllFrames / 60f / 60 + 0.05f;
            }
            else
            {
                _task.Duration = float.Parse(textBoxDuration.Text);
                _task.AllFrames = Convert.ToInt16(_task.Duration * 60f * 60f / _task.Exp);
            }

            _task.TimeEnd = _task.TimeStart.AddHours(_task.Duration);
            _task.Object = textBoxObject.Text;
            _task.Observer = textBoxObserver.Text;
            _task.Status = 0;
            _task.Xbin = short.Parse(textBoxXbin.Text);
            _task.Ybin = short.Parse(textBoxYbin.Text);

            _task.Filters = fil;
            _task.FrameType = comboBoxFrameType.Text;

            if (!_isNewTask) Tasker.DeleteTaskByRowIndex(_rowIndex);
            Tasker.AddTask(_task);
            buttonCancel_Click(sender, e);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Tasker.PaintTable();
            Close();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (_isNewTask || _task.Status > 1)
            {
                buttonCancel_Click(sender, e);
                return;
            }

            if (MessageBox.Show(@"Are you sure you want to reject this task?", @"confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            _task.Status = 3;
            Tasker.UpdateTaskInTable(_task);
            buttonCancel_Click(sender, e);
        }
    }
}
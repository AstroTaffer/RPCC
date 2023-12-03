using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RPCC.Comms;
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
                // _task.TaskNumber = Tasker.GetTasksLen();
                textBoxCoords.Text = $@"{ASCOM.Tools.Utilities.HoursToHMS(MountDataCollector.RightAsc)} {ASCOM.Tools.Utilities.DegreesToDMS(MountDataCollector.Declination)}";
            }
            else
            {
                try
                {
                    _task = Tasker.GetTaskByRowIndex(_rowIndex);
                }
                catch
                {
                    Tasker.DeleteTaskByRowIndex(_rowIndex);
                    Tasker.PaintTable();
                    Close();
                    return;
                }
                
                textBoxCoords.Text = _task.RaDec;
                textBoxObject.Text = _task.Object;
                textBoxObserver.Text = _task.Observer;
                numericUpDown_xbin.Value = _task.Xbin;
                numericUpDown_ybin.Value = _task.Ybin;
                textBoxDateTime.Text = _task.TimeStart.ToString(CultureInfo.CurrentCulture);
                textBoxExpN.Text = _task.AllFrames.ToString(CultureInfo.CurrentCulture);

                comboBoxExp.Text = _task.Exp.ToString(CultureInfo.CurrentCulture);
                textBoxDuration.Text = _task.Duration.ToString(CultureInfo.CurrentCulture);
                comboBoxFrameType.Text = _task.FrameType;
                comboBoxObjectType.Text = _task.ObjectType;
                var s = _task.Filters.Split(' ');
                if (s.Contains("g")) checkBoxFilg.Checked = true;
                if (s.Contains("r")) checkBoxFilr.Checked = true;
                if (s.Contains("i")) checkBoxFili.Checked = true;
            }

            labelTaskN.Text = $@"Task №{_task.TaskNumber}";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_task.Status > 0)
            {
                MessageBox.Show(@"Can't add task", @"OK", MessageBoxButtons.OK);
                buttonCancel_Click(sender, e);
                return;
            }
            if (MessageBox.Show(@"Are you sure you want to add this task?", @"confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            AddTask(sender, e);
        }

        private void AddTask(object sender, EventArgs e)
        {
            _task.FrameType = comboBoxFrameType.Text;
            var fil = "";
            if (checkBoxFilg.Checked) fil += "g ";
            if (checkBoxFilr.Checked) fil += "r ";
            if (checkBoxFili.Checked) fil += "i";

            if ((textBoxCoords.Text == "" & _task.FrameType == Head.Light) 
                || textBoxDateTime.Text == "" || comboBoxExp.Text == "" ||
                comboBoxFrameType.Text == "" || fil == "")
            {
                MessageBox.Show(@"Blank data, can't add task", @"OK", MessageBoxButtons.OK);
                return;
            }

            try
            {
                _task.TimeAdd = DateTime.UtcNow;
                _task.TimeStart = DateTime.Parse(textBoxDateTime.Text);
                
                if (_task.FrameType == Head.Light || _task.FrameType == Head.Test)
                {
                    _task.ComputeRaDec(textBoxCoords.Text);
                    if (!CoordinatesManager.CheckElevateLimit(_task.Ra, _task.Dec, _task.TimeStart))
                    {
                        MessageBox.Show(@"Target under elevation limit", @"OK", MessageBoxButtons.OK);
                        return;
                    }
                }
                
                _task.Exp = short.Parse(comboBoxExp.Text);

                if (textBoxDuration.Text == "")
                {
                    _task.AllFrames = short.Parse(textBoxExpN.Text);
                    _task.Duration = (float) Math.Round(_task.Exp * _task.AllFrames / 60f / 60 + 0.05f, 2);
                }
                else
                {
                    if (float.TryParse(textBoxDuration.Text, out var buf))
                    {
                        _task.Duration = buf;
                        _task.AllFrames = Convert.ToInt16(_task.Duration * 60f * 60f / _task.Exp);
                    }
                    else
                    {
                        MessageBox.Show(@"Can't calc duration", @"OK", MessageBoxButtons.OK);
                        return;
                    }

                }

                _task.TimeEnd = _task.TimeStart + TimeSpan.FromHours(_task.Duration);
                _task.Object = textBoxObject.Text;
                _task.Observer = textBoxObserver.Text;
                _task.ObjectType = comboBoxObjectType.Text;
                _task.Status = 0;

                _task.Xbin = (int)numericUpDown_xbin.Value;
                _task.Ybin = (int)numericUpDown_ybin.Value;

                _task.Filters = fil;
                
                if (!_isNewTask)
                {
                    DbCommunicate.AddTaskToDb(_task);
                    Tasker.UpdateTaskInTable(_task);
                }
                else
                {
                    DbCommunicate.UpdateTaskInDb(_task);
                    Tasker.AddTask(_task);
                }
            }
            catch
            {
                Logger.AddLogEntry("Can't add task");
            }

            buttonCancel_Click(sender, e);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Tasker.PaintTable();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            
            MainForm.IsTaskFormOpen = false;
            base.OnClosed(e);
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
            DbCommunicate.UpdateTaskInDb(_task);
            Tasker.UpdateTaskInTable(_task);
            Logger.AddLogEntry($"Task #{_task.TaskNumber} rejected");
            buttonCancel_Click(sender, e);
        }
    }
}
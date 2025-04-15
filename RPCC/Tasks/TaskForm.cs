using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Utils;

namespace RPCC.Tasks;

public partial class TaskForm : Form
{
    private readonly bool _isNewTask;
    private readonly int _rowIndex;
    private readonly ObservationTask _task = new();

    public TaskForm(bool isNewTask, int rowIndex = 0, ObservationTask newTask = null)
    {   
        InitializeComponent();
        _rowIndex = rowIndex;
        textBoxDateTime.Text = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        _isNewTask = isNewTask;
        if (_isNewTask)
        {
            if (newTask is not null)
            {
                _task = newTask;
                SetLabels();
            }
            else
            {
                textBoxCoords.Text = $@"{ASCOM.Tools.Utilities.HoursToHMS(MountDataCollector.RightAsc)} {ASCOM.Tools.Utilities.DegreesToDMS(MountDataCollector.Declination)}";

            }
        }
        else
        {
            try
            {
                var row = Tasker.DataGridViewTasker.Rows[rowIndex];
                // _task = Tasker.GetTaskByRowIndex(_rowIndex);
                _task = DbCommunicate.GetTaskFromDb(Convert.ToInt32(row.Cells["N"].Value));
            }
            catch (Exception e)
            {
                Logger.AddError("get task by row index", e, CameraControl.cams[0]);
                // Tasker.DeleteTaskByRowIndex(_rowIndex);
                // Tasker.PaintTable();
                Close();
                return;
            }
                
            SetLabels();
        }

        labelTaskN.Text = $@"Task №{_task.TaskNumber}";
    }

    private void SetLabels()
    {
        textBoxCoords.Text = _task.RaDec;
        textBoxObject.Text = _task.Object ?? "";
        textBoxObserver.Text = _task.Observer ?? "";
        textBoxDateTime.Text = _task.TimeStart.ToString(CultureInfo.CurrentCulture);
        textBoxExpN.Text = _task.AllFrames.ToString(CultureInfo.CurrentCulture);
        comboBoxExp.Text = _task.Exp.ToString(CultureInfo.CurrentCulture);
        textBoxDuration.Text = _task.Duration.ToString(CultureInfo.CurrentCulture);
        comboBoxFrameType.Text = _task.FrameType ?? "";
        comboBoxObjectType.Text = _task.ObjectType ?? "";

        // Безопасное отображение координат и времён повторных точек
        textBoxCoordsSSObjects.Text = string.Join("\n",
            (_task.RepointCoords ?? new List<string>())
            .Select(c => string.IsNullOrWhiteSpace(c) ? "" : c));

        textBoxDateTimeSSObjects.Text = string.Join("\n",
            (_task.RepointTimes ?? new List<DateTime>())
            .Select(t => t.ToString(CultureInfo.CurrentCulture)));

        // Парсинг фильтров
        var s = (_task.Filters ?? "").Split(' ');
        checkBoxFilg.Checked = s.Contains(StringHolder.FilG);
        checkBoxFilV.Checked = s.Contains(StringHolder.FilV);
        checkBoxFilr.Checked = s.Contains(StringHolder.FilR);
        checkBoxFili.Checked = s.Contains(StringHolder.FilI);
    }


    private void buttonAdd_Click(object sender, EventArgs e)
    {
        if (_task.Status > 0)
        {
            MessageBox.Show(@"Can't add task, status > 0", @"OK", MessageBoxButtons.OK);
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
        var fil = $"{StringHolder.FilG} {StringHolder.FilV} {StringHolder.FilR} {StringHolder.FilI}";

        string[] validFrameTypes =
        [StringHolder.Light, StringHolder.Dark, 
            StringHolder.Flat, StringHolder.Focus, StringHolder.Test];
        if (!Array.Exists(validFrameTypes, element => element == _task.FrameType))
        {
            Logger.AddLogEntry($"TASKFORM WARNING: Unknown frame type {_task.FrameType}");
            return;
        }
        
        if (textBoxCoords.Text == "" & _task.FrameType == StringHolder.Light
            || textBoxDateTime.Text == "" || comboBoxExp.Text == "" ||
            comboBoxFrameType.Text == "") //|| fil == ""
        {
            MessageBox.Show(@"Blank data, can't add task", @"OK", MessageBoxButtons.OK);
            return;
        }

        try
        {
            _task.TimeAdd = DateTime.UtcNow;
            _task.TimeStart = DateTime.Parse(textBoxDateTime.Text);
                
            if (_task.FrameType is StringHolder.Light or StringHolder.Test)
            {
                _task.ComputeRaDec(textBoxCoords.Text);
                if (!CoordinatesManager.CheckElevateLimit(_task.Ra, _task.Dec, _task.TimeStart))
                {
                    MessageBox.Show(@"Target under elevation limit", @"OK", MessageBoxButtons.OK);
                    return;
                }

                var times = textBoxDateTimeSSObjects.Text
                    .Replace("\r\n", "\n")
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                var coors = textBoxCoordsSSObjects.Text
                    .Replace("\r\n", "\n")
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                if (times.Count > 0 && !string.IsNullOrWhiteSpace(times[0]))
                {
                    if (times.Count == coors.Count)
                    {
                        _task.RepointCoords = coors;
                        var parsedTimes = new List<DateTime>();
                        for (int i = 0; i < times.Count; i++)
                        {
                            if (!DateTime.TryParse(times[i], out var dt))
                            {
                                MessageBox.Show(
                                    $"Ошибка в строке {i + 1} списка времён:\n\"{times[i]}\"\n\nУбедитесь, что формат корректный (например: 2025-04-12 18:30)",
                                    "Неверный формат времени", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            parsedTimes.Add(dt);
                        }
                        _task.RepointTimes = parsedTimes;

                    }
                    else
                    {
                        MessageBox.Show(@"Length of repoint coords not equal length of repoint times", 
                            @"OK", MessageBoxButtons.OK);
                        return;
                    }
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

            // _task.Xbin = (int)numericUpDown_xbin.Value;
            // _task.Ybin = (int)numericUpDown_ybin.Value;

            _task.Filters = fil;
                
            if (!_isNewTask)
            {
                DbCommunicate.UpdateTaskInDb(_task);
                // Tasker.UpdateTaskInTable(_task);
            }
            else
            {
                DbCommunicate.AddTaskToDb(_task);
                // Tasker.AddTask(_task);
            }
        }
        catch(Exception exception)
        {
            Logger.AddLogEntry("Can't add task");
            Logger.AddLogEntry(exception.Message);
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
            
        MainForm.isTaskFormOpen = false;
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
        // Tasker.UpdateTaskInTable(_task);
        Logger.AddLogEntry($"Task #{_task.TaskNumber} rejected");
        buttonCancel_Click(sender, e);
    }

    private void buttonCopy_Click(object sender, EventArgs e)
    {
        var taskForm = new TaskForm(true, _rowIndex);
        taskForm.textBoxCoords.Text = textBoxCoords.Text;
        taskForm.textBoxObserver.Text = textBoxObserver.Text;
        taskForm.comboBoxObjectType.Text = comboBoxObjectType.Text;
        taskForm.textBoxObject.Text = textBoxObject.Text;
        taskForm.comboBoxExp.Text = comboBoxExp.Text;
        taskForm.comboBoxFrameType.Text = comboBoxFrameType.Text;
        


        // taskForm.numericUpDown_xbin.Value = (int)numericUpDown_xbin.Value;
        // taskForm.numericUpDown_ybin.Value = (int)numericUpDown_ybin.Value;
            
        taskForm.Show();
    }
}
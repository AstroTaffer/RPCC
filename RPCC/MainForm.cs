using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using Npgsql;
using RPCC.Cams;
using RPCC.Focus;
using RPCC.Utils;
using RPCC.Comms;
using RPCC.Tasks;

namespace RPCC;

public partial class MainForm : Form
{
    /// <summary>
    ///     Логика работы основной формы программы
    ///     Обработка команд пользователя с помощью вызова готовых функций
    /// </summary>

    private static readonly System.Timers.Timer FocusTimer = new();

    public static bool isTaskFormOpen;

    #region General
    [Obsolete("Obsolete")]
    public MainForm()
    {
        InitializeComponent();
    
        // _isFirstLoad = true;

        Logger.LogBox = listBoxLogs;
        Logger.AddLogEntry("Application launched");

        Settings.LoadXmlConfig("SettingsDefault.xml");

        // Camera controls
        CameraControl.resetUi = ResetCamsUi;
        CameraControl.resetPics = RefreshImages;
            
        // MeteoDome connect
        WeatherSocket.Connect();

        // SiTechExe connect
        SiTechExeSocket.Connect();

        // Create timer for focus loop
        FocusTimer.Elapsed += OnTimedEvent_Clock;
        FocusTimer.Interval = 1000;
            
        // Focus connect
        // HACK: For the love of god stop exiting the program when something is not connected!
        // Call FindFocusToolStripMenuItem_Click
        if (!SerialFocus.Init())
        {
            MessageBox.Show(@"Can't open Focus serial port", @"OK", MessageBoxButtons.OK);
            Logger.AddLogEntry(@"Can't open Focus serial port");
        }
        groupBoxFocusSettings.Text = $@"Focus Settings (COMPORT {Settings.FocusComId})";
            
        Tasker.DataGridViewTasker = dataGridViewTasker;
        Tasker.ContextMenuStripTasker = contextMenuStripTasker;
        Tasker.SetHeader();
            
        Fli.SetDebugLevel(Logger.LogPath + "\\FLIdebug.log", Fli.DEBUG.ALL);
            
        // Donuts connect
        DonutsSocket.Connect();
            
        progressBarG.Style = ProgressBarStyle.Continuous;
        progressBarR.Style = ProgressBarStyle.Continuous;
        progressBarI.Style = ProgressBarStyle.Continuous;
            
        FocusTimer.Start();
        timerUi.Start();
        Head.StartThinking();
        if (checkBoxHead.Checked)
        {
            Head.isThinking = true;
            Head.ThinkingTimer.Start();
        }
        if (checkBoxAutoFocus.Checked)
        {
            CameraFocus.IsAutoFocus = true;
        }

        if (checkBoxDebugMode.Checked)
        {
            Logger.DebugMode = true;
        }

        NpgsqlConnection.GlobalTypeMapper.MapComposite<Spoint>("public.spoint");
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Logger.SaveLogs();
        timerUi.Stop();
            
        // Tasker.SaveTasksToXml();
        // DbCommunicate.DisconnectFromDb();
            
        WeatherSocket.Disconnect();
        DonutsSocket.Disconnect();
        SiTechExeSocket.Disconnect();

        CameraControl.DisconnectCameras();

        SerialFocus.Close_Port();
        CameraFocus.DeFocus = 0;
        CameraFocus.IsZenith = false;
        labelFocusPos.Dispose();
        FocusTimer.Dispose();
    }

    private void TimerUiUpdate(object sender, EventArgs e)
    {
        tSStatusClock.Text = @"UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss");
        checkBoxHead.Checked = Head.isThinking;
        checkBoxGuiding.Checked = Head.isGuid;
        try
        {
            switch (CameraControl.cams.Count)
            {
                case 3:
                    labelCam3CcdTemp.Text = @$"CCD Temp: {CameraControl.cams[2].CcdTemp:F3}";
                    labelCam3BaseTemp.Text = @$"Base Temp: {CameraControl.cams[2].BaseTemp:F3}";
                    labelCam3CoolerPwr.Text = @$"Cooler Power: {CameraControl.cams[2].CoolerPwr} %";
                    labelCam3Status.Text = @$"Status: {CameraControl.cams[2].Status}";
                    labelCam3RemTime.Text = @$"Remaining: {CameraControl.cams[2].RemTime / 1000}";
                    SetProgress(2);
                    goto case 2;
                case 2:
                    labelCam2CcdTemp.Text = @$"CCD Temp: {CameraControl.cams[1].CcdTemp:F3}";
                    labelCam2BaseTemp.Text = @$"Base Temp: {CameraControl.cams[1].BaseTemp:F3}";
                    labelCam2CoolerPwr.Text = @$"Cooler Power: {CameraControl.cams[1].CoolerPwr} %";
                    labelCam2Status.Text = @$"Status: {CameraControl.cams[1].Status}";
                    labelCam2RemTime.Text = @$"Remaining: {CameraControl.cams[1].RemTime / 1000}";
                    SetProgress(1);
                    goto case 1;
                case 1:
                    labelCam1CcdTemp.Text = @$"CCD Temp: {CameraControl.cams[0].CcdTemp:F3}";
                    labelCam1BaseTemp.Text = @$"Base Temp: {CameraControl.cams[0].BaseTemp:F3}";
                    labelCam1CoolerPwr.Text = @$"Cooler Power: {CameraControl.cams[0].CoolerPwr} %";
                    labelCam1Status.Text = @$"Status: {CameraControl.cams[0].Status}";
                    labelCam1RemTime.Text = @$"Remaining: {CameraControl.cams[0].RemTime / 1000}";
                    SetProgress(0);
                    break;
            }
        }
        catch (IndexOutOfRangeException)
        {
            Logger.AddLogEntry("WARNING Cameras list has been reset while updating GUI");
            // If the cameras have been disconnected when we were updating the labels,
            // IndexOutOfRangeException will be raised and silenced. It's not the best solution,
            // but Monitor.Enter will stop the GUI thread and Monitor.TryEnter may cause some
            // loops to be skipped if GUI timer and Cams timer would elapse at the same time.
            // Though I think it's really unlikely. Use Monitor.TryEnter in case of bugs.
        }
    }

    private void ResetCamsUi()
    {
        // Camera 1
        groupBoxImage1.Invoke((MethodInvoker)delegate
        {
            groupBoxCam1.Enabled = false;
            groupBoxImage1.Enabled = false;
            pictureBoxImage1.Image = null;
            labelCam1Model.Text = @"Model:";
            labelCam1Sn.Text = @"Serial Num:";
            labelCam1Filter.Text = @"Filter:";
            labelCam1CcdTemp.Text = @"CCD Temp:";
            labelCam1BaseTemp.Text = @"Base Temp:";
            labelCam1CoolerPwr.Text = @"Cooler Power:";
            labelCam1Status.Text = @"Status:";
            labelCam1RemTime.Text = @"Remaining:";

            // Camera 2
            groupBoxCam2.Enabled = false;
            groupBoxImage2.Enabled = false;
            pictureBoxImage2.Image = null;
            labelCam2Model.Text = @"Model:";
            labelCam2Sn.Text = @"Serial Num:";
            labelCam2Filter.Text = @"Filter:";
            labelCam2CcdTemp.Text = @"CCD Temp:";
            labelCam2BaseTemp.Text = @"Base Temp:";
            labelCam2CoolerPwr.Text = @"Cooler Power:";
            labelCam2Status.Text = @"Status:";
            labelCam2RemTime.Text = @"Remaining:";

            // Camera 3
            groupBoxCam3.Enabled = false;
            groupBoxImage3.Enabled = false;
            pictureBoxImage3.Image = null;
            labelCam3Model.Text = @"Model:";
            labelCam3Sn.Text = @"Serial Num:";
            labelCam3Filter.Text = @"Filter:";
            labelCam3CcdTemp.Text = @"CCD Temp:";
            labelCam3BaseTemp.Text = @"Base Temp:";
            labelCam3CoolerPwr.Text = @"Cooler Power:";
            labelCam3Status.Text = @"Status:";
            labelCam3RemTime.Text = @"Remaining:";
        });
            
        switch (CameraControl.cams.Count)
        {
            case 3:
                groupBoxCam3.Invoke((MethodInvoker) delegate
                {
                    groupBoxCam3.Enabled = true;
                    groupBoxImage3.Enabled = true;
                    labelCam3Model.Text = @$"Model: {CameraControl.cams[2].ModelName}";
                    labelCam3Sn.Text = @$"Serial Num: {CameraControl.cams[2].SerialNumber}";
                    labelCam3Filter.Text = @$"Filter: {CameraControl.cams[2].Filter}"; 
                });
                goto case 2;
            case 2:
                groupBoxCam2.Invoke((MethodInvoker) delegate
                {
                    groupBoxCam2.Enabled = true;
                    groupBoxImage2.Enabled = true;
                    labelCam2Model.Text = @$"Model: {CameraControl.cams[1].ModelName}";
                    labelCam2Sn.Text = @$"Serial Num: {CameraControl.cams[1].SerialNumber}";
                    labelCam2Filter.Text = @$"Filter: {CameraControl.cams[1].Filter}";
                });
                goto case 1;
            case 1:
                groupBoxCam1.Invoke((MethodInvoker) delegate
                {
                    groupBoxCam1.Enabled = true;
                    groupBoxImage1.Enabled = true;
                    labelCam1Model.Text = @$"Model: {CameraControl.cams[0].ModelName}";
                    labelCam1Sn.Text = @$"Serial Num: {CameraControl.cams[0].SerialNumber}";
                    labelCam1Filter.Text = @$"Filter: {CameraControl.cams[0].Filter}";
                });
                break;
        }
    }

    private void ButtonSurveyStop_Click(object sender, EventArgs e)
    {
        //_cameraControl.CancelSurvey();
        //Logger.AddLogEntry($"Survey cancelled, {_cameraControl.task.framesNum} {(_cameraControl.task.framesNum == 1 ? "frame" : "frames")} skipped");
        //_cameraControl.task.framesNum = 1;
        //numericUpDownSequence.Value = 1;

        //buttonSurveyStart.Enabled = true;
        //comboBoxImgType.Enabled = true;
        //numericUpDownSequence.Enabled = true;
        //numericUpDownExpTime.Enabled = true;
        //updateCamerasSettingsToolStripMenuItem.Enabled = true;
    }
    #endregion

    #region Launch Menu
    private void FindCamerasToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CameraControl.ReconnectCameras();
    }

    private void FindFocusToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // HACK: Can serial port be opened again if it has been closed? Check and implement properly
        //       This function must work properly when called more than once
        //       Watch out! SerialFocus.Init() keeps adding functions on ComTimer.Elapsed every time it is called!

        //SerialFocus.Close_Port();
        //FocusTimer.Elapsed -= OnTimedEvent_Clock;
        //SerialFocus.Init();
    }

    private void ReconnectMeteoDomeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (WeatherSocket.IsConnected) WeatherSocket.Disconnect();
        WeatherSocket.Connect();
    }

    private void ReconnectDonutsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // if(_donutsSocket.isConnected) _donutsSocket.Disconnect();
        DonutsSocket.Connect();
    }

    private void ReconnectSiTechExeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (SiTechExeSocket.IsConnected) SiTechExeSocket.Disconnect();
        SiTechExeSocket.Connect();
    }

    private void ReconnectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ReconnectMeteoDomeToolStripMenuItem_Click(sender, e);
        ReconnectDonutsToolStripMenuItem_Click(sender, e);
        ReconnectSiTechExeToolStripMenuItem_Click(sender, e);
    }
    #endregion

    #region Logs
    private void ListBoxLogs_DoubleClick(object sender, EventArgs e)
    {
        // TODO: Nice idea, but nothing points to this feature. Implement in a less obscure way.
        Logger.CopyLogItem();
    }

    private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // Clear log window
        Logger.ClearLogs();
        Logger.AddLogEntry("Logs have been cleaned");
    }

    private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // Save logs in file
        Logger.SaveLogs();
        Logger.AddLogEntry("Logs have been saved");
    }
    #endregion

    #region Camera Images
    private void RefreshImages(ICameraDevice camera)    
    {   
        switch (camera.Filter)
        {
            case StringHolder.FilI:
                pictureBoxImage3.Image = null;
                if (CameraControl.cams[2].LatestImageBitmap != null)
                    groupBoxImage3.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxImage3.Image = CameraControl.cams[2].LatestImageBitmap;
                    });
                break;
            // goto case 2;
            case StringHolder.FilR:
                pictureBoxImage2.Image = null;
                if (CameraControl.cams[1].LatestImageBitmap != null)
                    groupBoxImage2.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxImage2.Image = CameraControl.cams[1].LatestImageBitmap;
                    });
                break;
            // goto case 1;
            case StringHolder.FilG :
            case  StringHolder.FilV:
                pictureBoxImage1.Image = null;
                if (CameraControl.cams[0].LatestImageBitmap != null)
                    groupBoxImage1.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxImage1.Image = CameraControl.cams[0].LatestImageBitmap;
                    });
                break;
        }
    }
        
    #endregion

    #region Options
    private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var settingsForm = new SettingsForm();
        settingsForm.ShowDialog();
        if (settingsForm.DialogResult == DialogResult.OK) Logger.AddLogEntry("Settings changed");
    }

    private void UpdateCameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //_cameraControl.UpdateSettings();
    }

    private void LoadConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (openFileDialogConfig.ShowDialog() == DialogResult.OK)
            Settings.LoadXmlConfig(openFileDialogConfig.FileName);
    }

    private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (saveFileDialogConfig.ShowDialog() == DialogResult.OK)
            Settings.SaveXmlConfig(saveFileDialogConfig.FileName);
    }
    #endregion

    #region Debug Menu
    private void RestoreDefaultConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.RestoreDefaultXmlConfig();
    }
    #endregion

    #region Focus
    private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
    {
        GetData();
        // var getFocus = new Thread(GetData);
        // getFocus.Start();
    }
        
    private void GetData()
    {
        const int waitTime = 50;
        SerialFocus.UpdateData();
        Thread.Sleep(waitTime);
        try
        {
            labelFocusPos.Invoke((MethodInvoker) delegate
            {
                labelFocusPos.Text = $@"Focus position: {SerialFocus.CurrentPosition}";
            });

            labelEndSwitch.Text = @"Endswitch: " + (SerialFocus.Switches[6] ? "joint" : "unjoint");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
        
    private void CheckBoxAutoFocus_CheckedChanged(object sender, EventArgs e)
    {
        var isAutoFocusEnabled = checkBoxAutoFocus.Checked;

        //Settings
        buttonRun.Enabled = !isAutoFocusEnabled;
        buttonSetZeroPos.Enabled = !isAutoFocusEnabled;
        numericUpDownRun.Enabled = !isAutoFocusEnabled;

        //AutoFocus
        numericUpDownSetDefoc.Enabled = isAutoFocusEnabled;
        checkBoxGoZenith.Enabled = isAutoFocusEnabled;
        CameraFocus.IsAutoFocus = isAutoFocusEnabled;
    }

    private void NumericUpDownSetDefoc_ValueChanged(object sender, EventArgs e)
    {
        CameraFocus.DeFocus = (int)numericUpDownSetDefoc.Value;
    }

    private void ButtonSetZeroPos_Click(object sender, EventArgs e)
    {
        SerialFocus.Set_Zero();
    }

    private void ButtonRunStop_Click(object sender, EventArgs e)
    {
        SerialFocus.Stop();
    }

    private void ButtonRun_Click(object sender, EventArgs e)
    {
        if (radioButtonRunFast.Checked) SerialFocus.FRun_To((int)numericUpDownRun.Value);
        else if (radioButtonRunSlow.Checked) SerialFocus.SRun_To((int)numericUpDownRun.Value);
    }

    private void CheckBoxGoZenith_CheckedChanged(object sender, EventArgs e)
    {
        CameraFocus.IsZenith = checkBoxGoZenith.Checked;
    }
    #endregion

    #region Tasker
    private void AddToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // Logger.AddLogEntry("Add Task click");
        if (!isTaskFormOpen)
        {
            isTaskFormOpen = true;
            var taskForm = new TaskForm(true);
            taskForm.Show(); 
        }
    }

    private void DataGridViewTasker_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        // MessageBox.Show(e.RowIndex.ToString());
        if (e.RowIndex == -1) return;
        var taskForm = new TaskForm(false, e.RowIndex);
        taskForm?.Show();
    }

    private void CheckBoxHead_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBoxHead.Checked) Head.ThinkingTimer.Start();
        else Head.ThinkingTimer.Stop();

        Head.isThinking = checkBoxHead.Checked;
    }

    #endregion
        
    private void checkBoxGuiding_CheckedChanged(object sender, EventArgs e)
    {
        Head.isGuid = checkBoxGuiding.Checked;
    }

    private void SetProgress(int indx)
    {
        var value = 0;
        if (CameraControl.cams[indx].IsExposing)
        {
            if (Head.currentTask is null) return;
            value = 100 - CameraControl.cams[indx].RemTime / 10 / Head.currentTask.Exp;
            if (value < 0)
            {
                value = 0;
            }
        }
        switch (CameraControl.cams[indx].Filter)
        {
            case StringHolder.FilG:
            case StringHolder.FilV:
                progressBarG.Value = value;
                break;
            case StringHolder.FilR:
                progressBarR.Value = value;
                break;
            case StringHolder.FilI:
                progressBarI.Value = value;
                break;
        }
    }

    private void checkBoxDebugMode_CheckedChanged(object sender, EventArgs e)
    {
        Logger.DebugMode = checkBoxDebugMode.Checked;
    }
}

public static class StringHolder
{
    public const string FilG = "g";
    public const string FilV = "V";
    public const string FilR = "r";
    public const string FilI = "i";
    public const string Dark = "Dark";
    public const string Flat = "Flat";
    public const string Light = "Object";
    public const string Test = "Test";
    public const string Focus = "Focus";
    public const string AutoDark = "AUTO_DARK";
    public const string AutoFlat = "AUTO_FLAT";
    public const string Idle = "IDLE";
    public const string Wft = "WAITING FOR TRIGGER";
    public const string Exposing = "EXPOSING";
    public const string Reading = "READING CCD";
    public const string Error = "ERROR";
    public const string Unknown = "UNKNOWN";
}
using System;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using Newtonsoft.Json.Linq;
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
    public static MainForm Instance { get; private set; }
    public static bool isTaskFormOpen;

    #region General
    [Obsolete("Obsolete")]
    public MainForm()
    {
        InitializeComponent();
        Instance = this;
        // _isFirstLoad = true;

        Logger.LogBox = listBoxLogs;
        Logger.AddLogEntry("Application launched");

        Settings.LoadXmlConfig();

        // Camera controls
        // CameraControl.resetUi = ResetCamsUi;
            
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
            
        Fli.SetDebugLevel($"{Settings.MainOutFolder}\\LOGS\\RPCC_LOGS\\FLIdebug.log", Fli.DEBUG.ALL);
            
        // Donuts connect
        // DonutsSocket.Connect();
            
        progressBarG.Style = ProgressBarStyle.Continuous;
        progressBarR.Style = ProgressBarStyle.Continuous;
        progressBarI.Style = ProgressBarStyle.Continuous;
            
        FocusTimer.Start();
        timerUi.Start();
        Head.StartThinking();
        if (checkBoxHead.Checked)
        {
            Head.IsThinking = true;
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
            
        WeatherSocket.Disconnect();
        // DonutsSocket.Disconnect();
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
        checkBoxHead.Checked = Head.IsThinking;
        checkBoxGuiding.Checked = Head.IsGuid;
        checkBoxAutoFocus.Checked = CameraFocus.IsAutoFocus;
        foreach (var cam in CameraControl.cams)
        {

            cam.UpdateProgressBar();
            // cam.UpdatePreview();
            cam.UpdateUi();
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
        // DonutsSocket.Connect();
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
    // private void RefreshImages(ICameraDevice camera)    
    // {   
    //     switch (camera.Filter)
    //     {
    //         case StringHolder.FilI:
    //             pictureBoxImage3.Image = null;
    //             if (camera.LatestImageBitmap != null)
    //                 groupBoxCam3.Invoke((MethodInvoker)delegate
    //                 {
    //                     pictureBoxImage3.Image = camera.LatestImageBitmap;
    //                 });
    //             break;
    //         // goto case 2;
    //         case StringHolder.FilR:
    //             pictureBoxImage2.Image = null;
    //             if (camera.LatestImageBitmap != null)
    //                 groupBoxCam2.Invoke((MethodInvoker)delegate
    //                 {
    //                     pictureBoxImage2.Image = camera.LatestImageBitmap;
    //                 });
    //             break;
    //         // goto case 1;
    //         case StringHolder.FilG :
    //         case  StringHolder.FilV:
    //             pictureBoxImage1.Image = null;
    //             if (camera.LatestImageBitmap != null)
    //                 groupBoxCam1.Invoke((MethodInvoker)delegate
    //                 {
    //                     pictureBoxImage1.Image = camera.LatestImageBitmap;
    //                 });
    //             break;
    //     }
    // }
        
    #endregion

    #region Options

    private void RegenerateConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.RegeneratetXmlConfig();
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
        StatusUpdater.UpdateNestedField(
            new[] { "focuser", "position" },
            JToken.FromObject(SerialFocus.CurrentPosition)
        );
        Thread.Sleep(waitTime);
        try
        {
            labelFocusPos?.Invoke((MethodInvoker) delegate
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
        
    private void checkBoxAutoFocus_CheckedChanged(object sender, EventArgs e)
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

    private void numericUpDownSetDefoc_ValueChanged(object sender, EventArgs e)
    {
        CameraFocus.DeFocus = (int)numericUpDownSetDefoc.Value;
    }

    private void buttonSetZeroPos_Click(object sender, EventArgs e)
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

    private void checkBoxGoZenith_CheckedChanged(object sender, EventArgs e)
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

    private void checkBoxHead_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBoxHead.Checked) Head.ThinkingTimer.Start();
        else Head.ThinkingTimer.Stop();

        Head.IsThinking = checkBoxHead.Checked;
    }

    #endregion
        
    private void checkBoxGuiding_CheckedChanged(object sender, EventArgs e)
    {
        Head.IsGuid = checkBoxGuiding.Checked;
    }

    // private void SetProgress(int indx)
    // {
    //     var value = 0;
    //     if (CameraControl.cams[indx].IsExposing)
    //     {
    //         if (Head.CurrentTask is null) return;
    //         value = 100 - CameraControl.cams[indx].RemTime * 100 / Head.CurrentTask.Exp;
    //         if (value < 0)
    //         {
    //             value = 0;
    //         }
    //     }
    //     switch (CameraControl.cams[indx].Filter)
    //     {
    //         case StringHolder.FilG:
    //         case StringHolder.FilV:
    //             progressBarG.Value = value;
    //             break;
    //         case StringHolder.FilR:
    //             progressBarR.Value = value;
    //             break;
    //         case StringHolder.FilI:
    //             progressBarI.Value = value;
    //             break;
    //     }
    // }

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
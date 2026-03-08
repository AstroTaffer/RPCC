using System;
using System.Threading;
using System.Threading.Tasks;
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
    public static bool IsTaskFormOpen;
    private bool _uiUpdating;
    private bool _focusUpdating;
    private volatile bool _isShuttingDown;
    
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
        _ = WeatherSocket.ConnectAsync();

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
        groupBoxFocusSettings.Text = $@"Focus Settings (COMPORT {Settings.FocusComPort})";
            
        Tasker.DataGridViewTasker = dataGridViewTasker;
        Tasker.ContextMenuStripTasker = contextMenuStripTasker;
        Tasker.SetHeader();
            
        // Fli.SetDebugLevel($"{Settings.MainOutputFolder}\\LOGS\\RPCC_LOGS\\FLIdebug.log", Fli.DEBUG.ALL);
            
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

        CameraFocus.IsAutoFocus = checkBoxAutoFocus.Checked;
        Logger.DebugMode = checkBoxDebugMode.Checked;
        buttonOneShot.Enabled = checkBoxManualControl.Checked;
        NpgsqlConnection.GlobalTypeMapper.MapComposite<Spoint>("public.spoint");
    }

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_isShuttingDown) return;

        _isShuttingDown = true;
        e.Cancel = true;

        Logger.AddLogEntry("MainForm: shutdown started");

        await Task.Run(() => Shutdown());

        Logger.AddLogEntry("MainForm: shutdown finished");

        BeginInvoke(new Action(() =>
        {
            _isShuttingDown = false;
            Close();
        }));
    }

    private void Shutdown()
    {
        try
        {
            timerUi.Stop();

            WeatherSocket.Disconnect();
            SiTechExeSocket.Disconnect();
            CameraControl.DisconnectCameras();
            SerialFocus.Close_Port();

            Logger.SaveLogs();
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"ERROR during shutdown: {ex}");
        }
    }
    
    private void TimerUiUpdate(object sender, EventArgs e)
    {
        if (_uiUpdating)
        {
            Logger.AddDebugLogEntry($"[STACK] {Environment.StackTrace}");
            return;
        }
        _uiUpdating = true;
        tSStatusClock.Text = @"UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss");
        checkBoxHead.Checked = Head.IsThinking;
        checkBoxGuiding.Checked = Head.IsGuid;
        checkBoxAutoFocus.Checked = CameraFocus.IsAutoFocus;
        foreach (var cam in CameraControl.cams)
        {
            cam.UpdateProgressBar();
            cam.UpdateUi();
        }

        _uiUpdating = false;
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
        _ = WeatherSocket.ConnectAsync();
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

    #region Options

    private void ResetConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Are you sure?\n"
            + "This action cannot be reverted.",
            "Reset config?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button1) == DialogResult.Yes) Settings.ResetXmlConfig();
    }
    #endregion

    #region Focus
    private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
    {
        if (_focusUpdating)
        {
            Logger.AddDebugLogEntry($"[STACK] {Environment.StackTrace}");
            return;
        }
        _focusUpdating = true;
        GetData();
        _focusUpdating = false;
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
        if (!IsTaskFormOpen)
        {
            IsTaskFormOpen = true;
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

   

    private void checkBoxDebugMode_CheckedChanged(object sender, EventArgs e)
    {
        Logger.DebugMode = checkBoxDebugMode.Checked;
    }

    private void checkBoxManualControl_CheckedChanged(object sender, EventArgs e)
    {
        Head.IsManualControl = checkBoxManualControl.Checked;
        buttonOneShot.Enabled = checkBoxManualControl.Checked;
        CameraFocus.IsAutoFocus = !checkBoxManualControl.Checked;
    }

    private void buttonOneShot_Click(object sender, EventArgs e)
    {
        Head.ContinueSequence();
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
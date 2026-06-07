using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WarningApp
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon = null!;
        private ContextMenuStrip contextMenuStrip = null!;
        private ToolStripMenuItem settingMenuItem = null!;
        private ToolStripMenuItem exitMenuItem = null!;
        private System.Windows.Forms.Timer warningTimer = null!;
        private SettingForm? settingForm;
        private WarningForm? warningForm;
        private PictureBox mainPictureBox = null!;
        
        private string iniFilePath = Path.Combine(Application.StartupPath, "settings.ini");
        private string imagePath = Path.Combine(Application.StartupPath, "main.png");
        
        public int IntervalMinutes { get; set; } = 45;
        public int RestMinutes { get; set; } = 2;
        
        // INI文件操作API
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, System.Text.StringBuilder lpReturnedString, int nSize, string lpFileName);

        static MainForm()
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeMainPicture();
            
            // 确保窗口在启动时不可见
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            
            InitializeNotifyIcon();
            LoadSettings();
            InitializeTimer();
            this.Hide();
            
            // 显示启动提示
            notifyIcon.BalloonTipTitle = "休息提醒";
            notifyIcon.BalloonTipText = "应用已启动，将在右下角托盘运行。";
            notifyIcon.ShowBalloonTip(2000);
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "休息提醒";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;

            contextMenuStrip = new ContextMenuStrip();
            settingMenuItem = new ToolStripMenuItem("设置");
            settingMenuItem.Click += SettingMenuItem_Click;
            exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += ExitMenuItem_Click;

            contextMenuStrip.Items.Add(settingMenuItem);
            contextMenuStrip.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }
        
        private void InitializeMainPicture()
        {
            // 设置窗口大小为16:9比例
            this.Width = 800;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "休息提醒";
            
            // 创建并配置PictureBox
            mainPictureBox = new PictureBox();
            mainPictureBox.Dock = DockStyle.Fill;
            mainPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            
            // 加载图片
            if (File.Exists(imagePath))
            {
                try
                {
                    mainPictureBox.Image = Image.FromFile(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("加载图片失败: " + ex.Message);
                }
            }
            
            this.Controls.Add(mainPictureBox);
        }

        private void SettingMenuItem_Click(object? sender, EventArgs e)
        {
            if (settingForm == null || settingForm.IsDisposed)
            {
                settingForm = new SettingForm(this);
            }
            settingForm.Show();
            settingForm.Activate();
        }

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            warningTimer?.Dispose();
            Application.Exit();
        }
        
        private void ShowMenuItem_Click(object? sender, EventArgs e)
        {
            // 显示主界面
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.Show();
            this.Activate();
        }

        private void InitializeTimer()
        {
            warningTimer = new System.Windows.Forms.Timer();
            warningTimer.Interval = IntervalMinutes * 60 * 1000;
            warningTimer.Tick += WarningTimer_Tick;
            warningTimer.Start();
        }

        private void WarningTimer_Tick(object? sender, EventArgs e)
        {
            // 每次都创建新的警告窗口，确保倒计时从0开始
            if (warningForm != null && !warningForm.IsDisposed)
            {
                warningForm.Dispose();
            }
            warningForm = new WarningForm(RestMinutes, this);
            warningForm.ShowDialog();
        }

        public void ResetTimer()
        {
            warningTimer.Stop();
            warningTimer.Interval = IntervalMinutes * 60 * 1000;
            warningTimer.Start();
        }
        
        private void LoadSettings()
        {
            if (File.Exists(iniFilePath))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(255);
                
                // 加载休息间隔
                GetPrivateProfileString("Settings", "IntervalMinutes", "45", sb, sb.Capacity, iniFilePath);
                if (int.TryParse(sb.ToString(), out int interval))
                {
                    IntervalMinutes = interval;
                }
                
                // 加载休息时长
                GetPrivateProfileString("Settings", "RestMinutes", "2", sb, sb.Capacity, iniFilePath);
                if (int.TryParse(sb.ToString(), out int rest))
                {
                    RestMinutes = rest;
                }
            }
        }
        
        public void SaveSettings()
        {
            WritePrivateProfileString("Settings", "IntervalMinutes", IntervalMinutes.ToString(), iniFilePath);
            WritePrivateProfileString("Settings", "RestMinutes", RestMinutes.ToString(), iniFilePath);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }
        
        protected override void OnSizeChanged(EventArgs e)
        {
            // 最小化时隐藏主界面
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
            base.OnSizeChanged(e);
        }

        private System.ComponentModel.IContainer? components;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "休息提醒";
            this.ShowInTaskbar = false;
        }
    }
}
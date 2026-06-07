using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WarningApp
{
    public partial class WarningForm : Form
    {
        private int restSeconds;
        private int totalSeconds;
        private System.Windows.Forms.Timer countdownTimer = null!;
        private MainForm mainForm = null!;
        private PictureBox mainPictureBox = null!;
        
        private string imagePath = Path.Combine(Application.StartupPath, "main.png");
        
        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool fBlockIt);
        
        // 全局键盘钩子相关
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr hookId = IntPtr.Zero;
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        
        private LowLevelKeyboardProc? _proc;

        public WarningForm(int restMinutes, MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.totalSeconds = restMinutes * 60;
            this.restSeconds = this.totalSeconds;
            InitializeTimer();
            InitializeMainPicture();
            UpdateProgressBar();
            BlockInput(true);
            
            // 设置全局键盘钩子
            _proc = KeyboardHookCallback;
            hookId = SetHook(_proc);
        }

        private void InitializeTimer()
        {
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }
        
        private void InitializeMainPicture()
        {
            // 创建并配置PictureBox
            mainPictureBox = new PictureBox();
            mainPictureBox.Dock = DockStyle.Fill;
            mainPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            
            // 设置图片为背景，让文字显示在图片上方
            this.Controls.Add(mainPictureBox);
            this.Controls.SetChildIndex(mainPictureBox, this.Controls.Count - 1);
            
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
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            restSeconds--;
            UpdateProgressBar();
            
            if (restSeconds <= 0)
            {
                countdownTimer.Stop();
                BlockInput(false);
                this.Close();
            }
        }

        private void UpdateProgressBar()
        {
            int progress = (totalSeconds - restSeconds) * 100 / totalSeconds;
            progressBar.Value = progress;
            timeLabel.Text = $"{restSeconds / 60:00}:{restSeconds % 60:00}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            BlockInput(false);
            
            // 卸载键盘钩子
            if (hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookId);
                hookId = IntPtr.Zero;
            }
            
            base.OnFormClosing(e);
        }
        
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP || 
                               wParam == (IntPtr)WM_SYSKEYDOWN || wParam == (IntPtr)WM_SYSKEYUP))
            {
                // 屏蔽所有键盘事件
                return (IntPtr)1;
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private System.ComponentModel.IContainer? components;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            BlockInput(false);
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.timeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleLabel.Font = new System.Drawing.Font("微软雅黑", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.titleLabel.ForeColor = System.Drawing.Color.Red;
            this.titleLabel.Location = new System.Drawing.Point(0, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(800, 200);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "注意休息";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 430);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(800, 20);
            this.progressBar.TabIndex = 1;
            // 
            // timeLabel
            // 
            this.timeLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.timeLabel.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.timeLabel.Location = new System.Drawing.Point(0, 350);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(800, 80);
            this.timeLabel.TabIndex = 2;
            this.timeLabel.Text = "05:00";
            this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WarningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightYellow;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WarningForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "WarningForm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.WarningForm_Load);
            this.ResumeLayout(false);
        }

        private void WarningForm_Load(object? sender, EventArgs e)
        {
            if (Screen.PrimaryScreen != null)
            {
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
        }

        private Label titleLabel = null!;
        private ProgressBar progressBar = null!;
        private Label timeLabel = null!;
    }
}
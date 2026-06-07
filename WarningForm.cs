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
        
        private string imagePath = Path.Combine(Application.StartupPath, "main.png");
        
        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool fBlockIt);
        
        // 全局键盘钩子相关
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr hookId = IntPtr.Zero;
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]         private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
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
            this.DoubleBuffered = true;
            if (Screen.PrimaryScreen != null)
            {
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            LoadBackgroundImage();
            UpdateProgressBar();
            InitializeTimer();
            BlockInput(true);
            
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
        
        private void LoadBackgroundImage()
        {
            if (File.Exists(imagePath))
            {
                try
                {
                    this.BackgroundImage = Image.FromFile(imagePath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
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
            this.progressBar = new TransparentProgressBar();
            this.timeLabel = new TransparentLabel();
            this.SuspendLayout();
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
            this.timeLabel.BackColor = System.Drawing.Color.Transparent;
            this.timeLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.timeLabel.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.timeLabel.ForeColor = System.Drawing.Color.White;
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
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WarningForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "WarningForm";
            this.TopMost = true;
            this.ResumeLayout(false);
        }

        private TransparentProgressBar progressBar = null!;
        private TransparentLabel timeLabel = null!;

        private class TransparentProgressBar : Control
        {
            private int _value;
            private int _maximum = 100;

            public int Value
            {
                get => _value;
                set { _value = Math.Clamp(value, 0, _maximum); Invalidate(); }
            }

            public TransparentProgressBar()
            {
                this.SetStyle(
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                this.BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (_maximum == 0) return;
                int fillWidth = (int)(Width * ((double)_value / _maximum));
                using (var brush = new SolidBrush(Color.FromArgb(160, Color.White)))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, fillWidth, Height);
                }
                using (var pen = new Pen(Color.FromArgb(100, Color.White), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (Parent?.BackgroundImage != null)
                {
                    var img = Parent.BackgroundImage;
                    var parentSize = Parent.ClientSize;
                    var scaleX = (float)img.Width / parentSize.Width;
                    var scaleY = (float)img.Height / parentSize.Height;
                    var srcRect = new RectangleF(
                        Location.X * scaleX,
                        Location.Y * scaleY,
                        Width * scaleX,
                        Height * scaleY);
                    e.Graphics.DrawImage(img, ClientRectangle, srcRect, GraphicsUnit.Pixel);
                }
            }
        }

        private class TransparentLabel : Control
        {
            private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
            private string? _text;

            public ContentAlignment TextAlign
            {
                get => _textAlign;
                set { _textAlign = value; Invalidate(); }
            }

            public override string Text
            {
                get => _text ?? string.Empty;
                set { _text = value ?? string.Empty; Invalidate(); }
            }

            public TransparentLabel()
            {
                this.SetStyle(
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                this.BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var format = new StringFormat();
                switch (_textAlign)
                {
                    case ContentAlignment.MiddleCenter:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.TopLeft:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.TopCenter:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.TopRight:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.MiddleLeft:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.MiddleRight:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.BottomLeft:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                    case ContentAlignment.BottomCenter:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                    case ContentAlignment.BottomRight:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                }
                using (var brush = new SolidBrush(this.ForeColor))
                {
                    e.Graphics.DrawString(this.Text, this.Font, brush, this.ClientRectangle, format);
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (Parent?.BackgroundImage != null)
                {
                    var img = Parent.BackgroundImage;
                    var parentSize = Parent.ClientSize;
                    var scaleX = (float)img.Width / parentSize.Width;
                    var scaleY = (float)img.Height / parentSize.Height;
                    var srcRect = new RectangleF(
                        Location.X * scaleX,
                        Location.Y * scaleY,
                        Width * scaleX,
                        Height * scaleY);
                    e.Graphics.DrawImage(img, ClientRectangle, srcRect, GraphicsUnit.Pixel);
                }
            }
        }
    }
}
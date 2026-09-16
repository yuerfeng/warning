using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        
        // 全局键盘/鼠标钩子相关
        private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]         private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);
        
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
        
        // 全局鼠标钩子相关
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEWHEEL = 0x020A;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        
        public WarningForm(int restSeconds, MainForm mainForm, Rectangle screenBounds)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.totalSeconds = restSeconds;
            this.restSeconds = this.totalSeconds;
            this.DoubleBuffered = true;
            this.Bounds = screenBounds;
            LoadBackgroundImage();
            UpdateProgressBar();
            InitializeTimer();
            
            // 全局输入屏蔽（多显示器时所有提醒窗口共享同一套钩子）
            InputBlocker.Register(this);
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
                this.Close();
            }
        }

        private void UpdateProgressBar()
        {
            int progress = (totalSeconds - restSeconds) * 100 / totalSeconds;
            progressBar.Value = progress;
            timeLabel.Text = FormatTime(restSeconds);
        }

        private static string FormatTime(int seconds)
        {
            if (seconds >= 3600)
            {
                return $"{seconds / 3600:00}:{seconds % 3600 / 60:00}:{seconds % 60:00}";
            }
            return $"{seconds / 60:00}:{seconds % 60:00}";
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            // 点击退出按钮：立即结束所有显示器上的本次休息
            countdownTimer.Stop();
            InputBlocker.CloseAll();
        }

        private System.ComponentModel.IContainer? components;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            countdownTimer?.Dispose();
            // 注销后若所有提醒窗口都已关闭，将自动卸载全局钩子
            InputBlocker.Unregister(this);
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.progressBar = new TransparentProgressBar();
            this.timeLabel = new TransparentLabel();
            this.exitButton = new TransparentButton();
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
            // exitButton
            //
            this.exitButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.exitButton.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.exitButton.ForeColor = System.Drawing.Color.White;
            this.exitButton.Location = new System.Drawing.Point(670, 20);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(110, 40);
            this.exitButton.TabIndex = 3;
            this.exitButton.TabStop = false;
            this.exitButton.Text = "退出休息";
            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // WarningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.exitButton);
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
        private TransparentButton exitButton = null!;

        // 全局输入拦截器：屏蔽键盘与鼠标点击（仅放行“退出按钮”），多显示器时所有提醒窗口共享同一套钩子
        private static class InputBlocker
        {
            private static IntPtr keyboardHookId = IntPtr.Zero;
            private static IntPtr mouseHookId = IntPtr.Zero;
            private static LowLevelHookProc? keyboardProc;
            private static LowLevelHookProc? mouseProc;
            private static readonly List<WarningForm> activeForms = new List<WarningForm>();

            public static void Register(WarningForm form)
            {
                activeForms.Add(form);
                if (activeForms.Count == 1)
                {
                    InstallHooks();
                }
            }

            public static void Unregister(WarningForm form)
            {
                if (activeForms.Remove(form) && activeForms.Count == 0)
                {
                    UninstallHooks();
                }
            }

            public static void CloseAll()
            {
                foreach (WarningForm form in activeForms.ToArray())
                {
                    if (!form.IsDisposed)
                    {
                        form.Close();
                    }
                }
            }

            private static void InstallHooks()
            {
                keyboardProc = KeyboardHookCallback;
                keyboardHookId = SetHook(WH_KEYBOARD_LL, keyboardProc);
                mouseProc = MouseHookCallback;
                mouseHookId = SetHook(WH_MOUSE_LL, mouseProc);
            }

            private static void UninstallHooks()
            {
                if (keyboardHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(keyboardHookId);
                    keyboardHookId = IntPtr.Zero;
                }
                if (mouseHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(mouseHookId);
                    mouseHookId = IntPtr.Zero;
                }
            }

            private static IntPtr SetHook(int idHook, LowLevelHookProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule!)
                {
                    return SetWindowsHookEx(idHook, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP ||
                                   wParam == (IntPtr)WM_SYSKEYDOWN || wParam == (IntPtr)WM_SYSKEYUP))
                {
                    // 屏蔽所有键盘事件
                    return (IntPtr)1;
                }
                return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
            }

            private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    int msg = wParam.ToInt32();
                    if (msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP ||
                        msg == WM_MBUTTONDOWN || msg == WM_MBUTTONUP ||
                        msg == WM_MOUSEWHEEL)
                    {
                        // 屏蔽右键、中键与滚轮
                        return (IntPtr)1;
                    }
                    if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP)
                    {
                        // 仅放行“退出按钮”区域的左键点击，其余位置一律屏蔽
                        if (IsOverAnyExitButton(lParam))
                        {
                            return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
                        }
                        return (IntPtr)1;
                    }
                }
                return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
            }

            private static bool IsOverAnyExitButton(IntPtr lParam)
            {
                var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                foreach (WarningForm form in activeForms)
                {
                    if (form.IsDisposed || !form.IsHandleCreated)
                    {
                        continue;
                    }
                    var rect = form.exitButton.RectangleToScreen(form.exitButton.ClientRectangle);
                    if (rect.Contains(data.pt))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

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

        private class TransparentButton : Control
        {
            private bool _hover;

            public TransparentButton()
            {
                this.SetStyle(
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                this.BackColor = Color.Transparent;
                this.Cursor = Cursors.Hand;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                _hover = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _hover = false;
                Invalidate();
                base.OnMouseLeave(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var brush = new SolidBrush(Color.FromArgb(_hover ? 90 : 50, Color.White)))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
                using (var pen = new Pen(Color.FromArgb(160, Color.White), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using (var textBrush = new SolidBrush(this.ForeColor))
                {
                    e.Graphics.DrawString(this.Text, this.Font, textBrush, this.ClientRectangle, format);
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

            [AllowNull]
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
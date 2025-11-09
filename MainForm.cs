using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DiscordRPC;
using DiscordRPC.Logging;

namespace DiscordActivityMonitor
{
    public class MainForm : Form
    {
        private DiscordRpcClient? client;
        private ActivityMonitor? monitor;
        private System.Windows.Forms.Timer? updateTimer;
        private NotifyIcon? trayIcon;
        
        // UI Components
        private Panel? titleBar;
        private Label? titleLabel;
        private System.Windows.Forms.Button? minimizeButton;
        private System.Windows.Forms.Button? closeButton;
        private RichTextBox? logBox;
        private Label? statusLabel;
        private Label? userLabel;
        private PictureBox? currentIconBox;
        private TextBox? clientIdTextBox;
        private System.Windows.Forms.Button? connectButton;
        private Panel? settingsPanel;
        
        // For dragging borderless window
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        
        private string DISCORD_CLIENT_ID = "";
        private bool isConnected = false;
        private string configPath = Path.Combine(Path.GetTempPath(), "DiscordActivityMonitor_config.txt");
        private System.Windows.Forms.Button? saveButton;
        
        // Clean dark color palette
        private static class Colors
        {
            public static readonly Color Background = Color.FromArgb(17, 17, 17);
            public static readonly Color Surface = Color.FromArgb(28, 28, 30);
            public static readonly Color Border = Color.FromArgb(45, 45, 48);
            public static readonly Color Primary = Color.FromArgb(88, 101, 242);
            public static readonly Color PrimaryHover = Color.FromArgb(71, 82, 196);
            public static readonly Color Text = Color.FromArgb(255, 255, 255);
            public static readonly Color TextMuted = Color.FromArgb(142, 142, 147);
            public static readonly Color Success = Color.FromArgb(67, 181, 129);
            public static readonly Color Error = Color.FromArgb(237, 66, 69);
        }

        public MainForm()
        {
            LoadConfig();
            InitializeUI();
            InitializeTrayIcon();
        }

        private void InitializeUI()
        {
            // Form settings - clean and simple
            this.Text = "Discord Activity Monitor";
            this.Size = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Colors.Background;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = new Size(600, 500);
            this.MinimumSize = new Size(600, 500);
            
            // Title bar - minimal
            titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Colors.Surface
            };
            titleBar.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Colors.Surface), titleBar.ClientRectangle);
                e.Graphics.DrawLine(new Pen(Colors.Border), 0, titleBar.Height - 1, titleBar.Width, titleBar.Height - 1);
            };
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseUp += TitleBar_MouseUp;
            
            // Title
            titleLabel = new Label
            {
                Text = "Discord Activity Monitor",
                ForeColor = Colors.Text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            titleLabel.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseMove += TitleBar_MouseMove;
            titleLabel.MouseUp += TitleBar_MouseUp;
            
            // Window controls - minimal
            closeButton = new System.Windows.Forms.Button
            {
                Text = "✕",
                Size = new Size(50, 50),
                Location = new Point(this.Width - 50, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Colors.Error;
            closeButton.Click += (s, e) => this.Hide();
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.White;
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Colors.TextMuted;
            
            minimizeButton = new System.Windows.Forms.Button
            {
                Text = "─",
                Size = new Size(50, 50),
                Location = new Point(this.Width - 100, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatAppearance.MouseOverBackColor = Colors.Border;
            minimizeButton.Click += (s, e) => this.Hide();
            minimizeButton.MouseEnter += (s, e) => minimizeButton.ForeColor = Color.White;
            minimizeButton.MouseLeave += (s, e) => minimizeButton.ForeColor = Colors.TextMuted;
            
            // Status section - clean
            statusLabel = new Label
            {
                Text = "Ready",
                ForeColor = Colors.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 65),
                AutoSize = true
            };
            
            userLabel = new Label
            {
                Text = "Not connected",
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 8),
                Location = new Point(20, 87),
                AutoSize = true
            };
            
            // Icon display - clean square
            currentIconBox = new PictureBox
            {
                Location = new Point(this.Width - 90, 60),
                Size = new Size(70, 70),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Colors.Surface
            };
            currentIconBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, currentIconBox.Width - 1, currentIconBox.Height - 1), 8))
                {
                    e.Graphics.FillPath(new SolidBrush(Colors.Surface), path);
                    e.Graphics.DrawPath(new Pen(Colors.Border), path);
                }
            };
            
            // Settings - super clean
            settingsPanel = new Panel
            {
                Location = new Point(20, 145),
                Size = new Size(this.Width - 40, 85),
                BackColor = Colors.Surface
            };
            settingsPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(settingsPanel.ClientRectangle, 8))
                {
                    e.Graphics.FillPath(new SolidBrush(Colors.Surface), path);
                    e.Graphics.DrawPath(new Pen(Colors.Border), path);
                }
            };
            
            var settingsTitle = new Label
            {
                Text = "Client ID",
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 8),
                Location = new Point(15, 15),
                AutoSize = true
            };
            
            // Clean textbox
            clientIdTextBox = new TextBox
            {
                Location = new Point(15, 35),
                Size = new Size(settingsPanel.Width - 235, 26),
                BackColor = Colors.Background,
                ForeColor = Colors.Text,
                Font = new Font("Consolas", 9),
                Text = DISCORD_CLIENT_ID,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter Discord Application ID"
            };
            
            // Clean button
            connectButton = new System.Windows.Forms.Button
            {
                Text = "Connect",
                Location = new Point(settingsPanel.Width - 215, 35),
                Size = new Size(95, 26),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            connectButton.FlatAppearance.BorderSize = 0;
            connectButton.FlatAppearance.MouseOverBackColor = Colors.PrimaryHover;
            connectButton.Click += ConnectButton_Click;
            
            // Save button
            saveButton = new System.Windows.Forms.Button
            {
                Text = "Save",
                Location = new Point(settingsPanel.Width - 110, 35),
                Size = new Size(95, 26),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 150, 100);
            saveButton.Click += SaveButton_Click;
            
            settingsPanel.Controls.Add(settingsTitle);
            settingsPanel.Controls.Add(clientIdTextBox);
            settingsPanel.Controls.Add(connectButton);
            settingsPanel.Controls.Add(saveButton);
            
            // Log - minimal
            var logPanel = new Panel
            {
                Location = new Point(20, 245),
                Size = new Size(this.Width - 40, this.Height - 275),
                BackColor = Colors.Surface
            };
            logPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(logPanel.ClientRectangle, 8))
                {
                    e.Graphics.FillPath(new SolidBrush(Colors.Surface), path);
                    e.Graphics.DrawPath(new Pen(Colors.Border), path);
                }
            };
            
            var logTitle = new Label
            {
                Text = "Activity Log",
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 8),
                Location = new Point(15, 12),
                AutoSize = true
            };
            
            logBox = new RichTextBox
            {
                Location = new Point(12, 32),
                Size = new Size(logPanel.Width - 24, logPanel.Height - 40),
                BackColor = Colors.Background,
                ForeColor = Colors.Text,
                Font = new Font("Consolas", 8),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            
            logPanel.Controls.Add(logTitle);
            logPanel.Controls.Add(logBox);
            
            // Add all controls
            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(minimizeButton);
            titleBar.Controls.Add(closeButton);
            
            this.Controls.Add(titleBar);
            this.Controls.Add(statusLabel);
            this.Controls.Add(userLabel);
            this.Controls.Add(currentIconBox);
            this.Controls.Add(settingsPanel);
            this.Controls.Add(logPanel);
            
            AddLog("Discord Activity Monitor", Colors.Primary);
            AddLog("Enter Client ID to start", Colors.TextMuted);
            
            // Handle resize
            this.Resize += (s, e) =>
            {
                if (closeButton != null)
                    closeButton.Location = new Point(this.Width - 50, 0);
                if (minimizeButton != null)
                    minimizeButton.Location = new Point(this.Width - 100, 0);
                if (currentIconBox != null)
                    currentIconBox.Location = new Point(this.Width - 90, 60);
            };
        }

        private void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (clientIdTextBox == null || connectButton == null) return;
            
            string newClientId = clientIdTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(newClientId))
            {
                AddLog("❌ Please enter a valid Client ID", Color.FromArgb(237, 66, 69));
                return;
            }
            
            if (isConnected)
            {
                // Disconnect
                DisconnectDiscord();
            }
            else
            {
                // Connect
                DISCORD_CLIENT_ID = newClientId;
                InitializeDiscord();
            }
        }

        private void DisconnectDiscord()
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
            }
            
            if (client != null)
            {
                client.ClearPresence();
                client.Dispose();
                client = null;
            }
            
            isConnected = false;
            
                if (connectButton != null)
                {
                    connectButton.Text = "Connect";
                    connectButton.BackColor = Colors.Primary;
                }            if (statusLabel != null)
                statusLabel.Text = "Status: Disconnected";
            
            if (userLabel != null)
                userLabel.Text = "Discord: Not connected";
            
            AddLog("⚠️ Disconnected from Discord", Color.FromArgb(250, 166, 26));
        }

        private void InitializeDiscord()
        {
            try
            {
                AddLog($"🔄 Connecting to Discord with Client ID: {DISCORD_CLIENT_ID}", Color.FromArgb(250, 166, 26));
                
                client = new DiscordRpcClient(DISCORD_CLIENT_ID);
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                
                client.OnReady += (sender, e) =>
                {
                    this.Invoke(() =>
                    {
                        if (userLabel != null)
                            userLabel.Text = $"Discord: Connected as {e.User.Username}";
                        AddLog($"✓ Connected to Discord as {e.User.Username}", Color.FromArgb(67, 181, 129));
                    });
                };

                client.OnPresenceUpdate += (sender, e) =>
                {
                    this.Invoke(() =>
                    {
                        AddLog("✓ Presence updated", Color.FromArgb(114, 137, 218));
                    });
                };

                if (client.Initialize())
                {
                    isConnected = true;
                    
                    if (connectButton != null)
                    {
                        connectButton.Text = "Disconnect";
                        connectButton.BackColor = Colors.Error;
                    }
                    
                    if (statusLabel != null)
                        statusLabel.Text = "Status: Running";
                    AddLog("✓ Discord RPC initialized successfully", Color.FromArgb(67, 181, 129));
                    
                    monitor = new ActivityMonitor(client, 
                        (msg, color) => AddLog(msg, color),
                        (icon) => UpdateCurrentIcon(icon));
                    
                    if (updateTimer == null)
                    {
                        updateTimer = new System.Windows.Forms.Timer();
                        updateTimer.Interval = 1000; // 1 second
                        updateTimer.Tick += (s, e) => monitor?.Update();
                    }
                    
                    updateTimer.Start();
                    AddLog("● Monitoring started...", Color.FromArgb(250, 166, 26));
                }
                else
                {
                    isConnected = false;
                    if (statusLabel != null)
                        statusLabel.Text = "Status: Discord connection failed";
                    AddLog("✗ Failed to connect to Discord. Make sure Discord is running!", Color.FromArgb(237, 66, 69));
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                AddLog($"❌ Error connecting to Discord: {ex.Message}", Color.FromArgb(237, 66, 69));
            }
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Discord Activity Monitor";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            
            // Context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = Color.FromArgb(47, 49, 54);
            contextMenu.ForeColor = Color.White;
            
            var showItem = new ToolStripMenuItem("Show", null, (s, e) => 
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            });
            
            var downloadIconsItem = new ToolStripMenuItem("Download Discord Icons", null, async (s, e) =>
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var iconsFolder = Path.Combine(desktopPath, "DiscordRPC_Icons");
                await IconDownloader.DownloadAllIconsForDiscord(iconsFolder);
                
                System.Diagnostics.Process.Start("explorer.exe", iconsFolder);
                
                AddLog($"✅ Icons downloaded to: {iconsFolder}", Color.FromArgb(67, 181, 129));
                MessageBox.Show(
                    $"Icons downloaded to your Desktop!\n\n" +
                    $"Next steps:\n" +
                    $"1. Go to Discord Developer Portal\n" +
                    $"2. Upload these icons to 'Rich Presence' → 'Art Assets'\n" +
                    $"3. Use the filename (without .png) as the icon name\n\n" +
                    $"Folder opened: {iconsFolder}",
                    "Icons Downloaded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            });
            
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            });
            
            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(downloadIconsItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);
            
            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };
        }

        private void AddLog(string message, Color? color = null)
        {
            if (logBox == null) return;
            
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(() => AddLog(message, color));
                return;
            }
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string fullMessage = $"[{timestamp}] {message}\n";
            
            int startIndex = logBox.TextLength;
            logBox.AppendText(fullMessage);
            
            if (color.HasValue)
            {
                logBox.Select(startIndex, fullMessage.Length);
                logBox.SelectionColor = color.Value;
                logBox.Select(logBox.TextLength, 0);
            }
            
            logBox.ScrollToCaret();
            
            // Limit log size
            if (logBox.Lines.Length > 500)
            {
                var lines = logBox.Lines;
                logBox.Lines = lines[^300..]; // Keep last 300 lines
            }
        }

        private void UpdateCurrentIcon(Image? icon)
        {
            if (currentIconBox == null) return;
            
            if (currentIconBox.InvokeRequired)
            {
                currentIconBox.Invoke(() => UpdateCurrentIcon(icon));
                return;
            }
            
            if (icon != null)
            {
                currentIconBox.Image = icon;
            }
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            dragging = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                if (trayIcon != null)
                    trayIcon.Visible = false;
                if (updateTimer != null)
                    updateTimer.Stop();
                if (client != null)
                    client.Dispose();
            }
            base.OnFormClosing(e);
        }

        // Add rounded corners (Windows 11 style)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Colors.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
        
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            
            // Top left arc
            path.AddArc(arc, 180, 90);
            
            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            
            path.CloseFigure();
            return path;
        }
        
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    DISCORD_CLIENT_ID = File.ReadAllText(configPath).Trim();
                }
            }
            catch (Exception ex)
            {
                // Silent fail, use empty string
            }
        }
        
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (clientIdTextBox != null && !string.IsNullOrWhiteSpace(clientIdTextBox.Text))
                {
                    File.WriteAllText(configPath, clientIdTextBox.Text.Trim());
                    DISCORD_CLIENT_ID = clientIdTextBox.Text.Trim();
                    AddLog("Client ID saved successfully!", Colors.Success);
                    MessageBox.Show("Client ID saved to config!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AddLog("Please enter a Client ID first", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Failed to save config: {ex.Message}", Colors.Error);
            }
        }
        
    }
}

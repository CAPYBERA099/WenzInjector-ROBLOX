using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuorumAPI;
using Guna.UI2.WinForms;

namespace RobloxInjector_by_WenzHack
{
    public partial class Form1 : Form
    {
        private enum NavTab
        {
            Main,
            Settings,
            ScriptHub
        }

        private int shineOffset = -150;
        private int indicatorTargetY = 68;
        private int indicatorCurrentY = 68;
        private Timer indicatorAnimationTimer;
        private HttpClient httpClient = new HttpClient();
        private List<ScriptItem> scriptItems = new List<ScriptItem>();
        private Timer textAnimationTimer;
        private string animatedText = "";
        private int animationCharIndex = 0;

        public Form1()
        {
            InitializeComponent();
            
            // Инициализация QuorumAPI
            QuorumAPI.QuorumModule.AutoUpdate();
            QuorumAPI.QuorumModule.SetLevel("8");
            
            InitializeNavigation();
            InitializeIndicatorAnimation();
            InitializeTabs();
            InitializeButtonIcons();
            InitializeScriptHub();
            InitializeTextAnimation();
            ShowTab(NavTab.Main);
        }

        private void Inject_Click(object sender, EventArgs e)
        {
            int result = QuorumAPI.QuorumModule.AttachAPIWithState();
            if (result == 1)
            {
                // Успешное подключение
                UpdateStatusIndicator(true);
            }
            else
            {
                // Ошибка подключения
                UpdateStatusIndicator(false);
            }
        }

        private void Execute_Click(object sender, EventArgs e)
        {
            string script = GetCurrentTabScript();
            if (!string.IsNullOrWhiteSpace(script))
            {
                QuorumAPI.QuorumModule.ExecuteScript(script);
            }
        }

        private void shineTimer_Tick(object sender, EventArgs e)
        {
            shineOffset += 6;
            if (shineOffset > this.NEVERWENZ.Width + 120)
            {
                shineOffset = -200;
            }

            this.NEVERWENZ.Invalidate();
        }

        private void NEVERWENZ_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            Rectangle textRect = new Rectangle(Point.Empty, this.NEVERWENZ.Size);
            using (GraphicsPath path = new GraphicsPath())
            {
                float emSize = this.NEVERWENZ.Font.SizeInPoints * e.Graphics.DpiX / 72f;
                path.AddString(this.NEVERWENZ.Text,
                    this.NEVERWENZ.Font.FontFamily,
                    (int)this.NEVERWENZ.Font.Style,
                    emSize,
                    textRect,
                    StringFormat.GenericDefault);

                using (LinearGradientBrush gradient = new LinearGradientBrush(
                    textRect,
                    Color.FromArgb(34, 162, 255),
                    Color.FromArgb(142, 233, 255),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillPath(gradient, path);
                }

                Rectangle shineRect = new Rectangle(shineOffset, 0, 90, this.NEVERWENZ.Height);
                using (LinearGradientBrush shineBrush = new LinearGradientBrush(
                    shineRect,
                    Color.FromArgb(200, Color.White),
                    Color.FromArgb(0, Color.White),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.SetClip(path);
                    e.Graphics.FillRectangle(shineBrush, shineRect);
                    e.Graphics.ResetClip();
                }
            }
        }

        private void NavButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is Guna2Button button && button.Checked && button.Tag is NavTab tab)
            {
                ShowTab(tab);
                AnimateIndicatorToButton(button);
            }
        }

        private void InitializeIndicatorAnimation()
        {
            indicatorAnimationTimer = new Timer();
            indicatorAnimationTimer.Interval = 10;
            indicatorAnimationTimer.Tick += IndicatorAnimationTimer_Tick;
            indicatorAnimationTimer.Start();
        }

        private void IndicatorAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (indicatorCurrentY != indicatorTargetY)
            {
                int diff = indicatorTargetY - indicatorCurrentY;
                int step = Math.Sign(diff) * Math.Min(Math.Abs(diff), 4);
                indicatorCurrentY += step;
                navIndicator.Location = new Point(0, indicatorCurrentY);
            }
        }

        private void AnimateIndicatorToButton(Guna2Button button)
        {
            indicatorTargetY = button.Location.Y;
        }

        private void InitializeNavigation()
        {
            navMainButton.Tag = NavTab.Main;
            navSettingsButton.Tag = NavTab.Settings;
            navHubButton.Tag = NavTab.ScriptHub;

            navMainButton.Image = CreateNavIcon(NavTab.Main);
            navSettingsButton.Image = CreateNavIcon(NavTab.Settings);
            navHubButton.Image = CreateNavIcon(NavTab.ScriptHub);
        }

        private void ShowTab(NavTab tab)
        {
            bool isMain = tab == NavTab.Main;
            editorContainer.Visible = isMain;
            tabsContainer.Visible = isMain;
            guna2Panel1.Visible = isMain;
            settingsPanel.Visible = tab == NavTab.Settings;
            scriptHubPanel.Visible = tab == NavTab.ScriptHub;

            if (settingsPanel.Visible)
            {
                settingsPanel.BringToFront();
            }

            if (scriptHubPanel.Visible)
            {
                scriptHubPanel.BringToFront();
            }

            if (isMain)
            {
                editorContainer.BringToFront();
                tabsContainer.BringToFront();
            }
        }

        private Bitmap CreateNavIcon(NavTab tab)
        {
            Bitmap bmp = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Rectangle bounds = new Rectangle(8, 8, 32, 32);
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds,
                    Color.FromArgb(45, 152, 255),
                    Color.FromArgb(126, 222, 255),
                    LinearGradientMode.ForwardDiagonal))
                using (Pen pen = new Pen(Color.FromArgb(200, 255, 255), 2))
                {
                    switch (tab)
                    {
                        case NavTab.Main:
                            Point[] roof =
                            {
                                new Point(24, 10),
                                new Point(12, 20),
                                new Point(36, 20)
                            };
                            g.FillPolygon(brush, roof);
                            g.FillRectangle(brush, 16, 20, 16, 16);
                            using (SolidBrush doorBrush = new SolidBrush(Color.FromArgb(8, 9, 18)))
                            {
                                g.FillRectangle(doorBrush, 22, 26, 4, 10);
                            }
                            break;
                        case NavTab.Settings:
                            g.FillEllipse(brush, bounds);
                            using (SolidBrush centerBrush = new SolidBrush(Color.FromArgb(8, 9, 18)))
                            {
                                g.FillEllipse(centerBrush, new Rectangle(19, 19, 10, 10));
                            }
                            for (int i = 0; i < 6; i++)
                            {
                                double angle = i * Math.PI / 3;
                                int x = (int)(24 + Math.Cos(angle) * 14);
                                int y = (int)(24 + Math.Sin(angle) * 14);
                                g.FillEllipse(brush, x - 3, y - 3, 6, 6);
                            }
                            break;
                        case NavTab.ScriptHub:
                            pen.Color = Color.FromArgb(160, 210, 255);
                            pen.Width = 3;
                            g.DrawLines(pen, new[]
                            {
                                new Point(18, 16),
                                new Point(12, 24),
                                new Point(18, 32)
                            });
                            g.DrawLines(pen, new[]
                            {
                                new Point(30, 16),
                                new Point(36, 24),
                                new Point(30, 32)
                            });
                            g.DrawLine(pen, new Point(22, 20), new Point(26, 28));
                            g.DrawLine(pen, new Point(26, 20), new Point(22, 28));
                            break;
                    }
                }
            }
            return bmp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Инициализация завершена
        }

        private void autoInjectSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (autoInjectSwitch.Checked)
            {
                QuorumAPI.QuorumModule.SetAutoInject(true);
            }
            else
            {
                QuorumAPI.QuorumModule.SetAutoInject(false);
            }
        }

        private void topMostSwitch_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = topMostSwitch.Checked;
        }

        private void robloxStatusTimer_Tick(object sender, EventArgs e)
        {
            // Проверяем статус подключения через QuorumAPI
            int attachStatus = QuorumAPI.QuorumModule.IsAttached();
            bool isAttached = (attachStatus == 1);
            
            UpdateStatusIndicator(isAttached);
        }

        private void UpdateStatusIndicator(bool isAttached)
        {
            if (isAttached)
            {
                robloxStatusIndicator.BackColor = Color.FromArgb(40, 167, 69); // Зеленый
            }
            else
            {
                robloxStatusIndicator.BackColor = Color.FromArgb(220, 53, 69); // Красный
            }
        }

        private List<ScriptTab> scriptTabs = new List<ScriptTab>();
        private ScriptTab activeTab = null;

        public int lastLineCount { get; private set; }

        private class ScriptTab
        {
            public string Name { get; set; }
            public string Content { get; set; }
            public Guna2Button TabButton { get; set; }
            public RichTextBox Editor { get; set; }
        }

        private void InitializeTabs()
        {
            // Создаем первую вкладку по умолчанию
            AddNewTab("Tab 1");
        }

        private void tabAddButton_Click(object sender, EventArgs e)
        {
            int tabNumber = scriptTabs.Count + 1;
            AddNewTab($"Tab {tabNumber}");
        }

        private void AddNewTab(string tabName)
        {
            var tab = new ScriptTab
            {
                Name = tabName,
                Content = "",
                Editor = scriptTabs.Count == 0 ? richTextBox1 : null
            };

            // Создаем кнопку вкладки
            var tabButton = new Guna2Button
            {
                Text = tabName,
                Size = new Size(100, 28),
                Location = new Point(scriptTabs.Count * 102, 0),
                FillColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(175, 205, 255),
                Font = new Font("Segoe UI", 9F),
                BorderRadius = 4,
                Animated = true,
                Image = CreateTabIcon(),
                ImageAlign = HorizontalAlignment.Left,
                ImageOffset = new Point(6, 0),
                TextOffset = new Point(4, 0)
            };

            tabButton.Click += (s, e) => SwitchToTab(tab);
            tab.TabButton = tabButton;

            // Кнопка закрытия вкладки
            var closeButton = new Guna2Button
            {
                Text = "×",
                Size = new Size(20, 20),
                Location = new Point(tabButton.Width - 22, 4),
                FillColor = Color.Transparent,
                ForeColor = Color.FromArgb(175, 205, 255),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BorderRadius = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            closeButton.Click += (s, e) =>
            {
                RemoveTab(tab);
            };

            tabButton.Controls.Add(closeButton);
            closeButton.BringToFront();

            scriptTabs.Add(tab);
            tabsPanel.Controls.Add(tabButton);

            if (scriptTabs.Count == 1)
            {
                activeTab = tab;
                tabButton.FillColor = Color.FromArgb(29, 107, 206);
            }

            UpdateTabsLayout();
        }

        private void SwitchToTab(ScriptTab tab)
        {
            if (activeTab != null)
            {
                activeTab.Content = richTextBox1.Text;
                activeTab.TabButton.FillColor = Color.FromArgb(20, 20, 20);
            }

            activeTab = tab;
            richTextBox1.Text = tab.Content;
            tab.TabButton.FillColor = Color.FromArgb(29, 107, 206);
        }

        private void RemoveTab(ScriptTab tab)
        {
            if (scriptTabs.Count <= 1) return; // Не удаляем последнюю вкладку

            if (activeTab == tab)
            {
                int index = scriptTabs.IndexOf(tab);
                int newIndex = index > 0 ? index - 1 : 0;
                SwitchToTab(scriptTabs[newIndex]);
            }

            scriptTabs.Remove(tab);
            tabsPanel.Controls.Remove(tab.TabButton);
            tab.TabButton.Dispose();

            UpdateTabsLayout();
        }

        private void UpdateTabsLayout()
        {
            for (int i = 0; i < scriptTabs.Count; i++)
            {
                scriptTabs[i].TabButton.Location = new Point(i * 102, 0);
            }
        }

        private void InitializeButtonIcons()
        {
            Execute.Image = CreateButtonIcon("Execute");
            Inject.Image = CreateButtonIcon("Attach");
        }

        private Bitmap CreateButtonIcon(string type)
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.White, 2))
                {
                    if (type == "Execute")
                    {
                        // Иконка Play/Execute (треугольник)
                        Point[] triangle = {
                            new Point(6, 4),
                            new Point(6, 20),
                            new Point(18, 12)
                        };
                        g.FillPolygon(new SolidBrush(Color.White), triangle);
                    }
                    else if (type == "Attach")
                    {
                        // Иконка Attach (скрепка/соединение)
                        g.DrawLine(pen, new Point(8, 6), new Point(16, 14));
                        g.DrawLine(pen, new Point(16, 6), new Point(8, 14));
                        g.DrawEllipse(pen, 6, 4, 4, 4);
                        g.DrawEllipse(pen, 14, 16, 4, 4);
                    }
                }
            }
            return bmp;
        }

        private Bitmap CreateTabIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.FromArgb(175, 205, 255), 1.5f))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(175, 205, 255)))
                {
                    // Иконка файла/скрипта
                    Point[] fileShape = {
                        new Point(3, 2),
                        new Point(10, 2),
                        new Point(13, 5),
                        new Point(13, 14),
                        new Point(3, 14)
                    };
                    g.FillPolygon(brush, fileShape);
                    g.DrawPolygon(pen, fileShape);
                    
                    // Складка документа
                    Point[] fold = {
                        new Point(10, 2),
                        new Point(10, 5),
                        new Point(13, 5)
                    };
                    using (SolidBrush foldBrush = new SolidBrush(Color.FromArgb(120, 150, 200)))
                    {
                        g.FillPolygon(foldBrush, fold);
                    }
                }
            }
            return bmp;
        }

        private string GetCurrentTabScript()
        {
            // Сохраняем текущий контент перед получением
            if (activeTab != null)
            {
                activeTab.Content = richTextBox1.Text;
            }
            return richTextBox1.Text;
        }

        private void robloxStatusIndicator_SizeChanged(object sender, EventArgs e)
        {
            // Делаем индикатор круглым
            if (robloxStatusIndicator.Width == robloxStatusIndicator.Height)
            {
                robloxStatusIndicator.BorderRadius = robloxStatusIndicator.Width / 2;
            }
        }

        private void robloxStatusIndicator_Paint(object sender, PaintEventArgs e)
        {
            // Обеспечиваем идеально круглую форму
            robloxStatusIndicator.BorderRadius = robloxStatusIndicator.Width / 2;
        }

        private void lineNumbersPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Получаем информацию о первой видимой строке
            Point pt = new Point(0, 0);
            int firstCharIndex = richTextBox1.GetCharIndexFromPosition(pt);
            int firstLine = richTextBox1.GetLineFromCharIndex(firstCharIndex);
            int firstLineNumber = firstLine + 1;
            
            // Вычисляем высоту строки
            int lineHeight = (int)richTextBox1.Font.GetHeight();
            if (lineHeight == 0) lineHeight = 16;
            
            // Вычисляем смещение первой строки
            int firstLineY = richTextBox1.GetPositionFromCharIndex(firstCharIndex).Y;
            int offsetY = -firstLineY;
            
            // Количество видимых строк
            int visibleLines = (int)Math.Ceiling((double)lineNumbersPanel.Height / lineHeight) + 1;
            int totalLines = richTextBox1.Lines.Length;
            if (totalLines == 0) totalLines = 1;

            // Рисуем разделитель
            using (Pen dividerPen = new Pen(Color.FromArgb(40, 50, 70), 1))
            {
                e.Graphics.DrawLine(dividerPen, lineNumbersPanel.Width - 1, 0, 
                    lineNumbersPanel.Width - 1, lineNumbersPanel.Height);
            }

            // Рисуем номера строк
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 137, 166)))
            using (Font lineFont = new Font("Consolas", 9F, FontStyle.Regular))
            using (StringFormat format = new StringFormat 
            { 
                Alignment = StringAlignment.Far, 
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                for (int i = 0; i < visibleLines; i++)
                {
                    int lineNumber = firstLineNumber + i;
                    if (lineNumber > totalLines) break;

                    int yPos = i * lineHeight + offsetY;
                    Rectangle lineRect = new Rectangle(0, yPos, lineNumbersPanel.Width - 8, lineHeight);
                    
                    // Подсветка текущей строки (опционально)
                    int currentLine = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) + 1;
                    if (lineNumber == currentLine)
                    {
                        using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(30, 40, 60)))
                        {
                            e.Graphics.FillRectangle(highlightBrush, 
                                new Rectangle(0, yPos, lineNumbersPanel.Width - 1, lineHeight));
                        }
                        using (SolidBrush currentBrush = new SolidBrush(Color.FromArgb(175, 205, 255)))
                        {
                            e.Graphics.DrawString(lineNumber.ToString(), lineFont, currentBrush, lineRect, format);
                        }
                    }
                    else
                    {
                        e.Graphics.DrawString(lineNumber.ToString(), lineFont, brush, lineRect, format);
                    }
                }
            }
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            lineNumbersPanel.Invalidate();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Обновляем номера строк при изменении текста
            int currentLineCount = richTextBox1.Lines.Length;
            if (currentLineCount != lastLineCount)
            {
                lastLineCount = currentLineCount;
                lineNumbersPanel.Invalidate();
            }
            
            // Плавная анимация обновления
            if (!lineNumbersPanel.IsDisposed)
            {
                lineNumbersPanel.Invalidate();
            }
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            // Плавная анимация при изменении выделения
            lineNumbersPanel.Invalidate();
        }

        private class ScriptItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Script { get; set; }
            public string ImageUrl { get; set; }
            public Guna2Panel Panel { get; set; }
        }

        private void InitializeScriptHub()
        {
            LoadScriptsFromRbxScripts();
        }

        private async void LoadScriptsFromRbxScripts()
        {
            try
            {
                // Популярные скрипты с rbxscripts.com (примеры)
                var scripts = new List<ScriptItem>
                {
                    new ScriptItem
                    {
                        Name = "Infinite Yield",
                        Description = "Admin commands script",
                        Script = "loadstring(game:HttpGet('https://raw.githubusercontent.com/EdgeIY/infiniteyield/master/source'))()",
                        ImageUrl = ""
                    },
                    new ScriptItem
                    {
                        Name = "Remote Spy",
                        Description = "Monitor remote events",
                        Script = "loadstring(game:HttpGet('https://raw.githubusercontent.com/78n/SimpleSpy/main/SimpleSpySource.lua'))()",
                        ImageUrl = ""
                    },
                    new ScriptItem
                    {
                        Name = "FPS Unlocker",
                        Description = "Unlock FPS cap",
                        Script = "setfpscap(999)",
                        ImageUrl = ""
                    }
                };

                // Можно добавить реальный парсинг с сайта
                // string html = await httpClient.GetStringAsync("https://rbxscripts.com/");
                // Parse scripts from HTML...

                foreach (var script in scripts)
                {
                    CreateScriptCard(script);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки скриптов: {ex.Message}");
            }
        }

        private void CreateScriptCard(ScriptItem script)
        {
            var card = new Guna2Panel
            {
                Size = new Size(640, 120),
                Location = new Point(10, scriptItems.Count * 130 + 10),
                FillColor = Color.FromArgb(15, 16, 25),
                BorderRadius = 12,
                BorderColor = Color.FromArgb(40, 50, 70),
                BorderThickness = 1
            };

            // Название скрипта
            var nameLabel = new Guna2HtmlLabel
            {
                Text = script.Name,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(175, 205, 255),
                Location = new Point(15, 15),
                Size = new Size(400, 25),
                BackColor = Color.Transparent
            };
            card.Controls.Add(nameLabel);

            // Описание
            var descLabel = new Guna2HtmlLabel
            {
                Text = script.Description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(150, 170, 200),
                Location = new Point(15, 40),
                Size = new Size(400, 20),
                BackColor = Color.Transparent
            };
            card.Controls.Add(descLabel);

            // Кнопка копирования
            var copyButton = new Guna2Button
            {
                Text = "Копировать",
                Size = new Size(100, 32),
                Location = new Point(430, 15),
                FillColor = Color.FromArgb(29, 107, 206),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                BorderRadius = 6,
                Animated = true
            };
            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(script.Script);
                copyButton.Text = "Скопировано!";
                Timer resetTimer = new Timer { Interval = 2000 };
                resetTimer.Tick += (s2, e2) =>
                {
                    copyButton.Text = "Копировать";
                    resetTimer.Stop();
                    resetTimer.Dispose();
                };
                resetTimer.Start();
            };
            card.Controls.Add(copyButton);

            // Кнопка инжекта
            var injectButton = new Guna2Button
            {
                Text = "Инжект",
                Size = new Size(100, 32),
                Location = new Point(430, 55),
                FillColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                BorderRadius = 6,
                Animated = true
            };
            injectButton.Click += (s, e) =>
            {
                // Переключаемся на главную вкладку и вставляем скрипт
                navMainButton.Checked = true;
                ShowTab(NavTab.Main);
                if (activeTab != null)
                {
                    activeTab.Content = script.Script;
                    richTextBox1.Text = script.Script;
                }
            };
            card.Controls.Add(injectButton);

            // Превью скрипта (мини-окно)
            var previewPanel = new Guna2Panel
            {
                Size = new Size(200, 80),
                Location = new Point(15, 65),
                FillColor = Color.FromArgb(8, 9, 18),
                BorderRadius = 6,
                BorderColor = Color.FromArgb(30, 40, 60),
                BorderThickness = 1
            };

            var previewLabel = new Guna2HtmlLabel
            {
                Text = TruncateScript(script.Script, 100),
                Font = new Font("Consolas", 8F),
                ForeColor = Color.FromArgb(120, 140, 170),
                Location = new Point(5, 5),
                Size = new Size(190, 70),
                BackColor = Color.Transparent
            };
            previewPanel.Controls.Add(previewLabel);
            card.Controls.Add(previewPanel);

            script.Panel = card;
            scriptItems.Add(script);
            scriptHubScrollPanel.Controls.Add(card);
        }

        private string TruncateScript(string script, int maxLength)
        {
            if (script.Length <= maxLength) return script;
            return script.Substring(0, maxLength) + "...";
        }

        private void InitializeTextAnimation()
        {
            textAnimationTimer = new Timer { Interval = 30 };
            textAnimationTimer.Tick += TextAnimationTimer_Tick;
        }

        private void TextAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (animationCharIndex < animatedText.Length)
            {
                int currentLength = richTextBox1.TextLength;
                string newChar = animatedText[animationCharIndex].ToString();
                
                richTextBox1.SelectionStart = currentLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectedText = newChar;
                
                animationCharIndex++;
            }
            else
            {
                textAnimationTimer.Stop();
            }
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Анимация ввода для RichTextBox
            if (!char.IsControl(e.KeyChar))
            {
                // Отменяем стандартную обработку и добавляем с анимацией
                e.Handled = true;
                AnimateCharacterInput(e.KeyChar);
            }
        }

        private void AnimateCharacterInput(char character)
        {
            // Анимация плавного появления символа
            int startPos = richTextBox1.SelectionStart;
            Color originalColor = richTextBox1.SelectionColor;
            
            // Вставляем символ с начальной прозрачностью
            richTextBox1.SelectionStart = startPos;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionColor = Color.FromArgb(0, originalColor.R, originalColor.G, originalColor.B);
            richTextBox1.SelectedText = character.ToString();
            
            // Плавное появление символа
            Timer fadeTimer = new Timer { Interval = 8 };
            int alpha = 0;
            fadeTimer.Tick += (s, e) =>
            {
                alpha += 30;
                if (alpha >= 255)
                {
                    alpha = 255;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                    
                    // Восстанавливаем нормальный цвет
                    richTextBox1.SelectionStart = startPos;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionColor = originalColor;
                }
                else
                {
                    richTextBox1.SelectionStart = startPos;
                    richTextBox1.SelectionLength = 1;
                    richTextBox1.SelectionColor = Color.FromArgb(alpha, originalColor.R, originalColor.G, originalColor.B);
                }
            };
            fadeTimer.Start();
        }
    }
}

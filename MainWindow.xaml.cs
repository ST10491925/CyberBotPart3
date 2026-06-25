using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly ChatbotEngine _engine;
        private readonly AudioPlayer   _audio;
        private bool _isTyping;

        private const string PlaceholderChat = "Ask me anything about cybersecurity…";
        private const string PlaceholderTask = "e.g. add task - Enable two-factor authentication";

        public MainWindow()
        {
            InitializeComponent();
            _audio  = new AudioPlayer();
            _engine = new ChatbotEngine();
            _engine.OnBotResponse = async (msg, type) => await AddMessageAsync(msg, type);

            InputTextBox.Text       = PlaceholderChat;
            InputTextBox.Foreground = Muted;
            TaskInputBox.Text       = PlaceholderTask;
            TaskInputBox.Foreground = Muted;
        }

        private static readonly SolidColorBrush Primary = new SolidColorBrush(Color.FromRgb(240, 244, 255));
        private static readonly SolidColorBrush Muted   = new SolidColorBrush(Color.FromRgb(45, 61, 96));

        // ════════════════════════════════════════════════════════
        //  LOAD
        // ════════════════════════════════════════════════════════
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AsciiArtBlock.Text = AsciiArt.Logo;
            await _audio.PlayGreetingAsync();
            await _engine.StartConversationAsync();
        }

        // ════════════════════════════════════════════════════════
        //  TAB NAVIGATION
        // ════════════════════════════════════════════════════════
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelChat == null) return;
            string tag = ((RadioButton)sender).Tag?.ToString() ?? "";
            PanelChat.Visibility  = tag == "Chat"  ? Visibility.Visible : Visibility.Collapsed;
            PanelTasks.Visibility = tag == "Tasks" ? Visibility.Visible : Visibility.Collapsed;
            PanelQuiz.Visibility  = tag == "Quiz"  ? Visibility.Visible : Visibility.Collapsed;
            PanelLog.Visibility   = tag == "Log"   ? Visibility.Visible : Visibility.Collapsed;
            if (tag == "Log") RefreshLogPanel();
        }

        // ════════════════════════════════════════════════════════
        //  CHAT INPUT
        // ════════════════════════════════════════════════════════
        private async void SendButton_Click(object sender, RoutedEventArgs e)
            => await ProcessChatInputAsync();

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.IsRepeat)
            {
                e.Handled = true;
                await ProcessChatInputAsync();
            }
        }

        private async Task ProcessChatInputAsync()
        {
            if (_isTyping) return;
            string text = InputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || text == PlaceholderChat)
            {
                await _engine.HandleEmptyInputAsync();
                ResetBox(InputTextBox, PlaceholderChat);
                return;
            }

            AddUserMessage(text);
            ResetBox(InputTextBox, PlaceholderChat);

            _isTyping            = true;
            SendButton.IsEnabled = false;
            StatusLabel.Text     = "ONLINE  ·  Thinking…";

            await _engine.ProcessInputAsync(text);

            _isTyping            = false;
            SendButton.IsEnabled = true;
            UpdateStatusBar();
            ScrollToBottom();
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == PlaceholderChat)
            {
                InputTextBox.Text       = string.Empty;
                InputTextBox.Foreground = Primary;
            }
        }
        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Text       = PlaceholderChat;
                InputTextBox.Foreground = Muted;
            }
        }

        // ════════════════════════════════════════════════════════
        //  TASKS PANEL
        // ════════════════════════════════════════════════════════
        private async void SendTaskCommand_Click(object sender, RoutedEventArgs e)
        {
            string text = TaskInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || text == PlaceholderTask) return;
            string cmd = text.ToLower().StartsWith("add task") ? text : $"add task - {text}";
            ResetBox(TaskInputBox, PlaceholderTask);
            TabChat.IsChecked = true;
            await _engine.ProcessInputAsync(cmd);
            ScrollToBottom();
        }

        private void AddTaskDialog_Click(object sender, RoutedEventArgs e)
        {
            if (TaskInputBox.Text == PlaceholderTask)
            {
                TaskInputBox.Text       = "add task - ";
                TaskInputBox.Foreground = Primary;
            }
            TaskInputBox.Focus();
            TaskInputBox.CaretIndex = TaskInputBox.Text.Length;
        }

        private void TaskInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskInputBox.Text == PlaceholderTask)
            {
                TaskInputBox.Text       = string.Empty;
                TaskInputBox.Foreground = Primary;
            }
        }
        private void TaskInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskInputBox.Text))
            {
                TaskInputBox.Text       = PlaceholderTask;
                TaskInputBox.Foreground = Muted;
            }
        }

        // ════════════════════════════════════════════════════════
        //  QUIZ PANEL
        // ════════════════════════════════════════════════════════
        private async void StartQuizFromPanel_Click(object sender, RoutedEventArgs e)
        {
            TabChat.IsChecked = true;
            await _engine.ProcessInputAsync("quiz");
            ScrollToBottom();
        }

        // ════════════════════════════════════════════════════════
        //  LOG PANEL
        // ════════════════════════════════════════════════════════
        private void RefreshLog_Click(object sender, RoutedEventArgs e) => RefreshLogPanel();
        private void RefreshLogPanel()
        {
            if (LogTextBlock != null)
                LogTextBlock.Text = ActivityLog.GetLog(20);
        }

        // ════════════════════════════════════════════════════════
        //  SIDEBAR
        // ════════════════════════════════════════════════════════
        private async void TopicBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TabChat.IsChecked       = true;
                InputTextBox.Text       = btn.Tag?.ToString() ?? btn.Content?.ToString() ?? "";
                InputTextBox.Foreground = Primary;
                await ProcessChatInputAsync();
            }
        }

        private void QuickAddTask_Click(object sender, RoutedEventArgs e)
        {
            TabTasks.IsChecked      = true;
            TaskInputBox.Text       = "add task - ";
            TaskInputBox.Foreground = Primary;
            TaskInputBox.Focus();
            TaskInputBox.CaretIndex = TaskInputBox.Text.Length;
        }

        private async void QuickViewTasks_Click(object sender, RoutedEventArgs e)
        {
            TabChat.IsChecked = true;
            await _engine.ProcessInputAsync("show tasks");
            ScrollToBottom();
        }

        private async void QuickStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            TabChat.IsChecked = true;
            await _engine.ProcessInputAsync("quiz");
            ScrollToBottom();
        }

        private void QuickLog_Click(object sender, RoutedEventArgs e)
        {
            TabLog.IsChecked = true;
            RefreshLogPanel();
        }

        // ════════════════════════════════════════════════════════
        //  TOOLBAR
        // ════════════════════════════════════════════════════════
        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
            => await _audio.PlayGreetingAsync();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _engine.ClearHistory();
            ActivityLog.Log("Chat cleared by user");
            AddSystemMessage("── chat cleared ──");
        }

        private async void TipsButton_Click(object sender, RoutedEventArgs e)
        {
            TabChat.IsChecked = true;
            await _engine.ProcessInputAsync("random tip");
            ScrollToBottom();
        }

        // ════════════════════════════════════════════════════════
        //  CHAT MESSAGE RENDERING
        // ════════════════════════════════════════════════════════
        private void AddUserMessage(string text)
        {
            // Right-aligned user bubble with violet-left border
            var bubble = new Border
            {
                CornerRadius        = new CornerRadius(14, 4, 14, 14),
                Padding             = new Thickness(16, 11, 16, 11),
                Margin              = new Thickness(100, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth            = 680,
                BorderThickness     = new Thickness(0, 0, 2, 0),
            };

            // Glass background
            bubble.Background = new LinearGradientBrush(
                Color.FromRgb(26, 15, 58),
                Color.FromRgb(15, 26, 53), 45);

            // Violet right border glow
            bubble.BorderBrush = new LinearGradientBrush(
                Color.FromRgb(124, 58, 237),
                Color.FromRgb(0, 212, 255), 90);

            bubble.Effect = new DropShadowEffect
            {
                Color       = Color.FromRgb(124, 58, 237),
                BlurRadius  = 20,
                ShadowDepth = 0,
                Opacity     = 0.2
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text       = $"You  ·  {DateTime.Now:HH:mm}",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize   = 10,
                Margin     = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Color.FromRgb(157, 98, 255))
            });
            stack.Children.Add(new TextBlock
            {
                Text         = text,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 13.5,
                TextWrapping = TextWrapping.Wrap,
                LineHeight   = 22,
                Foreground   = new SolidColorBrush(Color.FromRgb(240, 244, 255))
            });

            bubble.Child = stack;
            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private async Task AddMessageAsync(string text, MessageType type)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var bubble = BuildBotBubble(type);
                var body   = (TextBlock)((StackPanel)bubble.Child).Children[1];
                ChatPanel.Children.Add(bubble);
                ScrollToBottom();
                await TypingEffectAsync(body, text);
                ScrollToBottom();
                UpdateStatusBar();
            });
        }

        private static Border BuildBotBubble(MessageType type)
        {
            // Colour scheme per type
            (Color accent, Color bg1, Color bg2, string label) = type switch
            {
                MessageType.Bot       => (Color.FromRgb(0, 229, 160),  Color.FromRgb(0, 22, 14),  Color.FromRgb(5,  14, 10),  "CyberBot"),
                MessageType.Warning   => (Color.FromRgb(255, 68, 102), Color.FromRgb(22, 5,  10),  Color.FromRgb(15, 5,  8),   "Alert"),
                MessageType.Sentiment => (Color.FromRgb(255, 183, 0),  Color.FromRgb(22, 16, 3),  Color.FromRgb(14, 10, 3),   "Empathy"),
                _                     => (Color.FromRgb(74, 85, 120),  Color.FromRgb(10, 14, 24), Color.FromRgb(8,  11, 18),  "System")
            };

            var bubble = new Border
            {
                CornerRadius        = new CornerRadius(4, 14, 14, 14),
                Padding             = new Thickness(16, 11, 16, 11),
                Margin              = new Thickness(0, 5, 100, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth            = 680,
                BorderThickness     = new Thickness(2, 0, 0, 0),
                BorderBrush         = new SolidColorBrush(accent),
                Background          = new LinearGradientBrush(bg1, bg2, 45),
                Effect = new DropShadowEffect
                {
                    Color       = accent,
                    BlurRadius  = 18,
                    ShadowDepth = 0,
                    Opacity     = 0.18
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text       = $"{label}  ·  {DateTime.Now:HH:mm}",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize   = 10,
                Margin     = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(accent)
            });
            stack.Children.Add(new TextBlock
            {
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 13.5,
                TextWrapping = TextWrapping.Wrap,
                LineHeight   = 22,
                Foreground   = new SolidColorBrush(Color.FromRgb(240, 244, 255))
            });

            bubble.Child = stack;
            return bubble;
        }

        private static async Task TypingEffectAsync(TextBlock block, string text)
        {
            int delay = text.Length > 300 ? 4 : text.Length > 150 ? 8 : 12;
            foreach (char c in text)
            {
                block.Text += c;
                await Task.Delay(delay);
            }
        }

        private void AddSystemMessage(string text)
        {
            ChatPanel.Children.Add(new TextBlock
            {
                Text                = text,
                FontFamily          = new FontFamily("Consolas"),
                FontSize            = 10,
                Foreground          = new SolidColorBrush(Color.FromRgb(29, 37, 60)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 10, 0, 10)
            });
        }

        // ════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════
        private void ScrollToBottom() => ChatScrollViewer.ScrollToEnd();

        private void ResetBox(TextBox box, string placeholder)
        {
            box.Text       = string.Empty;
            box.Foreground = Primary;
            box.Focus();
        }

        private void UpdateStatusBar()
        {
            string user        = _engine.GetCurrentUserName();
            StatusLabel.Text   = "ONLINE  ·  Ready to assist";
            UserInfoLabel.Text = string.IsNullOrEmpty(user) ? "Not identified" : user;
        }
    }

    public enum MessageType { Bot, Warning, Sentiment, System }
}

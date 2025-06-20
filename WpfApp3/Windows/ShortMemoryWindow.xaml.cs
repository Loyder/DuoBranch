using System;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp3.Windows
{
    public partial class ShortMemoryWindow : Window
    {
        public event EventHandler<int> IQRewarded;
        private bool _ready = false;

        public ShortMemoryWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => ShowWords();
        }

        private void ShowWords()
        {
            var dlg = new Window
            {
                Width = 250,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = null,
                Content = new System.Windows.Controls.TextBlock
                {
                    Text = "КОТ СТОЛ ЛИСА",
                    FontSize = 32,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Topmost = true,
                ShowInTaskbar = false,
                Owner = this
            };
            dlg.Show();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            timer.Tick += (s, e) =>
            {
                dlg.Close();
                timer.Stop();
                _ready = true;
            };
            timer.Start();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (!_ready)
            {
                MessageBox.Show("Сначала посмотрите слова!");
                return;
            }
            var ans = AnswerTextBox.Text.ToUpper();
            if (ans.Contains("КОТ") && ans.Contains("СТОЛ") && ans.Contains("ЛИСА"))
            {
                MessageBox.Show("Верно! +10 к IQ");
                IQRewarded?.Invoke(this, 10);
            }
            else
            {
                MessageBox.Show("Неверно! Попытка была одна.");
            }
            this.Close();
        }
    }
}
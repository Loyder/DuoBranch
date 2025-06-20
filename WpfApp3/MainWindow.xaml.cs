using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfApp3.Windows;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        private int _iq = 0;
        private string _currentImagePath = "";

        public MainWindow()
        {
            InitializeComponent();
            UpdateIqUi();
        }

        private void OnIncreaseIQClick(object sender, RoutedEventArgs e)
        {
            _iq += 20;
            if (_iq > 200) _iq = 200;
            UpdateIqUi();
        }

        private void OnDoomClick(object sender, RoutedEventArgs e)
        {
            var win = new DoomWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnLogicSequenceClick(object sender, RoutedEventArgs e)
        {
            var win = new LogicSequenceWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnLogicOddOneClick(object sender, RoutedEventArgs e)
        {
            var win = new OddOneWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnMemorySequenceClick(object sender, RoutedEventArgs e)
        {
            var win = new MemorySequenceWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnAnalogyClick(object sender, RoutedEventArgs e)
        {
            var win = new AnalogyWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnArithmeticClick(object sender, RoutedEventArgs e)
        {
            var win = new ArithmeticWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnPairsClick(object sender, RoutedEventArgs e)
        {
            var win = new PairsWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnShortMemoryClick(object sender, RoutedEventArgs e)
        {
            var win = new ShortMemoryWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void OnSortNumbersClick(object sender, RoutedEventArgs e)
        {
            var win = new SortNumbersWindow();
            win.IQRewarded += (s, reward) =>
            {
                _iq += reward;
                if (_iq > 200) _iq = 200;
                UpdateIqUi();
            };
            win.ShowDialog();
        }

        private void UpdateIqUi()
        {
            IQprogress.Value = _iq;
            ProgressText.Text = $"{_iq} IQ";
            IQDescription.Text = GetIqDescription(_iq);
            UpdateComparisonImage(_iq);
        }

        private string GetIqDescription(int iq)
        {
            if (iq < 20) return "Абсолютный кретин";
            if (iq < 50) return "Кретин";
            if (iq < 80) return "Средний обыватель";
            if (iq < 120) return "Умный";
            if (iq < 160) return "Очень умный";
            if (iq < 200) return "Гений";
            return "Гений вселенной";
        }

        private void UpdateComparisonImage(int iq)
        {
            string newPath;
            if (iq <= 50)
                newPath = "Resources/Images/sonofwhore.jpg";
            else if (iq <= 80)
                newPath = "Resources/Images/zolo.png";
            else
                newPath = "Resources/Images/genius.jpg";

            if (_currentImagePath == newPath)
                return;

            _currentImagePath = newPath;
            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(180)));
            fadeOut.Completed += (s, e) =>
            {
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri($"pack://application:,,,/{newPath}", UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                ComparisonImage.Source = img;

                var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(220)));
                ComparisonImage.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, fadeIn);
            };
            ComparisonImage.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, fadeOut);
        }
    }
}
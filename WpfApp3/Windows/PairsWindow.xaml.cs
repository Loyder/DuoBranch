using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp3.Windows
{
    public partial class PairsWindow : Window
    {
        public event EventHandler<int> IQRewarded;

        private List<Button> _cards = new List<Button>();
        private List<string> _values = new List<string>();
        private Button _firstCard = null;
        private Button _secondCard = null;
        private int _foundPairs = 0;
        private bool _gameOver = false;

        public PairsWindow()
        {
            InitializeComponent();
            SetupGame();
        }

        private void SetupGame()
        {
            _values = new List<string> { "🍎", "🍎", "🍊", "🍊" };
            var rnd = new Random();
            _values = _values.OrderBy(x => rnd.Next()).ToList();

            _cards.Clear();
            CardGrid.Children.Clear();

            for (int i = 0; i < 4; i++)
            {
                var btn = new Button
                {
                    Tag = _values[i],
                    Content = "",
                    FontSize = 32,
                    Margin = new Thickness(8)
                };
                btn.Click += Card_Click;
                _cards.Add(btn);
                CardGrid.Children.Add(btn);
            }

            _firstCard = null;
            _secondCard = null;
            _foundPairs = 0;
            _gameOver = false;
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver)
                return;

            var btn = sender as Button;
            if (btn == null || btn.Content.ToString() != "" || _secondCard != null)
                return;

            btn.Content = btn.Tag.ToString();

            if (_firstCard == null)
            {
                _firstCard = btn;
            }
            else
            {
                _secondCard = btn;
                if (_firstCard.Tag.ToString() == _secondCard.Tag.ToString())
                {
                    _firstCard.IsEnabled = false;
                    _secondCard.IsEnabled = false;
                    _firstCard = null;
                    _secondCard = null;
                    _foundPairs++;
                    if (_foundPairs == 2)
                    {
                        MessageBox.Show("Победа! +10 к IQ");
                        IQRewarded?.Invoke(this, 10);
                        _gameOver = true;
                        this.Close();
                    }
                }
                else
                {
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(700);
                    timer.Tick += (s, ev) =>
                    {
                        _firstCard.Content = "";
                        _secondCard.Content = "";
                        timer.Stop();
                        MessageBox.Show("Неверно! Попытка была одна.");
                        _gameOver = true;
                        this.Close();
                    };
                    timer.Start();
                }
            }
        }
    }
}
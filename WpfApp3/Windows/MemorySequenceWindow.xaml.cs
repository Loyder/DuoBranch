using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp3.Windows
{
    public partial class MemorySequenceWindow : Window
    {
        public event EventHandler<int> IQRewarded;

        private int _currentStage = 1;
        private const int MaxStages = 5;
        private int _correctCount = 0;
        private bool _closedByButton = false;
        private bool _alreadyWarned = false;

        private Random _rnd = new Random();
        private List<int> _currentSequence = new List<int>();
        private DispatcherTimer _showTimer;

        public MemorySequenceWindow()
        {
            InitializeComponent();
            this.Closing += MemorySequenceWindow_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStage();
        }

        private void ShowStage()
        {
            StageTextBlock.Text = $"Этап {_currentStage} из {MaxStages}";
            TaskTextBlock.Text = $"Запомните и повторите последовательность:";
            SequenceTextBlock.Text = "";
            AnswerTextBox.Text = "";
            AnswerTextBox.IsEnabled = false;
            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Ответить";
            ResultTextBlock.Text = "";
            ResultTextBlock.Foreground = System.Windows.Media.Brushes.Black;

            GenerateAndShowSequence();
        }

        private void GenerateAndShowSequence()
        {
            int length = 3 + _currentStage; // Например: 4-8 цифр
            _currentSequence = Enumerable.Range(0, length)
                .Select(x => _rnd.Next(0, 10))
                .ToList();

            SequenceTextBlock.Text = string.Join(" ", _currentSequence);

            _showTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2 + length * 0.25) };
            _showTimer.Tick += (s, e) =>
            {
                _showTimer.Stop();
                SequenceTextBlock.Text = "";
                AnswerTextBox.IsEnabled = true;
                SubmitButton.IsEnabled = true;
            };
            _showTimer.Start();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (SubmitButton.Content.ToString() == "Закрыть")
            {
                _closedByButton = true;
                this.Close();
                return;
            }

            if (!AnswerTextBox.IsEnabled)
                return;

            string user = new string(AnswerTextBox.Text.Where(c => char.IsDigit(c)).ToArray());
            string correct = string.Join("", _currentSequence);

            if (user == correct)
            {
                ResultTextBlock.Text = "Верно!";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                _correctCount++;
            }
            else
            {
                ResultTextBlock.Text = $"Неверно! Правильно: {string.Join(" ", _currentSequence)}";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.DarkRed;
            }

            AnswerTextBox.IsEnabled = false;
            SubmitButton.IsEnabled = false;

            if (_currentStage < MaxStages)
            {
                _currentStage++;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.3) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    ShowStage();
                };
                timer.Start();
            }
            else
            {
                ShowFinalResult();
            }
        }

        private void ShowFinalResult()
        {
            int totalIQ = _correctCount * 10;
            StageTextBlock.Text = "Тест завершён!";
            TaskTextBlock.Text = "";
            SequenceTextBlock.Text = "";
            ResultTextBlock.Text = $"Результат: {_correctCount} из {MaxStages} правильных.\nНачислено IQ: {totalIQ}";
            ResultTextBlock.Foreground = System.Windows.Media.Brushes.DarkGreen;

            IQRewarded?.Invoke(this, totalIQ);

            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Закрыть";
        }

        private void MemorySequenceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_closedByButton && !_alreadyWarned)
            {
                e.Cancel = true;
                _alreadyWarned = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(
                        "Вы прервали текущее задание.\nЗа это вам прилагается штраф в размере 10 IQ.",
                        "Штраф за прерывание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    IQRewarded?.Invoke(this, -10);
                    _closedByButton = true;
                    this.Close();
                }));
            }
        }
    }
}
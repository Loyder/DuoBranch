using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp3.Windows
{
    public partial class LogicSequenceWindow : Window
    {
        public event EventHandler<int> IQRewarded;

        private int _currentStage = 1;
        private const int MaxStages = 5;
        private int _correctCount = 0;
        private bool _closedByButton = false;
        private bool _alreadyWarned = false;

        private enum SeqType { Arithmetic, Geometric, Squares, Cubes, Triangular, Fibonacci }
        private static readonly Random _rnd = new Random();
        private (string sequence, string answer) _currentTask;

        public LogicSequenceWindow()
        {
            InitializeComponent();
            this.Closing += LogicSequenceWindow_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStage();
        }

        private void ShowStage()
        {
            StageTextBlock.Text = $"Этап {_currentStage} из {MaxStages}";
            TaskTextBlock.Text = $"Продолжите последовательность:";
            SequenceTextBlock.Text = "";
            AnswerTextBox.Text = "";
            AnswerTextBox.IsEnabled = true;
            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Ответить";
            ResultTextBlock.Text = "";
            ResultTextBlock.Foreground = System.Windows.Media.Brushes.Black;

            _currentTask = GenerateRandomSequence();
            SequenceTextBlock.Text = _currentTask.sequence;
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

            string userAnswer = AnswerTextBox.Text.Trim();
            bool correct = string.Equals(userAnswer, _currentTask.answer, StringComparison.OrdinalIgnoreCase);

            if (correct)
            {
                ResultTextBlock.Text = "Верно!";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                _correctCount++;
            }
            else
            {
                ResultTextBlock.Text = $"Неверно! Правильный ответ: {_currentTask.answer}";
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

        private void LogicSequenceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        // Генерация одной случайной числовой последовательности
        private (string sequence, string answer) GenerateRandomSequence()
        {
            SeqType type = (SeqType)_rnd.Next(0, 6);

            switch (type)
            {
                case SeqType.Arithmetic:
                    {
                        int start = _rnd.Next(1, 15);
                        int step = _rnd.Next(2, 7);
                        int len = _rnd.Next(4, 6);
                        List<int> seq = new List<int>();
                        for (int i = 0; i < len; i++)
                            seq.Add(start + step * i);
                        string answer = (start + step * len).ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
                case SeqType.Geometric:
                    {
                        int start = _rnd.Next(1, 5);
                        int mult = _rnd.Next(2, 4);
                        int len = _rnd.Next(4, 6);
                        List<int> seq = new List<int>();
                        int val = start;
                        for (int i = 0; i < len; i++)
                        {
                            seq.Add(val);
                            val *= mult;
                        }
                        string answer = val.ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
                case SeqType.Squares:
                    {
                        int n = _rnd.Next(2, 6);
                        List<int> seq = new List<int>();
                        for (int i = 1; i <= n; i++)
                            seq.Add(i * i);
                        string answer = ((n + 1) * (n + 1)).ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
                case SeqType.Cubes:
                    {
                        int n = _rnd.Next(2, 4);
                        List<int> seq = new List<int>();
                        for (int i = 1; i <= n; i++)
                            seq.Add(i * i * i);
                        string answer = ((n + 1) * (n + 1) * (n + 1)).ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
                case SeqType.Triangular:
                    {
                        int n = _rnd.Next(3, 6);
                        List<int> seq = new List<int>();
                        for (int i = 1; i <= n; i++)
                            seq.Add(i * (i + 1) / 2);
                        string answer = ((n + 1) * (n + 2) / 2).ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
                case SeqType.Fibonacci:
                default:
                    {
                        int len = _rnd.Next(5, 7);
                        List<int> seq = new List<int> { 1, 1 };
                        for (int i = 2; i < len; i++)
                            seq.Add(seq[i - 1] + seq[i - 2]);
                        string answer = (seq[len - 1] + seq[len - 2]).ToString();
                        return ($"{string.Join(", ", seq)}, ?", answer);
                    }
            }
        }
    }
}
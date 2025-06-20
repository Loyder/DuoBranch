using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp3.Windows
{
    public partial class OddOneWindow : Window
    {
        public event EventHandler<int> IQRewarded;

        private int _currentStage = 1;
        private const int MaxStages = 5;
        private int _correctCount = 0;

        private bool _closedByButton = false;
        private bool _alreadyWarned = false;

        private static readonly Random _rnd = new Random();

        // Словари тематик для генерации "лишнего"
        private readonly List<string[]> _themes = new List<string[]>
        {
            // Животные млекопитающие
            new[] { "кот", "собака", "корова", "лошадь", "тигр", "волк" },
            // Птицы
            new[] { "орёл", "воробей", "голубь", "ласточка", "сова", "аист" },
            // Фрукты
            new[] { "яблоко", "груша", "банан", "слива", "апельсин", "абрикос" },
            // Овощи
            new[] { "помидор", "огурец", "морковь", "свёкла", "перец", "капуста" },
            // Цвета
            new[] { "красный", "жёлтый", "зелёный", "синий", "голубой", "оранжевый" },
            // Времена года
            new[] { "зима", "весна", "лето", "осень" },
            // Транспорт
            new[] { "поезд", "самолёт", "автобус", "корабль", "трамвай", "метро" }
        };

        // Список "помех" — лишних слов разных категорий
        private readonly List<string> _oddWords = new List<string>
        {
            "дерево", "река", "солнце", "стол", "телефон", "мышь", "камень", "чайник",
            "молоко", "сыр", "час", "кофта", "лампа", "рефрижератор"
        };

        // Текущее задание
        private string[] _currentWords;
        private int _oddIndex;

        public OddOneWindow()
        {
            InitializeComponent();
            this.Closing += OddOneWindow_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStage();
        }

        private void ShowStage()
        {
            GenerateOddOneTask();

            StageTextBlock.Text = $"Этап {_currentStage} из {MaxStages}";
            TaskTextBlock.Text = $"Что лишнее?\n{string.Join(", ", _currentWords)}";
            AnswerTextBox.Text = "";
            AnswerTextBox.IsEnabled = true;
            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Ответить";
            ResultTextBlock.Text = "";
            ResultTextBlock.Foreground = System.Windows.Media.Brushes.Black;
        }

        // Генерация нового задания
        private void GenerateOddOneTask()
        {
            // Выбор темы
            int themeIndex = _rnd.Next(_themes.Count);
            var theme = _themes[themeIndex];

            // Копируем 3 случайных слова из темы
            var words = new List<string>();
            var used = new HashSet<int>();
            while (words.Count < 3)
            {
                int idx = _rnd.Next(theme.Length);
                if (!used.Contains(idx))
                {
                    words.Add(theme[idx]);
                    used.Add(idx);
                }
            }

            // Решаем, какое слово будет "лишним": 50% это из oddWords, 50% — из другой темы
            string odd;
            if (_rnd.Next(2) == 0)
            {
                odd = _oddWords[_rnd.Next(_oddWords.Count)];
            }
            else
            {
                // Берём слово из другой рандомной темы
                int otherTheme;
                do { otherTheme = _rnd.Next(_themes.Count); } while (otherTheme == themeIndex);
                var other = _themes[otherTheme];
                odd = other[_rnd.Next(other.Length)];
            }

            // Вставляем "лишнее" слово на случайную позицию
            int oddPos = _rnd.Next(4);
            words.Insert(oddPos, odd);

            _currentWords = words.ToArray();
            _oddIndex = oddPos;
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

            var userAnswer = AnswerTextBox.Text.Trim();
            bool correct = (userAnswer == (_oddIndex + 1).ToString()) ||
                           (userAnswer.Equals(_currentWords[_oddIndex], StringComparison.OrdinalIgnoreCase));

            if (correct)
            {
                ResultTextBlock.Text = "Верно!";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                _correctCount++;
            }
            else
            {
                ResultTextBlock.Text = $"Неверно! Лишнее: {_oddIndex + 1}. {_currentWords[_oddIndex]}";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.DarkRed;
            }

            AnswerTextBox.IsEnabled = false;
            SubmitButton.IsEnabled = false;

            if (_currentStage < MaxStages)
            {
                _currentStage++;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.4) };
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
            ResultTextBlock.Text = $"Результат: {_correctCount} из {MaxStages} правильных.\nНачислено IQ: {totalIQ}";
            ResultTextBlock.Foreground = System.Windows.Media.Brushes.DarkGreen;

            IQRewarded?.Invoke(this, totalIQ);

            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Закрыть";
        }

        private void OddOneWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
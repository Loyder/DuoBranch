using System;
using System.Windows;

namespace WpfApp3.Windows
{
    public partial class AnalogyWindow : Window
    {
        public event EventHandler<int> IQRewarded;
        public AnalogyWindow()
        {
            InitializeComponent();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            var ans = AnswerTextBox.Text.ToLower();
            if (ans.Contains("ученик") || ans.Contains("школьник"))
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
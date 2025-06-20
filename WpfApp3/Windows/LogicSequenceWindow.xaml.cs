using System;
using System.Windows;

namespace WpfApp3.Windows
{
    public partial class LogicSequenceWindow : Window
    {
        public event EventHandler<int> IQRewarded;
        public LogicSequenceWindow()
        {
            InitializeComponent();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (AnswerTextBox.Text.Trim() == "32")
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
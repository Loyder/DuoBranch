using System;
using System.Windows;

namespace WpfApp3.Windows
{
    public partial class OddOneWindow : Window
    {
        public event EventHandler<int> IQRewarded;
        public OddOneWindow()
        {
            InitializeComponent();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (AnswerTextBox.Text.ToLower().Contains("слон"))
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
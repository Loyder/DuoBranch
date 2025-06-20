using System;
using System.Windows;

namespace WpfApp3.Windows
{
    public partial class ArithmeticWindow : Window
    {
        public event EventHandler<int> IQRewarded;
        public ArithmeticWindow()
        {
            InitializeComponent();
        }

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (AnswerTextBox.Text.Trim() == "43")
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
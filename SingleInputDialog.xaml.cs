using System.Windows;

namespace EasyFramework
{
    /// <summary>
    /// Interaction logic for SingleInputDialog.xaml
    /// </summary>
    public partial class SingleInputDialog : Window
    {
        public string LabelText;
        public string Value;
        public SingleInputDialog(Window owner, string value, string labelText)
        {
            Owner = owner;
            this.Value = value;
            this.LabelText = $"{labelText}:";
            InitializeComponent();
            Title = $"Set {labelText}";
            LabelValue.Content = this.LabelText;
            TextBoxValue.Text = value;
            TextBoxValue.Focus();
            TextBoxValue.SelectAll();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Value = TextBoxValue.Text;
            DialogResult = Value.Length > 0;
        }
    }
}
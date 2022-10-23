using System.Windows;

namespace BOLL7708
{
    /// <summary>
    /// Interaction logic for SingleInputDialog.xaml
    /// </summary>
    public partial class SingleInputDialog : Window
    {
        public string labelText;
        public string value;
        public SingleInputDialog(Window owner, string value, string labelText)
        {
            Owner = owner;
            this.value = value;
            this.labelText = $"{labelText}:";
            InitializeComponent();
            Title = $"Set {labelText}";
            labelValue.Content = this.labelText;
            textBoxValue.Text = value;
            textBoxValue.Focus();
            textBoxValue.SelectAll();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            value = textBoxValue.Text;
            DialogResult = value.Length > 0;
        }
    }
}
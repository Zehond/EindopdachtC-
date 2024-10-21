using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// [StructLayout(LayoutKind.Sequential)]

    public partial class MainWindow : Window
    {
        /// <summary>
        /// ObservableCollection zorgt ervoor dat de UI automatisch wordt bijgewerkt wanneer een taak wordt verplaatst.
        /// </summary>
        public ObservableCollection<string> TodoItems { get; set; }
        public ObservableCollection<string> InProgressItems { get; set; }
        public ObservableCollection<string> DoneItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            TodoItems = new ObservableCollection<string>() { "Task 1", "Task 2" };
            InProgressItems = new ObservableCollection<string>() { "Task 3" };
            DoneItems = new ObservableCollection<string>() { "Task 4" };
            DataContext = this;
        }
        /// <summary>
        /// deze methode start het drag en drop proces wanneer de muis wordt ingedrukt en bewogen
        /// </summary>
        private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock != null)
                {
                    DragDrop.DoDragDrop(textBlock, textBlock.Text, DragDropEffects.Move);
                }
            }
        }
        /// <summary>
        /// handelt het drag over evenement. vooral voor gebruiksvriendelijkheid (visuele feedback)
        /// </summary>
        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        /// <summary>
        /// handelt de drop van de item en verplaats de taak naar de andere colom. 
        /// </summary>
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string task = (string)e.Data.GetData(DataFormats.StringFormat);
                ListBox listBox = sender as ListBox;
                ObservableCollection<string> targetList = listBox.ItemsSource as ObservableCollection<string>;

                if (targetList != null)
                {
                    if (TodoItems.Contains(task)) TodoItems.Remove(task);
                    else if (InProgressItems.Contains(task)) InProgressItems.Remove(task);
                    else if (DoneItems.Contains(task)) DoneItems.Remove(task);

                    
                    targetList.Add(task);
                }
            }
        }

    }
}

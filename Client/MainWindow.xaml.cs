using ClientServerUtilsSharedProject;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;




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
        public ObservableCollection<TaskItem> TodoItems { get; set; }
        public ObservableCollection<TaskItem> InProgressItems { get; set; }
        public ObservableCollection<TaskItem> DoneItems { get; set; }
        private NetworkManager networkManager = NetworkManager.Instance;

        public MainWindow()
        {
            InitializeComponent();
            TodoItems = new ObservableCollection<TaskItem>();
            InProgressItems = new ObservableCollection<TaskItem>();
            DoneItems = new ObservableCollection<TaskItem>();
            DataContext = this;
            
            OpenNetworkManagerConnectDialog();
            networkManager.TasksUpdated += OnTasksUpdated;


            // CHECKTHIS this is fucked and doesnt work, can you pls check it? operation order:
            // start server > start client > everything should work
            // disconect server > client reconnect window pops up
            // start server again > reconnect on client > (currently breaks) everything should be flushed
            networkManager.ServerDisconnected += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenNetworkManagerConnectDialog();
                });
                networkManager.getAllTask();
            };
            networkManager.NoTasksOnServer += () =>
            {
                MessageBox.Show("No tasks found on server. Feel free to Add tasks.");
            };
            // END


            networkManager.getAllTask();
        }

        private void OpenNetworkManagerConnectDialog()
        {
            var dialog = new ConnectDialog();
            if (dialog.ShowDialog() == false)
            {
                Close();
                return;
            }
        }

        private void OnTasksUpdated(List<TaskItem> taskItems)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TodoItems.Clear();
                InProgressItems.Clear();
                DoneItems.Clear();

                foreach (var task in taskItems)
                {
                    // Verdeel de taken over de juiste lijsten
                    switch (task.State)
                    {
                        case TaskItem.TaskState.ToDo:
                            TodoItems.Add(task);
                            break;
                        case TaskItem.TaskState.Progress:
                            InProgressItems.Add(task);
                            break;
                        case TaskItem.TaskState.Done:
                            DoneItems.Add(task);
                            break;
                    }
                }
            });

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO remove if needed
        }

        /// <summary>
        /// deze methode start het drag en drop proces wanneer de muis wordt ingedrukt en bewogen
        /// </summary>
        private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var taskItem = ((FrameworkElement)sender).DataContext as TaskItem;
                if (taskItem != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, taskItem, DragDropEffects.Move);
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
            if (e.Data.GetDataPresent(typeof(TaskItem)))
            {
                var task = (TaskItem)e.Data.GetData(typeof(TaskItem));
                var listBox = sender as ListBox;
                var targetList = listBox.ItemsSource as ObservableCollection<TaskItem>;

                if (targetList != null)
                {
                    if (TodoItems.Contains(task)) TodoItems.Remove(task);
                    else if (InProgressItems.Contains(task)) InProgressItems.Remove(task);
                    else if (DoneItems.Contains(task)) DoneItems.Remove(task);

                    if (listBox == ToDoListBox)
                    {
                        task.State = TaskItem.TaskState.ToDo;
                    }
                    else if (listBox == InProgressListBox)
                    {
                        task.State = TaskItem.TaskState.Progress;
                    }
                    else if (listBox == DoneListBox)
                    {
                        task.State = TaskItem.TaskState.Done;
                    }

                    targetList.Add(task);
                }
                listBox.Items.Refresh();
            }
        }

        private int taskIdCounter = 1; // Task ID counter

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var task = new TaskItem
                {
                    Id = taskIdCounter.ToString(),
                    Name = dialog.TaskName,
                    Description = dialog.TaskDescription,
                    State = TaskItem.TaskState.ToDo// todo make state change depending where it is
                };
                taskIdCounter++;
                AddTask(task);
            }
        }

        private void AddTask(TaskItem task)
        {
            TodoItems.Add(task);
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            // Logic to remove a task
            var selectedTask = GetSelectedTask();
            if (selectedTask != null)
            {
                RemoveTask(selectedTask);
            }
        }

        //private void EditTask_Click(object sender, RoutedEventArgs e)
        //{
        //    // Logic to edit a task
        //    var selectedTask = GetSelectedTask();
        //    if (selectedTask != null)
        //    {
        //        var dialog = new AddTaskDialog
        //        {
        //            TaskName = selectedTask.Name,
        //            TaskDescription = selectedTask.Description
        //        };

        //        if (dialog.ShowDialog() == true)
        //        {
        //            selectedTask.Name = dialog.TaskName;
        //            selectedTask.Description = dialog.TaskDescription;
        //            EditTask(selectedTask);
        //        }
        //    }
        //}
        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = GetSelectedTask();
            if (selectedTask != null)
            {
                var dialog = new AddTaskDialog
                {
                    TaskName = selectedTask.Name,
                    TaskDescription = selectedTask.Description
                };

                if (dialog.ShowDialog() == true)
                {
                    selectedTask.Name = dialog.TaskName;
                    selectedTask.Description = dialog.TaskDescription;
                    EditTask(selectedTask);

                    // Refresh ListBox to reflect changes
                    ToDoListBox.Items.Refresh();
                    InProgressListBox.Items.Refresh();
                    DoneListBox.Items.Refresh();
                }
            }
        }

        private TaskItem GetSelectedTask()
        {
            return ToDoListBox.SelectedItem as TaskItem ??
                   InProgressListBox.SelectedItem as TaskItem ??
                   DoneListBox.SelectedItem as TaskItem;

        }

        private void RemoveTask(TaskItem task)
        {
            //todo needs work, maybe look at ID
            // Remove task from appropriate list
            TodoItems.Remove(task);
            InProgressItems.Remove(task);
            DoneItems.Remove(task);
        }

        private void EditTask(TaskItem task)
        {
            // Update task locally work on also sending to server
            var targetList = TodoItems.FirstOrDefault(t => t.Id == task.Id) != null ? TodoItems :
                             InProgressItems.FirstOrDefault(t => t.Id == task.Id) != null ? InProgressItems :
                             DoneItems;

            var index = targetList.IndexOf(task);
            targetList[index] = task;
        }
    }
}

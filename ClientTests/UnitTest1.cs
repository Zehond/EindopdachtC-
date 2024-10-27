using Client;
using ClientServerUtilsSharedProject;
using System.Windows;
using System.Windows.Controls;

namespace ClientTests
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void MainWindow_DataBinding_Test()
        //{
        //    var thread = new Thread(() =>
        //    {
        //        MainWindow mainWindow = new MainWindow();
        //        mainWindow.TodoItems.Add(new TaskItem { Id = "1", Description = "test'", Name = "Test Task", State = TaskItem.TaskState.ToDo });
        //        Assert.AreEqual(1, mainWindow.TodoItems.Count);
        //        Assert.AreEqual("Test Task", mainWindow.TodoItems[0].Name);
        //    });
        //    //omdat het een WPF project is, kan het geen Multi-Threaded Apartment runnen, alleen Single-Thread appartment
        //    //STA is dus nu zo gemaakt: maakt een nieuwe thread aan om de test op te runnen, dan set je de apartment van de thread op STA, en dan start je de thread en wacht je tot die klaar is met join.
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //    thread.Join();
        //}
        //[TestMethod]
        //public void MainWindow_DragDrop_Test()
        //{
        //    var thread = new Thread(() =>
        //    {
        //        MainWindow mainWindow = new MainWindow();
        //        mainWindow.Show(); // Ensure the window is loaded

        //        // Find the ToDoListBox by name
        //        ListBox toDoListBox = (ListBox)mainWindow.FindName("ToDoListBox");

        //        TaskItem task = new TaskItem { Id = "1", Description = "test'", Name = "Test Task", State = TaskItem.TaskState.ToDo };
        //        mainWindow.TodoItems.Add(task);

        //        // Simulate drag-and-drop
        //        mainWindow.SimulateDragDrop(task, toDoListBox);

        //        Assert.AreEqual(TaskItem.TaskState.ToDo, task.State);
        //    });

        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //    thread.Join();
        //}
        [TestMethod]
        public void MainWindow_AddTask_Test()
        {
            var thread = new Thread(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show(); // Ensure the window is loaded

                TaskItem task = new TaskItem { Id = "1", Description = "test'", Name = "Test Task", State = TaskItem.TaskState.ToDo };
                mainWindow.AddTask(task);

                // Check if the task is added to the collection
                Assert.AreEqual(1, mainWindow.TodoItems.Count);
                Assert.AreEqual("Test Task", mainWindow.TodoItems[0].Name);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
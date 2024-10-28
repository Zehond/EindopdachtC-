using ClientServerUtilsSharedProject;
using System.Linq;

namespace Server.nUnitTest
{
    public class ProgramTests
    {
        private const string FILE_PATH = "test_file.json";
        private TaskItem task1;
        private TaskItem task2;
        private TaskItem task3;

        [SetUp]
        public void Setup()
        {
            task1 = new TaskItem() { Id = "1", Name = "test1", Description = "testing item 1", State = TaskItem.TaskState.ToDo };
            task2 = new TaskItem() { Id = "1", Name = "test2", Description = "edited item 1 into item 2", State = TaskItem.TaskState.Progress };
            task3 = new TaskItem() { Id = "3", Name = "test3", Description = "edited item 1 into item 2", State = TaskItem.TaskState.Progress };
        }

        [Test]
        public void AddTaskTest()
        {
            Program.AddTask(task1);
            Assert.Contains(task1, Program.TasksItems);
        }

        [Test]
        public void EditTaskTest()
        {
            Program.EditTask(task2);
            Assert.Contains(task2, Program.TasksItems, "item 1 was not converted to item 2");
            Assert.IsEmpty(Program.TasksItems.Intersect([task1]), "item 1 was not deleted from list");
        }

        [Test]
        public void RemoveTaskTest()
        {
            Program.AddTask(task3);
            Assert.Contains(task3, Program.TasksItems, "add task is required for this task, henceforth the failure");

            Program.RemoveTask(task3);
            Assert.IsEmpty(Program.TasksItems.Intersect([task3]), "task was added, but removing failed");
        }

        [Test]
        public void GenerateUniqueIdTest()
        {
            if (Program.TasksItems.Count != 0)
            {
                // items from last test found
                Program.TasksItems.Clear();
            }

            Program.TasksItems.Add(new TaskItem() { Id = "1", Name = "generateIdTest1", Description = "testing item 1", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "2", Name = "generateIdTest2", Description = "testing item 2", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "3", Name = "generateIdTest3", Description = "testing item 3", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "5", Name = "generateIdTestNumberSkip5", Description = "testing item 5", State = TaskItem.TaskState.ToDo });

            Assert.That(Program.GenerateUniqueId(), Is.EqualTo(4));
        }

        [Test]
        public void SaveFileTest()
        {
            if (Program.TasksItems.Count != 0)
            {
                // items from last test found
                Program.TasksItems.Clear();
            }

            Program.TasksItems.Add(new TaskItem() { Id = "1", Name = "generateIdTest1", Description = "testing item 1", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "2", Name = "generateIdTest2", Description = "testing item 2", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "3", Name = "generateIdTest3", Description = "testing item 3", State = TaskItem.TaskState.ToDo });
            Program.TasksItems.Add(new TaskItem() { Id = "5", Name = "generateIdTestNumberSkip5", Description = "testing item 5", State = TaskItem.TaskState.ToDo });

            Program.saveTasksToFile(FILE_PATH);
            Assert.IsTrue(File.Exists(FILE_PATH));
        }

        [Test]
        public void LoadListFromFileTest()
        {
            if (Program.TasksItems.Count != 0)
            {
                // items from last test found
                Program.TasksItems.Clear();
            }

            Program.loadFromFile(FILE_PATH);

            string ID1 = Program.TasksItems[0].Id;
            string ID2 = Program.TasksItems[1].Id;
            string ID3 = Program.TasksItems[2].Id;
            string ID4 = Program.TasksItems[3].Id;

            Assert.That(ID1, Is.EqualTo("1"));
            Assert.That(ID2, Is.EqualTo("2"));
            Assert.That(ID3, Is.EqualTo("3"));
            Assert.That(ID4, Is.EqualTo("5"));
        }
    }
}
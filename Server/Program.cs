using ClientServerUtilsSharedProject;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Server;

public class Program
{
    private const string SAVE_FILE_NAME = "TaskInJsonFormatCBD.json";

    public static TcpListener TCPServer { get; private set; }
    public static List<TcpClient> Clients { get; private set; } = new List<TcpClient>();
    public static List<TaskItem> TasksItems { get; private set; } = new List<TaskItem>();
    
    public static void Main(string[] args)
    {
        loadFromFile(SAVE_FILE_NAME);

        TCPServer = new TcpListener(IPAddress.Any, 1234);
        TCPServer.Start();
        Console.WriteLine("server started on port 1234");

        while (true)
        {
            try
            {
                var tcpClient = TCPServer.AcceptTcpClient();
                Console.WriteLine($"client connected = {tcpClient.Client.RemoteEndPoint}");
                Clients.Add(tcpClient);
                new Thread(() => { ListenForMessages(tcpClient); }).Start();
                Console.WriteLine($"connections open: {Clients.Count}");
            } catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }
    }

    private static async void ListenForMessages(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1500];

        while (tcpClient.Connected)
        {
            NetworkJsonObject? networkJsonObject = await ClientServerUtils.ReadNetWorkJsonObject(stream);
            if (networkJsonObject == null)
            {
                Console.WriteLine($"a faulty networkJsonObject was recieved, Possible disconnect");
                continue;
            }

            //server logic, read message status -> act acordingly -> broadcast update
            StatusType type = networkJsonObject.Status;
            switch (type) {
                case StatusType.Add:
                    if (networkJsonObject.Items.Length == 0)
                    {
                        Console.WriteLine("no taksItems where added in a Add message");
                        continue;
                    }
                    AddTask(networkJsonObject.Items[0]);
                    break;
                case StatusType.Remove:
                    if (networkJsonObject.Items.Length == 0)
                    {
                        Console.WriteLine("no taksItems where added in a Remove message");
                        continue;
                    }
                    RemoveTask(networkJsonObject.Items[0]);
                    break;
                case StatusType.Edit:
                    if (networkJsonObject.Items.Length == 0)
                    {
                        Console.WriteLine("no taksItems where added in a Edit message");
                        continue;
                    }
                    EditTask(networkJsonObject.Items[0]);
                    break;
                case StatusType.Get:
                    Console.WriteLine("get message recieved, sending update =");
                    SendClientUpdate(tcpClient);
                    break;
            }

        }

        //exited whileloop: connection closed
        Console.WriteLine("connection closed");
        Clients.Remove(tcpClient);
        Console.WriteLine($"connections open: {Clients.Count}");
    }
    public static int GenerateUniqueId() 
    {
        if (TasksItems.Count == 0)
        {
            return 1;
        }

        var existingIds = TasksItems.Select(task => int.Parse(task.Id)).ToList();
        existingIds.Sort();

        for (int i = 1; i <= existingIds.Count; i++)
        {
            if (i != existingIds[i - 1])
            {
                return i;
            }
        }

        return existingIds.Count + 1;
    }
    public static void AddTask(TaskItem task)
    {
        task.Id = GenerateUniqueId().ToString();
        TasksItems.Add(task);
        Console.WriteLine($"task added = id: {task.Id}, name: {task.Name}, description: {task.Description}, state = {task.State.ToString()}");
        BroadcastUpdate();
        saveTasksToFile(SAVE_FILE_NAME);
    }

    public static void RemoveTask(TaskItem task)
    {
        string taskId = task.Id;
        TasksItems.RemoveAll(tasksItem => tasksItem.Id == taskId);
        Console.WriteLine($"removed task = id: {task.Id}, name: {task.Name}, description: {task.Description}, state = {task.State.ToString()}");
        BroadcastUpdate();
        saveTasksToFile(SAVE_FILE_NAME);
    }

    public static void EditTask(TaskItem task)
    {
        TaskItem? existingTaskItem = TasksItems.Where(taskItem => taskItem.Id == task.Id).FirstOrDefault();
        int index = TasksItems.IndexOf(existingTaskItem);
        if (index != -1)
        {
            TasksItems[index] = task;
            Console.WriteLine($"edited task = id: {existingTaskItem.Id}, name: {existingTaskItem.Name}, description: {existingTaskItem.Description}");
            Console.WriteLine($"into task = id: {task.Id}, name: {task.Name}, description: {task.Description}, state = {task.State.ToString()}");
            BroadcastUpdate();
            saveTasksToFile(SAVE_FILE_NAME);
        } else
        {
            Console.WriteLine($"failed attempt to edit task = id: {task.Id}, name: {task.Name}, description: {task.Description}, state = {task.State.ToString()}");
        }
    }


    private static void BroadcastUpdate()
    {
        Console.WriteLine("sending update to all clients =");
        foreach (var client in Clients)
        {
            SendClientUpdate(client);
        }
    }

    private static void SendClientUpdate(TcpClient tcpClient)
    {
        Console.WriteLine($"send update to client: {tcpClient.Client.RemoteEndPoint}");
        var tasksArray = TasksItems.ToArray();
        NetworkJsonObject networkJsonObject;
        networkJsonObject = new NetworkJsonObject() { Status = StatusType.Get, Items = tasksArray };
        ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
    }

    public static void saveTasksToFile(string file_name)
    {
        string jsonTasks = JsonConvert.SerializeObject(TasksItems, Formatting.Indented);
        string werkdirectory = Environment.CurrentDirectory;
        string path = Path.Combine(werkdirectory, file_name);
        File.WriteAllText(path, jsonTasks);
        
        Console.WriteLine($"tasks saved in file: {path}");
    }


    public static void loadFromFile(string file_name)
    {
        if (File.Exists(file_name))
        {
            {
                string json = File.ReadAllText(file_name);
                TasksItems = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? [];

                Console.WriteLine("tasks loaded from file:");
                foreach (var taskItem in TasksItems)
                {
                    Console.WriteLine($"task loaded = id: {taskItem.Id}, name: {taskItem.Name}, description: {taskItem.Description}, state = {taskItem.State.ToString()}");
                }
            }
        }
    }
}
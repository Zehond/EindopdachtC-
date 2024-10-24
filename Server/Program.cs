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

    private static TcpListener server;
    private static List<TcpClient> clients = new List<TcpClient>();
    private static List<TaskItem> tasksItems = new List<TaskItem>();
    
    public static void Main(string[] args)
    {
        server = new TcpListener(IPAddress.Any, 1234);
        server.Start();
        Console.WriteLine("server started");

        while (true)
        {
            try
            {
                var tcpClient = server.AcceptTcpClient();
                Console.WriteLine($"client connected = {tcpClient.Client.RemoteEndPoint}");
                Task.Run(() => { ListenForMessages(tcpClient); });
            } catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }

        //server.BeginAcceptTcpClient(new AsyncCallback(onConnection), null);

    }

    //TODO: see if can make work again
    //public static void onConnection(IAsyncResult ar) { 
    //    TcpClient client = server.EndAcceptTcpClient(ar);
    //    clients.Add(client);
    //    Task.Run(() => { ListenForMessages(client); });
    //    server.BeginAcceptTcpClient(new AsyncCallback(onConnection), null);
    //}

    private static async void ListenForMessages(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1500];

        while (tcpClient.Connected)
        {
            NetworkJsonObject? networkJsonObject = await ClientServerUtils.ReadNetWorkJsonObject(stream);
            if (networkJsonObject == null)
            {
                Console.WriteLine($"a faulty networkJsonObject was recieved");
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
    }

    private static void AddTask(TaskItem task)
    {
        tasksItems.Add(task);
        Console.WriteLine($"task added = id: {task.Id}, name: {task.Name}, description: {task.Description}");
        BroadcastUpdate();
        saveToFile();
    }

    private static void RemoveTask(TaskItem task)
    {
        string taskId = task.Id;
        tasksItems.RemoveAll(tasksItem => tasksItem.Id == taskId);
        Console.WriteLine($"removed task = id: {task.Id}, name: {task.Name}, description: {task.Description}");
        BroadcastUpdate();
        saveToFile();
    }

    private static void EditTask(TaskItem task)
    {
        TaskItem? existingTaskItem = tasksItems.Where(taskItem => taskItem.Id == task.Id).FirstOrDefault();
        int index = tasksItems.IndexOf(existingTaskItem);
        if (index != -1)
        {
            tasksItems[index] = task;
            Console.WriteLine($"edited task = id: {existingTaskItem.Id}, name: {existingTaskItem.Name}, description: {existingTaskItem.Description}");
            Console.WriteLine($"into task = id: {task.Id}, name: {task.Name}, description: {task.Description}");
            BroadcastUpdate();
            saveToFile();
        } else
        {
            Console.WriteLine($"failed attempt to edit task = id: {task.Id}, name: {task.Name}, description: {task.Description}");
        }
    }


    private static void BroadcastUpdate()
    {
        Console.WriteLine("sending update to all clients =");
        foreach (var client in clients)
        {
            SendClientUpdate(client);
        }
    }

    private static void SendClientUpdate(TcpClient tcpClient)
    {
        Console.WriteLine($"send update to client: {tcpClient.Client.RemoteEndPoint}");
        NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Get, Items = tasksItems.ToArray() };
        ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
    }

    private static void saveToFile()
    {
        
        string jsonTasks = JsonConvert.SerializeObject(tasksItems, Formatting.Indented);
        string werkdirectory = Environment.CurrentDirectory;
        string path = Path.Combine(werkdirectory, SAVE_FILE_NAME);
        File.WriteAllText(path, jsonTasks);
        
        Console.WriteLine($"Bestand opgeslagen in: {path}");
    }

    private static void loadFromFile()
    {
        if (File.Exists(SAVE_FILE_NAME))
        {
            {
                string json = File.ReadAllText(SAVE_FILE_NAME);
                tasksItems = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? [];
            }
        }
    }
}
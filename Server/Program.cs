using ClientServerUtilsSharedProject;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class Program
{

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
                Console.WriteLine($"client connected = {tcpClient}");
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
    }

    private static void RemoveTask(TaskItem task)
    {
        string taskId = task.Id;
        tasksItems.RemoveAll(tasksItem => tasksItem.Id == taskId);
        Console.WriteLine($"removed task = id: {task.Id}, name: {task.Name}, description: {task.Description}");
        BroadcastUpdate();
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
        Console.WriteLine($"send update to client: {tcpClient}");
        NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Get, Items = tasksItems.ToArray() };
        ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
    }
}
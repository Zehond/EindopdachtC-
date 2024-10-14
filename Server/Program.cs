

using Newtonsoft.Json;
using Server;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

public class Program
{

    private static TcpListener server;
    private static List<TcpClient> clients = new List<TcpClient>();
    private static List<string> tasks = new List<string>();
    public static void Main(string[] args)
    {
        server = new TcpListener(IPAddress.Any, 1234);
        server.Start();
        Console.WriteLine("server started");

        server.BeginAcceptTcpClient(new AsyncCallback(onConnection), null);

    }

    public static void onConnection(IAsyncResult ar) { 
        TcpClient client = server.EndAcceptTcpClient(ar);
        Task.Run(() => { ListenForMessages(client); });
        server.BeginAcceptTcpClient(new AsyncCallback(onConnection), null);
    }

    private static void ListenForMessages(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1500];
        while (true)
        {
            try
            {
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                
                if (byteCount == 0) return; // Client disconnected

                string jsonString = Encoding.ASCII.GetString(buffer, 0, byteCount);
                TaskItem task = JsonConvert.DeserializeObject<TaskItem>(jsonString);
                if (task.status == "add") {
                    AddTask(task);
                }
                Console.WriteLine($"Task Name: {task.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
                break;
            }
        }
    }

    private static void AddTask(TaskItem task) {
        tasks.Add(JsonConvert.SerializeObject(task));
        BroadcastUpdate();
    }

    private static void BroadcastUpdate()
    {
        string json = JsonConvert.SerializeObject(tasks);
        byte[] data = Encoding.ASCII.GetBytes(json);

        foreach (var client in clients)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
    }
}
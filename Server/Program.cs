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
    private static List<string> tasks = new List<string>();
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

        //TODO make whileloop exitable
        while (true)
        {
            NetworkJsonObject? networkJsonObject = await ClientServerUtils.ReadNetWorkJsonObject(stream);
            if (networkJsonObject == null)
            {
                Console.WriteLine($"a faulty networkJsonObject was recieved");
                continue;
            }
            StatusType type = networkJsonObject.Status;
            switch (type) {
                case StatusType.Add:
                    if (networkJsonObject.Items.Length == 0)
                    {
                        Console.WriteLine("no taksItems where added in a Add message");
                    }
                    TaskItem taskItem = networkJsonObject.Items[0];
                    AddTask(networkJsonObject.Items[0]);
                    Console.WriteLine(taskItem.Name);
                    break;
                case StatusType.Remove:
                    //TODO implement
                    break;
                case StatusType.Edit:
                    //TODO implement
                    break;
                case StatusType.Get:
                    //TODO implement
                    break;
            }

        }
    }

    private static void AddTask(TaskItem task) {
        tasks.Add(JsonConvert.SerializeObject(task));
        //BroadcastUpdate();
    }

    
    //private static void BroadcastUpdate()
    //{
    //    string json = JsonConvert.SerializeObject(tasks);
    //    byte[] data = Encoding.ASCII.GetBytes(json);

    //    foreach (var client in clients)
    //    {
    //        NetworkStream stream = client.GetStream();
    //        stream.Write(data, 0, data.Length);
    //    }
    //}
}
using ClientServerUtilsSharedProject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    // singleton = er is altijd maar een instantie van de server mananger, aangezien de client altijd maar een connectie heeft is dit veiliger.
    internal class NetworkManager
    {
        // de class kan alleen maar bereikt worden door deze instance variable, omdat de constructor private is.
        public static NetworkManager Instance { get; } = new();

        private TcpClient tcpClient;
        public event Action<List<TaskItem>> TasksUpdated;
        public event Action NoTasksOnServer;
        public event Action ServerDisconnected;
        public bool isConnected { get {  return tcpClient.Connected; } }

        public async Task<bool> ConnectTcpClient(string ip, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ip, port);
                new Thread(() => { listenToServer(); }).Start();
                return true;
            } catch (Exception ex)
            {
                //TCP connection failed
                return false;
            }
        }

        /// <summary>
        /// sends a message to request all taskItems from server, recieve automatically via de TasksUpdate Action, make sure the server is connected
        /// </summary>
        public void getAllTask()
        {
            if (!isConnected) return; 
            NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Get, Items = [] };
            ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
        }

        /// <summary>
        /// sends a message to add a taskItem to the server, make sure the server is connected
        /// </summary>
        public void sendAddTask(TaskItem taskItem)
        {
            if (!isConnected) return; 
            NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Add, Items = [taskItem] };
            ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
        }

        /// <summary>
        /// sends a message to edit a taskItem on the server, make sure the server is connected
        /// </summary>
        public void sendEditTask(TaskItem taskItem)
        {
            if (!isConnected) return;
            NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Edit, Items = [taskItem] };
            ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
        }

        /// <summary>
        /// sends a message to remove a taskItem from the server, make sure the server is connected
        /// </summary>
        public void sendRemoveTask(TaskItem taskItem)
        {
            if (!isConnected) return;
            NetworkJsonObject networkJsonObject = new NetworkJsonObject() { Status = StatusType.Remove, Items = [taskItem] };
            ClientServerUtils.SendNetworkJsonObject(tcpClient.GetStream(), networkJsonObject);
        }

        private async void listenToServer()
        {
            while (isConnected)
            {
                NetworkJsonObject? networkJsonObject = await ClientServerUtils.ReadNetWorkJsonObject(tcpClient.GetStream());
                if (networkJsonObject == null)
                {
                    //most likely network broken, still continue instead of break just because the object could be broeken
                    continue;
                }
                if (networkJsonObject.Status == StatusType.Get)
                {
                    TaskItem[] taskItems = networkJsonObject.Items;
                    if (taskItems.Length == 0)
                    {
                        //TODO no items found, react 
                        continue;
                    }
                    List<TaskItem> tasklist = new List<TaskItem>(taskItems);
                    TasksUpdated?.Invoke(tasklist);
                }
            }

            //server disconnected
            ServerDisconnected?.Invoke();
        }      
    }
}

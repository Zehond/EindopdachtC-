using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace ClientServerUtilsSharedProject
{
    public static class ClientServerUtils
    {
        private const int BUFFER_SIZE = 1500;

        public static void SendNetworkJsonObject(NetworkStream networkStream, NetworkJsonObject networkJsonObject)
        {
            string serialisedJsonStr = JsonConvert.SerializeObject(networkJsonObject);
            byte[] bytes = Encoding.ASCII.GetBytes(serialisedJsonStr);
            networkStream.Write(bytes, 0, bytes.Length);
        }

        public static async Task<NetworkJsonObject?> ReadNetWorkJsonObject(NetworkStream networkStream)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            try
            {
                int byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                if (byteCount == 0)
                {
                    return null;
                }

                string jsonString = Encoding.ASCII.GetString(buffer, 0, byteCount);
                return JsonConvert.DeserializeObject<NetworkJsonObject>(jsonString);
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

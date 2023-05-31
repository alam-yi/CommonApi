using System.Net;
using System.Net.Sockets;

namespace PublicTool
{
    public class IPTool
    {
        //获取本机IP终端地址，不受虚拟机，VPN等影响
        public string GetIp()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
    }
}

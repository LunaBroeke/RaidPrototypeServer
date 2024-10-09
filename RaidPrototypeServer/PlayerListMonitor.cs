using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaidPrototypeServer
{
    public static class PlayerListMonitor
    {
        public static MainWindow main;
        public static void WritePlayerList()
        {
            if (main.PlayerListBox.InvokeRequired)
            {
                main.PlayerListBox.Invoke(new Action(() => WritePlayerList()));
            }
            else
            {

                int i = 0;
                string s = null;
                List<ServerPlayer> players = Server.players;
                foreach (ServerPlayer player in players)
                {
                    string a = null;
                    i++;
                    string name = player.name;
                    int id = -1;
                    IPEndPoint endpoint = (IPEndPoint)player.tcpClient.Client.RemoteEndPoint;
                    if (player.player != null)
                    {
                        if (player.player.name == null) name = endpoint.ToString();
                        else name = player.player.name;
                        if (player.player.puppetID == -1) id = endpoint.Port;
                        else id = player.player.puppetID;
                    }
                    a = $"{i}. {name}({id})";
                    s += a + "\r\n";
                }

                main.PlayerListBox.Text = s;
                Thread.Sleep(100);
            }
        }
    }
}

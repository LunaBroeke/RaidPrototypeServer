using System.Diagnostics;
using System.Net;

namespace RaidPrototypeServer
{
    public static class InfoMonitor
    {
        public static MainWindow main;
        private static Thread updateThread;
        public static bool stopThread;

        public static void WriteInfo(ServerPlayer player)
        {
            if (updateThread != null)
            {
                stopThread = true;
                updateThread.Join();
            }
            stopThread = false;
            updateThread = new Thread(() =>
            {
                while (!stopThread)
                {
                    try
                    {
                        IPEndPoint endPoint = (IPEndPoint)player.tcpClient.Client.RemoteEndPoint;
                        string s = string.Empty;
                        s += $"Player Name: {player.player.name}\r\n";
                        s += $"PuppetID: {player.player.puppetID}\r\n";
                        s += $"Remote Endpoint: {endPoint}\r\n";
                        s += $"Position: \r\n\tx: {player.player.position.x} \r\n\ty: {player.player.position.y} \r\n\tz: {player.player.position.z}\r\n";
                        s += $"Rotation: \r\n\tx: {player.player.rotation.x} \r\n\ty: {player.player.rotation.y} \r\n\tz: {player.player.rotation.y} \r\n\tw: {player.player.rotation.w}\r\n";
                        s += $"Stats:";
                        ApplyInfo(s);
                        Thread.Sleep(500);
                    }
                    catch (NullReferenceException e)
                    {
                        stopThread = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        player.logger.LogError($"General Error: ");
                        stopThread = true;
                        break;
                    }
                }
            });
            updateThread.Start();
        }

        public static void WriteInfo(Server server)
        {
            if (updateThread != null)
            {
                stopThread = true;
                updateThread.Join();
            }
            stopThread = false;
            updateThread = new Thread(() =>
            {
                while (!stopThread)
                {
                    try
                    {
                        IPEndPoint endPoint = (IPEndPoint)Server.server.LocalEndpoint;
                        string s = string.Empty;
                        s += $"PendingConnections: {Server.server.Pending}\r\n";
                        ApplyInfo(s);
                        Thread.Sleep(500);
                    }
                    catch (NullReferenceException e)
                    {
                        stopThread = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        Server.logger.LogError($"General Error: ");
                        stopThread = true;
                        break;
                    }
                }
            });
            updateThread.Start();
        }

        private static void ApplyInfo(string m)
        {
            if (main.InfoBox.InvokeRequired)
            {
                main.InfoBox.Invoke(new Action(() => ApplyInfo(m)));
            }
            else
            {
                main.InfoBox.Text = m;
            }
        }
    }
}

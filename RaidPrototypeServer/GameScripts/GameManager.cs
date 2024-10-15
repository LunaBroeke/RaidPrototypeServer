using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RaidPrototypeServer.EnemyScripts;
using Newtonsoft.Json;
namespace RaidPrototypeServer.GameScripts
{
    public class Floor
    {
        public int floorNum;
        public int sceneNum;
        public List<Enemy> enemyList;
    }
    public static class GameManager
    {
        public static List<Floor> floorList = Floors();

        public static void StartGame(Floor f)
        {
            SendToScene(f.sceneNum);
            Server.logger.Log(JsonConvert.SerializeObject(f));
            foreach (var enemy in f.enemyList)
            {
                enemy.Start();
            }
        }
        public static List<Floor> Floors()
        {
            List<Floor> floors = new List<Floor>
            {
                new Floor{ floorNum = 10, sceneNum = 1, enemyList = new List<Enemy>{ TrainingDummy.InitEnemy(TrainingDummy.EnemyInfo(1))} },
                new Floor{ floorNum = 1, sceneNum = 2},
            };
            return floors;
        }
        public static void SendToScene(int sceneNum)
        {
            Command c = new Command { command = "LoadScene", arguments = new string[] { $"{sceneNum}" } };
            foreach(ServerPlayer player in Server.players)
            {
                NetworkStream stream = player.tcpClient.GetStream();
                PacketHandler.WriteStream(stream, c);
            }
        }
        public static Floor GetFloorByNum(int num)
        {
            foreach(Floor floor in floorList)
            {
                if (floor.floorNum == num)
                    return floor;
            }
            return new Floor { floorNum = 0, sceneNum = 0};
        }
    }
}

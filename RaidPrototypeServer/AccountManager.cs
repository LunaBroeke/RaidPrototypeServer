using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.VisualBasic.Devices;
using System.Net.Sockets;

namespace RaidPrototypeServer
{
    public class Root
    {
        public List<Account> accounts;
    }
    public class Account
    {
        public string name;
        public string passwordHash;
        public string salt;
        public string typeToken = "User";
        public bool accepted;
    }
    public class AccountManager
    {
        public static List<Account> accounts;
        private static string path = "Accounts.json";
        private static Logger logger = new Logger() { name = "Account Manager" };
        public static void GetAccountDatabase()
        {
            if (File.Exists(path))
            {
                string s = File.ReadAllText(path);
                accounts = JsonConvert.DeserializeObject<Root>(s).accounts;
                logger.Log($"Loaded {path}");
            }
            else
            {
                logger.LogWarning($"File {path} not found, creating new file");
                accounts = new List<Account>
                {
                    RegisterAccount("admin", "admin")
                };
                accounts.First().typeToken = "admin";
                WriteAccountDatabase();
            }
        }

        private static void WriteAccountDatabase()
        {
            Root root = new Root() { accounts = accounts };
            string json = JsonConvert.SerializeObject(root);
            File.WriteAllText(path, json);
            logger.Log($"Saved {path}");
        }
        public static void AwaitAccount(ServerPlayer user)
        {
            user.logger.Log("1");
            Command c = new Command() { command = "PleaseLogin" };
            string s = JsonConvert.SerializeObject(c);
            NetworkStream stream = user.tcpClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(s);
            stream.Write(data, 0, data.Length);
            bool success = false;
            Account account = null;
            do
            {
                try
                {
                    c = null;
                    s = null;
                    data = new byte[512];
                    int bytesRead = stream.Read(data, 0, data.Length);
                    if (bytesRead != 0)
                    {
                        s = Encoding.UTF8.GetString(data);
                        user.logger.LogWarning(s);
                        c = JsonConvert.DeserializeObject<Command>(s);
                        switch (c.command)
                        {
                            case "login":
                                (bool b, account) = Login(c.arguments[0], c.arguments[1]);
                                if (b)
                                {
                                    user.logger.Log($"{account.name} Logged in successfully!");
                                    success = true;
                                }
                                else
                                {
                                    user.logger.LogWarning("Login failed");
                                }
                                break;
                            case "register":
                                account = RegisterAccount(c.arguments[0], c.arguments[1]);
                                if (account.accepted)
                                {
                                    accounts.Add(account);
                                    user.logger.Log($"Account {account.name} Registered!");
                                    success = true;
                                }
                                break;
                            default:
                                throw new InvalidDataException($"Command {c.command} unexpected");
                        }
                    }
                }
                catch (Exception e) { user.logger.LogError(e.ToString()); success = false; continue; }
            }
            while (!success);
            c = new Command() { command = "LoginSuccess", arguments = new string[] { account.name } };
            WriteAccountDatabase();
        }
        public static Account RegisterAccount(string name, string password)
        {
            Account account = new Account();
            if (!NameTaken(name))
            {
                account.name = name;
                byte[] saltBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                string salt = Convert.ToBase64String(saltBytes);
                account.salt = salt;
                account.passwordHash = GetPassword(password,saltBytes);
                account.accepted = true;
            }
            else
            {
                account.accepted = false;
            }
            return account;
        }
        public static string GetPassword(string m, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(m, salt, 10, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32); // Hash size in bytes
                byte[] hashBytes = new byte[48]; // Salt (16 bytes) + Hash (32 bytes)
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);
                return Convert.ToBase64String(hashBytes);
            }
        }
        public static (bool,Account) Login(string name, string password)
        {
            Account account = new Account();
            foreach (Account acc in accounts)
            {
                if (acc.name == name) { account = acc; break; }
            }
            if (account != null)
            {
                byte[] saltBytes = Convert.FromBase64String(account.salt);
                string passHash = GetPassword(password,saltBytes);
                logger.LogWarning(passHash);
                if (account.passwordHash == passHash)
                {
                    logger.Log($"User {name} Logged in successfully");
                    return(true,account);
                }
                else
                {
                    return(false,null);
                }
            }
            else
            {
                return(false,null);
            }
        }
        private static bool NameTaken(string m)
        {
            if (accounts == null ) return false;
            foreach (Account a in accounts)
            {
                if (a.name == m) return true;
            }
            return false;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.VisualBasic.Devices;
using System.Net.Sockets;
using System.Web;

namespace RaidPrototypeServer
{
    public class AccountRoot
    {
        public string type = "AccountRoot";
        public List<Account> accounts;
    }
    public class Account
    {
        public string name;
        public string passwordHash;
        public string salt;
        public string typeToken = "User";
        public bool accepted;
        public DateTime accountCreation = DateTime.UtcNow;
        public DateTime lastLogin = DateTime.UtcNow;
        public DateTime banExpire = DateTime.UnixEpoch;
    }
    public class LoginResult
    {

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
                accounts = JsonConvert.DeserializeObject<AccountRoot>(s).accounts;
                logger.Log($"Loaded {path}");
                WriteAccountDatabase();
            }
            else
            {
                logger.LogWarning($"File {path} not found, creating new file");
                accounts = new List<Account>
                {
                    RegisterAccount("admin", "admin"),
                    RegisterAccount("null", "null")
                };
                accounts[0].typeToken = "admin";
                accounts[1].typeToken = "null";
                WriteAccountDatabase();
            }
        }

        public static void WriteAccountDatabase()
        {
            AccountRoot root = new AccountRoot() { accounts = accounts };
            string json = JsonConvert.SerializeObject(root,Formatting.Indented);
            File.WriteAllText(path, json);
            logger.Log($"Saved {path}");
        }
        public static void AwaitAccount(ServerPlayer user)
        {
            Command c = new Command() { command = "PleaseLogin", arguments = new string[] { "Please Log In" } };
            string s = JsonConvert.SerializeObject(c);
            NetworkStream stream = user.tcpClient.GetStream();
            PacketHandler.WriteStream(stream, c);
            bool success = false;
            Account account = null;
            int attempts = 0;
            int attemptsMax = 5;
            do
            {
                if (attempts >= attemptsMax)
                {
                    user.logger.Log("Too many login attempts, forcefully disconnecting.");
                    Server.Disconnect(user);
                    return;
                }
                try
                {
                    c = null;
                    s = null;
                    s = PacketHandler.ReadStream(stream, 1024);
                    c = JsonConvert.DeserializeObject<Command>(s);
                    try { if (c.arguments.Length < 3) { LoginFail(stream, $"Invalid Arguments, Disconnecting"); Server.Disconnect(user); return; } } catch { Server.Disconnect(user); }
                    switch (c.command)
                    {
                        case "login":
                            (bool b, account) = Login(c.arguments[0], c.arguments[1]);
                            if (b)
                            {
                                if (!IsLoggedIn(account))
                                    success = LoginHandler(user, stream, account, c);
                                else
                                    LoginFail(stream, "Already logged in");
                                continue;
                            }
                            else
                            {
                                user.logger.LogWarning("Login failed");
                                LoginFail(stream, "Incorrect username or password");
                                attempts++;
                                continue;
                            }
                        case "register":
                            account = RegisterAccount(c.arguments[0], c.arguments[1]);
                            if (account.accepted)
                            {
                                accounts.Add(account);
                                user.logger.Log($"Account {account.name} Registered!");
                                success = LoginHandler(user, stream, account, c);
                            }
                            else
                            {
                                LoginFail(stream, account.typeToken);
                                attempts++;
                            }
                            break;
                        case "Disconnect":
                            Server.Disconnect(user);
                            break;
                        default:
                            throw new InvalidDataException($"Command {c.command} unexpected");
                    }

                }
                catch (IOException e) { Server.Disconnect(user); }
                catch (Exception e) { user.logger.LogError(e.Message); success = false; attempts++; continue; }
            }
            while (!success);
            c = new Command() { command = "LoginSuccess", arguments = new string[] { account.name, account.typeToken } };
            user.logger.Log($"{account.name} Logged in successfully!");
            user.name = account.name;
            account.lastLogin = DateTime.UtcNow;
            PacketHandler.WriteStream(stream, c);
            WriteAccountDatabase();
            switch (user.userType)
            {
                case UserClient.Player:
                    Server.HandlePlayer(user);
                    break;
                case UserClient.Spectator:
                    Server.HandleSpectator(user);
                    break;
                case UserClient.AdminPanel:
                    Server.HandleAdminPanel(user);
                    break;
            }
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
                account.passwordHash = GetPassword(password, saltBytes);
                account.accepted = true;
            }
            else
            {
                account.accepted = false;
                account.typeToken = "Name has been taken already, please register with a new name";
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
        public static (bool, Account) Login(string name, string password)
        {
            Account account = null;
            foreach (Account acc in accounts)
            {
                if (acc.name == name) { account = acc; break; }
            }
            if (account != null)
            {
                byte[] saltBytes = Convert.FromBase64String(account.salt);
                string passHash = GetPassword(password, saltBytes);
                if (account.passwordHash == passHash)
                {
                    return (true, account);
                }
                else
                {
                    return (false, null);
                }
            }
            else
            {
                return (false, null);
            }
        }
        private static bool LoginHandler(ServerPlayer user, NetworkStream stream, Account account, Command c)
        {
            if (account.typeToken == "null")
            {
                LoginFail(stream, "Incorrect username or password");
                return false;
            }
            if (isBanned(account))
            {
                LoginFail(stream, $"Logged in successfully, but you are banned until {account.banExpire}");
                return false;
            }
            else
            {
                account.banExpire = DateTime.UnixEpoch;
            }
            user.userType = Settings.CanLogin(account, c);
            if (user.userType == UserClient.None)
            {
                LoginFail(stream, $"This account is not compatible with this software");
                return false;
            }
            return true;
        }
        public static Account FindAccountByName(string s)
        {
            foreach(Account acc in accounts)
            {
                if (acc.name == s) return acc;
            }
            throw new InvalidAccountException($"Account Name not found");
        }
        private static bool NameTaken(string m)
        {
            if (accounts == null) return false;
            foreach (Account a in accounts)
            {
                if (a.name == m) return true;
            }
            return false;
        }
        private static bool IsLoggedIn(Account account)
        {
            foreach (ServerPlayer p in Server.players)
            {
                string s = p.name.Replace("[AdminPanel]", "").Replace("[Spectator]", "").Trim();

                if (s == account.name) return true;
            }
            return false;
        }


        private static void LoginFail(NetworkStream stream, string msg)
        {
            Command c = new Command() { command = "LoginFailed", arguments = new string[] { msg } };
            PacketHandler.WriteStream(stream, c);
            Thread.Sleep(500);
            c = new Command() { command = "PleaseLogin" };
            PacketHandler.WriteStream(stream, c);
        }
        private static bool isBanned(Account account)
        {
            if (account.banExpire != null)
            {
                DateTime now = DateTime.UtcNow;
                DateTime ban = account.banExpire;
                TimeSpan span = ban - now;
                if (span > TimeSpan.Zero)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        public static AccountRoot GetAccountRoot()
        {
            AccountRoot root = new AccountRoot() { accounts = accounts };
            return root;
        }
        private static void TokenCheck()
        {

        }
    }

    public class InvalidAccountException : Exception
    {
        public InvalidAccountException() : base("Invalid Account"){ }
        public InvalidAccountException(string message) : base(message) { }
        public InvalidAccountException(string message, Exception innerException) : base(message, innerException) { }
        
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaidPrototypeServer
{
    public enum SpecialRights
    {
        None = 0,
        Spectator = 1,
        AdminPanel = 151,
        Administrator = 150,
        Example = 99,
    }
    public class Authorization
    {
        public string token;
        public SpecialRights rights = 0;
        public string[] permissions;
    }
    public class Settings
    {
        public string address = "0.0.0.0";
        public int port = 2051;
        public List<Authorization> auth = new List<Authorization>();
        private static string path = "settings.json";
        public static Settings LoadSettings()
        {
            if (File.Exists(path))
            {
                string s = File.ReadAllText(path);
                Server.logger.Log($"loaded {path}");
                Settings settings = JsonConvert.DeserializeObject<Settings>(s);
#if DEBUG
                settings.WriteSettings();
#endif
                return settings;
            }
            else
            {
                Server.logger.Log($"{path} not found, creating new {path}");
                Settings settings = new Settings();
                settings.auth.Add(Example());
                settings.WriteSettings();
                return settings;
            }
        }
        public void WriteSettings()
        {
            string s = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, s);
            Server.logger.Log($"Saved {path}");
        }
        private static Authorization Example()
        {
            Authorization auth = new Authorization();
            auth.token = "example";
            auth.rights = SpecialRights.Example;
            auth.permissions = new string[]
            {
                "Login/User",
                "Login/UserAdmin",
            };
            return auth;
        }
        public static Authorization checkAuth(Account account, Command c)
        {
            Logger logger = new Logger { name = account.name };
            if (c.arguments.Length < 3) return null;
            foreach(Authorization auth in Server.settings.auth)
            {
                if (auth.token == c.arguments[2]) return auth;
            }
            return null;
        }
        public static UserClient CanLogin(Account account,Command c)
        {
            List<Authorization> auths = Server.settings.auth;
            if (PlayerLogin(account, c)) return UserClient.Player;
            if (AdminPanelLogin(account, c)) return UserClient.AdminPanel;
            return UserClient.None;
        }
        private static bool PlayerLogin(Account account, Command c)
        {
            List<Authorization> auths = new List<Authorization>();
            foreach(Authorization auth in Server.settings.auth)
            {
                if (auth.rights == SpecialRights.None)
                {
                    auths.Add(auth);
                }
            }
            foreach(Authorization auth in auths)
            {
                if (auth.token == c.arguments[2])
                {
                    string a = $"Login/{account.typeToken}";
                    if (auth.permissions.Contains(a)) return true;
                }
            }
            return false;
        }

        private static bool AdminPanelLogin(Account account, Command c)
        {
            List<Authorization> auths = new List<Authorization>();
            foreach(Authorization auth in Server.settings.auth)
            {
                if (auth.rights == SpecialRights.AdminPanel)
                {
                    auths.Add(auth);
                }
            }
            foreach(Authorization auth in auths)
            {
                if (auth.token == c.arguments[2])
                {
                    string a = $"Login/{account.typeToken}";
                    if (auth.permissions.Contains(a)) return true;
                }
            }
            return false;
        }
    }
}

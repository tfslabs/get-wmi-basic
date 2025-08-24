using System.Linq;
using System.Management;
using System.Security;
using System.Threading.Tasks;

namespace GetWMIBasic.WMIMethods
{
    public class MachineMethods
    {
        /*
         * Class properties
         */

        protected (string computerName, string username, SecureString password) userCredential;
        protected ConnectionOptions credential;
        protected ManagementScope scope;
        protected ManagementObjectSearcher searcher;

        protected string scopePath;
        protected bool isLocal;

        /*
         * Machine Method
         * 1. Empty - Local
         * 2. Or Connect through WMI
         */

        public MachineMethods()
        {
            userCredential.computerName = "localhost";
            isLocal = true;
            credential = new ConnectionOptions
            {
                Username = userCredential.username,
                SecurePassword = userCredential.password,
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            };
        }

        public MachineMethods((string computerName, string username, SecureString password) userCredential)
        {
            if (userCredential.computerName == "localhost")
            {
                userCredential.computerName = "localhost";
                isLocal = true;
                credential = new ConnectionOptions
                {
                    Username = userCredential.username,
                    SecurePassword = userCredential.password,
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.PacketPrivacy,
                    EnablePrivileges = true
                };
            }
            else
            {
                this.userCredential.computerName = userCredential.computerName;
                this.userCredential.username = userCredential.username;
                this.userCredential.password = userCredential.password;
                isLocal = false;
                credential = new ConnectionOptions
                {
                    Username = userCredential.username,
                    SecurePassword = userCredential.password,
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.PacketPrivacy,
                    EnablePrivileges = true
                };
            }
        }

        /*
         * Get value
         */
        public string GetComputerName()
        {
            return userCredential.computerName;
        }

        /*
         * Connection method
         */

        public async Task Connect(string nameSpace)
        {
            scopePath = $"\\\\{userCredential.computerName}\\{nameSpace}";
            scope = (isLocal) ? new ManagementScope(scopePath) : new ManagementScope(scopePath, credential);
            await Task.Run(() => scope.Connect());
        }

        // Fix for CS1983 and CS1998: Change return type to Task<ManagementObjectCollection> and use await Task.Run(...) to make the method truly asynchronous.
        public async Task<ManagementObjectCollection> GetObjects(string className, string fields)
        {
            ObjectQuery query = new ObjectQuery($"SELECT {fields} FROM {className}");
            searcher = new ManagementObjectSearcher(scope, query);
            return await Task.Run(() => searcher?.Get());
        }

        /*
         * Call WMI Method
         */
        public async Task CallMethod(string className, string fields, string methodName, object[] args)
        {
            ObjectQuery query = new ObjectQuery($"SELECT {fields} FROM {className}");
            await Task.Run(() =>
            {
                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject manageObject in searcher.Get().Cast<ManagementObject>())
                    {
                        manageObject.InvokeMethod(methodName, args);
                    }
                }
            });
        }
    }
}

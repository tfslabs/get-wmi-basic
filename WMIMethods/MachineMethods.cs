using System.Linq;
using System.Management;
using System.Security;
using System.Threading.Tasks;

/*
 * The default WMI method class includes:
 *  1. CriticalException.cs - A custom exception class for critical WMI errors
 *  2. CommonException.cs - A custom exception class for common WMI errors
 *  3. MachineMethods.cs - A base class for WMI operations on local and remote machines
 */
namespace GetWMIBasic.WMIMethods
{
    /*
     * Machine Method class
     * It is used to connect and perform WMI tasks to local and remote machines through WMI
     * 
     * Security Note:
     *  1. When connecting to remote machines, ensure that the credentials are handled securely.
     *  2. Use SecureString for passwords to enhance security.
     *  3. The application should run with appropriate permissions to access WMI on the target machines.
     *      For example, locally it may require administrative privileges and do not run with external credentials.
     */
    public class MachineMethods
    {

        ////////////////////////////////////////////////////////////////
        /// Global Properties and Constructors Region
        ///     This region contains global properties and constructors 
        ///     for the MachineMethods class.
        ////////////////////////////////////////////////////////////////

        /*
         * Global properties
         * userCredential: A tuple to store computer name, username, and password
         * credential: A ConnectionOptions object to store WMI connection options
         * scope: A ManagementScope object to define the WMI scope
         * searcher: A ManagementObjectSearcher object to perform WMI queries
         * scopePath: A string to store the WMI scope path
         * isLocal: A boolean to indicate whether the connection is local or remote
         */
        protected (string computerName, string username, SecureString password) userCredential;
        protected ConnectionOptions credential;
        protected ManagementScope scope;
        protected ManagementObjectSearcher searcher;

        protected string scopePath;
        protected bool isLocal;

        /*
         * Constructor for local connection
         *  Note: Local connections do not require username and password. 
         *  It uses current user context.
         */
        public MachineMethods()
        {
            // Set default computer name to localhost
            userCredential.computerName = "localhost";
            
            // Set local connection flag
            isLocal = true;

            // Set default connection options for local connection
            credential = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            };
        }

        /*
         * Constructor for remote connection
         *  Note: Remote connections require username and password.
         */
        public MachineMethods((string computerName, string username, SecureString password) userCredential)
        {
            // Set user credentials for remote connection
            this.userCredential.computerName = userCredential.computerName;
            this.userCredential.username = userCredential.username;
            this.userCredential.password = userCredential.password;
            
            // Set remote connection flag
            isLocal = false;

            // Set connection options for remote connection
            credential = new ConnectionOptions
            {
                Username = userCredential.username,
                SecurePassword = userCredential.password,
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            };
        }

        ////////////////////////////////////////////////////////////////
        ///  Connect to a name space region
        ///     This region contains methods to connect to a WMI name space.
        ///     For example, "root\\cimv2".
        ////////////////////////////////////////////////////////////////

        public void Connect(string nameSpace)
        {
            // Set the scope path based on local or remote connection
            scopePath = $"\\\\{userCredential.computerName}\\{nameSpace}";

            // Create the ManagementScope object and connect
            scope = (isLocal) ? new ManagementScope(scopePath) : new ManagementScope(scopePath, credential);

            // Connect to the WMI scope
            scope.Connect();
        }

        ////////////////////////////////////////////////////////////////
        /// Get methods region (connection setting, not the WMI data)
        ///     Various get methods to retrieve information about the machine
        ///     For example, computer name, current user name, etc.
        ////////////////////////////////////////////////////////////////

        /*
         * Get methods for computer name 
         */
        public string GetComputerName()
        {
            return userCredential.computerName;
        }

        /*
         * Get methods for current user name
         */
        public string GetUsername()
        {
            return userCredential.username;
        }

        ////////////////////////////////////////////////////////////////
        /// Get methods region (for WMI data)
        ////////////////////////////////////////////////////////////////

        /*
         * Get WMI objects
         */
        public async Task<ManagementObjectCollection> GetObjects(string className, string fields)
        {
            // Create the WMI query
            ObjectQuery query = new ObjectQuery($"SELECT {fields} FROM {className}");

            // Create the ManagementObjectSearcher object
            searcher = new ManagementObjectSearcher(scope, query);

            // Execute the query and return the results asynchronously
            return await Task.Run(() => searcher?.Get());
        }

        ////////////////////////////////////////////////////////////////
        /// Set methods region (for WMI object)
        ////////////////////////////////////////////////////////////////

        /*
         * Call WMI Method
         */
        public async Task CallMethod(string className, string fields, string methodName, object[] args)
        {
            // Create the WMI query
            ObjectQuery query = new ObjectQuery($"SELECT {fields} FROM {className}");
            // Execute the method asynchronously
            await Task.Run(() =>
            {
                // Create the ManagementObjectSearcher object
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    // Invoke the method on each object returned by the query
                    foreach (ManagementObject manageObject in searcher.Get().Cast<ManagementObject>())
                    {
                        // Invoke the specified method with arguments
                        _ = manageObject.InvokeMethod(methodName, args);
                    }
                }
            });
        }
    }
}

using GetWMIBasic.DefaultUI;
using GetWMIBasic.WMIMethods;
using System;
using System.Linq;
using System.Management;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

/*
 * Primary namespace for the application
 *  It includes the MainWindow class which is the main UI window of the application
 *  and additional UI for each featuring
 */
namespace GetWMIBasic
{
    /*
     * Main Window class
     * It is used to display the main UI of the application
     */

    public partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////
        /// Global Properties and Constructors Region
        ///     This region contains global properties and constructors 
        ///     for the MachineMethods class.
        ////////////////////////////////////////////////////////////////

        /*
         * Global properties
         * machine: An instance of the MachineMethods class to perform WMI operations
         */
        protected MachineMethods machine;

        /*
         * Constructor of the MainWindow class
         * Initializes the components and sets up event handlers
         */
        public MainWindow()
        {
            // Initialize components
            InitializeComponent();

            // Initialize the MachineMethods instance for local machine
            machine = new MachineMethods();

            // Set up the sync loading event handler. See the method below for further info.
            Loaded += MainWindow_Loaded;
        }
        
        /*
         * Main Windows async loading method.
         * It is used prevent long loading times on the UI thread
         */
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Call the Refresh method to load initial data
            await Refresh();
        }

        ////////////////////////////////////////////////////////////////
        /// User Action Methods Region
        ///     This region contains methods that handle user actions.
        ///     For example, button clicks, changes in order.
        ////////////////////////////////////////////////////////////////

        /*
         * Actions for button "Connect to Another Computer"
         */
        private async void ConnectToAnotherComputer_Button(object sender, RoutedEventArgs e)
        {
            // Initialize the ConnectForm instance
            ConnectForm connectForm = new ConnectForm();

            // Get the user credentials from the ConnectForm
            (string computerName, string username, SecureString password) userCredential = connectForm.ReturnValue();

            // If the computer name is not empty, create a new MachineMethods instance
            if (userCredential.computerName != "")
            {
                machine = !userCredential.computerName.Equals("localhost") ? new MachineMethods(userCredential) : new MachineMethods();
            }

            // Refresh the data on the UI
            await Refresh();
        }

        /*
         * Actons for button "Exit Application"
         */
        private void Exit_Button(object sender, RoutedEventArgs e)
        {
            // Shut down the application
            Application.Current.Shutdown();
        }

        /*
         *   
         */
        private async void Restart_Button(object sender, RoutedEventArgs e)
        {
            // Confirm the restart action with the user
            if (MessageBox.Show($"Do you want to restart the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Update the status bar to indicate loading
                StatusBarChange("Loading", true);

                /*
                 * Try to restart the remote machine using WMI, with the target computer name
                 * Catch the exception and display it using the ExceptionView if any error occurs
                 * In any case, no matter if it succeeds or fails, update the status bar to "Done"
                 */
                try
                {
                    // Since the local machine cannot be restarted remotely, throw an exception if the target is localhost
                    // It should be done via the user context menu instead
                    if (machine.GetComputerName() == "localhost")
                    {
                        throw new ManagementException("Local Host cannot be restarted");
                    }

                    // Connect to the WMI namespace
                    machine.Connect("root\\cimv2");

                    // Call the Win32Shutdown method with the restart flag (6)
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 6 });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions using the ExceptionView
                    (new ExceptionView()).HandleException(ex);
                }
                finally
                {
                    // Update the status bar to indicate completion
                    StatusBarChange("Done", false);
                }
            }
        }

        /*
         * Actions for button "Shutdown Computer"
         */
        private async void Shutdown_Button(object sender, RoutedEventArgs e)
        {
            // Confirm the shutdown action with the user
            if (MessageBox.Show($"Do you want to shutdown the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Update the status bar to indicate loading
                StatusBarChange("Loading", true);

                /*
                 * Try to shutdown the remote machine using WMI, with the target computer name
                 * Catch the exception and display it using the ExceptionView if any error occurs
                 * In any case, no matter if it succeeds or fails, update the status bar to "Done"
                 */
                try
                {
                    // Since the local machine cannot be shutdown remotely, throw an exception if the target is localhost
                    // It should be done via the user context menu instead
                    if (machine.GetComputerName() == "localhost")
                    {
                        throw new ManagementException("Local Host cannot be shut down");
                    }

                    // Connect to the WMI namespace
                    machine.Connect("root\\cimv2");

                    // Call the Win32Shutdown method with the shutdown flag (5)
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 5 });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions using the ExceptionView
                    (new ExceptionView()).HandleException(ex);
                }
                finally
                {
                    // Update the status bar to indicate completion
                    StatusBarChange("Done", false);
                }
            }
        }

        /*
         * Actions for button "Refresh Information" 
         */
        private async void Refresh_Button(object sender, RoutedEventArgs e)
        {
            // Call the Refresh method to update the data on the UI
            await Refresh();
        }

        /*
         * Actions for button "Regular Exception Example"
         * (It will throw and catch a common exception to demonstrate the ExceptionView)
         */
        private void RegularException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new CommonException("This is a common exception example.");
            }
            catch (Exception ex)
            {
                (new ExceptionView()).HandleException(ex);
            }
        }

        /*
         * Actions for button "Critical Exception Example"
         * (It will throw and catch a critical exception to demonstrate the ExceptionView)
         */
        private void CriticalException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new CriticalException("This is a critical exception example.");
            }
            catch (Exception ex)
            {
                (new ExceptionView()).HandleException(ex);
            }
        }

        ////////////////////////////////////////////////////////////////
        /// Non-User Action Methods Region
        /// 
        /// This region contains methods that do not handle user actions.
        /// 
        /// Think about this is the back-end section.
        /// It should not be in a seperated class, because it directly interacts with the UI elements.
        ////////////////////////////////////////////////////////////////

        /*
         * Refresh method to update the information displayed on the UI
         */
        private async Task Refresh()
        {
            // Update the status bar to indicate loading
            StatusBarChange("Loading", true);

            /*
             * Try to get info from the remote machine using WMI, with the target computer name
             * Catch the exception and display it using the ExceptionView if any error occurs
             * In any case, no matter if it succeeds or fails, update the status bar to "Done"
             */
            try
            {
                // Connect to the WMI namespace
                machine.Connect("root\\cimv2");

                // Get BIOS properties
                // In case the property is null, set the text to an empty string
                Task<ManagementObjectCollection> biosProperties = machine.GetObjects("Win32_BIOS", "Manufacturer, Name, SerialNumber, Version");
                foreach (ManagementObject biosProperty in (await biosProperties).Cast<ManagementObject>())
                {
                    BIOS_Manufacturer.Text = biosProperty["Manufacturer"]?.ToString() ?? string.Empty;
                    BIOS_Name.Text = biosProperty["Name"]?.ToString() ?? string.Empty;
                    BIOS_SN.Text = biosProperty["SerialNumber"]?.ToString() ?? string.Empty;
                    BIOS_Version.Text = biosProperty["Version"]?.ToString() ?? string.Empty;
                }

                // Get Operating System properties
                // In case the property is null, set the text to an empty string
                Task<ManagementObjectCollection> osProperties = machine.GetObjects("Win32_ComputerSystem", "Name, Domain, TotalPhysicalMemory, SystemType");
                foreach (ManagementObject osProperty in (await osProperties).Cast<ManagementObject>())
                {
                    Computer_Name.Text = osProperty["Name"]?.ToString() ?? string.Empty;
                    Computer_Domain.Text = osProperty["Domain"]?.ToString() ?? string.Empty;
                    Computer_Memory.Text = osProperty["TotalPhysicalMemory"]?.ToString() ?? string.Empty;
                    Computer_SysType.Text = osProperty["SystemType"]?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions using the ExceptionView
                (new ExceptionView()).HandleException(ex);
            }
            finally
            {
                // Update the status bar to indicate completion
                StatusBarChange("Done", false);
            }
        }

        /*
         * Set the status bar text and progress bar state
         */
        protected void StatusBarChange(string label, bool progressbarLoading)
        {
            Bottom_Label.Text = label;
            Bottom_ProgressBar.IsIndeterminate = progressbarLoading;
        }
    }
}

using GetWMIBasic.DefaultUI;
using GetWMIBasic.WMIMethods;
using System;
using System.Linq;
using System.Management;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace GetWMIBasic
{
    public partial class MainWindow : Window
    {
        /*
         *  Global properties
         */
        protected MachineMethods machine;

        public MainWindow()
        {
            InitializeComponent();
            machine = new MachineMethods();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        /* 
         * Button-based methods
         */
        private async void ConnectToAnotherComputer_Button(object sender, RoutedEventArgs e)
        {
            ConnectForm connectForm = new ConnectForm();

            (string computerName, string username, SecureString password) userCredential = connectForm.ReturnValue();

            if (userCredential.computerName != "")
            {
                machine = null;
                machine = !userCredential.computerName.Equals("localhost") ? new MachineMethods(userCredential) : new MachineMethods();

                await Refresh();
            }
        }

        private void Exit_Button(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Restart_Button(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Do you want to restart the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                StatusBarChange("Loading", true);

                try
                {
                    if (machine.GetComputerName() == "localhost")
                    {
                        throw new ManagementException("Local Host cannot be restarted");
                    }

                    machine.Connect("root\\cimv2");
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 6 });
                }
                catch (Exception ex)
                {
                    exView.HandleException(ex);
                }
                finally
                {
                    StatusBarChange("Done", false);
                }
            }
        }

        private async void Shutdown_Button(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Do you want to shutdown the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                StatusBarChange("Loading", true);

                try
                {
                    if (machine.GetComputerName() == "localhost")
                    {
                        throw new ManagementException("Local Host cannot be shut down");
                    }

                    machine.Connect("root\\cimv2");
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 5 });
                }
                catch (Exception ex)
                {
                    (new ExceptionView()).HandleException(ex);
                }
                finally
                {
                    StatusBarChange("Done", false);
                }
            }
        }

        private async void Refresh_Button(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private void RegularException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new CommonException("This is a common exception example.");
            }
            catch (Exception ex)
            {
                exView.HandleException(ex);
            }
        }

        private void CriticalException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new CriticalException("This is a critical exception example.");
            }
            catch (Exception ex)
            {
                exView.HandleException(ex);
            }
        }

        /*
         * Non-button methods
         */
        private async Task Refresh()
        {
            StatusBarChange("Loading", true);

            try
            {
                machine.Connect("root\\cimv2");

                Task<ManagementObjectCollection> biosProperties = machine.GetObjects("Win32_BIOS", "Manufacturer, Name, SerialNumber, Version");
                foreach (ManagementObject biosProperty in (await biosProperties).Cast<ManagementObject>())
                {
                    BIOS_Manufacturer.Text = biosProperty["Manufacturer"]?.ToString() ?? string.Empty;
                    BIOS_Name.Text = biosProperty["Name"]?.ToString() ?? string.Empty;
                    BIOS_SN.Text = biosProperty["SerialNumber"]?.ToString() ?? string.Empty;
                    BIOS_Version.Text = biosProperty["Version"]?.ToString() ?? string.Empty;
                }

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
                exView.HandleException(ex);
            }
            finally
            {
                StatusBarChange("Done", false);
            }
        }

        protected void StatusBarChange(string label, bool progressbarLoading)
        {
            Bottom_Label.Text = label;
            Bottom_ProgressBar.IsIndeterminate = progressbarLoading;
        }
    }
}

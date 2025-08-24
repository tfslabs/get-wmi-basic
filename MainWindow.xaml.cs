using GetWMIBasic.WMIMethods;
using System.Linq;
using System.Management;
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
            Bottom_Label.Text = "Loading";
            await Refresh();
        }

        /* 
         * Button-based methods
         */
        private async void ConnectToAnotherComputer_ButtonAsync(object sender, RoutedEventArgs e)
        {
            ConnectForm connectForm = new ConnectForm();
            machine = new MachineMethods(connectForm.ReturnValue());

            await Refresh();
        }

        private void Exit_Button(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Restart_ButtonAsync(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Do you want to restart the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await machine.Connect("root\\cimv2");
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 6 });
                }
                catch (ManagementException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Shutdown_Button(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Do you want to shutdown the computer {machine.GetComputerName()}?", "Proceed?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await machine.Connect("root\\cimv2");
                    await machine.CallMethod("Win32_OperatingSystem", "*", "Win32Shutdown", new object[] { 5 });
                }
                catch (ManagementException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Refresh_ButtonAsync(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        /*
         * Non-button methods
         */
        private async Task Refresh()
        {
            try
            {
                await machine.Connect("root\\cimv2");

                var biosProperties = machine.GetObjects("Win32_BIOS", "*");
                foreach (ManagementObject biosProperty in (await biosProperties).Cast<ManagementObject>())
                {
                    BIOS_Manufacturer.Text = biosProperty["Manufacturer"]?.ToString() ?? string.Empty;
                    BIOS_Name.Text = biosProperty["Name"]?.ToString() ?? string.Empty;
                    BIOS_SN.Text = biosProperty["SerialNumber"]?.ToString() ?? string.Empty;
                    BIOS_Version.Text = biosProperty["Version"]?.ToString() ?? string.Empty;
                }

                var osProperties = machine.GetObjects("Win32_ComputerSystem", "*");
                foreach (ManagementObject osProperty in (await osProperties).Cast<ManagementObject>())
                {
                    Computer_Name.Text = osProperty["Name"]?.ToString() ?? string.Empty;
                    Computer_Domain.Text = osProperty["Domain"]?.ToString() ?? string.Empty;
                    Computer_Memory.Text = osProperty["TotalPhysicalMemory"]?.ToString() ?? string.Empty;
                    Computer_SysType.Text = osProperty["SystemType"]?.ToString() ?? string.Empty;
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using GetWMIBasic.WMIMethods;

/* The Default UI includes
 * 1. ConnectForm.xaml - A form to connect to a local or remote computer
 * 2. ExceptionView.xaml - A window to handle exception messages
 */
namespace GetWMIBasic.DefaultUI
{
    /*
     * Exception View class
     * It is used to handle exception messages
     * 
     * Excepted behaviour:
     *  1. An exception is passed to the HandleException method
     *  2. If the exception is of an allowed type, show the exception message 
     *      and give the user the option to ignore it; else, force close the application
     */
    public partial class ExceptionView : Window
    {
        /*
         * Global properties
         *  allowedExceptionTypes: A list of exception types that are allowed to be ignored
         */
        private readonly HashSet<Type> allowedExceptionTypes = new HashSet<Type>
        {
            typeof(UnauthorizedAccessException),
            typeof(COMException),
            typeof(ManagementException)
        };

        // Constructor of the Exception View class
        public ExceptionView()
        {
            InitializeComponent();
        }

        // Button-based methods

        /*
         * Click Ignore Exception Button - The "Ignore" button
         * Let user ignore the exception and continue using the application
         */
        private void Click_IgnoreException(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /*
         * Click Close Application Button - The "Close Application" button
         * Close the application in case of a critical or unhandled exception
         */
        private void Click_CloseApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Non-button methods

        /*
         * Handle Exception Method
         * Get the exception info, and decide whether to allow the user to ignore it
         */
        public void HandleException(Exception e)
        {
            // If the exception is null, ignore it
            if (e == null)
            {
                return;
            }

            // Check if the exception type is in the allowed list
            bool isAllowed = this.allowedExceptionTypes.Any(t => t.IsInstanceOfType(e));

            // If not allowed, change the message and hide the ignore button
            if (!isAllowed)
            {
                // Reset the exception message
                ExceptionMessage_TextBlock.Text = "An unexpected error has occurred.\n" +
                    "You should close the application and check for the system";
                // Hide the ignore button
                Button_Ignore.Visibility = Visibility.Collapsed;
            }

            // Show the detailed exception info
#if !DEBUG
            DetailedExceptionDetail_TextBox.Text = e.Message;
#else
            DetailedExceptionDetail_TextBox.Text = e.ToString();
#endif

            // Show the exception view window
            ShowDialog();
        }
    }
}

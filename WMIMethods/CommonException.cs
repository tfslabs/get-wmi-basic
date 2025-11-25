using System;

/*
 * The default WMI method class includes:
 *  1. CriticalException.cs - A custom exception class for critical WMI errors
 *  2. CommonException.cs - A custom exception class for common WMI errors
 *  3. MachineMethods.cs - A base class for WMI operations on local and remote machines
 */
namespace GetWMIBasic.WMIMethods
{
    /*
     * Common Exception class
     * It is used to test common exceptions in WMI operations
     */
    public class CommonException : SystemException
    {
        // Default constructor - No custom message (use default message)
        public CommonException()
        {
            // Throw a new exception with a default message
            throw new Exception("This is a custom exception for WMI-related errors.");
        }

        // Default constructor - with custom message
        public CommonException(string message) : base(message)
        {
            // Throw a new exception with custom message
            throw new Exception(message);
        }
    }
}

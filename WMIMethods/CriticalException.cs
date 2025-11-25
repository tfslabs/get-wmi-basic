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
     * Critical Exception class
     * It is used to test critical exceptions in WMI operations
     */
    public class CriticalException : SystemException
    {
        // Default constructor - No custom message (use default message)
        public CriticalException()
        {
            // Throw a new exception with a default message
            throw new Exception("A critical exception has occurred in the WMI operations.");
        }

        // Default constructor - with custom message
        public CriticalException(string message) : base(message)
        {
            // Throw a new exception with custom message
            throw new Exception(message);
        }
    }
}

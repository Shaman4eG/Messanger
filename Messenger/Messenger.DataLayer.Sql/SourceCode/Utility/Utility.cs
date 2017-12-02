using System.Data.SqlClient;
using System.Reflection;
using NLog;
using System;

namespace Messenger.DataLayer.Sql
{
    /// <summary>
    /// Useful methods within Messenger.DataLayer.Sql.
    /// </summary>
    static class Utility
    {
        /// <summary>
        /// Logs caught exception and rethrows it.
        /// </summary>
        /// <param name="ex"> Thrown exception </param>
        /// <param name="exceptionThrowingMethod"> Method, which threw exception. </param>
        /// <param name="inputedData"> Data inputed by user, when exception was thrown. </param>
        /// <param name="logger"> Logger, which will save information to log file. </param>
        internal static void SqlExceptionHandler(
            SqlException ex, 
            MethodBase exceptionThrowingMethod, 
            string inputedData, 
            Logger logger)
        {
            var type = exceptionThrowingMethod.DeclaringType;
            var methodFullName = string.Format("{0}.{1}", type.FullName, exceptionThrowingMethod.Name);

            logger.Error(
                ex.Message +
                $" Exception thrown in method: [{methodFullName}]. " +
                $"Inputed data: {inputedData}");

            // TODO: maybe better: throw new Exception("Some error" /* no idea what text, sources different */, ex);
            throw ex;
        }

    }
}

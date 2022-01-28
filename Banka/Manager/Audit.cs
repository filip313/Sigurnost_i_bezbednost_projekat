using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class Audit : IDisposable
    {

        public static EventLog customLog = null;
        const string SourceName = "Manager.Audit";
        const string LogName = "BankaServis";

        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName, Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }


        public static void IzdavanjeSuccess(string userName)
        {
            if (customLog != null)
            {
                string IzdavanjeSuccess = AuditEvents.IzdavanjeSuccess;
                string message = String.Format(IzdavanjeSuccess, userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.IzdavanjeSuccess));
            }
        }

        public static void IzdavanjeFailure(string userName, string error)
        {
            if (customLog != null)
            {
                string IzdavanjeFailure = AuditEvents.IzdavanjeFailure;
                string message = String.Format(IzdavanjeFailure, userName, error);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.IzdavanjeFailure));
            }
        }

        public static void ObnovaSuccess(string userName)
        {
            if (customLog != null)
            {
                string ObnovaSuccess = AuditEvents.ObnovaSuccess;
                string message = String.Format(ObnovaSuccess, userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.ObnovaSuccess));
            }
        }

        public static void ObnovaFailure(string userName, string error)
        {
            if (customLog != null)
            {
                string ObnovaFailure = AuditEvents.ObnovaFailure;
                string message = String.Format(ObnovaFailure, userName, error);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.ObnovaFailure));
            }
        }

        public static void UplataSuccess(string userName, string iznos)
        {
            if (customLog != null)
            {
                string UplataSuccess = AuditEvents.UplataSuccess;
                string message = String.Format(UplataSuccess, userName, iznos);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.UplataSuccess));
            }
        }

        public static void UplataFailure(string userName, string error)
        {
            if (customLog != null)
            {
                string UplataFailure = AuditEvents.UplataFailure;
                string message = String.Format(UplataFailure, userName, error);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.UplataFailure));
            }
        }

        public static void IsplataSuccess(string userName, string iznos)
        {
            if (customLog != null)
            {
                string IsplataSuccess = AuditEvents.IsplataSuccess;
                string message = String.Format(IsplataSuccess, userName, iznos);
                customLog.WriteEntry(message, EventLogEntryType.Information, (int)AuditEventTypes.IsplataSuccess);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.IsplataSuccess));
            }
        }

        public static void IsplataFailure(string userName, string error)
        {
            if (customLog != null)
            {
                string IsplataFailure = AuditEvents.IsplataFailure;
                string message = String.Format(IsplataFailure, userName, error);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.IsplataFailure));
            }
        }

        public static void PromenaPinaSuccess(string userName)
        {
            if (customLog != null)
            {
                string PromenaPinaSuccess = AuditEvents.PromenaPinaSuccess;
                string message = String.Format(PromenaPinaSuccess, userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.PromenaPinaSuccess));
            }
        }

        public static void PromenaPinaFailure(string userName, string error)
        {
            if (customLog != null)
            {
                string PromenaPinaFailure = AuditEvents.PromenaPinaFailure;
                string message = String.Format(PromenaPinaFailure, userName, error);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.PromenaPinaFailure));
            }
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}

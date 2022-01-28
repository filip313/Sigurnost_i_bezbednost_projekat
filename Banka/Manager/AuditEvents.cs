using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public enum AuditEventTypes
    {
        IzdavanjeSuccess = 0,
        IzdavanjeFailure = 1,
        ObnovaSuccess = 2,
        ObnovaFailure = 3,
        UplataSuccess = 4,
        UplataFailure = 5,
        IsplataSuccess = 6,
        IsplataFailure = 7,
        PromenaPinaSuccess = 8,
        PromenaPinaFailure = 9
    }

    public class AuditEvents
    {
        private static ResourceManager resourceManager = null;
        private static object resourceLock = new object();

        private static ResourceManager ResourceMgr
        {
            get
            {
                lock (resourceLock)
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager(typeof(AuditEventFile).ToString(),
                            Assembly.GetExecutingAssembly());
                    }
                    return resourceManager;
                }
            }
        }

        public static string IzdavanjeSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.IzdavanjeSuccess.ToString());
            }
        }

        public static string IzdavanjeFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.IzdavanjeFailure.ToString());
            }
        }

        public static string ObnovaSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.ObnovaSuccess.ToString());
            }
        }

        public static string ObnovaFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.ObnovaFailure.ToString());
            }
        }

        public static string UplataSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.UplataSuccess.ToString());
            }
        }

        public static string UplataFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.UplataFailure.ToString());
            }
        }

        public static string IsplataSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.IsplataSuccess.ToString());
            }
        }

        public static string IsplataFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.IsplataFailure.ToString());
            }
        }

        public static string PromenaPinaSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.PromenaPinaSuccess.ToString());
            }
        }

        public static string PromenaPinaFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.PromenaPinaFailure.ToString());
            }
        }
    }
}

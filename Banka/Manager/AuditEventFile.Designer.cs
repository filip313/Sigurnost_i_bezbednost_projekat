﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Manager {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AuditEventFile {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AuditEventFile() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Manager.AuditEventFile", typeof(AuditEventFile).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} nije uspesno podigao novac sa racuna. Razlog: {1}.
        /// </summary>
        internal static string IsplataFailure {
            get {
                return ResourceManager.GetString("IsplataFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik [{0}] je uspesno podigao [{1}] sa racuna..
        /// </summary>
        internal static string IsplataSuccess {
            get {
                return ResourceManager.GetString("IsplataSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} nije uspesno registrovan. Razlog: {1}.
        /// </summary>
        internal static string IzdavanjeFailure {
            get {
                return ResourceManager.GetString("IzdavanjeFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} je uspesno registrovan..
        /// </summary>
        internal static string IzdavanjeSuccess {
            get {
                return ResourceManager.GetString("IzdavanjeSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Obnova kartice korisnika {0} nije uspesno izvrsena. Razlog: {1}.
        /// </summary>
        internal static string ObnovaFailure {
            get {
                return ResourceManager.GetString("ObnovaFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Obnova kartice korisnika {0} je uspesno izvrsena..
        /// </summary>
        internal static string ObnovaSuccess {
            get {
                return ResourceManager.GetString("ObnovaSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} nije uspesno promenio PIN kod. Razlog: {1}.
        /// </summary>
        internal static string PromenaPinaFailure {
            get {
                return ResourceManager.GetString("PromenaPinaFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} je uspesno promenio PIN kod..
        /// </summary>
        internal static string PromenaPinaSuccess {
            get {
                return ResourceManager.GetString("PromenaPinaSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} nije uspesno uplatio novac na racun. Razlog: {1}.
        /// </summary>
        internal static string UplataFailure {
            get {
                return ResourceManager.GetString("UplataFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Korisnik {0} je uspesno uplatio [{1}] na racun..
        /// </summary>
        internal static string UplataSuccess {
            get {
                return ResourceManager.GetString("UplataSuccess", resourceCulture);
            }
        }
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace apcurium.MK.Booking.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Thriev {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Thriev() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("apcurium.MK.Booking.Resources.Thriev", typeof(Thriev).Assembly);
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
        ///   Looks up a localized string similar to Car #{0} is assigned to you.
        /// </summary>
        internal static string OrderStatus_CabDriverNumberAssigned {
            get {
                return ResourceManager.GetString("OrderStatus_CabDriverNumberAssigned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Car is at pickup location.
        /// </summary>
        internal static string OrderStatus_wosARRIVED {
            get {
                return ResourceManager.GetString("OrderStatus_wosARRIVED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Passengers are in the car.
        /// </summary>
        internal static string OrderStatus_wosLOADED {
            get {
                return ResourceManager.GetString("OrderStatus_wosLOADED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry no Thriev cars are available for the next hour due to high demand, please try later. Thank you for trying Thriev..
        /// </summary>
        internal static string OrderStatus_wosTIMEOUT {
            get {
                return ResourceManager.GetString("OrderStatus_wosTIMEOUT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Driver will confirm within 2 minutes.
        /// </summary>
        internal static string OrderStatus_wosWAITING {
            get {
                return ResourceManager.GetString("OrderStatus_wosWAITING", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your car (#{0}) has arrived.
        /// </summary>
        internal static string PushNotification_wosARRIVED {
            get {
                return ResourceManager.GetString("PushNotification_wosARRIVED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Car #{0} has been assigned to you.
        /// </summary>
        internal static string PushNotification_wosASSIGNED {
            get {
                return ResourceManager.GetString("PushNotification_wosASSIGNED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry no Thriev cars are available for the next hour due to high demand, please try later..
        /// </summary>
        internal static string PushNotification_wosTIMEOUT {
            get {
                return ResourceManager.GetString("PushNotification_wosTIMEOUT", resourceCulture);
            }
        }
    }
}

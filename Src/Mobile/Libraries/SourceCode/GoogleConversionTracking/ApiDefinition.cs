using System;
using System.Drawing;

using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace GoogleConversionTracking
{
    /**
     * 
     * Generated using Objective Sharpie with Xcode 5.1.1 - SDK 7.1
     * 
     * */
    [BaseType (typeof (NSObject))]
    public partial interface ACTReporter {

        [Static, Export ("SDKVersion")]
        string SDKVersion { get; }

        [Export ("report")]
        bool Report { get; }
    }

    [BaseType (typeof (ACTReporter))]
    public partial interface ACTConversionReporter {

        [Export ("value", ArgumentSemantic.Copy)]
        string Value { get; set; }

        [Static, Export ("reportWithConversionID:label:value:isRepeatable:")]
        void ReportWithConversionID (string conversionID, string label, string value, bool isRepeatable);

        [Static, Export ("reportWithProductID:value:isRepeatable:")]
        void ReportWithProductID (string productID, string value, bool isRepeatable);

        [Static, Export ("registerReferrer:")]
        bool RegisterReferrer (NSUrl clickURL);

        [Export ("initWithConversionID:label:value:isRepeatable:")]
        IntPtr Constructor (string conversionID, string label, string value, bool isRepeatable);

        [Export ("initWithProductID:value:isRepeatable:")]
        IntPtr Constructor (string productID, string value, bool isRepeatable);
    }

    [BaseType (typeof (ACTReporter))]
    public partial interface ACTRemarketingReporter {

        [Static, Export ("reportWithConversionID:customParameters:")]
        void ReportWithConversionID (string conversionID, NSDictionary customParameters);

        [Export ("initWithConversionID:customParameters:")]
        IntPtr Constructor (string conversionID, NSDictionary customParameters);
    }

    [BaseType (typeof (NSObject))]
    public partial interface ACTAutomatedUsageTracker {

        [Static, Export ("enableAutomatedUsageReportingWithConversionID:")]
        void EnableAutomatedUsageReportingWithConversionID (string conversionID);

        [Static, Export ("disableAutomatedUsageReportingWithConversionID:")]
        void DisableAutomatedUsageReportingWithConversionID (string conversionID);
    }
}


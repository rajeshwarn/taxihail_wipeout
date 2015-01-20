using System;
using ObjCRuntime;
using Foundation;
using UIKit;

namespace GoogleConversionTracking.Unified
{
    [BaseType (typeof (NSObject))]
    public partial interface ACTReporter 
    {
        [Static, Export ("SDKVersion")]
        NSString SDKVersion { get; }

        [Export ("report")]
        bool Report { get; }
    }

    [BaseType (typeof (ACTReporter))]
    public partial interface ACTConversionReporter 
    {
        [Export ("value", ArgumentSemantic.Copy)]
        NSString Value { get; set; }

        [Static, Export ("reportWithConversionID:label:value:isRepeatable:")]
        void ReportWithConversionID (NSString conversionID, NSString label, NSString value, bool isRepeatable);

        [Static, Export ("reportWithProductID:value:isRepeatable:")]
        void ReportWithProductID (NSString productID, NSString value, bool isRepeatable);

        [Static, Export ("registerReferrer:")]
        bool RegisterReferrer (NSUrl clickURL);

        [Export ("initWithConversionID:label:value:isRepeatable:")]
        IntPtr Constructor (NSString conversionID, NSString label, NSString value, bool isRepeatable);

        [Export ("initWithProductID:value:isRepeatable:")]
        IntPtr Constructor (NSString productID, NSString value, bool isRepeatable);
    }

    [BaseType (typeof (ACTReporter))]
    public partial interface ACTRemarketingReporter 
    {
        [Static, Export ("reportWithConversionID:customParameters:")]
        void ReportWithConversionID (NSString conversionID, NSDictionary customParameters);

        [Export ("initWithConversionID:customParameters:")]
        IntPtr Constructor (NSString conversionID, NSDictionary customParameters);
    }

    [BaseType (typeof (NSObject))]
    public partial interface ACTAutomatedUsageTracker {

        [Static, Export ("enableAutomatedUsageReportingWithConversionID:")]
        void EnableAutomatedUsageReportingWithConversionID (NSString conversionID);

        [Static, Export ("disableAutomatedUsageReportingWithConversionID:")]
        void DisableAutomatedUsageReportingWithConversionID (NSString conversionID);
    }
}


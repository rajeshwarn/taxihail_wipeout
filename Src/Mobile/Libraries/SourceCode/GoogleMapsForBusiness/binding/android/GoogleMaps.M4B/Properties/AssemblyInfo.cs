using System.Reflection;
using System.Runtime.CompilerServices;
using Android.App;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle ("GoogleMaps.M4B")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("")]
[assembly: AssemblyCopyright ("billholmes")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion ("1.0.0")]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

[assembly: Android.IncludeAndroidResourcesFromAttribute ("maps/google-maps-sdk-m4b_lib",
	PackageName = __Consts.PackageName,
	SourceUrl   = __Consts.SupportUrl,
	Version     = __Consts.Version)]

[assembly: Java.Interop.JavaLibraryReference ("maps/google-maps-sdk-m4b_lib/libs/guava-jdk5-14.0.1.jar",
	PackageName = __Consts.PackageName,
	SourceUrl   = __Consts.SupportUrl,
	Version     = __Consts.Version)]

[assembly: Java.Interop.JavaLibraryReference ("maps/google-maps-sdk-m4b_lib/libs/maps_m4b.jar",
	PackageName = __Consts.PackageName,
	SourceUrl   = __Consts.SupportUrl,
	Version     = __Consts.Version)]

static class __Consts {
	public const string PackageName = "Google M4B";
	public const string SupportUrl  = "https://dl.google.com/dl/geosdk/googlemaps-android-m4b-2.9.0.rc1.zip";
	public const string Version = "2.9.0.rc1";
}


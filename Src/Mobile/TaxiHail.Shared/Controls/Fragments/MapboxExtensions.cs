// Some helper to fit the legacy API and overcome java bindings issues with getters/setters and abstract.

using Com.Mapbox.Mapboxsdk.Annotations;
using Com.Mapbox.Mapboxsdk.Geometry;

public static class MarkerExtensions
{
	public static MarkerOptions SetPosition(this MarkerOptions markerOptions, LatLng position)
	{
		return (MarkerOptions)markerOptions.Position(position);
	}

	public static MarkerOptions SetIcon(this MarkerOptions markerOptions, Icon icon)
	{
		return (MarkerOptions)markerOptions.Icon(icon);
	}

	public static MarkerOptions SetTitle(this MarkerOptions markerOptions, string title)
	{
		return (MarkerOptions)markerOptions.Title(title);
	}

	public static MarkerOptions SetSnippet(this MarkerOptions markerOptions, string snippet)
	{
		return (MarkerOptions)markerOptions.Snippet(snippet);
	}
}
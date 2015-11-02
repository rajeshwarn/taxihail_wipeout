namespace Com.Mapbox.Mapboxsdk.Annotations
{
    public partial class Polyline
    {
        public override global::System.Int32 CompareTo (global::Java.Lang.Object another)
        {
            var annotation = another as Annotation;

            if (another != null)
            {
                return CompareTo(annotation);
            }

            return -1;
        }
    }
}

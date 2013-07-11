namespace ninePatchMaker
{
    public class NinePatchRange
    {
        public double StartPercentage { get; set; }
        public double EndPercentage { get; set; }

        private NinePatchRange(double startPercentage, double endPercentage)
        {
            StartPercentage = startPercentage;
            EndPercentage = endPercentage;
        }

        public static NinePatchRange CreateFromStart(double startPercentage)
        {
            return new NinePatchRange(startPercentage, startPercentage);
        }

        public static NinePatchRange CreateFromStart(double startPercentage, double lengthPercentage)
        {
            return new NinePatchRange(startPercentage, startPercentage+lengthPercentage);   
        }
        public static NinePatchRange CreateFromEnd(double endPercentage, double lengthPercentage)
        {
            return new NinePatchRange(endPercentage - lengthPercentage, endPercentage);
        }

        public static NinePatchRange CreateFromRange(double startPercentage, double endPercentage)
        {
            return new NinePatchRange(startPercentage, endPercentage);
        }

        public bool Contains(int number, int length)
        {
            return number >= (StartPercentage*length)
                && number <= (EndPercentage * length);
        }
    }
}
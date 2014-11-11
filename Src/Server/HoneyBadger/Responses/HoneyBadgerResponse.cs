
namespace HoneyBadger.Responses
{
    public class HoneyBadgerResponse
    {
        public int Total { get; set; }

        public int Working { get; set; }

        public int Hired { get; set; }

        public int Available { get; set; }

        public HoneyBadgerContent[] Entities { get; set; }
    }
}

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    public class AccountCmtRequest
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public string Language { get; set; }

        public int AccountStatus { get; set; }
    }
}
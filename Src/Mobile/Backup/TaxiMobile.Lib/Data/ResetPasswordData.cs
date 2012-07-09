namespace TaxiMobile.Lib.Data
{
	public class ResetPasswordData
	{
		
			public string Email {
				get;
				set;
			}

	    public string OldEmail { get; set; }

	    public string OldPassword { get; set; }

	    public string NewPassword { get; set; }
	}
}


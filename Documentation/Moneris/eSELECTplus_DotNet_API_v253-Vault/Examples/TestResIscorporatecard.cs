namespace Moneris
{
    using System;
    using System.Text;
    using System.Collections;
    
	public class TestResIscorporatecard
	{
	  public static void Main(string[] args)
	  {

		string host = "esqa.moneris.com";
        string store_id = "store5";
        string api_token = "yesguy";
        string data_key = "9A143sx23Y2Sb426J45GXYYM8";

        ResIscorporatecard resIscorporatecard = new ResIscorporatecard(data_key);

        HttpsPostRequest mpgReq = new HttpsPostRequest(host, store_id, api_token, resIscorporatecard);
	                     
		/**********************   REQUEST  ************************/	                     
	      
	        try
	        {
	            Receipt receipt = mpgReq.GetReceipt();

                Console.WriteLine("DataKey = " + receipt.GetDataKey());
                Console.WriteLine("CorporateCard = " + receipt.GetCorporateCard());
                Console.WriteLine("ResponseCode = " + receipt.GetResponseCode());
                Console.WriteLine("Message = " + receipt.GetMessage());
                Console.WriteLine("TransDate = " + receipt.GetTransDate());
                Console.WriteLine("TransTime = " + receipt.GetTransTime());
                Console.WriteLine("Complete = " + receipt.GetComplete());
                Console.WriteLine("TimedOut = " + receipt.GetTimedOut());
                Console.WriteLine("ResSuccess = " + receipt.GetResSuccess());
                Console.WriteLine("PaymentType = " + receipt.GetPaymentType());
               
	        }
	        catch (Exception e)
	        {
	            Console.WriteLine(e);
	        }
	  }
				
	} // end TestResIscorporatecard
}

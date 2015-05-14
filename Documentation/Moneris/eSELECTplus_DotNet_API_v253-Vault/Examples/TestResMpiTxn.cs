namespace Moneris
{
    using System;
    using System.Text;
    using System.Collections;
    
	public class TestResMpiTxn
	{
	  public static void Main(string[] args)
	  {

		string host = "esqa.moneris.com";
        string store_id = "moneris";
        string api_token = "hurgle";
        string data_key = "UeYmtqFeowPYroEBEdjXXSb0r";
        string amount = "1.00";
        string xid = "12345678910111214005";
        string MD = xid + "mycardinfo" + amount;
        string merchantUrl = "www.mystoreurl.com";
        string accept = "true";
        string userAgent = "Mozilla";

        ResMpiTxn resMpiTxn = new ResMpiTxn(data_key, xid, amount, MD, merchantUrl, accept, userAgent);

        //************************OPTIONAL VARIABLES***************************

        HttpsPostRequest mpgReq = new HttpsPostRequest(host, store_id, api_token, resMpiTxn);
	                     
		/**********************   REQUEST  ************************/	                     
	      
	        try
	        {
	            Receipt receipt = mpgReq.GetReceipt();

                Console.WriteLine("MpiMessage = " + receipt.GetMpiMessage());
                Console.WriteLine("MpiSuccess = " + receipt.GetMpiSuccess());

                if (receipt.GetMpiSuccess() == "true")
                {
                    Console.WriteLine(receipt.GetInLineForm());
                }
            }
	        catch (Exception e)
	        {
	            Console.WriteLine(e);
	        }
	  }
				
	} // end TestResMpiTxn
}

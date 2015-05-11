namespace Moneris
{
    using System;
    using System.Text;
    using System.Collections;
    
	public class TestResPurchaseCC
	{
	  public static void Main(string[] args)
	  {

		string host = "esqa.moneris.com";
        string store_id = "store5";
        string api_token = "yesguy";
        string data_key = "g90io5hS63qXu10Pu51512M8G";
		string order_id = "res_purchase_7";
        string amount = "1.00";
        string cust_id = "customer1"; //if sent will be submitted, otherwise cust_id from profile will be used
        string crypt_type = "2";
  
        ResPurchaseCC resPurchaseCC = new ResPurchaseCC(data_key, order_id, cust_id, amount, crypt_type);

        /************************* Recur Variables **********************************/

        string recur_unit = "month";
        string start_now = "true";
        string start_date = "2009/12/01";
        string num_recurs = "12";
        string period = "1";
        string recur_amount = "30.00";

        /************************* Recur Object Option1 ******************************/

        Recur recurring_cycle = new Recur(recur_unit, start_now, start_date,
                           num_recurs, period, recur_amount);

        resPurchaseCC.SetRecur(recurring_cycle);
	                     
		/**********************   REQUEST  ************************/
        HttpsPostRequest mpgReq = new HttpsPostRequest(host, store_id, api_token, resPurchaseCC);
  
	        try
	        {
	            Receipt receipt = mpgReq.GetReceipt();

                Console.WriteLine("DataKey = " + receipt.GetDataKey());
                Console.WriteLine("ReceiptId = " + receipt.GetReceiptId());
                Console.WriteLine("ReferenceNum = " + receipt.GetReferenceNum());
                Console.WriteLine("ResponseCode = " + receipt.GetResponseCode());
                Console.WriteLine("AuthCode = " + receipt.GetAuthCode());
                Console.WriteLine("Message = " + receipt.GetMessage());
                Console.WriteLine("TransDate = " + receipt.GetTransDate());
                Console.WriteLine("TransTime = " + receipt.GetTransTime());
                Console.WriteLine("TransType = " + receipt.GetTransType());
                Console.WriteLine("Complete = " + receipt.GetComplete());
                Console.WriteLine("TransAmount = " + receipt.GetTransAmount());
                Console.WriteLine("CardType = " + receipt.GetCardType());
                Console.WriteLine("TxnNumber = " + receipt.GetTxnNumber());
                Console.WriteLine("TimedOut = " + receipt.GetTimedOut());
                Console.WriteLine("AVSResponse = " + receipt.GetAvsResultCode());
                Console.WriteLine("CVDResponse = " + receipt.GetCvdResultCode());
                Console.WriteLine("RecurSuccess = " + receipt.GetRecurSuccess());
                Console.WriteLine("ResSuccess = " + receipt.GetResSuccess());
                Console.WriteLine("PaymentType = " + receipt.GetPaymentType());

                //ResolveData
                Console.WriteLine("\nCust ID = " + receipt.GetResDataCustId());
                Console.WriteLine("Phone = " + receipt.GetResDataPhone());
                Console.WriteLine("Email = " + receipt.GetResDataEmail());
                Console.WriteLine("Note = " + receipt.GetResDataNote());
                Console.WriteLine("Masked Pan = " + receipt.GetResDataMaskedPan());
                Console.WriteLine("Exp Date = " + receipt.GetResDataExpdate());
                Console.WriteLine("Crypt Type = " + receipt.GetResDataCryptType());
                Console.WriteLine("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
                Console.WriteLine("Avs Street Name = " + receipt.GetResDataAvsStreetName());
                Console.WriteLine("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
                
                	
	        }
	        catch (Exception e)
	        {
	            Console.WriteLine(e);
	        }
	  }
				
	} // end TestResPurchaseCC-Recur
}

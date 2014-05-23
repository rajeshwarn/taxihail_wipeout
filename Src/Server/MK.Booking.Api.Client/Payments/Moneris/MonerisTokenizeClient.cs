using System;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.Payments.Moneris
{
	public class MonerisTokenizeClient
	{
        private readonly MonerisPaymentSettings _settings;
		private readonly ILogger _logger;

		private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisTokenizeClient(MonerisPaymentSettings settings, ILogger logger)
		{
			_logger = logger;
			_settings = settings;
		}

		public async Task<Receipt> TokenizeAsync(string cardNumber, string expirationDate)
		{
			var tokenizeCommand = new ResAddCC(cardNumber, expirationDate, CryptType_SSLEnabledMerchant);
			var request = new HttpsPostRequest(_settings.Host, _settings.StoreId, _settings.ApiToken, tokenizeCommand, _logger);
			return await request.GetReceiptAsync ();
		}

		private class HttpsPostRequest
		{
			private Receipt receiptObj = new Receipt();
			private WebProxy proxy = (WebProxy)null;
			private Transaction transaction;
			private ILogger logger;
			private string storeId;
			private string apiToken;
			private string status;
			private string url;

			public HttpsPostRequest(string host, string store, string apiTok, Transaction t, ILogger logger)
			{
				this.storeId = store;
				this.apiToken = apiTok;
				this.transaction = t;
				this.url = "https://" + host + ":443/gateway2/servlet/MpgRequest";
				this.logger = logger;
			}

			public async Task<Receipt> GetReceiptAsync()
			{
				byte[] bytes = Encoding.ASCII.GetBytes(this.toXML());
				try
				{
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(this.url));
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded";
					if (this.proxy != null)
					{
						httpWebRequest.Proxy = (IWebProxy)this.proxy;
						httpWebRequest.Credentials = this.proxy.Credentials;
					}
					httpWebRequest.ContentLength = (long)bytes.Length;
					httpWebRequest.UserAgent = "DOTNET - 2.5.3 - Resolver";

					using(var requestStream = await ((WebRequest)httpWebRequest).GetRequestStreamAsync())
					{
						requestStream.Write(bytes, 0, bytes.Length);
						requestStream.Flush();

						using(var response = await httpWebRequest.GetResponseAsync())
						{
							var responseStream = response.GetResponseStream();
							this.receiptObj = new Receipt(responseStream);
						}
					}
				}
				catch (Exception ex)
				{
					this.receiptObj = new Receipt ();
					logger.LogError (ex);
				}

				return this.receiptObj;
			}

			public Receipt GetReceipt()
			{
				return this.receiptObj;
			}

			public string toXML()
			{
				string str = "<?xml version=\"1.0\"?>";
				if (this.status == "true")
					return ((object)new StringBuilder(str + "<request><store_id>" + this.storeId + "</store_id><api_token>" + this.apiToken + "</api_token><status_check>" + this.status + "</status_check>" + this.transaction.toXML() + "</request>")).ToString();
				else if (this.status == "false")
					return ((object)new StringBuilder(str + "<request><store_id>" + this.storeId + "</store_id><api_token>" + this.apiToken + "</api_token><status_check>" + this.status + "</status_check>" + this.transaction.toXML() + "</request>")).ToString();
				else
					return ((object)new StringBuilder(str + "<request><store_id>" + this.storeId + "</store_id><api_token>" + this.apiToken + "</api_token>" + this.transaction.toXML() + "</request>")).ToString();
			}
		}

		public class Receipt
		{
			private Stream inStream = (Stream)null;
			private XmlTextReader xtr = (XmlTextReader)null;
			private string currentTag = (string)null;
			private string currentTermID = (string)null;
			private string currentCardType = (string)null;
			private string currentTxnType = (string)null;
			private string currentDataKey = (string)null;
			private bool isBatchTotals = false;
			private bool isResolveData = false;
			private bool hasMultipleDataKey = false;
			private Hashtable termIDHash = new Hashtable();
			private Hashtable cardHash = new Hashtable();
			private Hashtable responseDataHash = new Hashtable();
			private Hashtable resDataHash = new Hashtable();
			private Hashtable dataKeyHash = new Hashtable();
			private Stack dataKeyStack = new Stack();
			private string globalErrorReceipt = "<?xml version=\"1.0\"?><response><receipt><ReceiptId>Global Error Receipt</ReceiptId><ReferenceNum>null</ReferenceNum><ResponseCode>null</ResponseCode><ISO>null</ISO> <AuthCode>null</AuthCode><TransTime>null</TransTime><TransDate>null</TransDate><TransType>null</TransType><Complete>false</Complete><Message>null</Message><TransAmount>null</TransAmount><CardType>null</CardType><TransID>null</TransID><TimedOut>null</TimedOut></receipt></response>";
			private Hashtable purchaseHash;
			private Hashtable refundHash;
			private Hashtable correctionHash;

			public Receipt(Stream aStream)
			{
				this.inStream = aStream;
				this.xtr = new XmlTextReader(this.inStream);
				this.doParse();
				this.xtr.Close();
			}

			public Receipt()
			{
				this.xtr = new XmlTextReader((TextReader)new StringReader(this.globalErrorReceipt));
				this.doParse();
				this.xtr.Close();
			}

			private void doParse()
			{
				while (this.xtr.Read())
				{
					switch (this.xtr.NodeType)
					{
					case XmlNodeType.Element:
						this.beginHandler(this.xtr.Name);
						break;
					case XmlNodeType.Text:
						this.textHandler(this.xtr.Value);
						break;
					case XmlNodeType.EndElement:
						this.endHandler(this.xtr.Name);
						break;
					}
				}
			}

			private void beginHandler(string tag)
			{
				this.currentTag = tag;
				if (tag.Equals("BankTotals"))
				{
					this.isBatchTotals = true;
					this.purchaseHash = new Hashtable();
					this.refundHash = new Hashtable();
					this.correctionHash = new Hashtable();
				}
				if (this.isBatchTotals)
				{
					if (this.currentTag.Equals("Purchase"))
						this.currentTxnType = "Purchase";
					else if (this.currentTag.Equals("Refund"))
						this.currentTxnType = "Refund";
					else if (this.currentTag.Equals("Correction"))
						this.currentTxnType = "Correction";
				}
				if (!tag.Equals("ResolveData"))
					return;
				this.isResolveData = true;
				this.resDataHash = new Hashtable();
			}

			private void endHandler(string tag)
			{
				if (tag.Equals("BankTotals"))
					this.isBatchTotals = false;
				if (!tag.Equals("ResolveData"))
					return;
				this.isResolveData = false;
			}

			private void textHandler(string data)
			{
				if (this.isBatchTotals)
				{
					if (this.currentTag.Equals("term_id"))
					{
						this.currentTermID = data;
						this.cardHash.Add((object)this.currentTermID, (object)new Stack());
						this.purchaseHash.Add((object)this.currentTermID, (object)new Hashtable());
						this.refundHash.Add((object)this.currentTermID, (object)new Hashtable());
						this.correctionHash.Add((object)this.currentTermID, (object)new Hashtable());
					}
					else if (this.currentTag.Equals("closed"))
						this.termIDHash.Add((object)this.currentTermID, (object)data);
					else if (this.currentTag.Equals("CardType"))
					{
						((Stack)this.cardHash[(object)this.currentTermID]).Push((object)data);
						this.currentCardType = data;
						((Hashtable)this.purchaseHash[(object)this.currentTermID])[(object)this.currentCardType] = (object)new Hashtable();
						((Hashtable)this.refundHash[(object)this.currentTermID])[(object)this.currentCardType] = (object)new Hashtable();
						((Hashtable)this.correctionHash[(object)this.currentTermID])[(object)this.currentCardType] = (object)new Hashtable();
					}
					else if (this.currentTag.Equals("Amount"))
					{
						if (this.currentTxnType.Equals("Purchase"))
							((Hashtable)((Hashtable)this.purchaseHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Amount"] = (object)data;
						else if (this.currentTxnType.Equals("Refund"))
						{
							((Hashtable)((Hashtable)this.refundHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Amount"] = (object)data;
						}
						else
						{
							if (!this.currentTxnType.Equals("Correction"))
								return;
							((Hashtable)((Hashtable)this.correctionHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Amount"] = (object)data;
						}
					}
					else
					{
						if (!this.currentTag.Equals("Count"))
							return;
						if (this.currentTxnType.Equals("Purchase"))
							((Hashtable)((Hashtable)this.purchaseHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Count"] = (object)data;
						else if (this.currentTxnType.Equals("Refund"))
							((Hashtable)((Hashtable)this.refundHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Count"] = (object)data;
						else if (this.currentTxnType.Equals("Correction"))
							((Hashtable)((Hashtable)this.correctionHash[(object)this.currentTermID])[(object)this.currentCardType])[(object)"Count"] = (object)data;
					}
				}
				else if (this.isResolveData && !data.Equals("null"))
				{
					if (this.currentTag.Equals("data_key"))
					{
						this.currentDataKey = data;
						this.dataKeyHash.Add((object)this.currentDataKey, (object)new Hashtable());
						this.dataKeyStack.Push((object)this.currentDataKey);
					}
					else
						((Hashtable)this.dataKeyHash[(object)this.currentDataKey])[(object)this.currentTag] = (object)data;
					this.resDataHash[(object)this.currentTag] = (object)data;
				}
				else
				{
					this.responseDataHash[(object)this.currentTag] = (object)data;
					if (this.currentTag.Equals("DataKey"))
					{
						if (data.Equals("null"))
						{
							this.hasMultipleDataKey = true;
						}
						else
						{
							this.currentDataKey = data;
							this.dataKeyHash.Add((object)this.currentDataKey, (object)new Hashtable());
							this.dataKeyStack.Push((object)this.currentDataKey);
						}
					}
				}
			}

			public string GetPurchaseAmount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.purchaseHash[(object)ecr_no])[(object)card_type])[(object)"Amount"];
				return str == null ? "0" : str;
			}

			public string GetPurchaseCount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.purchaseHash[(object)ecr_no])[(object)card_type])[(object)"Count"];
				return str == null ? "0" : str;
			}

			public string GetRefundAmount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.refundHash[(object)ecr_no])[(object)card_type])[(object)"Amount"];
				return str == null ? "0" : str;
			}

			public string GetRefundCount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.refundHash[(object)ecr_no])[(object)card_type])[(object)"Count"];
				return str == null ? "0" : str;
			}

			public string GetCorrectionAmount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.correctionHash[(object)ecr_no])[(object)card_type])[(object)"Amount"];
				return str == null ? "0" : str;
			}

			public string GetCorrectionCount(string ecr_no, string card_type)
			{
				string str = (string)((Hashtable)((Hashtable)this.correctionHash[(object)ecr_no])[(object)card_type])[(object)"Count"];
				return str == null ? "0" : str;
			}

			public string GetTerminalStatus(string ecr_no)
			{
				return (string)this.termIDHash[(object)ecr_no];
			}

			public string[] GetTerminalIDs()
			{
				string[] strArray = new string[this.termIDHash.Count];
				IDictionaryEnumerator enumerator = this.termIDHash.GetEnumerator();
				int num = 0;
				while (enumerator.MoveNext())
					strArray[num++] = (string)enumerator.Key;
				return strArray;
			}

			public string[] GetCreditCards(string ecr_no)
			{
				Stack stack = (Stack)this.cardHash[(object)ecr_no];
				string[] strArray = new string[stack.Count];
				IEnumerator enumerator = stack.GetEnumerator();
				int num = 0;
				while (enumerator.MoveNext())
					strArray[num++] = (string)enumerator.Current;
				return strArray;
			}

			public string GetITDResponse()
			{
				return (string)this.responseDataHash[(object)"ITDResponse"];
			}

			public string GetCardType()
			{
				return (string)this.responseDataHash[(object)"CardType"];
			}

			public string GetTransAmount()
			{
				return (string)this.responseDataHash[(object)"TransAmount"];
			}

			public string GetTxnNumber()
			{
				return (string)this.responseDataHash[(object)"TransID"];
			}

			public string GetReceiptId()
			{
				return (string)this.responseDataHash[(object)"ReceiptId"];
			}

			public string GetTransType()
			{
				return (string)this.responseDataHash[(object)"TransType"];
			}

			public string GetReferenceNum()
			{
				return (string)this.responseDataHash[(object)"ReferenceNum"];
			}

			public string GetResponseCode()
			{
				return (string)this.responseDataHash[(object)"ResponseCode"];
			}

			public string GetISO()
			{
				return (string)this.responseDataHash[(object)"ISO"];
			}

			public string GetBankTotals()
			{
				return (string)this.responseDataHash[(object)"BankTotals"];
			}

			public string GetMessage()
			{
				return (string)this.responseDataHash[(object)"Message"];
			}

			public string GetRecurSuccess()
			{
				return (string)this.responseDataHash[(object)"RecurSuccess"];
			}

			public string GetAuthCode()
			{
				return (string)this.responseDataHash[(object)"AuthCode"];
			}

			public string GetComplete()
			{
				return (string)this.responseDataHash[(object)"Complete"];
			}

			public string GetTransDate()
			{
				return (string)this.responseDataHash[(object)"TransDate"];
			}

			public string GetTransTime()
			{
				return (string)this.responseDataHash[(object)"TransTime"];
			}

			public string GetTicket()
			{
				return (string)this.responseDataHash[(object)"Ticket"];
			}

			public string GetTimedOut()
			{
				return (string)this.responseDataHash[(object)"TimedOut"];
			}

			public string GetAvsResultCode()
			{
				return (string)this.responseDataHash[(object)"AvsResultCode"];
			}

			public string GetCvdResultCode()
			{
				return (string)this.responseDataHash[(object)"CvdResultCode"];
			}

			public string GetRecurUpdateSuccess()
			{
				return (string)this.responseDataHash[(object)"RecurUpdateSuccess"];
			}

			public string GetNextRecurDate()
			{
				return (string)this.responseDataHash[(object)"NextRecurDate"];
			}

			public string GetCorporateCard()
			{
				return (string)this.responseDataHash[(object)"CorporateCard"];
			}

			public string GetRecurEndDate()
			{
				return (string)this.responseDataHash[(object)"RecurEndDate"];
			}

			public string GetDataKey()
			{
				return (string)this.responseDataHash[(object)"DataKey"];
			}

			public string GetResSuccess()
			{
				return (string)this.responseDataHash[(object)"ResSuccess"];
			}

			public string GetPaymentType()
			{
				return (string)this.responseDataHash[(object)"PaymentType"];
			}

			public string GetCavvResultCode()
			{
				return (string)this.responseDataHash[(object)"CavvResultCode"];
			}

			public string GetCardLevelResult()
			{
				return (string)this.responseDataHash[(object)"CardLevelResult"];
			}

			public string GetIsVisaDebit()
			{
				return (string)this.responseDataHash[(object)"IsVisaDebit"];
			}

			public string GetStatusCode()
			{
				return (string)this.responseDataHash[(object)"status_code"];
			}

			public string GetStatusMessage()
			{
				return (string)this.responseDataHash[(object)"status_message"];
			}

			public string GetResDataCustId()
			{
				return (string)this.resDataHash[(object)"cust_id"];
			}

			public string GetResDataPhone()
			{
				return (string)this.resDataHash[(object)"phone"];
			}

			public string GetResDataEmail()
			{
				return (string)this.resDataHash[(object)"email"];
			}

			public string GetResDataNote()
			{
				return (string)this.resDataHash[(object)"note"];
			}

			public string GetResDataPan()
			{
				return (string)this.resDataHash[(object)"pan"];
			}

			public string GetResDataMaskedPan()
			{
				return (string)this.resDataHash[(object)"masked_pan"];
			}

			public string GetResDataExpdate()
			{
				return (string)this.resDataHash[(object)"expdate"];
			}

			public string GetResDataCryptType()
			{
				return (string)this.resDataHash[(object)"crypt_type"];
			}

			public string GetResDataAvsStreetNumber()
			{
				return (string)this.resDataHash[(object)"avs_street_number"];
			}

			public string GetResDataAvsStreetName()
			{
				return (string)this.resDataHash[(object)"avs_street_name"];
			}

			public string GetResDataAvsZipcode()
			{
				return (string)this.resDataHash[(object)"avs_zipcode"];
			}

			public string GetResDataDataKey()
			{
				return (string)this.resDataHash[(object)"data_key"];
			}

			public string[] GetDataKeys()
			{
				string[] strArray = new string[this.dataKeyStack.Count];
				IEnumerator enumerator = this.dataKeyStack.GetEnumerator();
				int num = 0;
				while (enumerator.MoveNext())
					strArray[num++] = (string)enumerator.Current;
				return strArray;
			}

			public string GetExpPaymentType(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"payment_type"];
			}

			public string GetExpCustId(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"cust_id"];
			}

			public string GetExpPhone(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"phone"];
			}

			public string GetExpEmail(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"email"];
			}

			public string GetExpNote(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"note"];
			}

			public string GetExpMaskedPan(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"masked_pan"];
			}

			public string GetExpExpdate(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"expdate"];
			}

			public string GetExpCryptType(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"crypt_type"];
			}

			public string GetExpAvsStreetNumber(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"avs_street_number"];
			}

			public string GetExpAvsStreetName(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"avs_street_name"];
			}

			public string GetExpAvsZipCode(string dataKey)
			{
				return (string)((Hashtable)this.dataKeyHash[(object)dataKey])[(object)"avs_zipcode"];
			}

			public string GetInLineForm()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("<html><head><title>Title for Page</title></head>\n");
				stringBuilder.Append("<SCRIPT LANGUAGE=\"Javascript\">\n");
				stringBuilder.Append("<!--\n");
				stringBuilder.Append("function OnLoadEvent()\n");
				stringBuilder.Append("{\n");
				stringBuilder.Append("document.downloadForm.submit();\n");
				stringBuilder.Append("}\n");
				stringBuilder.Append("-->\n");
				stringBuilder.Append("</SCRIPT>\n");
				stringBuilder.Append("<body onload=\"OnLoadEvent()\">\n");
				stringBuilder.Append("<form name=\"downloadForm\" action=\"" + this.GetMpiACSUrl() + "\" method=\"POST\">\n");
				stringBuilder.Append("<noscript>\n");
				stringBuilder.Append("<br>\n");
				stringBuilder.Append("<br>\n");
				stringBuilder.Append("<center>\n");
				stringBuilder.Append("<h1>Processing your 3-D Secure Transaction</h1>\n");
				stringBuilder.Append("<h2>\n");
				stringBuilder.Append("JavaScript is currently disabled or is not supported\n");
				stringBuilder.Append("by your browser.<br>\n");
				stringBuilder.Append("<h3>Please click on the Submit button to continue\n");
				stringBuilder.Append("the processing of your 3-D secure\n");
				stringBuilder.Append("transaction.</h3>");
				stringBuilder.Append("<input type=\"submit\" value=\"Submit\">\n");
				stringBuilder.Append("</center>\n");
				stringBuilder.Append("</noscript>\n");
				stringBuilder.Append("<input type=\"hidden\" name=\"PaReq\" value=\"" + this.GetMpiPaReq() + "\">\n");
				stringBuilder.Append("<input type=\"hidden\" name=\"MD\" value=\"" + this.GetMpiMD() + "\">\n");
				stringBuilder.Append("<input type=\"hidden\" name=\"TermUrl\" value=\"" + this.GetMpiTermUrl() + "\">\n");
				stringBuilder.Append("</form>\n");
				stringBuilder.Append("</body>\n");
				stringBuilder.Append("</html>\n");
				return ((object)stringBuilder).ToString();
			}

			public string GetMpiSuccess()
			{
				return (string)this.responseDataHash[(object)"MpiSuccess"];
			}

			public string GetMpiMessage()
			{
				return (string)this.responseDataHash[(object)"MpiMessage"];
			}

			public string GetMpiPaReq()
			{
				return (string)this.responseDataHash[(object)"MpiPaReq"];
			}

			public string GetMpiTermUrl()
			{
				return (string)this.responseDataHash[(object)"MpiTermUrl"];
			}

			public string GetMpiMD()
			{
				return (string)this.responseDataHash[(object)"MpiMD"];
			}

			public string GetMpiACSUrl()
			{
				return (string)this.responseDataHash[(object)"MpiACSUrl"];
			}

			public string GetMpiCavv()
			{
				return (string)this.responseDataHash[(object)"MpiCavv"];
			}

			public string GetMpiPAResVerified()
			{
				return (string)this.responseDataHash[(object)"MpiPAResVerified"];
			}
		}
		public class ResAddCC : Transaction
		{
			private static string[] xmlTags = new string[3]
			{
				"pan",
				"expdate",
				"crypt_type"
			};

			private Hashtable keyHashes = new Hashtable();

			static ResAddCC()
			{
			}

			public ResAddCC(Hashtable resaddcc)
				: base(resaddcc, ResAddCC.xmlTags)
			{
			}

			public ResAddCC(string pan, string expdate, string crypt_type)
				: base(ResAddCC.xmlTags)
			{
				this.transactionParams.Add((object)"pan", (object)pan);
				this.transactionParams.Add((object)"expdate", (object)expdate);
				this.transactionParams.Add((object)"crypt_type", (object)crypt_type);
			}

			public void SetAvsAddress(string avs_address)
			{
				this.transactionParams.Add((object)"avs_address", (object)avs_address);
			}

			public void SetAvsZipCode(string avs_zipcode)
			{
				this.transactionParams.Add((object)"avs_zipcode", (object)avs_zipcode);
			}

			public void SetCustId(string cust_id)
			{
				this.keyHashes.Add((object)"cust_id", (object)cust_id);
			}

			public void SetPhone(string phone)
			{
				this.keyHashes.Add((object)"phone", (object)phone);
			}

			public void SetEmail(string email)
			{
				this.keyHashes.Add((object)"email", (object)email);
			}

			public void SetNote(string note)
			{
				this.keyHashes.Add((object)"note", (object)note);
			}

			public override string toXML()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("<res_add_cc>");
				stringBuilder.Append(base.toXML());
				IDictionaryEnumerator enumerator = this.keyHashes.GetEnumerator();
				while (enumerator.MoveNext())
					stringBuilder.Append("<" + enumerator.Key.ToString() + ">" + enumerator.Value.ToString() + "</" + enumerator.Key.ToString() + ">");
				stringBuilder.Append("</res_add_cc>");
				return ((object)stringBuilder).ToString();
			}
		}
		public class Transaction
		{
			protected Hashtable transactionParams = new Hashtable();
			protected string[] xmlFormatTags;

			public Transaction(Hashtable transHash, string[] xmlFormat)
			{
				this.transactionParams = transHash;
				this.xmlFormatTags = xmlFormat;
			}

			public Transaction(string[] xmlFormat)
			{
				this.xmlFormatTags = xmlFormat;
			}

			public Transaction()
			{
			}

			public virtual string toXML()
			{
				StringBuilder sb = new StringBuilder();
				this.toXML_low(sb, this.xmlFormatTags, this.transactionParams);
				return ((object)sb).ToString();
			}

			private void toXML_low(StringBuilder sb, string[] xmlTags, Hashtable xmlData)
			{
				foreach (string str1 in xmlTags)
				{
					string str2 = (string)xmlData[(object)str1];
					sb.Append("<" + str1 + ">" + str2 + "</" + str1 + ">");
				}
			}
		}
	}
}


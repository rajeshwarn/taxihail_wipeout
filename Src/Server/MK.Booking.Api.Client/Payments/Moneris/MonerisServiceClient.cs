using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.Payments.Moneris
{
	public class MonerisServiceClient : BaseServiceClient, IPaymentServiceClient
	{
		private MonerisTokenizeClient MonerisClient { get; set; }

		public MonerisServiceClient(string url, string sessionId, MonerisPaymentSettings monerisSettings, IPackageInfo packageInfo, ILogger logger)
            : base(url, sessionId, packageInfo)
		{
			MonerisClient = new MonerisTokenizeClient(monerisSettings, logger);
		}

		public Task<TokenizedCreditCardResponse> Tokenize (string creditCardNumber, DateTime expiryDate, string cvv)
		{
			return Tokenize(MonerisClient, creditCardNumber, expiryDate);
		}

		public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard (string cardToken)
		{
			return Client.DeleteAsync(new DeleteTokenizedCreditcardRequest
			{
				CardToken = cardToken
			});
		}

		public Task<CommitPreauthorizedPaymentResponse> PreAuthorizeAndCommit (string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
		{
			return Client.PostAsync(new PreAuthorizeAndCommitPaymentRequest
			{
				Amount = Convert.ToDecimal(amount),
				MeterAmount = Convert.ToDecimal(meterAmount),
				TipAmount = Convert.ToDecimal(tipAmount),
				CardToken = cardToken,
				OrderId = orderId
			});
		}

		public Task<PairingResponse> Pair (Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
		{
			throw new NotImplementedException ();
		}

		public Task<BasePaymentResponse> Unpair (Guid orderId)
		{
			throw new NotImplementedException ();
		}

		public Task ResendConfirmationToDriver (Guid orderId)
		{
			return Client.PostAsync<string>("/payment/ResendConfirmationRequest", new ResendPaymentConfirmationRequest {OrderId = orderId});
		}

        public static bool TestClient(MonerisPaymentSettings serverPaymentSettings, string number, DateTime date, ILogger logger)
        {
            var monerisTokenizeClient = new MonerisTokenizeClient(serverPaymentSettings, logger);
            var result = monerisTokenizeClient.Tokenize(number, date.ToString("yyMM"));
            string message;
            return RequestSuccesful(result, out message);
        }

		private static bool RequestSuccesful(MonerisTokenizeClient.Receipt receipt, out string message)
		{
			message = string.Empty;
			if (!bool.Parse(receipt.GetComplete()) || bool.Parse(receipt.GetTimedOut()))
			{
				message = receipt.GetMessage();
				return false;
			}

			if (int.Parse(receipt.GetResponseCode()) >= 50)
			{
				message = receipt.GetMessage();
				return false;
			}

			return true;
		}

		private async Task<TokenizedCreditCardResponse> Tokenize(MonerisTokenizeClient monerisClient, string cardNumber, DateTime expiryDate)
		{
			try
			{
				var response = await monerisClient.TokenizeAsync(cardNumber, expiryDate.ToString("yyMM"));

				var message = string.Empty;
				var success = RequestSuccesful(response, out message);

				if (success)
				{
					return new TokenizedCreditCardResponse
					{
						CardOnFileToken = response.GetDataKey(),
						IsSuccessfull = true,
						Message = "Success",
						CardType = GetCreditCardType(cardNumber),
						LastFour = cardNumber.Substring(cardNumber.Length - 4, 4),
					};
				}

				return new TokenizedCreditCardResponse()
				{
					IsSuccessfull = false,
					Message = message
				};
			}
			catch (Exception e)
			{
				return new TokenizedCreditCardResponse
				{
					IsSuccessfull = false,
					Message = e.Message
				};
			}
		}

		private string GetCreditCardType(string cardNumber)
		{
			string[] VisaElectronFirstNumbers = new[] { "4026", "417500", "4405", "4508", "4844", "4913", "4917" };
			string VisaEx = @"^4[0-9]{12}(?:[0-9]{3})?$";
			string MasterCardEx = @"^5[1-5][0-9]{14}$";
			string AmexEx = @"^3[47][0-9]{13}$";
			string DinersClubEx = @"^3(?:0[0-5]|[68][0-9])[0-9]{11}$";
			string DiscoverEx = @"^6(?:011|5[0-9]{2})[0-9]{12}$";
			string JCBEx = @"^(?:2131|1800|35\d{3})\d{11}$";

			if (Regex.IsMatch(cardNumber, VisaEx))
			{
				return VisaElectronFirstNumbers.Any(cardNumber.StartsWith) 
					? "Visa Electron" 
						: "Visa";
			}

			if (Regex.IsMatch(cardNumber, MasterCardEx))
			{
				return "MasterCard";
			}

			if (Regex.IsMatch(cardNumber, AmexEx))
			{
				return "Amex";
			}

			if (Regex.IsMatch(cardNumber, DinersClubEx))
			{
				return "DinersClub";
			}

			if (Regex.IsMatch(cardNumber, DiscoverEx))
			{
				return "Discover";
			}

			if (Regex.IsMatch(cardNumber, JCBEx))
			{
				return "JCB";
			}

			return string.Empty;
		}
	}
}


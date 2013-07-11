using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class CmtPaymentService : Service
    {
        private CmtPaymentServiceClient Client { get; set; }
        public CmtPaymentService(IConfigurationManager configurationManager)
        {

            Client = new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, true);

        }
        
  
        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
        {
            var response = Client.Delete(new TokenizeDeleteRequest()
            {
                CardToken = request.CardToken
            });

            return new DeleteTokenizedCreditcardResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage
            };
        }

        public PreAuthorizePaymentResponse Post(PreAuthorizePaymentCmtRequest preAuthorizeRequest)
        {
            var request = new AuthorizationRequest()
            {
                Amount = (int)(preAuthorizeRequest.Amount * 100),
                CardOnFileToken = preAuthorizeRequest.CardToken,
                TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = preAuthorizeRequest.OrderNumber
                }
            };
            var response = Client.Post(request);

            return new PreAuthorizePaymentResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage,
                TransactionId = response.TransactionId + "",
            };

        }


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentCmtRequest request)
        {
            var response = Client.Post(new CaptureRequest
            {
                TransactionId = request.TransactionId.ToLong(),
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = request.OrderNumber
                }
            });

            return new CommitPreauthorizedPaymentResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage,
            };
        }

    }
}

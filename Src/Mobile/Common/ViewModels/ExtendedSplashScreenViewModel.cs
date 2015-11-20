using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ExtendedSplashScreenViewModel : PageViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IBookingService _bookingService;


        public ExtendedSplashScreenViewModel(IOrderWorkflowService orderWorkflowService, IBookingService bookingService)
        {
            _orderWorkflowService = orderWorkflowService;
            _bookingService = bookingService;
        }

        public override void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);

            Task.Run(GetCurrentOrderAsync).FireAndForget();
        }


        private async Task GetCurrentOrderAsync()
        {
            var currentOrder = await _orderWorkflowService.GetLastActiveOrder();

            if (currentOrder == null)
            {
                ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = true });

                _bookingService.ClearLastOrder();

                return;
            }

            if (currentOrder.Item1.IsManualRideLinq)
            {
                var orderManualRideLinqDetail = await Task.Run(() => _bookingService.GetTripInfoFromManualRideLinq(currentOrder.Item1.Id));

                ShowViewModelAndRemoveFromHistory<HomeViewModel>(new
                {
                    manualRidelinqDetail = orderManualRideLinqDetail.Data.ToJson(),
                    locateUser = false
                });

                return;
            }

            ShowViewModelAndRemoveFromHistory<HomeViewModel>(new
            {
                order = currentOrder.Item1.ToJson(),
                orderStatusDetail = currentOrder.Item2.ToJson(),
                locateUser = false
            });
        }
    }
}


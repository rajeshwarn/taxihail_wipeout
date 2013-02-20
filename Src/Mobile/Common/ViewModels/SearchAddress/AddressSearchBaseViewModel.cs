//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//
//namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
//{
//    public abstract class AddressSearchBaseViewModel
//    {
//        public string Criteria { get; set; }
//
//        public virtual Task<IEnumerable<AddressViewModel>> OnSearchExecute(CancellationToken cancellationToken)
//        {
//            var task = new Task<IEnumerable<AddressViewModel>>(SearchAddresses, cancellationToken);
//            return task;
//        }
//
//        protected virtual IEnumerable<AddressViewModel> SearchAddresses()
//        {
//            return new List<AddressViewModel>();
//        }
//
//        public virtual bool CriteriaValid { get { return true; } }
//    }
//}
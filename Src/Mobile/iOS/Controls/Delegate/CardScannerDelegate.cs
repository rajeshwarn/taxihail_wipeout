//using System;
//using Card.IO;
//
//namespace CardIO
//{
//	internal class CardScannerDelegate : PaymentViewControllerDelegate
//    {
//        private Action _userDidCancel;
//        private Action<CardIOCreditCardInfo, CardIOPaymentViewController> _cardScanned;
//
//        public CardScannerDelegate (Action userDidCancel, Action<CardIOCreditCardInfo, CardIOPaymentViewController> cardScanned)
//        {
//            _userDidCancel = userDidCancel;
//            _cardScanned = cardScanned ;
//        }
//
//        public override void InPaymentViewController(CardIOCreditCardInfo info, CardIOPaymentViewController _controller)
//        {
//            _cardScanned(info, _controller);
//        }
//
//        public override void UserDidCancelPaymentViewController(CardIOPaymentViewController _controller)
//        {
//            _userDidCancel();
//        }
//    }
//}
//

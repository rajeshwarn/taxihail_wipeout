using apcurium.MK.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using PagedList;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class AccountManagementModel
    {
        public int OrdersPageSize { get { return 10; } }

        public AccountManagementModel()
        {
            OrdersPaged = new PagedList<OrderModel>(new List<OrderModel>(), 1, OrdersPageSize);
        }

        public int OrdersPageIndex { get; set; }

        public Guid RefundOrderId { get; set; }

        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Customer Number")]
        public string CustomerNumber { get; set; }

        [Display(Name = "Creation Date")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Facebook Account")]
        public string FacebookAccount { get; set; }

        [Display(Name = "IBS Account ID")]
        public int? IBSAccountId { get; set; }

        [Display(Name = "Confirmed")]
        public bool IsConfirmed { get; set; }

        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Country Code")]
        public CountryISOCode CountryCode { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Charge Type")]
        public string ChargeType { get; set; }

        [Display(Name = "Default Tip Percent")]
        public int? DefaultTipPercent { get; set; }

        [Display(Name = "PayPal Account Linked")]
        public bool IsPayPalAccountLinked { get; set; }

        [Display(Name = "Credit card last 4 digits")]
        public string CreditCardLast4Digits { get; set; }

        public PagedList<OrderModel> OrdersPaged { get; set; }

        public string NotePopupContent { get; set; }

        public string DisableAccountNotePopupContent { get; set; }

        public string RefundOrderNotePopupContent { get; set; }

        public List<NoteModel> Notes { get; set; }
    }

    public class OrderModel : OrderDetail
    {
        public OrderModel()
        {
        }

        public OrderModel(OrderDetail orderDetail)
        {
            // Initialize properties of base class dynamically
            foreach (var prop in typeof (OrderDetail).GetProperties())
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(orderDetail, null), null);
            }
        }

        public string PromoCode { get; set; }

        public string FareString { get; set; }
        public string TaxString { get; set; }
        public string TollString { get; set; }
        public string TipString { get; set; }
        public string SurchargeString { get; set; }
        public string TotalAmountString { get; set; }

        public bool IsRideLinqCMTPaymentMode { get; set; }
    }

    public class NoteModel : AccountNoteEntry
    {
        public NoteModel()
        {
        }

        public NoteModel(AccountNoteEntry accountNoteEntry)
        {
            // Initialize properties of base class dynamically
            foreach (var prop in typeof(AccountNoteEntry).GetProperties())
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(accountNoteEntry, null), null);
            }
        }

        public string TypeString
        {
            get
            {
                switch (Type)
                {
                    case NoteType.Standard:
                        return "Standard";
                    case NoteType.DeactivateAccount:
                        return "Deactivate account";
                    case NoteType.Refunded:
                        return "Refunded";
                    default:
                        return "";
                };
            }
        }
    }

    /// <summary>
    /// Class that contains combobox elements
    /// </summary>
    public static class CountryCodeSelectList
    {
        public static IList<SelectListItem> CountryCodeElements()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = string.Empty,
                    Text = string.Empty,
                    Selected = false
                }
            };

            list.AddRange(CountryCode.CountryCodes
                .Select(x => new SelectListItem
                {
                    Value = x.CountryISOCode.Code,
                    Text = HttpUtility.HtmlDecode(x.GetTextCountryDialCodeAndCountryName())
                }));

            return list;
        }
    }

    public static class OrderExtensions
    {
        public static double? TotalAmount(this OrderDetail order)
        {
            return order.Fare.GetValueOrDefault()
                + order.Tax.GetValueOrDefault()
                + order.Toll.GetValueOrDefault()
                + order.Tip.GetValueOrDefault()
                + order.Surcharge.GetValueOrDefault();
        }
    }
}
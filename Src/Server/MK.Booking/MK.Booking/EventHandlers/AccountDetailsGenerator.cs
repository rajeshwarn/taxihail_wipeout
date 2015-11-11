#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Common.Helpers;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Collections;
using EntityFramework.BulkInsert.Extensions;
using System.Data.Common;
using System.Data.EntityClient;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountDetailsGenerator :
        IEventHandler<AccountRegistered>,
        IEventHandler<AccountConfirmed>,
        IEventHandler<AccountDisabled>,
        IEventHandler<AccountUpdated>,
        IEventHandler<BookingSettingsUpdated>,
        IEventHandler<AccountPasswordReset>,
        IEventHandler<AccountPasswordUpdated>,
        IEventHandler<RoleAddedToUserAccount>,
        IEventHandler<RoleUpdatedToUserAccount>,
        IEventHandler<CreditCardAddedOrUpdated>,
        IEventHandler<DefaultCreditCardUpdated>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<AccountLinkedToIbs>,
        IEventHandler<AccountUnlinkedFromIbs>,
        IEventHandler<PayPalAccountLinked>,
        IEventHandler<PayPalAccountUnlinked>,
        IEventHandler<OverduePaymentSettled>,
        IEventHandler<ChargeAccountPaymentDisabled>
    {
        private readonly IProjectionStore<AccountDetail> _store;

        public AccountDetailsGenerator(IProjectionStore<AccountDetail> projectionStore)
        {
            _store = projectionStore;
        }

        public void Handle(AccountConfirmed @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.IsConfirmed = true;
                account.DisabledByAdmin = false;
            });
        }

        public void Handle(AccountDisabled @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.IsConfirmed = false;
                account.DisabledByAdmin = true;
            });
        }

        public void Handle(AccountPasswordReset @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Password = @event.Password;
            });
        }

        public void Handle(AccountPasswordUpdated @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Password = @event.Password;
            });
        }

        public void Handle(AccountRegistered @event)
        {
            var account = new AccountDetail
            {
                Name = @event.Name,
                Email = @event.Email,
                Password = @event.Password,
                Id = @event.SourceId,
                IBSAccountId = @event.IbsAcccountId,
                FacebookId = @event.FacebookId,
                TwitterId = @event.TwitterId,
                Language = @event.Language,
                CreationDate = @event.EventDate,
                ConfirmationToken = @event.ConfirmationToken,
                IsConfirmed = @event.AccountActivationDisabled
            };

            if (@event.IsAdmin)
            {
                account.Roles |= (int) Roles.Admin;
            }

            account.Settings = new BookingSettings
            {
                Name = account.Name,
                NumberOfTaxi = 1,
                Passengers = @event.NbPassengers.Value,
                Country = @event.Country,
                Phone = @event.Phone,
                PayBack = @event.PayBack
            };

            _store.Add(account);
        }

        public void Handle(AccountUpdated @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Name = @event.Name;
            });
        }

        public void Handle(BookingSettingsUpdated @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                var settings = account.Settings ?? new BookingSettings();
                settings.Name = @event.Name;

                settings.ChargeTypeId = @event.ChargeTypeId;
                settings.ProviderId = @event.ProviderId;
                settings.VehicleTypeId = @event.VehicleTypeId;

                settings.NumberOfTaxi = @event.NumberOfTaxi;
                settings.Passengers = @event.Passengers;
                settings.Country = @event.Country;
                settings.Phone = @event.Phone;
                settings.AccountNumber = @event.AccountNumber;
                settings.CustomerNumber = @event.CustomerNumber;
                settings.PayBack = @event.PayBack;
                account.Email = @event.Email;
                account.DefaultTipPercent = @event.DefaultTipPercent;

                account.Settings = settings;
            });
        }

        public void Handle(RoleAddedToUserAccount @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Roles |= (int)Enum.Parse(typeof(Roles), @event.RoleName);
            });
        }

        public void Handle(RoleUpdatedToUserAccount @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Roles = (int)Enum.Parse(typeof(Roles), @event.RoleName);
            });
        }

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                if (!account.DefaultCreditCard.HasValue)
                {
                    account.DefaultCreditCard = @event.CreditCardId;
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                }
            });
        }

        public void Handle(DefaultCreditCardUpdated @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                account.DefaultCreditCard = @event.CreditCardId;
            });
        }

        public void Handle(CreditCardRemoved @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                // used for migration, if user removed one card but had another one, we set this one as the default card
                account.DefaultCreditCard = @event.NextDefaultCreditCardId;
                account.Settings.ChargeTypeId = @event.NextDefaultCreditCardId.HasValue ? ChargeTypes.CardOnFile.Id : ChargeTypes.PaymentInCar.Id;
            });
            
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.DefaultCreditCard = null;

                account.Settings.ChargeTypeId = account.IsPayPalAccountLinked
                    ? ChargeTypes.PayPal.Id
                    : ChargeTypes.PaymentInCar.Id;

            });
        }

        public void Handle(CreditCardDeactivated @event)
        {
            if (!@event.IsOutOfAppPaymentDisabled.Value)
            {
                _store.Update(@event.SourceId, account =>
                {
                    account.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                });
            }
    }

        public void Handle(AccountLinkedToIbs @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                if (!@event.CompanyKey.HasValue())
                {
                    account.IBSAccountId = @event.IbsAccountId;
                }
            });
        }

        public void Handle(AccountUnlinkedFromIbs @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.IBSAccountId = null;
            });
        }

        public void Handle(PayPalAccountLinked @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.IsPayPalAccountLinked = true;
                account.Settings.ChargeTypeId = ChargeTypes.PayPal.Id;
            });
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            _store.Update(@event.SourceId, account =>
            {
                account.IsPayPalAccountLinked = false;
            });
        }

        public void Handle(ChargeAccountPaymentDisabled @event)
        {
            _store.Update(HasChargeAccount, account =>
            {
                account.Settings.CustomerNumber = null;
                account.Settings.AccountNumber = null;
            });
        }

        private bool HasChargeAccount(AccountDetail accountDetail)
        {
            return accountDetail.Settings.AccountNumber.HasValue() ||
                   accountDetail.Settings.CustomerNumber.HasValue();
        }

        public void Handle(OverduePaymentSettled @event)
        {
            if (@event.IsPayInTaxiEnabled.Value)
            {
                _store.Update(@event.SourceId, account =>
                {
                    //Re-enable card on file as the default payment method
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                });
            }
        }

        
    }

    public interface IProjectionStore: IProjectionStore<object>
    {

    }

    public interface IProjectionStore<TProjection>: IProjectionStore<TProjection, Guid> where TProjection : class
    {

    }

    public interface IProjectionStore<TProjection, TIdentifier>: IEnumerable<TProjection> where TProjection :class
    {
        void Update(TIdentifier identifier, Action<TProjection> action);
        void Update(Func<TProjection, bool> predicate, Action<TProjection> action);
        void Add(TProjection projection);
        void AddRange(IEnumerable<TProjection> projections);
    }

    public class MemoryProjectionStore<TProjection> : IProjectionStore<TProjection> where TProjection : class
    {
        readonly IDictionary<Guid, TProjection> _cache = new Dictionary<Guid, TProjection>();
        readonly Func<TProjection, Guid> _getId;
        public MemoryProjectionStore(Func<TProjection, Guid> getId)
        {
            _getId = getId;
        }

        public void Add(TProjection projection)
        {
            _cache.Add(_getId(projection), projection);
        }

        public void AddRange(IEnumerable<TProjection> projections)
        {
            foreach(var projection in projections)
            {
                Add(projection);
            }
        }

        public void Update(Guid identifier, Action<TProjection> action)
        {
            TProjection item;
            if(!_cache.TryGetValue(identifier, out item))
            {
                throw new InvalidOperationException("Projection not found");
            }
            action.Invoke(item);
        }

        public void Update(Func<TProjection, bool> predicate, Action<TProjection> action)
        {

            foreach (var item in _cache
                .Select(x => x.Value)
                .Where(predicate))
            {
                action(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator<TProjection> IEnumerable<TProjection>.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }
    }

    public class EntityProjectionStore<TProjection> : IProjectionStore<TProjection> where TProjection : class
    {
        private readonly BookingDbContext _context;
        private readonly Func<BookingDbContext> _contextFactory;
        public EntityProjectionStore(Func<BookingDbContext> contextFactory)
        {
            _context = contextFactory.Invoke();
            _contextFactory = contextFactory;
        }

        public void Add(TProjection projection)
        {
            _context.Set<TProjection>().Add(projection);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<TProjection> projections)
        {
            int count = 0;
            var context = _contextFactory.Invoke();
            context.BulkInsert(projections, new BulkInsertOptions
            {
                EnableStreaming = true,
            });
            ////context.Configuration.AutoDetectChangesEnabled = false;
            //context.Configuration.ValidateOnSaveEnabled = false;
            //foreach(var p in projections)
            //{
            //    count++;
            //    context.Set<TProjection>().Add(p);

            //    if((count % 100) == 0)
            //    {
            //        context.SaveChanges();
            //        context.Dispose();
            //        context = _contextFactory.Invoke();
            //        context.Configuration.AutoDetectChangesEnabled = false;
            //        context.Configuration.ValidateOnSaveEnabled = false;
            //    }
            //}
            //context.SaveChanges();
            context.Dispose();
        }

        public void Update(Guid identifier, Action<TProjection> action)
        {
            var projection = _context.Set<TProjection>().Find(identifier);
            if(projection == null)
            {
                throw new InvalidOperationException("Projection not found");
            }
            action.Invoke(projection);
            _context.SaveChanges();
        }

        public void Update(Func<TProjection, bool> predicate, Action<TProjection> action)
        {
            var projections = _context.Set<TProjection>().Where(predicate);
            foreach(var projection in projections)
            {
                action.Invoke(projection);

            }
            _context.SaveChanges();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_context.Set<TProjection>()).GetEnumerator();
        }

        IEnumerator<TProjection> IEnumerable<TProjection>.GetEnumerator()
        {
            return ((IEnumerable<TProjection>)_context.Set<TProjection>()).GetEnumerator();
        }

    }
}
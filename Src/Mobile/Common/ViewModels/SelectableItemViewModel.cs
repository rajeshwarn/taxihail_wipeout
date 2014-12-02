using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class SelectableItemViewModel<TItem> : BaseViewModel
    {
        public SelectableItemViewModel(TItem item, ICommand selectedCommand)
            :this(item, selectedCommand, null)
        {
        }

        public SelectableItemViewModel(TItem item, ICommand selectedCommand, ICommand optionalCommand)
        {
            _item = item;
            _selectedCommand = selectedCommand;
            _optionalCommand = optionalCommand;
        }

        TItem _item;
        public TItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                RaisePropertyChanged();
            }
        }

        readonly ICommand _selectedCommand;
        public ICommand SelectedCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
                    _selectedCommand.ExecuteIfPossible(Item);
                });
            }
        }

        readonly ICommand _optionalCommand;
        public ICommand OptionalCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
                    _optionalCommand.ExecuteIfPossible(Item);
                });
            }
        }
    }
}

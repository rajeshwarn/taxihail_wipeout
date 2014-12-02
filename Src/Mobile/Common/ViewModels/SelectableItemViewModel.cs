using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class SelectableItemViewModel<TItem> : BaseViewModel
    {
        public SelectableItemViewModel(TItem item, ICommand selectedCommand)
            :this(item, null, selectedCommand)
        {
        }

        public SelectableItemViewModel(TItem item, ICommand deleteCommand, ICommand selectedCommand)
        {
            _item = item;
            _deleteCommand = deleteCommand;
            _selectedCommand = selectedCommand;
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

        readonly ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
                    _deleteCommand.Execute(_item);
                });
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
    public class SpinnerSectionItem : SectionItem
    {
        private readonly Func<int> _getValue;
        private readonly Func<List<ListItemData>> _getValues;
        private readonly Action<int> _setValue;

        public SpinnerSectionItem(string label, Func<int> getValue, Action<int> setValue,
            Func<List<ListItemData>> getValues)
        {
            Label = label;
            _getValue = getValue;
            _setValue = setValue;
            _getValues = getValues;
        }

        public Func<int> GetValue
        {
            get { return _getValue; }
        }

        public Action<int> SetValue
        {
            get { return _setValue; }
        }

        public Func<List<ListItemData>> GetValues
        {
            get { return _getValues; }
        }
    }
}
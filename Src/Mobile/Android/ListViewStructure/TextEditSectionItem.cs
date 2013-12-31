using System;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
    public class TextEditSectionItem : SectionItem
    {
        private readonly Func<string> _getValue;
        private readonly Action<string> _setValue;

        public TextEditSectionItem(string label, Func<string> getValue, Action<string> setValue)
        {
            Label = label;
            _getValue = getValue;
            _setValue = setValue;
        }

        public TextEditSectionItem(string label, Func<int> getValue, Action<int> setValue)
        {
            Label = label;
            _getValue = () => getValue().ToString(CultureInfo.InvariantCulture);
            _setValue = x =>
            {
                int value;
                int.TryParse(x, out value);
                setValue(value);
            };
        }

        public Func<string> GetValue
        {
            get { return _getValue; }
        }

        public Action<string> SetValue
        {
            get { return _setValue; }
        }
    }
}
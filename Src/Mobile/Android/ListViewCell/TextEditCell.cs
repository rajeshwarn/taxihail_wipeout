using Android.Content;
using Android.Text;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
{
    internal class TextEditCell : LabelValueCell<TextEditSectionItem>
    {
        public TextEditCell(TextEditSectionItem item, ViewGroup parent, Context context) : base(item, parent, context)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            Value.TextChanged -= HandleValueTextChanged;
            Value.TextChanged += HandleValueTextChanged;
        }


        public override void LoadData()
        {
            base.LoadData();
            Label.Text = SectionItem.Label;
            Value.Text = SectionItem.GetValue();
        }

        private void HandleValueTextChanged(object sender, TextChangedEventArgs e)
        {
            SectionItem.SetValue(e.Text.ToString());
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;


namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
{
    internal class SpinnerCell : LabelValueCell<SpinnerSectionItem>
    {
        private List<ListItemData> _listItemData;

        public SpinnerCell(SpinnerSectionItem item, ViewGroup parent, Context context) : base(item, parent, context)
        {
        }

        private Spinner SpinnerCtl { get; set; }

        public override void Initialize()
        {
            var inflater = (LayoutInflater) OwnerContext.GetSystemService(Context.LayoutInflaterService);
            CellView = inflater.Inflate(Resource.Layout.SpinnerCell, Parent, false);
            Label = (TextView) CellView.FindViewById(Resource.Id.label);
            SpinnerCtl = (Spinner) CellView.FindViewById(Resource.Id.spinner);
            SpinnerCtl.ItemSelected -= HandleSpinnerCtlItemSelected;
            SpinnerCtl.ItemSelected += HandleSpinnerCtlItemSelected;
        }

        private void HandleSpinnerCtlItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            SectionItem.SetValue(_listItemData.ElementAt(e.Position).Key);
        }

        public override void LoadData()
        {
            _listItemData = SectionItem.GetValues();
            Label.Text = SectionItem.Label;
            SpinnerCtl.Prompt = OwnerContext.GetString(Resource.String.ListPromptSelectOne);
            var arrayAdapter = new ArrayAdapter<ListItemData>(OwnerContext, Resource.Layout.SpinnerText, _listItemData);
            arrayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinnerCtl.Adapter = arrayAdapter;
            SpinnerCtl.SetSelection(_listItemData.FindIndex(i => i.Key == SectionItem.GetValue()));
        }
    }
}
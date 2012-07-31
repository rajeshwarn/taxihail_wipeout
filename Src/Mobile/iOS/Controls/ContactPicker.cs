using System;
using MonoTouch.AddressBookUI;
using MonoTouch.UIKit;
using MonoTouch.AddressBook;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class ContactPicker
	{
		private ABPeoplePickerNavigationController _picker;
		private UIViewController _parent;
		private List<ABPersonProperty> _properties;

		public event EventHandler<ContactPickerResult> ContactSelected = delegate{};

		public ContactPicker ( UIViewController parent )
		{
			_properties = new List<ABPersonProperty>();
			_parent = parent;
			_picker = new ABPeoplePickerNavigationController();
			_picker.DisplayedProperties.Clear();
			_picker.SelectPerson += HandleSelectPerson;

			_picker.Cancelled += delegate {
				_picker.DismissModalViewControllerAnimated(true);
			};
		}

		public void AddProperty( ABPersonProperty property )
		{
			_picker.DisplayedProperties.Add( property );
			_properties.Add( property );
		}


     


		void HandleSelectPerson (object sender, ABPeoplePickerSelectPersonEventArgs e)
		{
			ABPerson selectedPerson = e.Person;		

			var viewer = new ABPersonViewController();

			viewer.DisplayedPerson = selectedPerson;
			_properties.ForEach( p => viewer.DisplayedProperties.Add(p) );
			
			viewer.PerformDefaultAction += delegate(object senderViewer, ABPersonViewPerformDefaultActionEventArgs argsViewer) {	
				if(argsViewer.Identifier.HasValue)
				{
					var values = argsViewer.Person.GetProperty(argsViewer.Property);
					switch( argsViewer.Property )
					{
					case ABPersonProperty.Address:
						var value = ((ABMultiValue<NSDictionary>)values)[argsViewer.Identifier.Value].Value;

						List<string> list = new List<string>();
						if( value.ContainsKey( NSObject.FromObject( "Street" ) ) )
						{
                                var fullStreet =  value.ValueForKey( new NSString("Street") ).ToString();
                                if ( fullStreet.Contains( "\n" ) )
                                {
                                    list.Add( fullStreet.Split( '\n' )[0]);
                                }
                                else
                                {
                                    list.Add( fullStreet);
                                }
							
						}

						if( value.ContainsKey( NSObject.FromObject( "City" ) ) )
						{
							list.Add( value.ValueForKey( new NSString("City") ).ToString() );
						}
						



						ContactSelected(this, new ContactPickerResult( string.Join(", ", list ), string.Join(" ", argsViewer.Person.FirstName, argsViewer.Person.LastName ) ) );
						break;
					}
				}
									
				_picker.DismissModalViewControllerAnimated(true);
			};
			_picker.PushViewController( viewer, true);			
		}

		public void Show()
		{
			_parent.PresentModalViewController( _picker, true );
		}

	}

	public class ContactPickerResult : EventArgs
	{
		public ContactPickerResult( string value, string name )
		{
			Value = value;
			Name = name;
		}

		public string Value { get; set; }

		public string Name { get; set; }
	}
}


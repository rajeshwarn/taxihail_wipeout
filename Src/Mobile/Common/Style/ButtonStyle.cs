using System;

namespace apcurium.MK.Booking.Mobile.Style
{
    public class ButtonStyle
    {
        public ButtonStyle()
        {
        }

        public string Key {get;set;}

        public ColorDefinition[] Colors{get;set;}

        public ColorDefinition StrokeColor{get;set;}

		public float StrokeLineWidth{get; set;}

        public ColorDefinition TextShadowColor{get;set;}
        
		public ColorDefinition TextColor{get;set;}

        public ShadowDefinition InnerShadow{get;set;}

        public ShadowDefinition DropShadow{get;set;}

    }
}


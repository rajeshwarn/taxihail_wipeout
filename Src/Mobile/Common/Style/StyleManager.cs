using System;
using System.IO;
using ServiceStack.Text;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Style
{
    public class StyleManager
    {

        private static StyleManager _current;

        public static StyleManager Current
        {
            get
            {
                if ( _current == null )
                {
                    _current = LoadStyle();
                }
                return _current;
            }

        }

        private static StyleManager LoadStyle()
        {
            StyleManager result = null;
            string resourceName = "";
            
            foreach ( string name in typeof(StyleManager).Assembly.GetManifestResourceNames() ) 
            { 
                if ( name.ToLower().EndsWith( ".style.json") )
                {
                    resourceName = name;
                    break;
                }
            }
            
            
            using (var stream = typeof(StyleManager).Assembly.GetManifestResourceStream( resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    
                    string serializedData = reader.ReadToEnd();
                    result = JsonSerializer.DeserializeFromString<StyleManager>(serializedData);
                }
            }

            return result;

        }

            
        public ButtonStyle[] Buttons {
            get;
            set;
        }

     
        public ColorDefinition LightCorporateTextColor {
            get;
            set;
        }        

        public ColorDefinition NavigationTitleColor
        {
            get;
            set;
        }
          
        public ColorDefinition NavigationBarColor
        {
            get;
            set;
        }

       
    }
}


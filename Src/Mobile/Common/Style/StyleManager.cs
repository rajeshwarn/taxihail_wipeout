using System.IO;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;

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
            var resourceName = "";
            
            foreach ( var name in typeof(StyleManager).Assembly.GetManifestResourceNames() ) 
            { 
                if ( name.ToLower().EndsWith( ".style.json") )
                {
                    resourceName = name;
                    break;
                }
            }
            
            
            using (var stream = typeof(StyleManager).Assembly.GetManifestResourceStream( resourceName))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                    {
                    
                        var serializedData = reader.ReadToEnd();

                        result = serializedData.FromJson<StyleManager>();
                    }
            }

            return result;

        }


        public bool UseCustomFonts{get;set;}

        public string CustomRegularFont{get;set;}
        public string CustomBoldFont{get;set;}
        public string CustomMediumFont{get;set;}
        public string CustomItalicFont{get;set;}
			


        public ButtonStyle[] Buttons { get; set; }

        public float? ButtonCornerRadius { get; set; }

        public float? TextboxCornerRadius { get; set; }

        public float? ButtonFontSize { get; set; }
     
        public bool? CenterLogo { get; set; }

        public ColorDefinition LightCorporateTextColor { get; set; }        

        public ColorDefinition NavigationTitleColor { get; set; }
          
        public ColorDefinition NavigationBarColor { get; set; }
    }
}


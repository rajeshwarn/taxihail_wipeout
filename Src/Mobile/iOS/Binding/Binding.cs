using System;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{

    public class B
    {
        string _binding;
        public B ()
        {
            _binding = "";
        }


        public B (string targetProperty,string targetViewModelProperty, Type converter, string converterParameter=null)
        {
            _binding = "";
            Add (targetProperty,targetViewModelProperty,converter,converterParameter);
        }
        
        public B (string targetProperty,string targetViewModelProperty,string converterName =null, string converterParameter=null)
        {
            _binding = "";
            Add (targetProperty,targetViewModelProperty,converterName,converterParameter);
        }

        public B (string targetProperty,string targetViewModelProperty, Mode mode)
        {
            _binding = "";
            Add (targetProperty,targetViewModelProperty, mode: mode);
        }

        public B Add(string targetProperty,string targetViewModelProperty,Type converter, string converterParameter=null)
        {
            return Add (targetProperty, targetViewModelProperty, converter.Name, converterParameter);
        }

        public B Add(string targetProperty,string targetViewModelProperty,string converterName =null, string converterParameter=null, Mode mode= Mode.Default)
        {
            var binding = "'" + targetProperty + "':{'Path':'" + targetViewModelProperty + "'";

            if (mode != Mode.Default) {

                binding +=",'Mode':'"+mode.ToString()+"'";


            }

            if (!string.IsNullOrWhiteSpace (converterName)) {
                binding += ", 'Converter':'"+converterName+"'";
                if(!string.IsNullOrWhiteSpace(converterParameter))
                {
                    binding += ", 'ConverterParameter':'"+converterParameter+"'";
                }
            }
            binding += "}";
            
            if (!string.IsNullOrEmpty (_binding)) {
                _binding+=",\n";
            }
            _binding += binding;
            return this;
        }
        
        public override string ToString ()
        {
            return "{" + _binding + "}";
        }



        public static implicit operator string(B m) 
        {
            return m.ToString ();
        }

        public enum Mode 
        {
            Default,
            OneWay,
            TwoWay
        }

    }

}


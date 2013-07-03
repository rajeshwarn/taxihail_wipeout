========== README ==============

If to regenerate the Web Service class, you need to modify the generated code:


http://stackoverflow.com/questions/10172197/paypal-setexpresscheckout-soap/14713368#14713368

Look in the generated code for the web service (Reference.cs) and find the AbstractResponseType. The last property is Any(). Change the attribute to match this (to ignore the property):

    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public System.Xml.XmlElement Any {
        get {
            return this.anyField;
        }
        set {
            this.anyField = value;
        }
    }
Following this, recompile and test again and you should now receive the Token property set correctly.

If you regenerate the web service code, this change will of course get replaced and you will have to re-do it unless PayPal fixes this. BTW, my WSDL Version number is 98.0 at this time.

Gary Davis

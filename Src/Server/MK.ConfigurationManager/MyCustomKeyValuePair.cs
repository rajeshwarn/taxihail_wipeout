namespace MK.ConfigurationManager
{
    public class MyCustomKeyValuePair
    {
        public MyCustomKeyValuePair()
        {
            
        }

        public MyCustomKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
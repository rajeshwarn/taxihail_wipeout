using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CustomerPortal.Web.Entities;
using MongoRepository;

namespace CompaniesData
{
    class Program
    {
        static MongoRepository<Company> repository = new MongoRepository<Company>();
        static void Main(string[] args)
        {

            
            //repository.DeleteAll();

            var dtCompanies = GetData("Select * from MKConfig.Company", "Company");
            var dtIBS = GetData("Select * from MKConfig.IBSServer", "IBS");
            foreach (DataRow dr in dtCompanies.Rows)
            {
                if (dr["Name"].ToString() != String.Empty)
                {
                    //var company = new Company();
                    //foreach (DataRow drIBS in dtIBS.Rows)
                    //{
                    //    if (dr[1].ToString() == drIBS[1].ToString())
                    //    {
                    //        company.IBS.ServiceUrl = drIBS["Url"].ToString();
                    //        company.IBS.Username = drIBS["Username"].ToString();
                    //        company.IBS.Password = drIBS["Password"].ToString();
                    //    }
                    //}
                    
                    //company.Id = dr["Id"].ToString();
                    //company.CompanyName = dr["Name"].ToString();

                    var settings = FromXml(dr["DictionaryAsXml"].ToString());
                    var mobileSettings = FromXml(dr["MobileDictionaryAsXml"].ToString());
                    //foreach (KeyValuePair<string, string> setting in settings)
                    //{
                    //    company.Settings.Add(setting.Key, setting.Value);
                    //}
                    //foreach (KeyValuePair<string, string> setting in mobileSettings)
                    //{

                    //    if (!company.Settings.ContainsKey(setting.Key))
                    //        company.Settings.Add(setting.Key, setting.Value);
                    //}
                    //company.CompanyKey = settings["TaxiHail.ServerCompanyName"];
                    //repository.Add(company);

                    var key = settings["TaxiHail.ServerCompanyName"];

                    var company = repository.FirstOrDefault(c => c.CompanyKey == key);

                    var update = new string[]
                    {
                        "DefaultPhoneNumber", "DefaultPhoneNumberDisplay", "AndroidSigningKeyAlias",
                        "AndroidSigningKeyPassStorePass",  "GoogleMapKey", "Package", "SupportEmail"
                    };


                    if (company != null)
                    {
                        foreach (var setting in settings)
                        {
                            if (setting.Key == "ApplicationName")
                            {                                
                                AddOrUpdateSetting(company, setting, true);
                            }
                            else if (setting.Key == "SiteUrl")
                            {                                
                                AddOrUpdateSetting(company, new  KeyValuePair<string, string>( "Client.AboutUsUrl", setting.Value), true);
                            }
                            else if (setting.Key == "SupportEmail")
                            {
                                AddOrUpdateSetting(company, new KeyValuePair<string, string>("Client.SupportEmail", setting.Value), true);
                            }
                            else if ( update.Contains( setting.Key ) )
                            {
                                AddOrUpdateSetting(company, setting, false);
                            }
                        }
                    }

                }
            }
        }

        private static void AddOrUpdateSetting(Company company, KeyValuePair<string, string> setting, bool client)
        {
            if (company.CompanySettings.All(s => setting.Key != s.Key))
            {
                company.CompanySettings.Add(new CompanySetting
                {
                    Key = setting.Key,
                    IsClientSetting = client,
                    Value = setting.Value
                });
            }
            else
            {
                company.CompanySettings.First(s => setting.Key == s.Key).Value = setting.Value;
                company.CompanySettings.First(s => setting.Key == s.Key).IsClientSetting = client;
            }
        }

        private static Dictionary<string, string> FromXml(string value)
        {
            if (value == null) return new Dictionary<string, string>();
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof(Dictionary<string, string>));
                return (Dictionary<string, string>)deserializer.ReadObject(stream);
            }
        }

        public static DataTable GetData(string sql, string tableName)
        {
            SqlConnection conn = new SqlConnection("Server=.;Database=MKConfigurationManager;Trusted_Connection=True;");
            DataSet dsTaxihail = new DataSet();
            DataTable dtTaxihail = new DataTable();
            SqlDataAdapter daTaxihail = new SqlDataAdapter(sql, conn);
            daTaxihail.Fill(dsTaxihail, tableName);
            dtTaxihail = dsTaxihail.Tables[tableName];
            return dtTaxihail;
        }
    }
}

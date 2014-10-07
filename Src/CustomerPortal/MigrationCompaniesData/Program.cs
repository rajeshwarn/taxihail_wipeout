#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CompaniesData
{
    internal class Program
    {
        private static readonly MongoRepository<Company> repository = new MongoRepository<Company>();

        private static void Main(string[] args)
        {
            repository.DeleteAll();

            var dtCompanies = GetData("Select * from MKConfig.Company", "Company");
            var dtIBS = GetData("Select * from MKConfig.IBSServer", "IBS");
            foreach (DataRow dr in dtCompanies.Rows)
            {
                if (dr["Name"].ToString() != String.Empty)
                {
                    var company = new Company();
                    foreach (DataRow drIBS in dtIBS.Rows)
                    {
                        if (dr[1].ToString() == drIBS[1].ToString())
                        {
                            company.IBS.ServiceUrl = drIBS["Url"].ToString();
                            company.IBS.Username = drIBS["Username"].ToString();
                            company.IBS.Password = drIBS["Password"].ToString();
                        }
                    }

                    company.Id = dr["Id"].ToString();
                    company.CompanyName = dr["Name"].ToString();

                    var settings = FromXml(dr["DictionaryAsXml"].ToString());
                    var mobileSettings = FromXml(dr["MobileDictionaryAsXml"].ToString());
//                    foreach (KeyValuePair<string, string> setting in settings)
//                    {
//                        company.Settings.Add(setting.Key, setting.Value);
//                    }
//                    foreach (KeyValuePair<string, string> setting in mobileSettings)
//                    {
//                        if (!company.Settings.ContainsKey(setting.Key))
//                            company.Settings.Add(setting.Key, setting.Value);
//                    }
                    company.CompanyKey = settings["TaxiHail.ServerCompanyName"];
                    repository.Add(company);
                }
            }
        }

        private static Dictionary<string, string> FromXml(string value)
        {
            if (value == null) return new Dictionary<string, string>();
            using (Stream stream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(value);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof (Dictionary<string, string>));
                return (Dictionary<string, string>) deserializer.ReadObject(stream);
            }
        }

        public static DataTable GetData(string sql, string tableName)
        {
            var conn = new SqlConnection("Server=taxihail01;Database=MKConfigurationManager;Trusted_Connection=True;");
            var dsTaxihail = new DataSet();
            var dtTaxihail = new DataTable();
            var daTaxihail = new SqlDataAdapter(sql, conn);
            daTaxihail.Fill(dsTaxihail, tableName);
            dtTaxihail = dsTaxihail.Tables[tableName];
            return dtTaxihail;
        }
    }
}
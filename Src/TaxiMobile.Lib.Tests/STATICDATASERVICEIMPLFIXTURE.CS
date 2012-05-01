using System.Linq;
using NUnit.Framework;
using TaxiMobile.Lib.Services.Impl;


namespace TaxiMobile.Lib.Tests
{
    [TestFixture]
    public class StaticDataServiceImplFixture : BaseTest
    {
         [TestFixtureSetUp]
        public void Setup()
        {
            base.Setup();
        }

         [Test]
         public void GetVehiclesListTest()
         {
             var service = new StaticDataServiceImpl();
             var liste = service.GetVehiclesList();
             Assert.IsTrue(liste.Any());
         }

         [Test]
         public void GetPaymentsListTest()
         {
             var service = new StaticDataServiceImpl();
             var liste = service.GetPaymentsList();
             Assert.IsTrue(liste.Any());
         }

         [Test]
         public void GetCompaniesListTest()
         {
             var service = new StaticDataServiceImpl();
             var liste = service.GetCompaniesList();
             Assert.IsTrue(liste.Any());
         }
    }
}
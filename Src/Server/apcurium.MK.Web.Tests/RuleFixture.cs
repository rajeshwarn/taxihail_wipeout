using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Web.Tests
{

    [TestFixture]
    public class RuleFixture : BaseTest
    {
        private Guid _knownRuleId;

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount();



            var sut = new RulesServiceClient(BaseUrl, SessionId);
            DeleteAllRules(sut);
            sut.CreateRule(new Common.Entity.Rule
            {
                Id = (_knownRuleId = Guid.NewGuid()),
                Type = RuleType.Recurring,
                Category = RuleCategory.DisableRule,
                DaysOfTheWeek = DayOfTheWeek.Sunday,
                StartTime = DateTime.MinValue.AddHours(2),
                EndTime = DateTime.MinValue.AddHours(3),
                ActiveFrom = DateTime.Now.AddDays(1),
                ActiveTo = DateTime.Now.AddDays(2),
                IsActive = true,
                Name = "Rate " + Guid.NewGuid().ToString(),
                Message = "Due to..."
            });
        }

        private static void CreateDefaultRules(RulesServiceClient sut )
        {

            if (sut.GetRules().None(r => (r.Category == RuleCategory.WarningRule) && (r.Type == RuleType.Default)))
            {
                // Default rate does not exist for this company 
                sut.CreateRule( new Rule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.WarningRule,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    Id = Guid.NewGuid(),
                    IsActive = false,
                    Message = "Due to the current volume of calls, please note that pickup may be delayed.",
                });
            }

            if (sut.GetRules().None(r => (r.Category == (int)RuleCategory.DisableRule) && (r.Type == (int)RuleType.Default)))
            {
                // Default rate does not exist for this company 
                sut.CreateRule(new Rule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    Id = Guid.NewGuid(),
                    Message = "Service is temporarily unavailable. Please call dispatch center for service.",
                });
            }

        }


        private void DeleteAllRules(RulesServiceClient sut)
        {
            var rules = sut.GetRules().Where(r => r.Type != RuleType.Default);

            foreach (var rule in rules)
            {
                sut.DeleteRule(rule.Id);
            }
        }

        [Test]
        public void AddRule()
        {
            var ruleId = Guid.NewGuid();
            var activeFromDateRef = DateTime.Today.AddDays(14);
            var name = "AddRuleTest" + Guid.NewGuid().ToString();

            var sut = new RulesServiceClient(BaseUrl, SessionId);
            sut.CreateRule(new Common.Entity.Rule
            {
                Id = ruleId,
                Name = name,
                Type = RuleType.Recurring,
                Category = RuleCategory.WarningRule,
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = true,
                ActiveFrom = activeFromDateRef,
                ActiveTo = activeFromDateRef.AddMonths(2),
                DaysOfTheWeek = DayOfTheWeek.Saturday | DayOfTheWeek.Sunday,
                StartTime = new DateTime(2000, 1, 1, 22, 11, 10),
                EndTime = new DateTime(2000, 1, 1, 23, 22, 11),
                Priority = 10,
                IsActive = true,
                Message = "DisableRuleMessage"

            });

            var rules = sut.GetRules();

            Assert.AreEqual(1, rules.Count(x => x.Id == ruleId));
            var rule = rules.Single(x => x.Id == ruleId);
            Assert.AreEqual(name, rule.Name);
            Assert.AreEqual(RuleCategory.WarningRule, rule.Category);
            Assert.AreEqual(RuleType.Recurring, rule.Type);
            Assert.AreEqual(true, rule.AppliesToFutureBooking);
            Assert.AreEqual(true, rule.AppliesToCurrentBooking);
            Assert.AreEqual(true, rule.IsActive);

            Assert.AreEqual(activeFromDateRef, rule.ActiveFrom);
            Assert.AreEqual(activeFromDateRef.AddMonths(2), rule.ActiveTo);

            Assert.AreEqual(DayOfTheWeek.Saturday | DayOfTheWeek.Sunday, rule.DaysOfTheWeek);
            Assert.AreEqual(new DateTime(2000, 1, 1, 22, 11, 10), rule.StartTime);
            Assert.AreEqual(new DateTime(2000, 1, 1, 23, 22, 11), rule.EndTime);

            Assert.AreEqual(10, rule.Priority);

            Assert.AreEqual("DisableRuleMessage", rule.Message);

        }

        [Test]
        public void UpdateRule()
        {
            var sut = new RulesServiceClient(BaseUrl, SessionId);
            var newMessage = "UpdateRuleTest" + Guid.NewGuid().ToString();
            var newName = "UpdateRuleTest" + Guid.NewGuid().ToString();
            var activeFromDateRef = DateTime.Today.AddDays(20);

            var rule = sut.GetRules().Single(r => r.Id == _knownRuleId);

            rule.Message = newMessage;
            rule.Name = newName;
            rule.Priority = 99;
            

            rule.AppliesToCurrentBooking = false;
            rule.AppliesToFutureBooking = true;
            rule.ActiveFrom = activeFromDateRef;
            rule.ActiveTo = activeFromDateRef.AddMonths(4);
            rule.DaysOfTheWeek = DayOfTheWeek.Monday | DayOfTheWeek.Wednesday;
            rule.StartTime = new DateTime(2000, 1, 1, 20, 18, 8);
            rule.EndTime = new DateTime(2000, 1, 1, 21, 19, 9);
            rule.IsActive = false;


            sut.UpdateRule(rule);

            rule = sut.GetRules().Single(r => r.Id == _knownRuleId);
            Assert.AreEqual(newMessage, rule.Message);
            Assert.AreEqual(newName, rule.Name);
            Assert.AreEqual(true, rule.AppliesToFutureBooking);
            Assert.AreEqual(false, rule.AppliesToCurrentBooking);
            Assert.AreEqual(false, rule.IsActive);
            Assert.AreEqual(activeFromDateRef, rule.ActiveFrom);
            Assert.AreEqual(activeFromDateRef.AddMonths(4), rule.ActiveTo);
            Assert.AreEqual( DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, rule.DaysOfTheWeek);
            Assert.AreEqual(new DateTime(2000, 1, 1, 20, 18, 8), rule.StartTime);
            Assert.AreEqual(new DateTime(2000, 1, 1, 21, 19, 9), rule.EndTime);
            Assert.AreEqual(99, rule.Priority);


        }


        [Test]
        public void ActivateDeactivate()
        {




            var sut = new RulesServiceClient(BaseUrl, SessionId);
            sut.ActivateRule(_knownRuleId);

            var rule = sut.GetRules().Single(x => x.Id == _knownRuleId);
            Assert.IsTrue(rule.IsActive);

            sut.DeactivateRule(_knownRuleId);
            rule = sut.GetRules().Single(x => x.Id == _knownRuleId);
            Assert.IsFalse(rule.IsActive);

            sut.ActivateRule(_knownRuleId);
            rule = sut.GetRules().Single(x => x.Id == _knownRuleId);
            Assert.IsTrue(rule.IsActive);


        }

        [Test]
        public void TestWarningRuleIsApplied()
        {
            var rules = new RulesServiceClient(BaseUrl, SessionId);
            var rule = rules.GetRules().Single(r => r.Category == RuleCategory.WarningRule && r.Type == RuleType.Default);
            rule.AppliesToCurrentBooking = true;
            rule.AppliesToFutureBooking= true;
            rule.IsActive = true;
            rules.UpdateRule(rule);

             
            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };
            
            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1 , ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var validation = sut.ValidateOrder(order);

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule.Message, validation.Message);


        }



        [Test]
        public void TestWarningRuleIsNotApplied()
        {
            var rules = new RulesServiceClient(BaseUrl, SessionId);
            var rule = rules.GetRules().Single(r => r.Category == RuleCategory.WarningRule && r.Type == RuleType.Default);
            rules.DeactivateRule(rule.Id);


            var sut = new OrderServiceClient(BaseUrl, SessionId);
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var validation = sut.ValidateOrder(order);

            Assert.IsFalse(validation.HasWarning);
            Assert.IsNullOrEmpty(validation.Message);


        }

    }
}

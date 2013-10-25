﻿using apcurium.MK.Booking.Api.Client.TaxiHail;
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
using ServiceStack.ServiceClient.Web;

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



            var sut = new RulesServiceClient(BaseUrl, SessionId, "Test");
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
                Message = "Due to...",
                Priority = new Random().Next()
            });
        }

        private static void CreateDefaultRules(RulesServiceClient sut)
        {

            if (sut.GetRules().None(r => (r.Category == RuleCategory.WarningRule) && (r.Type == RuleType.Default)))
            {
                // Default rate does not exist for this company 
                sut.CreateRule(new Rule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.WarningRule,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    Id = Guid.NewGuid(),
                    IsActive = false,
                    Message = "Due to the current volume of calls, please note that pickup may be delayed.",
                    ActiveFrom = DateTime.Now.AddHours(-1),
                    ActiveTo = DateTime.Now.AddHours(1)
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
            var rules = sut.GetRules();

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

            var sut = new RulesServiceClient(BaseUrl, SessionId, "Test");
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
            var sut = new RulesServiceClient(BaseUrl, SessionId, "Test");
            var newMessage = "UpdateRuleTest" + Guid.NewGuid().ToString();
            var newName = "UpdateRuleTest" + Guid.NewGuid().ToString();
            var activeFromDateRef = DateTime.Today.AddDays(20);

            var rule = sut.GetRules().Single(r => r.Id == _knownRuleId);

            rule.Message = newMessage;
            rule.Name = newName;
            rule.Priority = 99;


            rule.AppliesToCurrentBooking = false;
            rule.AppliesToFutureBooking = true;
            rule.ZoneList = "200,201";
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
            Assert.AreEqual("200,201", rule.ZoneList);
            Assert.AreEqual(false, rule.IsActive);
            Assert.AreEqual(activeFromDateRef, rule.ActiveFrom);
            Assert.AreEqual(activeFromDateRef.AddMonths(4), rule.ActiveTo);
            Assert.AreEqual(DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, rule.DaysOfTheWeek);
            Assert.AreEqual(new DateTime(2000, 1, 1, 20, 18, 8), rule.StartTime);
            Assert.AreEqual(new DateTime(2000, 1, 1, 21, 19, 9), rule.EndTime);
            Assert.AreEqual(99, rule.Priority);


        }


        [Test]
        public void ActivateDeactivate()
        {




            var sut = new RulesServiceClient(BaseUrl, SessionId, "Test");
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
        public void TestDefaultRule_Warning_NoZone()
        {
            var rule = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
            });



            var validation = ValidateOrder(null);

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule.Message, validation.Message);

            var createResult = CreateOrder(null);
            Assert.IsNullOrEmpty(createResult);
        }

        [Test]
        public void TestDefaultRule_Warning_Current_NoApplied()
        {
            var rule = CreateRule(r =>
            {
                r.AppliesToCurrentBooking = false;

                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
            });



            var validation = ValidateOrder(o => o.PickupDate = null);

            Assert.IsFalse(validation.HasWarning);


        }

        [Test]
        public void TestDefaultRule_Warning_Futuret_NoApplied()
        {
            var rule = CreateRule(r =>
            {
                r.AppliesToCurrentBooking = true;
                r.AppliesToFutureBooking = false;
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
            });



            var validation = ValidateOrder(o => o.PickupDate = DateTime.Now);

            Assert.IsFalse(validation.HasWarning);


        }


        [Test]
        public void TestDefaultRule_Disable_NoZone()
        {
            var rule = CreateRule(r =>
            {
                r.Category = RuleCategory.DisableRule;
                r.Type = RuleType.Default;
            });



            var validation = ValidateOrder(null);
            Assert.IsFalse(validation.HasWarning);

            var createResult = CreateOrder(null);
            Assert.AreEqual(createResult, rule.Message);
        }


        [Test]
        public void TestDefaultRule_Priority_NoZone()
        {
            var rule1 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.Priority = 2;
            });

            var rule2 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.Priority = 1;
            });

            var rule3 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.Priority = 6;
            });



            var validation = ValidateOrder(null);
            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule2.Message, validation.Message);

        }

        [Test]
        public void TestDefaultRule_SimpleNoZone_With_Rule_Zone()
        {
            var rule1 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.ZoneList = "100,101,200";
                r.Priority = 2;
            });

            var validation = ValidateOrder(null);

            Assert.IsFalse(validation.HasWarning);

        }


        [Test]
        public void TestDefaultRule_Simple_Zone()
        {
            var rule1 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.ZoneList = "100,101,200";
                r.Priority = 2;
            });

            var validation = ValidateOrder(null, "101");
            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule1.Message, validation.Message);


        }


        [Test]
        public void TestRecurrencyRule()
        {

            var activeFromDateRef = DateTime.Now;
            var dayOfTheWeek = 1 << (int)activeFromDateRef.DayOfWeek;

            var rule1 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Recurring;
                r.ZoneList = "100,101,200";
                r.Priority = 2;

                r.ActiveFrom = activeFromDateRef.AddHours(-1);
                r.ActiveTo = activeFromDateRef.AddHours(1);
                r.DaysOfTheWeek = (DayOfTheWeek)dayOfTheWeek;
                r.StartTime = activeFromDateRef.AddHours(-1);
                r.EndTime = activeFromDateRef.AddHours(1);

            });

            var validation = ValidateOrder(null, "101");

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule1.Message, validation.Message);
        }


        [Test]
        public void TestAllRulesPriorities()
        {

            var activeFromDateRef = DateTime.Now;
            var dayOfTheWeek = 1 << (int)activeFromDateRef.DayOfWeek;

            var rule1 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Recurring;
                r.ZoneList = "100,101,200";
                r.Priority = 2;

                r.ActiveFrom = activeFromDateRef.AddHours(-1);
                r.ActiveTo = activeFromDateRef.AddHours(1);
                r.DaysOfTheWeek = (DayOfTheWeek)dayOfTheWeek;
                r.StartTime = activeFromDateRef.AddHours(-1);
                r.EndTime = activeFromDateRef.AddHours(1);

            });


            var rule2 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Default;
                r.Priority = 3;
            });




            var rule3 = CreateRule(r =>
            {
                r.Category = RuleCategory.WarningRule;
                r.Type = RuleType.Date;
                r.ZoneList = "100,101,200";
                r.Priority = 4;
                r.StartTime = activeFromDateRef.AddHours(-1);
                r.EndTime = activeFromDateRef.AddHours(1);
                r.ActiveFrom = activeFromDateRef.AddHours(-1);
                r.ActiveTo = activeFromDateRef.AddHours(1);
                r.DaysOfTheWeek = (DayOfTheWeek)dayOfTheWeek;
                r.StartTime = activeFromDateRef.AddHours(-1);
                r.EndTime = activeFromDateRef.AddHours(1);

            });


            var validation = ValidateOrder(null, "101");


            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule1.Message, validation.Message);
        }



        [Test]
        public void TestRecurrencyRuleIsApplied()
        {
            var ruleId = Guid.NewGuid();
            var activeFromDateRef = DateTime.Now;
            var name = "ReccurencyRuleTest" + Guid.NewGuid().ToString();
            var mess = "ReccurencyRuleTestMessage";
            var dayOfTheWeek = 1 << (int)DateTime.Now.DayOfWeek;
            var rules = new RulesServiceClient(BaseUrl, SessionId, "Test");
            rules.CreateRule(new Common.Entity.Rule
            {
                Id = ruleId,
                Name = name,
                Type = RuleType.Recurring,
                Category = RuleCategory.WarningRule,
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = true,
                ActiveFrom = activeFromDateRef.AddHours(-1),
                ActiveTo = activeFromDateRef.AddHours(1),
                DaysOfTheWeek = (DayOfTheWeek)dayOfTheWeek,
                StartTime = activeFromDateRef.AddHours(-1),
                EndTime = activeFromDateRef.AddHours(1),
                Priority = 23,
                IsActive = true,
                Message = "ReccurencyRuleTestMessage",
                ZoneList = " "

            });

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var validation = sut.ValidateOrder(order);

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(mess, validation.Message);
        }

        [Test]
        public void TestDateRuleIsApplied()
        {
            var ruleId = Guid.NewGuid();
            var activeFromDateRef = DateTime.Now;
            var name = "DateRuleTest" + Guid.NewGuid().ToString();
            var mess = "DateRuleTestMessage";
            var dayOfTheWeek = 1 << (int)DateTime.Now.DayOfWeek;
            var rules = new RulesServiceClient(BaseUrl, SessionId, "Test");
            rules.CreateRule(new Common.Entity.Rule
            {
                Id = ruleId,
                Name = name,
                Type = RuleType.Date,
                Category = RuleCategory.WarningRule,
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = true,
                ActiveFrom = activeFromDateRef.AddHours(-1),
                ActiveTo = activeFromDateRef.AddHours(1),
                DaysOfTheWeek = (DayOfTheWeek)dayOfTheWeek,
                StartTime = activeFromDateRef.AddHours(-1),
                EndTime = activeFromDateRef.AddHours(1),
                Priority = 26,
                IsActive = true,
                Message = "DateRuleTestMessage",
                ZoneList = " "

            });

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var validation = sut.ValidateOrder(order);

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(mess, validation.Message);
        }

        [Test]
        public void TestWarningRuleIsApplied()
        {
            var rules = new RulesServiceClient(BaseUrl, SessionId, "Test");
            CreateDefaultRules(rules);
            var rule = rules.GetRules().Single(r => r.Category == RuleCategory.WarningRule && r.Type == RuleType.Default);
            rule.AppliesToCurrentBooking = true;
            rule.AppliesToFutureBooking = true;
            rule.IsActive = true;
            rules.UpdateRule(rule);


            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            var validation = sut.ValidateOrder(order);

            Assert.IsTrue(validation.HasWarning);
            Assert.AreEqual(rule.Message, validation.Message);


        }



        [Test]
        public void TestWarningRuleIsNotApplied()
        {
            var rules = new RulesServiceClient(BaseUrl, SessionId, "Test");
            CreateDefaultRules(rules);
            var rule = rules.GetRules().Single(r => r.Category == RuleCategory.WarningRule && r.Type == RuleType.Default);
            rules.DeactivateRule(rule.Id);


            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
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



        private OrderValidationResult ValidateOrder(Action<CreateOrder> update, string testZone = null)
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings { ChargeTypeId = 99, VehicleTypeId = 1, ProviderId = 13, Phone = "514-555-12129", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            if (update != null)
            {
                update(order);
            }

            return sut.ValidateOrder(order, testZone);
        }

        private string CreateOrder(Action<CreateOrder> update)
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                PickupAddress = TestAddresses.GetAddress1(),
                PickupDate = DateTime.Now,
                DropOffAddress = TestAddresses.GetAddress2(),
            };

            order.Settings = new BookingSettings
                                 {
                                     ChargeTypeId = 99,
                                     VehicleTypeId = 1,
                                     ProviderId = Provider.MobileKnowledgeProviderId,
                                     Phone = "514-555-12129",
                                     Passengers = 6,
                                     NumberOfTaxi = 1,
                                     Name = "Joe Smith"
                                 };

            if (update != null)
            {
                update(order);
            }

            try
            {
                var r = sut.CreateOrder(order);
            }
            catch (WebServiceException wEx)
            {
                return wEx.ErrorMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        private Common.Entity.Rule CreateRule(Action<Common.Entity.Rule> update)
        {
            var ruleId = Guid.NewGuid();
            var name = "TestRule" + Guid.NewGuid().ToString();
            var mess = "TestRule Message" + Guid.NewGuid().ToString(); ;
            var rules = new RulesServiceClient(BaseUrl, SessionId, "Test");
            var newRule = new Common.Entity.Rule
            {
                Id = ruleId,
                Name = name,
                Type = RuleType.Recurring,
                Category = RuleCategory.WarningRule,
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = true,
                Priority = 23,
                IsActive = true,
                Message = mess,
            };


            if (update != null)
            {
                update(newRule);
            }

            rules.CreateRule(newRule);

            return newRule;
        }



    }
}


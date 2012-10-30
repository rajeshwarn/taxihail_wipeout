using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Test.Maps.PriceCalculatorFixture
{
    [TestFixture]
    public class given_a_day_rate_ending_the_same_day
    {
        private PriceCalculator sut;
        private FakeRateProvider _rateProvider;

        [SetUp]
        public void Setup()
        {
            var defaultRate = new Rate
            {
                Name = "Default",
                Type = (int) RateType.Default
            };

            var rate1 = new Rate
            {
                Name = "Day rate ending the same day",
                StartTime = new DateTime(2012, 12, 17, 8, 0, 0),
                EndTime = new DateTime(2012, 12, 17, 20, 0, 0),
                Type = (int)RateType.Day // Monday
            };
           
            _rateProvider = new FakeRateProvider(new[]
            {
                rate1
            });
            sut = new PriceCalculator(new TestConfigurationManager(), _rateProvider);
        }

        [Test]
        public void when_date_match_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 17, 9, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Day rate ending the same day", rate.Name);
        }

        [Test]
        public void when_date_match_day_but_before_start_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 17, 6, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_and_start_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 17, 8, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Day rate ending the same day", rate.Name);
        }

        [Test]
        public void when_date_match_day_and_end_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 17, 20, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_but_after_end_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 17, 21, 0, 0));

            Assert.Null(rate);
        }
        [Test]
        public void when_date_doesnt_match_day()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 3, 21, 0, 0));

            Assert.Null(rate);
        }

        
    }

    [TestFixture]
    public class given_a_day_rate_ending_the_next_day
    {
        private PriceCalculator sut;
        private FakeRateProvider _rateProvider;

        [SetUp]
        public void Setup()
        {
            var rate1 = new Rate
            {
                Name = "Day rate ending the next day",
                StartTime = new DateTime(2012, 12, 18, 20, 0, 0),
                EndTime = new DateTime(2012, 12, 19, 8, 0, 0),
                Type = (int)RateType.Day //Tuesday
            };
           
            _rateProvider = new FakeRateProvider(new[]
            {
                rate1
            });
            sut = new PriceCalculator(new TestConfigurationManager(), _rateProvider);
        }

        [Test]
        public void when_date_match_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 18, 21, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Day rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_next_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 19, 1, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Day rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_day_but_before_start_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 18, 19, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_and_start_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 18, 20, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Day rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_next_day_and_end_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 19, 8, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_next_day_but_after_end_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 19, 21, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_doesnt_match_day()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 3, 21, 0, 0));

            Assert.Null(rate);
        }


    }

    [TestFixture]
    public class given_a_recurring_rate_ending_the_same_day
    {
        private PriceCalculator sut;
        private FakeRateProvider _rateProvider;

        [SetUp]
        public void Setup()
        {
            var rate1 = new Rate
            {
                Name = "Recurring rate ending the same day",
                StartTime = new DateTime(1900, 1, 1, 8, 0, 0),
                EndTime = new DateTime(1900, 1, 1, 20, 0, 0),
                DaysOfTheWeek = (int)(DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday),
                Type = (int)RateType.Recurring
            };

            _rateProvider = new FakeRateProvider(new[]
                                                     {
                                                         rate1
                                                     });
            sut = new PriceCalculator(new TestConfigurationManager(), _rateProvider);
        }

        [Test]
        public void when_date_match_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 9, 0, 0) /* Friday */);

            Assert.NotNull(rate);
            Assert.AreEqual("Recurring rate ending the same day", rate.Name);
        }

        [Test]
        public void when_date_match_day_but_before_start_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 7, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_and_start_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 8, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Recurring rate ending the same day", rate.Name);
        }

        [Test]
        public void when_date_match_day_and_end_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 20, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_but_after_end_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 22, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_doesnt_match_day()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 3, 21, 0, 0));

            Assert.Null(rate);
        }
    }

    [TestFixture]
    public class given_a_recurring_rate_ending_the_next_day
    {
        private PriceCalculator sut;
        private FakeRateProvider _rateProvider;

        [SetUp]
        public void Setup()
        {
            var rate1 = new Rate
            {
                Name = "Recurring rate ending the next day",
                StartTime = new DateTime(1900, 1, 1, 20, 0, 0),
                EndTime = new DateTime(1900, 1, 2, 8, 0, 0),
                DaysOfTheWeek = (int)(DayOfTheWeek.Friday | DayOfTheWeek.Saturday),
                Type = (int)RateType.Recurring
            };

            _rateProvider = new FakeRateProvider(new[]
            {
                rate1
            });
            sut = new PriceCalculator(new TestConfigurationManager(), _rateProvider);
        }

        [Test]
        public void when_date_match_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 21, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Recurring rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_next_day_and_inside_time_period()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 22, 7, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Recurring rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_day_but_before_start_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 10, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_day_and_start_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 21, 20, 0, 0));

            Assert.NotNull(rate);
            Assert.AreEqual("Recurring rate ending the next day", rate.Name);
        }

        [Test]
        public void when_date_match_next_day_and_end_time_exactly()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 22, 8, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_match_next_day_but_after_end_time()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 22, 12, 0, 0));

            Assert.Null(rate);
        }

        [Test]
        public void when_date_doesnt_match_day()
        {
            var rate = sut.GetRateFor(new DateTime(2012, 12, 3, 21, 0, 0));

            Assert.Null(rate);
        }


    }

    public class FakeRateProvider : IRateProvider
    {
        private readonly IList<Rate> _rates;

        public FakeRateProvider(IEnumerable<Rate> rates)
        {
            _rates = rates.ToList();
        }

        public void AddRate(Rate rate)
        {
            _rates.Add(rate);
        }

        public IEnumerable<Rate> GetRates()
        {
            return _rates;
        }
    }
}

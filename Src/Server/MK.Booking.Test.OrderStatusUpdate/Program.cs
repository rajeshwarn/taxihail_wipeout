using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Mono.Options;

namespace MK.Booking.Test.OrderStatusUpdate
{
    class Program
    {
        private static bool _interactive;
        private static bool _runOldUpdate;
        private static bool _runNewUpdate;
        private static bool _runBoth;

        static void Main(string[] args)
        {
            Console.WriteLine("WELCOME! Here you can test the old and the new OrderStatusUpdater.");

            var p = new OptionSet
            {
                {"i|interactive", "Interactive mode. Other options are ignored", i => _interactive = i != null},
                {"oldUpdate", "Run the OLD OrderStatusUpdater", c => _runOldUpdate = c != null},
                {"newUpdate", "Run the NEW OrderStatusUpdater", c => _runNewUpdate = c != null},
                {"both", "Run the NEW OrderStatusUpdater", c => _runBoth = c != null},

            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                ShowHelpAndExit(e.Message, p);
            }

           RunTest();
        }

        private static void RunTest()
        {
            _interactive = true;
            _runOldUpdate = false;
            _runNewUpdate = false;
            _runBoth = false;

            var maxDegreeOfParallelism = 16;

            if (_interactive)
            {
                Console.WriteLine();
                Console.WriteLine("Run the tests :");
                Console.WriteLine("To run the OLD OrderStatusUpdater Write  : 'old'");
                Console.WriteLine("To run the NEW OrderStatusUpdater Write  : 'new'");
                Console.WriteLine("To run the both Write  : 'both'");

                var response = Console.ReadLine();

                _runOldUpdate = response.Equals("old", StringComparison.OrdinalIgnoreCase);
                _runNewUpdate = response.Equals("new", StringComparison.OrdinalIgnoreCase);
                _runBoth = response.Equals("both", StringComparison.OrdinalIgnoreCase);

                var fakeServerSettings = new FakeServerSettings();

                var time = DateTime.Now;

                if (_runOldUpdate)
                {

                    var oldUpdateOrderStatusJob = new OldUpdateOrderStatusJob(
                        new FakeOrderDao(),
                        new FakeIBSServiceProvider(),
                        new FakeOrderStatusUpdateDao(),
                        new FakeOrderStatusUpdater(fakeServerSettings),
                        new FakeHoneyBadgerServiceClient(fakeServerSettings),
                        fakeServerSettings);

                    Console.WriteLine("Starting Old Order Update");
                    time = DateTime.Now;

                    oldUpdateOrderStatusJob.CheckStatus(string.Empty, 5);

                    Console.WriteLine("Old Order Update took :" + DateTime.Now.Subtract(time).TotalSeconds);
                }
                else if (_runNewUpdate)
                {

                    var updateOrderStatusJob = new UpdateOrderStatusJob(
                        new FakeOrderDao(),
                        new FakeIBSServiceProvider(),
                        new FakeOrderStatusUpdateDao(),
                        new FakeOrderStatusUpdater(fakeServerSettings),
                        new FakeHoneyBadgerServiceClient(fakeServerSettings),
                        fakeServerSettings);

                    updateOrderStatusJob.MaxParallelism = ChangeMaxDegreeOfParallelismIfRequested(maxDegreeOfParallelism);

                    Console.WriteLine("Starting New Order Update");
                    time = DateTime.Now;

                    updateOrderStatusJob.CheckStatus(string.Empty, 5);

                    Console.WriteLine("New Order Update took :" + DateTime.Now.Subtract(time).TotalSeconds);
                }
                else if (_runBoth)
                {
                    maxDegreeOfParallelism = ChangeMaxDegreeOfParallelismIfRequested(maxDegreeOfParallelism);

                    var oldUpdateOrderStatusJob = new OldUpdateOrderStatusJob(
                        new FakeOrderDao(),
                        new FakeIBSServiceProvider(),
                        new FakeOrderStatusUpdateDao(),
                        new FakeOrderStatusUpdater(fakeServerSettings),
                        new FakeHoneyBadgerServiceClient(fakeServerSettings),
                        fakeServerSettings);

                    Console.WriteLine("Starting Old Order Update");
                    time = DateTime.Now;

                    oldUpdateOrderStatusJob.CheckStatus(string.Empty, 5);

                    Console.WriteLine("Old Order Update took :" + DateTime.Now.Subtract(time).TotalSeconds);

                    time = DateTime.Now;

                    var updateOrderStatusJob = new UpdateOrderStatusJob(
                        new FakeOrderDao(),
                        new FakeIBSServiceProvider(),
                        new FakeOrderStatusUpdateDao(),
                        new FakeOrderStatusUpdater(fakeServerSettings),
                        new FakeHoneyBadgerServiceClient(fakeServerSettings),
                        fakeServerSettings);

                    updateOrderStatusJob.MaxParallelism = maxDegreeOfParallelism;
                    Console.WriteLine("Starting New Order Update");

                    updateOrderStatusJob.CheckStatus(string.Empty, 5);

                    Console.WriteLine("New Order Update took :" + DateTime.Now.Subtract(time).TotalSeconds);
                }

                Console.WriteLine();
                Console.WriteLine("TEST ENDED!! Do you want to run another Test ?");
                Console.WriteLine("y/N");

                if (Console.ReadLine().Equals("y", StringComparison.InvariantCultureIgnoreCase))
                {
                    RunTest();
                }
            }
        }

        private static int ChangeMaxDegreeOfParallelismIfRequested(int maxDegreeOfParallelism)
        {
            Console.WriteLine($"The Current degree of parallelism is {maxDegreeOfParallelism}. Do you want to Change it ? ");
            Console.WriteLine("y/N");

            if (Console.ReadLine().Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Write the new degree you want to use");
                int.TryParse(Console.ReadLine(), out maxDegreeOfParallelism);
            }

            return maxDegreeOfParallelism;
        }

        private static void ShowHelpAndExit(string message, OptionSet optionSet)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}

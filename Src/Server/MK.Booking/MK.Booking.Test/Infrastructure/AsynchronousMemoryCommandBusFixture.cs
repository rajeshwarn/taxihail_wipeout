using System;
using System.Runtime.InteropServices;
using System.Threading;
using apcurium.MK.Booking.Commands;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Serialization;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Infrastructure
{
     [TestFixture]
    public class AsynchronousMemoryCommandBusFixture
    {
         [SetUp]
         public void Setup()
         {
            
         }

         [Test]
         public void when_no_exception_then_invoke_once()
         {
             var counthandler = new FakeHandler();
             var sut = new AsynchronousMemoryCommandBus(new JsonTextSerializer(), counthandler);

             sut.Send(new RegisterAccount());

             Thread.Sleep(TimeSpan.FromSeconds(2));

             Assert.That(counthandler.Invocation, Is.EqualTo(0));
         }
    }

    public class FakeHandler : ICommandHandler<RegisterAccount>
    {
        private int _invocation;

        public int Invocation
        {
            get { return _invocation; }
            set { _invocation = value; }
        }

        public void Handle(RegisterAccount command)
        {
            Invocation++;
        }
    }
}
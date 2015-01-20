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
         public async void when_no_exception_then_invoke_once()
         {
             var counthandler = new FakeHandler();
             var sut = new AsynchronousMemoryCommandBus(new JsonTextSerializer(), counthandler);

             await sut.SendAwaitable(new RegisterAccount());

             Assert.That(counthandler.Invocation, Is.EqualTo(1));
         }

         [Test]
         public async void when_exception_then_invoke_retry()
         {
             var counthandler = new FakeHandler(true);
             var sut = new AsynchronousMemoryCommandBus(new JsonTextSerializer(), counthandler);

             var envelope = new Envelope<ICommand>(new RegisterAccount());
             envelope.RetryCount = 3;
             await sut.SendAwaitable(envelope);

             Assert.That(counthandler.Invocation, Is.EqualTo(3));
         }
    }

    public class FakeHandler : ICommandHandler<RegisterAccount>
    {
        private readonly bool _throwException;
        private int _invocation;

        public FakeHandler(bool throwException = false)
        {
            _throwException = throwException;
        }

        public int Invocation
        {
            get { return _invocation; }
            set { _invocation = value; }
        }

        public void Handle(RegisterAccount command)
        {
            Invocation++;
            if (_throwException)
            {
                throw new Exception();
            }
        }
    }
}
// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using NUnit.Framework;

namespace apcurium.MK.Booking.Common.Tests
{
    using System;
    using System.Threading;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Messaging.InMemory;
    using Moq;

    [TestFixture]
    public class MemoryEventBusFixture
    {
        [Test]
        public void WhenPublishingEvent_ThenInvokesCompatibleHandler()
        {
            var handler = new Mock<IEventHandler<TestEvent>>();
            var e = new ManualResetEventSlim();
            handler.Setup(x => x.Handle(It.IsAny<TestEvent>()))
                .Callback(() => e.Set());

            var bus = new MemoryEventBus(handler.Object);

            bus.Publish(new TestEvent());

            e.Wait(3000);

            handler.Verify(x => x.Handle(It.IsAny<TestEvent>()));
        }

        [Test]
        public void WhenPublishingEvent_ThenDoesNotInvokeIncompatibleHandler()
        {
            var compatibleHandler = new Mock<IEventHandler<TestEvent>>();
            var incompatibleHandler = new Mock<IEventHandler<FooEvent>>();
            var e = new ManualResetEventSlim();

            compatibleHandler.Setup(x => x.Handle(It.IsAny<TestEvent>()))
                .Callback(() => e.Set());

            var bus = new MemoryEventBus(incompatibleHandler.Object, compatibleHandler.Object);

            bus.Publish(new TestEvent());

            e.Wait(3000);

            incompatibleHandler.Verify(x => x.Handle(It.IsAny<FooEvent>()), Times.Never());
        }

        [Test]
        public void WhenPublishingMultipleEvents_ThenInvokesCompatibleHandlerMultipleTimes()
        {
            var handler = new Mock<IEventHandler<TestEvent>>();
            var e = new ManualResetEventSlim();

            var called = 0;
            handler.Setup(x => x.Handle(It.IsAny<TestEvent>()))
                .Callback(() => { if (Interlocked.Increment(ref called) == 4) e.Set(); });

            var bus = new MemoryEventBus(handler.Object);

            bus.Publish(new[] { new TestEvent(), new TestEvent(), new TestEvent(), new TestEvent() });

            e.Wait(10000);

            Assert.AreEqual(4, called);
        }

        public class TestEvent : IEvent
        {
            public Guid SourceId
            {
                get { throw new NotImplementedException(); }
            }
        }

        public class FooEvent : IEvent
        {
            public Guid SourceId
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}

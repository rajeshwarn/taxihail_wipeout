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

using BackOffice.Test;

using BackOffice.Test;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure.Messaging;
    using Moq;
    using apcurium.MK.Booking.EventHandlers;
    using apcurium.MK.Booking.Database;
    using apcurium.MK.Booking.Events;
    using apcurium.MK.Booking.ReadModel;
    
   
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected AccountDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new AccountDetailsGenerator(() => new BookingDbContext(dbName) );
        }
    }

    [TestFixture]
    public class given_no_account : given_a_view_model_generator
    {
        [Test]
        public void when_account_registered_then_account_dto_populated()
        {
            var accountId = Guid.NewGuid();

            this.sut.Handle(new AccountRegistered
            {
                SourceId = accountId,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@acpurium.com",
                Password = new byte[1] { 1 },
                IbsAcccountId = 666
            });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Find<AccountDetail>(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Bob", dto.FirstName);
                Assert.AreEqual("Smith", dto.LastName);
                Assert.AreEqual("bob.smith@acpurium.com", dto.Email);
                Assert.AreEqual(1, dto.Password.Length);
                Assert.AreEqual(666, dto.IBSAccountid);
            }
        }
    }

    [TestFixture]
    public class given_existing_account : given_a_view_model_generator
    {
        private Guid _accountId = Guid.NewGuid();

        public given_existing_account()
        {
            this.sut.Handle(new AccountRegistered
            {
                SourceId = _accountId,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@acpurium.com",
                Password = new byte[1] { 1 }
            });

        }

        [Test]
        public void when_account_updated_then_account_dto_populated()
        {
            this.sut.Handle(new AccountUpdated
            {
                SourceId = _accountId, 
                FirstName = "Robert",
                LastName = "Smither",                
            });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Find<AccountDetail>(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual("Robert", dto.FirstName);
                Assert.AreEqual("Smither", dto.LastName);                
            }
        }

    }
}

// Example tests that show how to use Mirror

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    class ExampleTests
    {
        public interface IAccountRepository
        {
            Account GetAccount(int accountNumber);
        }

        public class Account
        {
        }

        public class AccountService
        {
            private readonly IAccountRepository _accountStore;

            public AccountService(IAccountRepository accountStore)
            {
                _accountStore = accountStore;
            }

            public bool AccountExists(int accountNumber)
            {
                return _accountStore.GetAccount(accountNumber) != null;
            }
        }

        [Test]
        public void TestAccountExistsReturnsTrueIfAccountExists()
        {
            var mirrorRepository = new Mirror<IAccountRepository>();
            mirrorRepository.Returns(r => r.GetAccount(123), new Account());
            var accountService = new AccountService(mirrorRepository.It);

            Assert.IsTrue(accountService.AccountExists(123));
        }

        [Test]
        public void TestAccountExistsReturnsFalseIfAccountDoesNotExist()
        {
            var mirrorRepository = new Mirror<IAccountRepository>();
            var accountService = new AccountService(mirrorRepository.It);

            Assert.IsFalse(accountService.AccountExists(456));
        }
    }
}

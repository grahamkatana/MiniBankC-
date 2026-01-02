// Services/AccountServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using MiniBank.Api.Services;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Models;
using MiniBank.Api.Dtos.Account;
using MiniBank.Tests.Helpers;

namespace MiniBank.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _accountRepoMock = new Mock<IAccountRepository>();
            _accountService = new AccountService(_accountRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountExists_ReturnsAccountDto()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            _accountRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be("ACC-20260102-0001");
            result.Balance.Should().Be(1000.00m);
            result.UserId.Should().Be("test-user-id");
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountDoesNotExist_ReturnsNull()
        {
            // Arrange
            _accountRepoMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAccountNumberAsync_WhenAccountExists_ReturnsAccountDto()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-20260102-0001"))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetByAccountNumberAsync("ACC-20260102-0001");

            // Assert
            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be("ACC-20260102-0001");
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsAllUserAccounts()
        {
            // Arrange
            var accounts = new List<Account>
            {
                TestDataHelper.CreateTestAccount("user1", 1),
                TestDataHelper.CreateTestAccount("user1", 2)
            };
            _accountRepoMock
                .Setup(x => x.GetByUserIdAsync("user1"))
                .ReturnsAsync(accounts);

            // Act
            var result = await _accountService.GetByUserIdAsync("user1");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(a => a.UserId.Should().Be("user1"));
        }

        [Fact]
        public async Task CreateAsync_GeneratesUniqueAccountNumber()
        {
            // Arrange
            var createDto = TestDataHelper.CreateAccountDto();
            _accountRepoMock
                .Setup(x => x.AccountExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _accountRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            // Act
            var result = await _accountService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.AccountNumber.Should().StartWith("ACC-");
            result.Balance.Should().Be(0);
            result.UserId.Should().Be("test-user-id");
            _accountRepoMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountExists_UpdatesAndReturnsDto()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            var updateDto = new UpdateAccountDto
            {
                AccountType = "Savings",
                IsActive = false
            };

            _accountRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(account);
            _accountRepoMock
                .Setup(x => x.UpdateAsync(1, It.IsAny<Account>()))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.UpdateAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            _accountRepoMock.Verify(x => x.UpdateAsync(1, It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountDoesNotExist_ReturnsNull()
        {
            // Arrange
            var updateDto = new UpdateAccountDto { AccountType = "Savings" };
            _accountRepoMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.UpdateAsync(999, updateDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountExists_ReturnsTrue()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            _accountRepoMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _accountRepoMock
                .Setup(x => x.DeleteAsync(999))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetBalanceAsync_ReturnsAccountBalance()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            account.Balance = 2500.00m;
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-20260102-0001"))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetBalanceAsync("ACC-20260102-0001");

            // Assert
            result.Should().Be(2500.00m);
        }

        [Fact]
        public async Task GetBalanceAsync_WhenAccountDoesNotExist_ReturnsZero()
        {
            // Arrange
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("INVALID"))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.GetBalanceAsync("INVALID");

            // Assert
            result.Should().Be(0);
        }
    }
}
using FluentAssertions;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Models;
using MiniBank.Api.Services;
using MiniBank.Tests.Helpers;
using Moq;
using Xunit;

namespace MiniBank.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();

            _transactionService = new TransactionService(
                _transactionRepoMock.Object,
                _accountRepoMock.Object,
                _emailServiceMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task DepositAsync_IncreasesAccountBalance()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccountWithUser();
            var depositDto = TestDataHelper.CreateDepositDto();
            var initialBalance = account.Balance;

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(account.AccountNumber))
                .ReturnsAsync(account);
            _accountRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()))
                .ReturnsAsync(account);
            _transactionRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // Act
            var result = await _transactionService.DepositAsync(depositDto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(depositDto.Amount);
            result.TransactionType.Should().Be("Deposit");
            result.Status.Should().Be("Completed");

            _accountRepoMock.Verify(
                x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()),
                Times.Once
            );
            _transactionRepoMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task DepositAsync_ThrowsException_WhenAccountNotFound()
        {
            // Arrange
            var depositDto = TestDataHelper.CreateDepositDto("INVALID");
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("INVALID"))
                .ReturnsAsync((Account?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _transactionService.DepositAsync(depositDto));
        }

        [Fact]
        public async Task WithdrawAsync_DecreasesAccountBalance()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccountWithUser();
            account.Balance = 1000.00m;
            var withdrawDto = TestDataHelper.CreateWithdrawDto(account.AccountNumber);

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(account.AccountNumber))
                .ReturnsAsync(account);
            _accountRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()))
                .ReturnsAsync(account);
            _transactionRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // Act
            var result = await _transactionService.WithdrawAsync(withdrawDto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(withdrawDto.Amount);
            result.TransactionType.Should().Be("Withdrawal");
            _accountRepoMock.Verify(
                x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()),
                Times.Once
            );
        }

        [Fact]
        public async Task WithdrawAsync_ThrowsException_WhenInsufficientBalance()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccountWithUser();
            account.Balance = 100.00m;
            var withdrawDto = TestDataHelper.CreateWithdrawDto(account.AccountNumber);

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(account.AccountNumber))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _transactionService.WithdrawAsync(withdrawDto)
            );
            exception.Message.Should().Contain("Insufficient balance");
        }

        [Fact]
        public async Task TransferAsync_MovesMoneyBetweenAccounts()
        {
            // Arrange
            var fromAccount = TestDataHelper.CreateTestAccountWithUser(
                "user1",
                1,
                "user1@test.com"
            );
            fromAccount.Balance = 1000.00m;
            var toAccount = TestDataHelper.CreateTestAccountWithUser("user2", 2, "user2@test.com");
            toAccount.Balance = 500.00m;
            var transferDto = TestDataHelper.CreateTransferDto(
                fromAccount.AccountNumber,
                toAccount.AccountNumber
            );

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(fromAccount.AccountNumber))
                .ReturnsAsync(fromAccount);
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(toAccount.AccountNumber))
                .ReturnsAsync(toAccount);
            _accountRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()))
                .ReturnsAsync((int id, Account a) => a);
            _transactionRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // Act
            var result = await _transactionService.TransferAsync(transferDto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(300.00m);
            result.TransactionType.Should().Be("Transfer");
            result.FromAccountId.Should().Be(1);
            result.ToAccountId.Should().Be(2);
            _accountRepoMock.Verify(
                x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Account>()),
                Times.Exactly(2)
            );
        }

        [Fact]
        public async Task TransferAsync_ThrowsException_WhenSameAccount()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccountWithUser();
            var transferDto = TestDataHelper.CreateTransferDto(
                account.AccountNumber,
                account.AccountNumber
            );

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(account.AccountNumber))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _transactionService.TransferAsync(transferDto)
            );
            exception.Message.Should().Contain("Cannot transfer to the same account");
        }

        [Fact]
        public async Task TransferAsync_ThrowsException_WhenInsufficientBalance()
        {
            // Arrange
            var fromAccount = TestDataHelper.CreateTestAccountWithUser(
                "user1",
                1,
                "user1@test.com"
            );
            fromAccount.Balance = 100.00m;
            var toAccount = TestDataHelper.CreateTestAccountWithUser("user2", 2, "user2@test.com");
            var transferDto = TestDataHelper.CreateTransferDto(
                fromAccount.AccountNumber,
                toAccount.AccountNumber
            );

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(fromAccount.AccountNumber))
                .ReturnsAsync(fromAccount);
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(toAccount.AccountNumber))
                .ReturnsAsync(toAccount);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _transactionService.TransferAsync(transferDto)
            );
            exception.Message.Should().Contain("Insufficient balance");
        }

        [Fact]
        public async Task GetByAccountNumberAsync_ReturnsTransactions()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccountWithUser();
            var transactions = new List<Transaction>
            {
                TestDataHelper.CreateTestTransaction(account.Id, 1),
                TestDataHelper.CreateTestTransaction(account.Id, 2),
            };

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(account.AccountNumber))
                .ReturnsAsync(account);
            _transactionRepoMock
                .Setup(x => x.GetByAccountIdAsync(account.Id))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetByAccountNumberAsync(account.AccountNumber);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByAccountNumberAsync_ReturnsEmptyList_WhenAccountNotFound()
        {
            // Arrange
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("INVALID"))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _transactionService.GetByAccountNumberAsync("INVALID");

            // Assert
            result.Should().BeEmpty();
        }
    }
}

// Controllers/TransactionControllerTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MiniBank.Api.Controllers;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Dtos.Account;
using MiniBank.Api.Dtos.Transaction;
using MiniBank.Tests.Helpers;

namespace MiniBank.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly TransactionController _controller;
        private const string TestUserId = "test-user-id";

        public TransactionControllerTests()
        {
            _transactionServiceMock = new Mock<ITransactionService>();
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new TransactionController(_transactionServiceMock.Object, _accountServiceMock.Object);

            // Setup authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Deposit_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var depositDto = TestDataHelper.CreateDepositDto();
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = depositDto.AccountNumber };
            var transactionDto = new TransactionDto { Id = 1, Amount = depositDto.Amount, TransactionType = "Deposit" };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync(depositDto.AccountNumber))
                .ReturnsAsync(accountDto);
            _transactionServiceMock
                .Setup(x => x.DepositAsync(depositDto))
                .ReturnsAsync(transactionDto);

            // Act
            var result = await _controller.Deposit(depositDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        }

        [Fact]
        public async Task Deposit_ReturnsForbid_WhenUserIsNotOwner()
        {
            // Arrange
            var depositDto = TestDataHelper.CreateDepositDto();
            var accountDto = new AccountDto { Id = 1, UserId = "other-user", AccountNumber = depositDto.AccountNumber };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync(depositDto.AccountNumber))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.Deposit(depositDto);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Withdraw_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var withdrawDto = TestDataHelper.CreateWithdrawDto();
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = withdrawDto.AccountNumber };
            var transactionDto = new TransactionDto { Id = 1, Amount = withdrawDto.Amount, TransactionType = "Withdrawal" };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync(withdrawDto.AccountNumber))
                .ReturnsAsync(accountDto);
            _transactionServiceMock
                .Setup(x => x.WithdrawAsync(withdrawDto))
                .ReturnsAsync(transactionDto);

            // Act
            var result = await _controller.Withdraw(withdrawDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Transfer_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var transferDto = TestDataHelper.CreateTransferDto();
            var fromAccountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = transferDto.FromAccountNumber };
            var transactionDto = new TransactionDto { Id = 1, Amount = transferDto.Amount, TransactionType = "Transfer" };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync(transferDto.FromAccountNumber))
                .ReturnsAsync(fromAccountDto);
            _transactionServiceMock
                .Setup(x => x.TransferAsync(transferDto))
                .ReturnsAsync(transactionDto);

            // Act
            var result = await _controller.Transfer(transferDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task GetByAccountNumber_ReturnsOk_WhenUserIsOwner()
        {
            // Arrange
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = "ACC-001" };
            var transactions = new List<TransactionDto>
            {
                new TransactionDto { Id = 1, Amount = 500 }
            };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-001"))
                .ReturnsAsync(accountDto);
            _transactionServiceMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-001"))
                .ReturnsAsync(transactions);

            // Act
            var result = await _controller.GetByAccountNumber("ACC-001");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedTransactions = okResult.Value.Should().BeAssignableTo<List<TransactionDto>>().Subject;
            returnedTransactions.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByAccountNumber_ReturnsForbid_WhenUserIsNotOwner()
        {
            // Arrange
            var accountDto = new AccountDto { Id = 1, UserId = "other-user", AccountNumber = "ACC-001" };

            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-001"))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.GetByAccountNumber("ACC-001");

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
// Controllers/AccountControllerTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MiniBank.Api.Controllers;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Dtos.Account;
using MiniBank.Tests.Helpers;

namespace MiniBank.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly AccountController _controller;
        private const string TestUserId = "test-user-id";

        public AccountControllerTests()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new AccountController(_accountServiceMock.Object);

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
        public async Task GetAll_ReturnsOkWithUserAccounts()
        {
            // Arrange
            var accounts = new List<AccountDto>
            {
                new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = "ACC-001" },
                new AccountDto { Id = 2, UserId = TestUserId, AccountNumber = "ACC-002" }
            };

            _accountServiceMock
                .Setup(x => x.GetByUserIdAsync(TestUserId))
                .ReturnsAsync(accounts);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedAccounts = okResult.Value.Should().BeAssignableTo<List<AccountDto>>().Subject;
            returnedAccounts.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenAccountExistsAndUserIsOwner()
        {
            // Arrange
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = "ACC-001" };
            _accountServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            _accountServiceMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((AccountDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsForbid_WhenUserIsNotOwner()
        {
            // Arrange
            var accountDto = new AccountDto { Id = 1, UserId = "other-user", AccountNumber = "ACC-001" };
            _accountServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateAccountDto
            {
                UserId = TestUserId,
                AccountType = "Checking",
                Currency = "ZAR"
            };
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = "ACC-001" };

            _accountServiceMock
                .Setup(x => x.CreateAsync(createDto))
                .ReturnsAsync(accountDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        }

        [Fact]
        public async Task Create_ReturnsForbid_WhenUserIdDoesNotMatch()
        {
            // Arrange
            var createDto = new CreateAccountDto
            {
                UserId = "other-user",
                AccountType = "Checking",
                Currency = "ZAR"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var existingAccount = new AccountDto { Id = 1, UserId = TestUserId };
            var updateDto = new UpdateAccountDto { AccountType = "Savings" };
            var updatedAccount = new AccountDto { Id = 1, UserId = TestUserId, AccountType = "Savings" };

            _accountServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingAccount);
            _accountServiceMock
                .Setup(x => x.UpdateAsync(1, updateDto))
                .ReturnsAsync(updatedAccount);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateAccountDto { AccountType = "Savings" };
            _accountServiceMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((AccountDto?)null);

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var existingAccount = new AccountDto { Id = 1, UserId = TestUserId };
            _accountServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingAccount);
            _accountServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task GetBalance_ReturnsOk_WhenAccountExistsAndUserIsOwner()
        {
            // Arrange
            var accountDto = new AccountDto { Id = 1, UserId = TestUserId, AccountNumber = "ACC-001" };
            _accountServiceMock
                .Setup(x => x.GetByAccountNumberAsync("ACC-001"))
                .ReturnsAsync(accountDto);
            _accountServiceMock
                .Setup(x => x.GetBalanceAsync("ACC-001"))
                .ReturnsAsync(1500.00m);

            // Act
            var result = await _controller.GetBalance("ACC-001");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
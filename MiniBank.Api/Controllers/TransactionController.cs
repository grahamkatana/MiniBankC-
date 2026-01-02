using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Api.Dtos.Transaction;
using MiniBank.Api.Interfaces;

namespace MiniBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        public TransactionController(ITransactionService transactionService, IAccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound("Transaction not found");

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("account/{accountNumber}")]
        public async Task<IActionResult> GetByAccountNumber([FromRoute] string accountNumber)
        {
            // Verify user owns the account
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            try
            {
                var transactions = await _transactionService.GetByAccountNumberAsync(accountNumber);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("account/{accountNumber}/range")]
        public async Task<IActionResult> GetByDateRange(
            [FromRoute] string accountNumber,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // Verify user owns the account
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            try
            {
                var transactions = await _transactionService.GetByDateRangeAsync(accountNumber, startDate, endDate);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto depositDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify user owns the account
            var account = await _accountService.GetByAccountNumberAsync(depositDto.AccountNumber);
            if (account == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            try
            {
                var transaction = await _transactionService.DepositAsync(depositDto);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto withdrawDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify user owns the account
            var account = await _accountService.GetByAccountNumberAsync(withdrawDto.AccountNumber);
            if (account == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            try
            {
                var transaction = await _transactionService.WithdrawAsync(withdrawDto);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto transferDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify user owns the FROM account
            var fromAccount = await _accountService.GetByAccountNumberAsync(transferDto.FromAccountNumber);
            if (fromAccount == null)
                return NotFound("Source account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (fromAccount.UserId != userId)
                return Forbid();

            try
            {
                var transaction = await _transactionService.TransferAsync(transferDto);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transactions = await _transactionService.GetAllAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
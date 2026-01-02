using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Api.Dtos.Account;
using MiniBank.Api.Interfaces;

namespace MiniBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var accounts = await _accountService.GetByUserIdAsync(userId);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null)
                return NotFound("Account not found");

            // Check if user owns this account
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            return Ok(account);
        }

        [HttpGet("number/{accountNumber}")]
        public async Task<IActionResult> GetByAccountNumber([FromRoute] string accountNumber)
        {
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound("Account not found");

            // Check if user owns this account
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            return Ok(account);
        }

        [HttpGet("balance/{accountNumber}")]
        public async Task<IActionResult> GetBalance([FromRoute] string accountNumber)
        {
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound("Account not found");

            // Check if user owns this account
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (account.UserId != userId)
                return Forbid();

            var balance = await _accountService.GetBalanceAsync(accountNumber);
            return Ok(new { accountNumber, balance });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure user is creating account for themselves
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (createDto.UserId != userId)
                return Forbid();

            try
            {
                var account = await _accountService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAccountDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check ownership
            var existingAccount = await _accountService.GetByIdAsync(id);
            if (existingAccount == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (existingAccount.UserId != userId)
                return Forbid();

            try
            {
                var account = await _accountService.UpdateAsync(id, updateDto);
                return Ok(account);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            // Check ownership
            var existingAccount = await _accountService.GetByIdAsync(id);
            if (existingAccount == null)
                return NotFound("Account not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (existingAccount.UserId != userId)
                return Forbid();

            try
            {
                var deleted = await _accountService.DeleteAsync(id);
                if (deleted)
                    return NoContent();
                
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
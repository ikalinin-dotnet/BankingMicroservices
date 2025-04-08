using AccountService.DTOs;
using AccountService.Models;
using AccountService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerRepository customerRepository, 
        IAccountRepository accountRepository,
        IMapper mapper,
        ILogger<CustomersController> logger)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        try
        {
            _logger.LogInformation("Retrieving all customers");
            var customers = await _customerRepository.GetAllCustomersAsync();
            return Ok(_mapper.Map<IEnumerable<CustomerDto>>(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            return StatusCode(500, "An error occurred while retrieving customers");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                return NotFound();
            }
            return Ok(_mapper.Map<CustomerDto>(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
            return StatusCode(500, "An error occurred while retrieving the customer");
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email cannot be empty");
        }

        try
        {
            _logger.LogInformation("Retrieving customer with email: {Email}", email);
            var customer = await _customerRepository.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                _logger.LogWarning("Customer with email {Email} not found", email);
                return NotFound();
            }
            return Ok(_mapper.Map<CustomerDto>(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with email: {Email}", email);
            return StatusCode(500, "An error occurred while retrieving the customer");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
    {
        // Additional validation
        if (string.IsNullOrWhiteSpace(createCustomerDto.Email))
        {
            return BadRequest("Email is required");
        }

        if (string.IsNullOrWhiteSpace(createCustomerDto.FirstName) || string.IsNullOrWhiteSpace(createCustomerDto.LastName))
        {
            return BadRequest("First name and last name are required");
        }

        if (createCustomerDto.DateOfBirth > DateTime.UtcNow.AddYears(-18))
        {
            return BadRequest("Customer must be at least 18 years old");
        }

        try
        {
            // Check if customer with the same email already exists
            var existingCustomer = await _customerRepository.GetCustomerByEmailAsync(createCustomerDto.Email);
            if (existingCustomer != null)
            {
                _logger.LogWarning("Attempted to create duplicate customer with email: {Email}", createCustomerDto.Email);
                return Conflict("A customer with this email already exists");
            }

            var customer = _mapper.Map<Customer>(createCustomerDto);
            customer.CreatedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Creating new customer with email: {Email}", createCustomerDto.Email);
            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            
            return CreatedAtAction(
                nameof(GetCustomerById), 
                new { id = createdCustomer.Id }, 
                _mapper.Map<CustomerDto>(createdCustomer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with email: {Email}", createCustomerDto.Email);
            return StatusCode(500, "An error occurred while creating the customer");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, CustomerDto customerDto)
    {
        if (id != customerDto.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body");
        }

        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Attempted to update non-existent customer with ID: {CustomerId}", id);
                return NotFound();
            }

            // Check if email is being changed and if new email is already in use
            if (customer.Email != customerDto.Email)
            {
                var existingCustomer = await _customerRepository.GetCustomerByEmailAsync(customerDto.Email);
                if (existingCustomer != null && existingCustomer.Id != id)
                {
                    return Conflict("A customer with this email already exists");
                }
            }

            // Update customer properties
            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.Email = customerDto.Email;
            customer.PhoneNumber = customerDto.PhoneNumber;
            customer.DateOfBirth = customerDto.DateOfBirth;

            _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
            await _customerRepository.UpdateCustomerAsync(customer);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
            return StatusCode(500, "An error occurred while updating the customer");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Attempted to delete non-existent customer with ID: {CustomerId}", id);
                return NotFound();
            }

            // Check if customer has any accounts
            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(id);
            if (accounts.Any())
            {
                _logger.LogWarning("Attempted to delete customer with ID: {CustomerId} who has existing accounts", id);
                return BadRequest("Cannot delete customer with existing accounts. Please close all accounts first");
            }

            _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
            await _customerRepository.DeleteCustomerAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
            return StatusCode(500, "An error occurred while deleting the customer");
        }
    }

    [HttpGet("{id}/accounts")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetCustomerAccounts(Guid id)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Attempted to retrieve accounts for non-existent customer with ID: {CustomerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieving accounts for customer with ID: {CustomerId}", id);
            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(id);
            return Ok(_mapper.Map<IEnumerable<AccountDto>>(accounts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for customer with ID: {CustomerId}", id);
            return StatusCode(500, "An error occurred while retrieving customer accounts");
        }
    }
}
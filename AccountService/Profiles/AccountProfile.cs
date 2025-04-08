using AccountService.DTOs;
using AccountService.Models;
using AutoMapper;

namespace AccountService.Profiles;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        // Map from Entity to DTO
        CreateMap<Account, AccountDto>();
        CreateMap<Customer, CustomerDto>();
        
        // Map from DTO to Entity
        CreateMap<CreateAccountDto, Account>();
        CreateMap<CreateCustomerDto, Customer>();
    }
}
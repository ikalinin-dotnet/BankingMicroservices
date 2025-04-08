using AutoMapper;
using TransactionService.DTOs;
using TransactionService.Models;

namespace TransactionService.Profiles;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        // Map from Entity to DTO
        CreateMap<Transaction, TransactionDto>();
        
        // Map from DTO to Entity
        CreateMap<CreateTransactionDto, Transaction>();
    }
}
using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class AccountingMappingProfile : Profile
{
    public AccountingMappingProfile()
    {
        CreateMap<Account, AccountDrawerDto>().ReverseMap();
        CreateMap<CashTransaction, CashTransactionDto>().ReverseMap();
    }
}

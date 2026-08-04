using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class AccountingMappingProfile : Profile
{
    public AccountingMappingProfile()
    {
        CreateMap<Account, AccountDrawerDto>()
            .ForMember(dest => dest.PeriodStart, opt => opt.MapFrom(src => src.Period.Start))
            .ForMember(dest => dest.PeriodEnd, opt => opt.MapFrom(src => src.Period.End));
        CreateMap<CashTransaction, CashTransactionDto>();
    }
}

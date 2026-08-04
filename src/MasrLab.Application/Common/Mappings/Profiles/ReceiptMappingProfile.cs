using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class ReceiptMappingProfile : Profile
{
    public ReceiptMappingProfile()
    {
        CreateMap<Receipt, ReceiptDto>();
    }
}

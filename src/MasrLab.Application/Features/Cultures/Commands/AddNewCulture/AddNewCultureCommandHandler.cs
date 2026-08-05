using MediatR;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Commands.AddNewCulture;

public class AddNewCultureCommandHandler : IRequestHandler<AddNewCultureCommand, Unit>
{
    private readonly IRepository<Culture> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddNewCultureCommandHandler(IRepository<Culture> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddNewCultureCommand request, CancellationToken cancellationToken)
    {
        var culture = Culture.Create(request.VisitTestId);
        culture.Record(
            request.ColonyCount,
            request.OrganismA,
            request.OrganismB,
            request.OrganismC);
        culture.SampleType = request.SampleType;

        await _repository.AddAsync(culture, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

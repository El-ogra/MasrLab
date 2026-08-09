using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;

public class MarkSampleCollectedCommandHandler : IRequestHandler<MarkSampleCollectedCommand, Unit>
{
    private readonly IRepository<Sample> _sampleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkSampleCollectedCommandHandler(IRepository<Sample> sampleRepository, IUnitOfWork unitOfWork)
    {
        _sampleRepository = sampleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkSampleCollectedCommand request, CancellationToken cancellationToken)
    {
        var sample = await _sampleRepository.GetByIdAsync(request.SampleId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Sample), request.SampleId);

        if (request.IsCollected)
        {
            sample.Collect(request.PatientId);
        }
        else
        {
            sample.RevertCollection();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

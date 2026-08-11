using MediatR;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(User), request.Id);

        user.Username = request.Username;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = _passwordHasher.HashPassword(request.Password);
        }
        user.IsAdmin = request.IsAdmin;
        user.IsActive = request.IsActive;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

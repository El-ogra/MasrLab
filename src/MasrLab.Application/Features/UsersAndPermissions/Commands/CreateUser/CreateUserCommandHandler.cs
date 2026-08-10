using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = request.Username,
            Password = _passwordHasher.HashPassword(request.Password),
            IsAdmin = request.IsAdmin,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

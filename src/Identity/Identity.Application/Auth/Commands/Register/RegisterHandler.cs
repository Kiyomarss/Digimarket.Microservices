using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.UnitOfWork;
using Identity.Application.RepositoryContracts;
using Identity.Application.Security;
using Identity.Domain.Domain.Entities;

namespace Identity.Application.Auth.Commands.Register;

public class RegisterHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;

    public RegisterHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await _userRepository
                               .GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
            throw new ValidationException("Email already exists.", null);

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User(
                            Guid.NewGuid(),
                            email,
                            passwordHash);

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
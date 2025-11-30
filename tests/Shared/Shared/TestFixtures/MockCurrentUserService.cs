using BuildingBlocks.Services;
using Moq;

namespace Shared.TestFixtures;

public class MockCurrentUserService
{
    private readonly Mock<ICurrentUserService> _mock = new(MockBehavior.Loose);

    public MockCurrentUserService WithDefaultUser()
    {
        _mock.Setup(x => x.GetRequiredUserId())
             .ReturnsAsync(TestGuids.Guid3);

        return this;
    }

    public ICurrentUserService Build() => _mock.Object;
}
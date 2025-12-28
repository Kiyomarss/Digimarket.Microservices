using FluentAssertions;
using Shared.Tests.Types;
using Xunit;

namespace Shared.Tests.Types;

public class TypeSafeEnumTests
{
    [Fact]
    public void All_Should_Return_All_Defined_Instances()
    {
        // Act
        var statuses = Status.All;
        var roles = Role.All;

        // Assert
        statuses.Should().HaveCount(3)
                .And.Contain(Status.Active)
                .And.Contain(Status.Inactive)
                .And.Contain(Status.Pending);

        roles.Should().HaveCount(3)
              .And.Contain(Role.Admin)
              .And.Contain(Role.User)
              .And.Contain(Role.Guest);
    }

    [Fact]
    public void All_Should_Be_Cached_And_Return_Same_Reference_On_Subsequent_Calls()
    {
        // Act
        var firstCall = Status.All;
        var secondCall = Status.All;

        // Assert
        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }
    
    [Fact]
    public void All_Should_Be_Thread_Safe_And_Cached()
    {
        var results = Enumerable.Range(1, 100)
                                .AsParallel()
                                .Select(_ => Status.All)
                                .ToList();

        var first = results.First();
        results.All(r => ReferenceEquals(r, first)).Should().BeTrue();
    }

    [Fact]
    public void FromCode_Should_Return_Correct_Instance_Case_Insensitive_For_All_Defined_Values()
    {
        foreach (var expected in Status.All)
        {
            // Act & Assert برای حالت‌های مختلف casing
            Status.FromCode(expected.Code).Should().Be(expected);
            Status.FromCode(expected.Code.ToLowerInvariant()).Should().Be(expected);
            Status.FromCode(expected.Code.ToUpperInvariant()).Should().Be(expected);
            Status.FromCode(expected.Code.ToLowerInvariant().ToUpperInvariant()).Should().Be(expected); // ترکیب
        }

        foreach (var expected in Role.All)
        {
            Role.FromCode(expected.Code).Should().Be(expected);
            Role.FromCode(expected.Code.ToLowerInvariant()).Should().Be(expected);
            Role.FromCode(expected.Code.ToUpperInvariant()).Should().Be(expected);
        }
    }

    [Fact]
    public void FromCode_Should_Throw_ArgumentException_When_Code_Is_Invalid()
    {
        // Act & Assert
        FluentActions.
            Invoking(() => Status.FromCode("Invalid"))
                     .Should().Throw<ArgumentException>()
                     .WithMessage("کد نامعتبر: Invalid");

        FluentActions.Invoking(() => Role.FromCode("superuser"))
                     .Should().Throw<ArgumentException>()
                     .WithMessage("کد نامعتبر: superuser");
    }

    [Fact]
    public void TryFromCode_Should_Return_True_And_Correct_Instance_When_Code_Is_Valid_Case_Insensitive()
    {
        foreach (var expected in Status.All)
        {
            // Act
            var successLower = Status.TryFromCode(expected.Code.ToLowerInvariant(), out var resultLower);
            var successUpper = Status.TryFromCode(expected.Code.ToUpperInvariant(), out var resultUpper);

            // Assert
            successLower.Should().BeTrue();
            resultLower.Should().Be(expected);

            successUpper.Should().BeTrue();
            resultUpper.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryFromCode_Should_Return_False_And_Null_When_Code_Is_Invalid_Or_Empty(string? invalidCode)
    {
        // Act
        var success = Status.TryFromCode(invalidCode!, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void FromId_Should_Return_Correct_Instance_For_All_Defined_Values()
    {
        foreach (var expected in Status.All)
        {
            // Act
            var result = Status.FromId(expected.Id);

            // Assert
            result.Should().Be(expected);
        }

        foreach (var expected in Role.All)
        {
            var result = Role.FromId(expected.Id);
            result.Should().Be(expected);
        }
    }

    [Fact]
    public void FromId_Should_Throw_ArgumentException_When_Id_Is_Invalid()
    {
        // Act & Assert
        FluentActions.Invoking(() => Status.FromId(999))
                     .Should().Throw<ArgumentException>()
                     .WithMessage("شناسه نامعتبر: 999");

        FluentActions.Invoking(() => Role.FromId("invalid-role"))
                     .Should().Throw<ArgumentException>()
                     .WithMessage("شناسه نامعتبر: invalid-role");
    }

    [Fact]
    public void ToString_Should_Return_Title_For_All_Instances()
    {
        Status.Active.ToString().Should().Be("فعال");
        Status.Inactive.ToString().Should().Be("غیرفعال");
        Status.Pending.ToString().Should().Be("در انتظار");

        Role.Admin.ToString().Should().Be("مدیر");
        Role.User.ToString().Should().Be("کاربر");
        Role.Guest.ToString().Should().Be("مهمان");
    }

    [Fact]
    public void All_Instances_Should_Have_Consistent_Equality_And_HashCode()
    {
        foreach (var instance in Status.All)
        {
            var fromCode = Status.FromCode(instance.Code);
            var fromId = Status.FromId(instance.Id);

            // Equality
            instance.Should().Be(fromCode);
            instance.Should().Be(fromId);
            fromCode.Should().Be(fromId);

            // HashCode consistency
            instance.GetHashCode().Should().Be(fromCode.GetHashCode());
            instance.GetHashCode().Should().Be(fromId.GetHashCode());

            // Null handling
            ((Status?)null == instance).Should().BeFalse();
            (instance == (Status?)null).Should().BeFalse();
        }

        // Same for Role
        foreach (var instance in Role.All)
        {
            var fromCode = Role.FromCode(instance.Code);
            instance.Should().Be(fromCode);
            instance.GetHashCode().Should().Be(fromCode.GetHashCode());
        }
    }

    [Fact]
    public void GetHashCode_Should_Be_Consistent_With_Id()
    {
        Status.Active.GetHashCode().Should().Be(Status.Active.Id.GetHashCode());
        Status.Inactive.GetHashCode().Should().Be(Status.Inactive.Id.GetHashCode());

        Role.Admin.GetHashCode().Should().Be(Role.Admin.Id.GetHashCode());
        Role.Guest.GetHashCode().Should().Be(Role.Guest.Id.GetHashCode());
    }
}
using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.Idempotency;

[TestFixture(TestName = "Idempotent.Guid")]
[Category("Unit")]
[Category(nameof(Idempotent))]
public sealed class IdempotentGuidTests
{
    [Test]
    public void Guid_WhenCalledTwiceWithSameInputs_ReturnsSameValue()
    {
        // Arrange / Act
        var first = Idempotent.Guid<User>(0);
        var second = Idempotent.Guid<User>(0);

        // Assert
        first.Should().Be(second);
    }

    [Test]
    public void Guid_WhenCalledWithDifferentIndex_ReturnsDifferentValue()
    {
        // Arrange / Act
        var first = Idempotent.Guid<User>(0);
        var second = Idempotent.Guid<User>(1);

        // Assert
        first.Should().NotBe(second);
    }

    [Test]
    public void Guid_WhenCalledWithDifferentSeed_ReturnsDifferentValue()
    {
        // Arrange / Act
        var withDefaultSeed = Idempotent.Guid<User>(0);
        var withCustomSeed = Idempotent.Guid<User>(0, "custom");

        // Assert
        withDefaultSeed.Should().NotBe(withCustomSeed);
    }

    [Test]
    public void Guid_WhenCalledWithSameSeedAndDifferentTypes_ReturnsSameValue()
    {
        // Arrange / Act
        var user = Idempotent.Guid<User>(0, "shared-seed");
        var product = Idempotent.Guid<Product>(0, "shared-seed");

        // Assert
        user.Should().Be(product);
    }

    [Test]
    public void Guid_WhenCalledWithNoSeedAndDifferentTypes_ReturnsDifferentValue()
    {
        // Arrange / Act
        var user = Idempotent.Guid<User>(0);
        var product = Idempotent.Guid<Product>(0);

        // Assert
        user.Should().NotBe(product);
    }

    [Test]
    public void Guid_WhenCalledWithNullSeed_BehavesSameAsOmittingSeed()
    {
        // Arrange / Act
        var withNullSeed = Idempotent.Guid<User>(0, null);
        var withNoSeed = Idempotent.Guid<User>(0);

        // Assert
        withNullSeed.Should().Be(withNoSeed);
    }

    [Test]
    public void Guid_GenericAndNonGenericOverloads_ReturnSameValue()
    {
        // Arrange / Act
        var generic = Idempotent.Guid<User>(0);
        var nonGeneric = Idempotent.Guid(typeof(User), 0);

        // Assert
        generic.Should().Be(nonGeneric);
    }

    [Test]
    public void Guid_ReturnedValue_IsUuidV5()
    {
        // Arrange / Act
        var guid = Idempotent.Guid<User>(0);

        // Assert 
        guid.ToString()[14].Should().Be('5');
    }

    [Test]
    public void Guid_ReturnedValue_HasRfc4122Variant()
    {
        // Arrange / Act
        var guid = Idempotent.Guid<User>(0);

        // Assert 
        // RFC 4122 variant has upper 2 bits = 10, yielding hex digit 8, 9, a, or b
        var variantChar = guid.ToString()[19];
        new[] { '8', '9', 'a', 'b' }.Should().Contain(variantChar);
    }
}

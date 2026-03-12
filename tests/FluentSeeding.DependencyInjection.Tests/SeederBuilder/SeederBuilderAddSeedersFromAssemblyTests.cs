using System.Reflection;
using FluentAssertions;
using FluentSeeding.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FluentSeeding.DependencyInjection.Tests.SeederBuilder;

[TestFixture(TestName = "SeederBuilder.AddSeedersFromAssembly")]
[Category("Unit")]
[Category(nameof(global::FluentSeeding.DependencyInjection.SeederBuilder))]
public sealed class SeederBuilderAddSeedersFromAssemblyTests
{
    // Concrete seeders used as assembly-scan targets

    public class Widget
    {
        public int Id { get; set; }
    }

    public class Gadget
    {
        public int Id { get; set; }
    }

    public sealed class WidgetSeeder : EntitySeeder<Widget>
    {
        protected override void Configure(SeedBuilder<Widget> builder) => builder.Count(1);
    }

    public sealed class GadgetSeeder : EntitySeeder<Gadget>
    {
        protected override void Configure(SeedBuilder<Gadget> builder) => builder.Count(1);
    }

    public abstract class AbstractSeeder : EntitySeeder<Widget>
    {
        protected override void Configure(SeedBuilder<Widget> builder) => builder.Count(1);
    }

    private static ServiceProvider BuildProviderFromAssembly(Assembly assembly)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPersistenceLayer>());
        services.AddFluentSeeding(b => b.AddSeedersFromAssembly(assembly));
        return services.BuildServiceProvider();
    }

    [Test]
    public void AddSeedersFromAssembly_RegistersConcreteSeederTypes()
    {
        // Arrange & Act
        using var provider = BuildProviderFromAssembly(Assembly.GetExecutingAssembly());
        using var scope = provider.CreateScope();

        var widgetSeeder = scope.ServiceProvider.GetService<WidgetSeeder>();
        var gadgetSeeder = scope.ServiceProvider.GetService<GadgetSeeder>();

        // Assert
        widgetSeeder.Should().NotBeNull();
        gadgetSeeder.Should().NotBeNull();
    }

    [Test]
    public void AddSeedersFromAssembly_RegistersSeedersAsEntitySeederBase()
    {
        // Arrange & Act
        using var provider = BuildProviderFromAssembly(Assembly.GetExecutingAssembly());
        using var scope = provider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<EntitySeederBase>().ToList();

        // Assert
        seeders.Should().Contain(s => s is WidgetSeeder);
        seeders.Should().Contain(s => s is GadgetSeeder);
    }

    [Test]
    public void AddSeedersFromAssembly_SkipsAbstractTypes()
    {
        // Arrange & Act
        using var provider = BuildProviderFromAssembly(Assembly.GetExecutingAssembly());
        using var scope = provider.CreateScope();

        var abstractSeeder = scope.ServiceProvider.GetService<AbstractSeeder>();

        // Assert
        abstractSeeder.Should().BeNull();
    }

    [Test]
    public void AddSeedersFromAssembly_AbstractTypesDoNotAppearInEnumerable()
    {
        // Arrange & Act
        using var provider = BuildProviderFromAssembly(Assembly.GetExecutingAssembly());
        using var scope = provider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<EntitySeederBase>().ToList();

        // Assert
        seeders.Should().NotContain(s => s is AbstractSeeder);
    }

    [Test]
    public void AddSeedersFromAssembly_SameInstanceResolvedWithinScope()
    {
        // Arrange & Act
        using var provider = BuildProviderFromAssembly(Assembly.GetExecutingAssembly());
        using var scope = provider.CreateScope();

        var concrete = scope.ServiceProvider.GetRequiredService<WidgetSeeder>();
        var viaBase = scope.ServiceProvider.GetServices<EntitySeederBase>()
            .Single(s => s is WidgetSeeder);

        // Assert
        concrete.Should().BeSameAs(viaBase);
    }

    [Test]
    public void AddSeedersFromAssembly_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        global::FluentSeeding.DependencyInjection.SeederBuilder? capturedBuilder = null;
        global::FluentSeeding.DependencyInjection.SeederBuilder? returnedBuilder = null;

        // Act
        services.AddFluentSeeding(b =>
        {
            capturedBuilder = b;
            returnedBuilder = b.AddSeedersFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        returnedBuilder.Should().BeSameAs(capturedBuilder);
    }

    [Test]
    public void AddSeedersFromAssemblyContaining_UsesTypeAssembly()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPersistenceLayer>());

        // Act - use WidgetSeeder as the type marker; it lives in the test assembly
        services.AddFluentSeeding(b => b.AddSeedersFromAssemblyContaining<WidgetSeeder>());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var widgetSeeder = scope.ServiceProvider.GetService<WidgetSeeder>();

        // Assert
        widgetSeeder.Should().NotBeNull();
    }
}

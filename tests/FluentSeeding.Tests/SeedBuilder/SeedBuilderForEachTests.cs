using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedBuilder;

[TestFixture(TestName = "SeedBuilder.ForEach")]
[Category("Unit")]
[Category(nameof(SeedBuilder<>))]
public sealed class SeedBuilderForEachTests
{
    [Test]
    public void ForEach_ProducesOneEntityPerItem()
    {
        var roles = new[] { "Admin", "Editor", "Viewer" };

        var results = new SeedBuilder<User>()
            .RuleFor(u => u.Id).UseFactory(Guid.NewGuid)
            .ForEach(roles, (b, role) => b.RuleFor(u => u.Name).UseValue(role))
            .Build().ToList();

        results.Should().HaveCount(3);
        results.Select(u => u.Name).Should().BeEquivalentTo(roles);
    }

    [Test]
    public void ForEach_EmptyCollection_ProducesNoEntities()
    {
        var results = new SeedBuilder<User>()
            .ForEach(Array.Empty<string>(), (b, _) => b.RuleFor(u => u.Name).UseValue("x"))
            .Build();

        results.Should().BeEmpty();
    }

    [Test]
    public void ForEach_BaseRulesApplyToAllEntities()
    {
        var roles = new[] { "Admin", "Viewer" };

        var results = new SeedBuilder<User>()
            .RuleFor(u => u.Email).UseValue("shared@example.com")
            .ForEach(roles, (b, role) => b.RuleFor(u => u.Name).UseValue(role))
            .Build().ToList();

        results.Should().OnlyContain(u => u.Email == "shared@example.com");
        results.Select(u => u.Name).Should().BeEquivalentTo(roles);
    }

    [Test]
    public void ForEach_ItemRuleOverridesBaseRuleForSameProperty()
    {
        var names = new[] { "Alice", "Bob" };

        var results = new SeedBuilder<User>()
            .RuleFor(u => u.Name).UseValue("Default")
            .ForEach(names, (b, name) => b.RuleFor(u => u.Name).UseValue(name))
            .Build().ToList();

        results.Select(u => u.Name).Should().BeEquivalentTo(names);
    }

    [Test]
    public void ForEach_IndexPassedToRulesIncreasesAcrossItems()
    {
        var items = new[] { "a", "b", "c" };

        var results = new SeedBuilder<Purchase>()
            .ForEach(items, (b, _) => b.RuleFor(p => p.Quantity).UseFactory(i => i))
            .Build().ToList();

        results.Select(p => p.Quantity).Should().ContainInOrder(0, 1, 2);
    }

    [Test]
    public void ForEach_CountSettingIsIgnored()
    {
        var roles = new[] { "Admin", "Viewer" };

        var results = new SeedBuilder<User>()
            .Count(10)
            .ForEach(roles, (b, role) => b.RuleFor(u => u.Name).UseValue(role))
            .Build().ToList();

        results.Should().HaveCount(2);
    }

    [Test]
    public void ForEach_CalledTwice_ProducesCartesianProduct()
    {
        var tenants = new[] { "TenantA", "TenantB" };
        var roles   = new[] { "Admin", "Viewer" };

        var results = new SeedBuilder<User>()
            .ForEach(tenants, (b, tenant) => b.RuleFor(u => u.Email).UseValue(tenant))
            .ForEach(roles,   (b, role)   => b.RuleFor(u => u.Name).UseValue(role))
            .Build().ToList();

        results.Should().HaveCount(4); // 2 × 2
        results.Should().ContainSingle(u => u.Email == "TenantA" && u.Name == "Admin");
        results.Should().ContainSingle(u => u.Email == "TenantA" && u.Name == "Viewer");
        results.Should().ContainSingle(u => u.Email == "TenantB" && u.Name == "Admin");
        results.Should().ContainSingle(u => u.Email == "TenantB" && u.Name == "Viewer");
    }

    [Test]
    public void ForEach_CartesianProduct_BaseRulesApplyToAllCombinations()
    {
        var tenants = new[] { "T1", "T2" };
        var roles   = new[] { "R1", "R2" };

        var results = new SeedBuilder<User>()
            .RuleFor(u => u.Id).UseFactory(Guid.NewGuid)
            .ForEach(tenants, (b, tenant) => b.RuleFor(u => u.Email).UseValue(tenant))
            .ForEach(roles,   (b, role)   => b.RuleFor(u => u.Name).UseValue(role))
            .Build().ToList();

        results.Should().HaveCount(4);
        results.Should().OnlyContain(u => u.Id != Guid.Empty);
    }

    [Test]
    public void HasMany_WithSourceList_ProducesOneItemPerSource()
    {
        var quantities = new[] { 1, 2, 3 };

        var user = new SeedBuilder<User>()
            .RuleFor(u => u.Id).UseFactory(Guid.NewGuid)
            .RuleFor(u => u.Name).UseValue("Alice")
            .RuleFor(u => u.Email).UseValue("alice@example.com")
            .HasMany(u => u.Purchases, quantities, (b, qty) =>
                b.RuleFor(p => p.Id).UseFactory(Guid.NewGuid)
                 .RuleFor(p => p.Quantity).UseValue(qty))
            .Build().Single();

        user.Purchases.Should().HaveCount(3);
        user.Purchases.Select(p => p.Quantity).Should().BeEquivalentTo(quantities);
    }

    [Test]
    public void HasMany_WithForEach_ProducesOneItemPerSourceViaNestedBuilder()
    {
        var quantities = new[] { 5, 10 };

        var user = new SeedBuilder<User>()
            .RuleFor(u => u.Id).UseFactory(Guid.NewGuid)
            .RuleFor(u => u.Name).UseValue("Bob")
            .RuleFor(u => u.Email).UseValue("bob@example.com")
            .HasMany(u => u.Purchases, b => b
                .ForEach(quantities, (rb, qty) =>
                    rb.RuleFor(p => p.Id).UseFactory(Guid.NewGuid)
                      .RuleFor(p => p.Quantity).UseValue(qty)))
            .Build().Single();

        user.Purchases.Should().HaveCount(2);
        user.Purchases.Select(p => p.Quantity).Should().BeEquivalentTo(quantities);
    }
}

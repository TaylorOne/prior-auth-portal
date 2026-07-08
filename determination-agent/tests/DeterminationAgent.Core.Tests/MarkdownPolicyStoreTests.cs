using DeterminationAgent.Core.Policies;

namespace DeterminationAgent.Core.Tests;

public class MarkdownPolicyStoreTests
{
    private const string SamplePolicy = """
        # POLICY: TEST-POL-001
        payer: DemoHealth
        service: 12345
        indication: X10.0
        title: Test Policy

        ## C1: First criterion
        The member must have completed something specific.

        ## C2: Second criterion
        A value of at least 6 units must be documented.
        """;

    [Fact]
    public void Parse_ReadsMetadataAndClauses()
    {
        var policy = MarkdownPolicyStore.Parse(SamplePolicy);

        Assert.Equal("TEST-POL-001", policy.PolicyId);
        Assert.Equal("DemoHealth", policy.Payer);
        Assert.Equal("12345", policy.ServiceCode);
        Assert.Equal("X10.0", policy.IndicationCode);
        Assert.Equal("Test Policy", policy.Title);
        Assert.Equal(2, policy.Clauses.Count);
        Assert.Equal("C1", policy.Clauses[0].Id);
        Assert.Equal("First criterion", policy.Clauses[0].Title);
        Assert.Contains("completed something specific", policy.Clauses[0].Text);
        Assert.Equal("C2", policy.Clauses[1].Id);
    }

    [Fact]
    public void Parse_MissingPolicyHeader_Throws()
    {
        Assert.Throws<InvalidDataException>(() => MarkdownPolicyStore.Parse("## C1: orphan clause\nbody"));
    }

    [Fact]
    public async Task Resolve_MatchesServiceAndIndication()
    {
        var store = new MarkdownPolicyStore([MarkdownPolicyStore.Parse(SamplePolicy)]);

        Assert.NotNull(await store.ResolveAsync("12345", "X10.0"));
        Assert.Null(await store.ResolveAsync("12345", "Y99.9"));
        Assert.Null(await store.ResolveAsync("99999", "X10.0"));
    }
}

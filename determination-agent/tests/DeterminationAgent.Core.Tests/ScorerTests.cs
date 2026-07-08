using DeterminationAgent.Contracts;
using DeterminationAgent.Eval;

namespace DeterminationAgent.Core.Tests;

public class ScorerTests
{
    [Fact]
    public void ConfusionMatrix_ComputesPrecisionAndRecall()
    {
        var matrix = new ConfusionMatrix();
        matrix.Add(CriterionStatus.Met, CriterionStatus.Met);
        matrix.Add(CriterionStatus.NotMet, CriterionStatus.Met);
        matrix.Add(CriterionStatus.NotMet, CriterionStatus.NotMet);
        matrix.Add(CriterionStatus.InsufficientEvidence, CriterionStatus.NotMet);

        Assert.Equal(4, matrix.Total);
        Assert.Equal(2, matrix.Correct);

        // Predicted not-met twice, one of which was truly not-met.
        Assert.Equal(0.5, matrix.Precision(CriterionStatus.NotMet));
        // Truly not-met twice, one of which was predicted not-met.
        Assert.Equal(0.5, matrix.Recall(CriterionStatus.NotMet));
        // No insufficient-evidence predictions at all.
        Assert.True(double.IsNaN(matrix.Precision(CriterionStatus.InsufficientEvidence)));
        Assert.Equal(0, matrix.Recall(CriterionStatus.InsufficientEvidence));
    }

    [Fact]
    public void ToKebab_MatchesContractWireFormat()
    {
        Assert.Equal("pend-for-info", Scorer.ToKebab(Recommendation.PendForInfo));
        Assert.Equal("not-met", Scorer.ToKebab(CriterionStatus.NotMet));
        Assert.Equal("insufficient-evidence", Scorer.ToKebab(CriterionStatus.InsufficientEvidence));
        Assert.Equal("triage-only", Scorer.ToKebab(ModelPath.TriageOnly));
    }
}

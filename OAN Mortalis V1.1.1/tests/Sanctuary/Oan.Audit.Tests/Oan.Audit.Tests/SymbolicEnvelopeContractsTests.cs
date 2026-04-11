namespace Oan.Audit.Tests;

using System.Text.Json;
using Oan.Common;

public sealed class SymbolicEnvelopeContractsTests
{
    [Fact]
    public void SymbolicEnvelope_RoundTrips_DoctrinalPassportFields()
    {
        var envelope = new SymbolicEnvelope(
            Origin: "sli-lisp://compass/compass-update",
            Family: new SymbolicProductFamily("PostureProjection"),
            ProductClass: SymbolicProductClass.CandidateProduct,
            Intent: new SymbolicIntent("bounded-read"),
            Admissibility: AdmissibilityStatus.Pending,
            ContradictionState: ContradictionState.None,
            MaterializationEligibility: MaterializationEligibility.Restricted,
            PersistenceEligibility: PersistenceEligibility.AuditOnly,
            TraceId: "trace://session-a/passport-001");

        var json = JsonSerializer.Serialize(envelope);
        var roundTrip = JsonSerializer.Deserialize<SymbolicEnvelope>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(envelope.Origin, roundTrip!.Origin);
        Assert.Equal("PostureProjection", roundTrip.Family.Value);
        Assert.Equal(SymbolicProductClass.CandidateProduct, roundTrip.ProductClass);
        Assert.Equal("bounded-read", roundTrip.Intent.Value);
        Assert.Equal(AdmissibilityStatus.Pending, roundTrip.Admissibility);
        Assert.Equal(ContradictionState.None, roundTrip.ContradictionState);
        Assert.Equal(MaterializationEligibility.Restricted, roundTrip.MaterializationEligibility);
        Assert.Equal(PersistenceEligibility.AuditOnly, roundTrip.PersistenceEligibility);
        Assert.Equal(envelope.TraceId, roundTrip.TraceId);
    }

    [Fact]
    public void SymbolicEnvelope_PassportSurface_Remains_DoctrinallyBounded()
    {
        var propertyNames = typeof(SymbolicEnvelope)
            .GetProperties()
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
        [
            "Admissibility",
            "ContradictionState",
            "Family",
            "Intent",
            "MaterializationEligibility",
            "Origin",
            "PersistenceEligibility",
            "ProductClass",
            "TraceId"
        ],
        propertyNames);
    }

    [Fact]
    public void SymbolicProductClass_And_Decision_Lattices_Are_Finite_And_Aligned()
    {
        Assert.Equal(
        [
            "ReadProduct",
            "CandidateProduct",
            "DirectiveProduct",
            "CollapseProduct"
        ],
        Enum.GetNames<SymbolicProductClass>());

        Assert.Equal(
        [
            "Accept",
            "Transform",
            "Defer",
            "Refuse",
            "Collapse"
        ],
        Enum.GetNames<MembraneDecision>());
    }

    [Fact]
    public void SymbolicEnvelope_StateEnums_Match_MirroredMembraneLaw()
    {
        Assert.Equal(
        [
            "Pending",
            "Admissible",
            "Refused"
        ],
        Enum.GetNames<AdmissibilityStatus>());

        Assert.Equal(
        [
            "None",
            "Soft",
            "Hard"
        ],
        Enum.GetNames<ContradictionState>());

        Assert.Equal(
        [
            "No",
            "Restricted",
            "Yes"
        ],
        Enum.GetNames<MaterializationEligibility>());

        Assert.Equal(
        [
            "Never",
            "AuditOnly",
            "Promotable"
        ],
        Enum.GetNames<PersistenceEligibility>());
    }

    [Fact]
    public void SymbolicProductFamily_And_Intent_Remain_OpenText_Surfaces()
    {
        var family = new SymbolicProductFamily("PostureProjection");
        var intent = new SymbolicIntent("bounded-read");

        Assert.Equal("PostureProjection", family.Value);
        Assert.Equal("bounded-read", intent.Value);
    }
}

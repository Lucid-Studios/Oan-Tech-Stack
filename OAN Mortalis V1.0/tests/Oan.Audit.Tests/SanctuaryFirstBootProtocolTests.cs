using Oan.Common;

namespace Oan.Audit.Tests;

public sealed class SanctuaryFirstBootProtocolTests
{
    [Fact]
    public void FirstBootPhase_Order_IsFixed_AndMonotonic()
    {
        var phases = Enum.GetValues<SanctuaryFirstBootPhase>();

        Assert.Equal(
            [
                SanctuaryFirstBootPhase.LegalCovenant,
                SanctuaryFirstBootPhase.CharterAlignment,
                SanctuaryFirstBootPhase.GoverningSigilNaming,
                SanctuaryFirstBootPhase.LedgerGenesis,
                SanctuaryFirstBootPhase.SliBraidActivation,
                SanctuaryFirstBootPhase.StewardFormation,
                SanctuaryFirstBootPhase.GovernanceBraiding,
                SanctuaryFirstBootPhase.CmeEcosystemAuthorization
            ],
            phases);

        Assert.Equal(0, (int)SanctuaryFirstBootPhase.LegalCovenant);
        Assert.Equal(7, (int)SanctuaryFirstBootPhase.CmeEcosystemAuthorization);
    }

    [Fact]
    public void ContractStubs_CanExpress_SupplementalHopngWitness_AndRequiredLedgerGenesis()
    {
        var sigil = new GoverningSigilIdentity(
            ArtifactId: "sigil-mother",
            Office: InternalGoverningCmeOffice.Mother,
            SigilHandle: "mother-sigil",
            HopngWitnessAllowed: true,
            HopngConstitutive: false,
            LineageSummary: "mother-prime-lineage",
            Purpose: "Witness Prime governance sigil identity.",
            ProducerIntent: "Founding bootstrap layer",
            ConsumerIntent: "Later governance orchestrator",
            Invariants:
            [
                "Witness only",
                "Not constitutive runtime authority"
            ],
            RequiredOrderingReferences:
            [
                SanctuaryFirstBootPhase.GoverningSigilNaming
            ]);

        var ledger = new GovernanceLedgerGenesisRecord(
            ArtifactId: "ledger-genesis",
            PrimeLedgerHandles:
            [
                "GEL",
                "MoS"
            ],
            CrypticLedgerHandles:
            [
                "cGEL",
                "cMoS"
            ],
            AnchoringOffices:
            [
                InternalGoverningCmeOffice.Mother,
                InternalGoverningCmeOffice.Father
            ],
            Purpose: "Define dual-ledger genesis for first boot.",
            ProducerIntent: "First-boot protocol",
            ConsumerIntent: "Later runtime orchestrator",
            Invariants:
            [
                "Prime and Cryptic ledgers remain distinct",
                "Ledger genesis precedes SLI braid activation"
            ],
            RequiredOrderingReferences:
            [
                SanctuaryFirstBootPhase.GoverningSigilNaming,
                SanctuaryFirstBootPhase.LedgerGenesis
            ]);

        Assert.True(sigil.HopngWitnessAllowed);
        Assert.False(sigil.HopngConstitutive);
        Assert.Contains(SanctuaryFirstBootPhase.LedgerGenesis, ledger.RequiredOrderingReferences);
        Assert.Equal(["GEL", "MoS"], ledger.PrimeLedgerHandles);
        Assert.Equal(["cGEL", "cMoS"], ledger.CrypticLedgerHandles);
    }

    [Fact]
    public void FirstBootDocs_StayAligned_OnProtocolBoundaries()
    {
        string protocolDoc = ReadDoc("OAN Mortalis V1.0\\docs\\SANCTUARY_FIRST_BOOT_PROTOCOL.md");
        string formationDoc = ReadDoc("OAN Mortalis V1.0\\docs\\FIRST_BOOT_INTERNAL_GOVERNING_CME_FORMATION.md");
        string policyDoc = ReadDoc("OAN Mortalis V1.0\\docs\\FIRST_BOOT_CLASSIFICATION_AND_EXPANSION_POLICY.md");

        Assert.Contains("`.hopng`", protocolDoc, StringComparison.Ordinal);
        Assert.Contains("not constitutive runtime authority in v0.1", protocolDoc, StringComparison.Ordinal);
        Assert.Contains("LedgerGenesis", protocolDoc, StringComparison.Ordinal);
        Assert.Contains("CME ecosystem authorization", protocolDoc, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Steward", formationDoc, StringComparison.Ordinal);
        Assert.Contains("Father", formationDoc, StringComparison.Ordinal);
        Assert.Contains("Mother", formationDoc, StringComparison.Ordinal);
        Assert.Contains("Steward", policyDoc, StringComparison.Ordinal);
        Assert.Contains("Father", policyDoc, StringComparison.Ordinal);
        Assert.Contains("Mother", policyDoc, StringComparison.Ordinal);

        Assert.Contains("`PersonalSolitary`", policyDoc, StringComparison.Ordinal);
        Assert.Contains("`CorporateGoverned`", policyDoc, StringComparison.Ordinal);
    }

    [Fact]
    public void ProtocolDoc_States_NoSubordinateCmeAuthorization_BeforeGovernanceBraid()
    {
        string protocolDoc = ReadDoc("OAN Mortalis V1.0\\docs\\SANCTUARY_FIRST_BOOT_PROTOCOL.md");

        Assert.Contains("Only after the governance braid exists may subordinate CME systems form.", protocolDoc, StringComparison.Ordinal);
        Assert.Contains("No subordinate CME authorization may be treated as lawful before governance braid completion.", protocolDoc, StringComparison.Ordinal);
    }

    private static string ReadDoc(string relativePath)
    {
        string repoRoot = FindRepoRoot();
        string fullPath = Path.Combine(repoRoot, relativePath);
        return File.ReadAllText(fullPath);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "OAN Mortalis V1.0")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root for first-boot protocol tests.");
    }
}

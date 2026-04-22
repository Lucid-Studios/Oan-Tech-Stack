using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuarySelfGelLegalOrientationContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void SelfGel_Legal_Orientation_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated,
                SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated,
                SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid,
                SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound,
                SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface
            ],
            SanctuarySelfGelLegalOrientationAtlas.All.Select(static item => item.Predicate).ToArray());

        var governingBody = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated);
        Assert.Equal("selfgel.governing-body-seated", governingBody.Handle);
        Assert.Equal("cme-presides-under-a-specific-legal-body", governingBody.Meaning);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.LegalName,
                SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                SanctuaryLegalEvidenceKind.FilingReceipt
            ],
            governingBody.RequiredEvidenceKinds);
        Assert.Equal(
            [
                "legal-body-continuity",
                "jurisdictional-seat",
                "governor-lineage",
                "lawful-authority-boundary"
            ],
            governingBody.PreservedInvariants);
        Assert.Equal(
            [
                "legal-body-unbound",
                "registration-surface-missing",
                "entity-seat-ambiguous"
            ],
            governingBody.FailureStates);
        Assert.Equal("placeholder-contract-only", governingBody.OperationalStatus);

        var jurisdiction = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated);
        Assert.Equal("selfgel.jurisdiction-seated", jurisdiction.Handle);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.Jurisdiction,
                SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                SanctuaryLegalEvidenceKind.Ubi,
                SanctuaryLegalEvidenceKind.FilingReceipt
            ],
            jurisdiction.RequiredEvidenceKinds);
        Assert.Equal(
            [
                "jurisdiction-unbound",
                "regional-seat-missing",
                "jurisdiction-conflict"
            ],
            jurisdiction.FailureStates);

        var lineage = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid);
        Assert.Equal("selfgel.entity-lineage-valid", lineage.Handle);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.LegalName,
                SanctuaryLegalEvidenceKind.Ubi,
                SanctuaryLegalEvidenceKind.Ein,
                SanctuaryLegalEvidenceKind.FilingReceipt,
                SanctuaryLegalEvidenceKind.IrsNotice
            ],
            lineage.RequiredEvidenceKinds);
        Assert.Equal(
            [
                "lineage-break",
                "identifier-mismatch",
                "receipt-chain-incomplete"
            ],
            lineage.FailureStates);

        var governor = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound);
        Assert.Equal("selfgel.governor-bound", governor.Handle);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.GovernorSurface,
                SanctuaryLegalEvidenceKind.FilingReceipt
            ],
            governor.RequiredEvidenceKinds);
        Assert.Equal(
            [
                "governor-surface-missing",
                "governance-role-ambiguous",
                "unbound-governance-claim"
            ],
            governor.FailureStates);

        var lawfulOperatingSurface = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface);
        Assert.Equal("selfgel.lawful-operating-surface", lawfulOperatingSurface.Handle);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.EntityForm,
                SanctuaryLegalEvidenceKind.ReportingPosture,
                SanctuaryLegalEvidenceKind.Jurisdiction
            ],
            lawfulOperatingSurface.RequiredEvidenceKinds);
        Assert.Equal(
            [
                "entity-form-unbound",
                "reporting-posture-unclear",
                "authority-surface-overclaimed"
            ],
            lawfulOperatingSurface.FailureStates);
    }

    [Fact]
    public void SelfGel_Legal_Orientation_Install_Bridge_And_Protected_Data_Profile_Are_Exact()
    {
        var evidencePool = SanctuarySelfGelLegalOrientationAtlas.LegalEvidencePool;
        Assert.Equal("selfgel.legal-evidence-pool.v0", evidencePool.Handle);
        Assert.Equal(
            [
                SanctuaryLegalEvidenceKind.LegalName,
                SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                SanctuaryLegalEvidenceKind.Jurisdiction,
                SanctuaryLegalEvidenceKind.Ubi,
                SanctuaryLegalEvidenceKind.Ein,
                SanctuaryLegalEvidenceKind.FilingReceipt,
                SanctuaryLegalEvidenceKind.IrsNotice,
                SanctuaryLegalEvidenceKind.GovernorSurface,
                SanctuaryLegalEvidenceKind.EntityForm,
                SanctuaryLegalEvidenceKind.ReportingPosture
            ],
            evidencePool.EvidenceFields);
        Assert.Equal("mixed-legal-surfaces-local-only", evidencePool.EvidenceMixtureClass);
        Assert.Equal("organization-controlled-local-only-packet", evidencePool.StoragePolicy);
        Assert.Equal("placeholder-contract-only", evidencePool.OperationalStatus);

        var installPacket = SanctuarySelfGelLegalOrientationAtlas.InstallPacket;
        Assert.Equal("selfgel.legal-orientation-install.packet.v0", installPacket.Handle);
        Assert.Equal(
            [
                "legal_name",
                "state_registration_surface",
                "jurisdiction",
                "ubi",
                "ein",
                "filing_receipt_handles",
                "irs_notice_handle",
                "governor_surface",
                "entity_form",
                "reporting_posture",
                "evidence_mixture_class"
            ],
            installPacket.EvidencePoolFieldNames);
        Assert.Equal(
            [
                SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated,
                SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated,
                SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid,
                SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound,
                SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface
            ],
            installPacket.RootPredicateFamily);
        Assert.Equal(
            [
                "charitable-trust-seat",
                "bonded-operator-seat",
                "b-corp-transition-seat",
                "authority-delegation",
                "cryptographic-custody"
            ],
            installPacket.DeferredBranchFamilies);
        Assert.Equal("gel-owned-root-family-first-run-bridge-doctrine-only", installPacket.ProjectionBoundary);
        Assert.Equal("tracked-template-with-ignored-local-packet", installPacket.StoragePolicy);
        Assert.Equal("placeholder-contract-only", installPacket.OperationalStatus);

        var protectedProfile = SanctuaryProtectedDataAtlas.LocalLegalOrientationEvidence;
        Assert.Equal(SanctuaryProtectedDataClass.OrganizationControlledData, protectedProfile.DataClass);
        Assert.Equal(SanctuaryOwnershipSurface.OrganizationControlled, protectedProfile.OwnershipSurface);
        Assert.Equal(SanctuaryTelemetryClass.OperationalAudit, protectedProfile.TelemetryClass);
        Assert.True(protectedProfile.LocalOnlyByDefault);
        Assert.True(protectedProfile.ExplicitRemoteActivationRequired);
        Assert.Equal(
            [
                SanctuaryContractLayer.LocalBindingLicense,
                SanctuaryContractLayer.ProtectedDataAddendum,
                SanctuaryContractLayer.LocalizationResidencySchedule
            ],
            protectedProfile.GoverningLayers);
    }

    [Fact]
    public void SelfGel_Legal_Orientation_Contracts_Serialize_Stably()
    {
        var predicate = SanctuarySelfGelLegalOrientationAtlas.Get(SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid);
        var serializedPredicate = JsonSerializer.Serialize(predicate);
        var deserializedPredicate = JsonSerializer.Deserialize<SanctuarySelfGelLegalOrientationPredicateDefinition>(serializedPredicate, JsonOptions);

        Assert.NotNull(deserializedPredicate);
        Assert.Equal(predicate.Predicate, deserializedPredicate!.Predicate);
        Assert.Equal(predicate.Handle, deserializedPredicate.Handle);
        Assert.Equal(predicate.RequiredEvidenceKinds, deserializedPredicate.RequiredEvidenceKinds);
        Assert.Equal(predicate.FailureStates, deserializedPredicate.FailureStates);

        var evidencePool = SanctuarySelfGelLegalOrientationAtlas.LegalEvidencePool;
        var serializedPool = JsonSerializer.Serialize(evidencePool);
        var deserializedPool = JsonSerializer.Deserialize<SanctuaryLegalEvidencePoolDefinition>(serializedPool, JsonOptions);

        Assert.NotNull(deserializedPool);
        Assert.Equal(evidencePool.Handle, deserializedPool!.Handle);
        Assert.Equal(evidencePool.EvidenceFields, deserializedPool.EvidenceFields);
        Assert.Equal(evidencePool.StoragePolicy, deserializedPool.StoragePolicy);

        var installPacket = SanctuarySelfGelLegalOrientationAtlas.InstallPacket;
        var serializedPacket = JsonSerializer.Serialize(installPacket);
        var deserializedPacket = JsonSerializer.Deserialize<SanctuaryLegalOrientationInstallPacketDefinition>(serializedPacket, JsonOptions);

        Assert.NotNull(deserializedPacket);
        Assert.Equal(installPacket.Handle, deserializedPacket!.Handle);
        Assert.Equal(installPacket.EvidencePoolFieldNames, deserializedPacket.EvidencePoolFieldNames);
        Assert.Equal(installPacket.RootPredicateFamily, deserializedPacket.RootPredicateFamily);
        Assert.Equal(installPacket.DeferredBranchFamilies, deserializedPacket.DeferredBranchFamilies);
    }

    [Fact]
    public void SelfGel_Legal_Orientation_Docs_Template_And_Boundaries_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var familyNotePath = Path.Combine(lineRoot, "docs", "SELFGEL_LEGAL_ORIENTATION_PREDICATE_FAMILY_NOTE.md");
        var bridgeNotePath = Path.Combine(lineRoot, "docs", "SELFGEL_LEGAL_ORIENTATION_INSTALL_VALIDATOR_BRIDGE_NOTE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var actualizationPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_ACTUALIZATION_SEAL_PACKET.md");
        var ontologyBridgePath = Path.Combine(lineRoot, "docs", "SANCTUARY_BOOT_FIRST_RUN_ONTOLOGY_BRIDGE.md");
        var templatePath = Path.Combine(lineRoot, "docs", "templates", "legal_orientation_install.packet.template.json");
        var formationContractsPath = Path.Combine(lineRoot, "src", "TechStack", "GEL", "GEL.Contracts", "Sanctuary", "SanctuaryFormationPredicateContracts.cs");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var familyNoteText = File.ReadAllText(familyNotePath);
        var bridgeNoteText = File.ReadAllText(bridgeNotePath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var actualizationText = File.ReadAllText(actualizationPath);
        var ontologyBridgeText = File.ReadAllText(ontologyBridgePath);
        var templateText = File.ReadAllText(templatePath);
        var formationContractsText = File.ReadAllText(formationContractsPath);

        Assert.Contains("SELFGEL_LEGAL_ORIENTATION_PREDICATE_FAMILY_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SELFGEL_LEGAL_ORIENTATION_INSTALL_VALIDATOR_BRIDGE_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("templates/legal_orientation_install.packet.template.json", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("selfgel-legal-orientation-predicate-family-note: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("selfgel-legal-orientation-install-validator-bridge-note: frame-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("GEL owns this root predicate family.", familyNoteText, StringComparison.Ordinal);
        Assert.Contains("Lucid is the first local substantiating instance only.", familyNoteText, StringComparison.Ordinal);
        Assert.Contains("real legal identifiers remain local/private only", familyNoteText, StringComparison.Ordinal);
        Assert.Contains("not cryptographic seed entropy", familyNoteText, StringComparison.Ordinal);
        Assert.Contains("Those remain downstream branches.", familyNoteText, StringComparison.Ordinal);

        Assert.Contains("the bridge into first-run is doctrine-only", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("no first-run state-ladder change is authorized", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("Real legal identifiers remain local/private only.", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("it is not cryptographic seed entropy", bridgeNoteText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("charitable-trust-seat", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("bonded-operator-seat", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("b-corp-transition-seat", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("authority-delegation", bridgeNoteText, StringComparison.Ordinal);
        Assert.Contains("cryptographic-custody", bridgeNoteText, StringComparison.Ordinal);

        Assert.Contains("primitive `SelfGEL` may later inherit a `GEL`-owned legal", firstRunText, StringComparison.Ordinal);
        Assert.Contains("does not alter the first-run state ladder", firstRunText, StringComparison.Ordinal);
        Assert.Contains("not cryptographic seed entropy", firstRunText, StringComparison.Ordinal);

        Assert.Contains("predicate family as future-facing orientation truth", actualizationText, StringComparison.Ordinal);
        Assert.Contains("does not change the first-run packet fields", actualizationText, StringComparison.Ordinal);
        Assert.Contains("Real legal identifiers remain local/private only", actualizationText, StringComparison.Ordinal);

        Assert.Contains("GEL owns that family", ontologyBridgeText, StringComparison.Ordinal);
        Assert.Contains("first-run references it doctrine-only", ontologyBridgeText, StringComparison.Ordinal);
        Assert.Contains("the legal-evidence bridge remains local/private only", ontologyBridgeText, StringComparison.Ordinal);

        Assert.Contains("CareerContinuityHolder", formationContractsText, StringComparison.Ordinal);
        Assert.DoesNotContain("governing-body-seated", formationContractsText, StringComparison.Ordinal);
        Assert.DoesNotContain("jurisdiction-seated", formationContractsText, StringComparison.Ordinal);
        Assert.DoesNotContain("entity-lineage-valid", formationContractsText, StringComparison.Ordinal);

        using var templateDocument = JsonDocument.Parse(templateText);
        var root = templateDocument.RootElement;
        Assert.Equal("logical-source://legal-name", root.GetProperty("legal_name").GetString());
        Assert.Equal("logical-source://state-registration-surface", root.GetProperty("state_registration_surface").GetString());
        Assert.Equal("logical-source://jurisdiction", root.GetProperty("jurisdiction").GetString());
        Assert.Equal("logical-source://ubi", root.GetProperty("ubi").GetString());
        Assert.Equal("logical-source://ein", root.GetProperty("ein").GetString());
        Assert.Equal("logical-source://irs-notice", root.GetProperty("irs_notice_handle").GetString());
        Assert.Equal("logical-source://governor-surface", root.GetProperty("governor_surface").GetString());
        Assert.Equal("logical-source://entity-form", root.GetProperty("entity_form").GetString());
        Assert.Equal("logical-source://reporting-posture", root.GetProperty("reporting_posture").GetString());
        Assert.Equal("mixed-legal-surfaces-local-only", root.GetProperty("evidence_mixture_class").GetString());
        Assert.Equal("logical-source://filing-receipt", root.GetProperty("filing_receipt_handles")[0].GetString());

        Assert.DoesNotContain("LUCID TECHNOLOGIES", templateText, StringComparison.Ordinal);
        Assert.DoesNotContain("605 894 704", templateText, StringComparison.Ordinal);
        Assert.DoesNotContain("82-4783040", templateText, StringComparison.Ordinal);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Mortalis V1.1.1", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the V1.1.1 line root.");
    }
}

namespace San.Common;

public enum CommunicativeCarrierClassKind
{
    DecisionBearing = 0,
    ChosenPathBearing = 1,
    DistributedIntegrationBearing = 2,
    DeferredEdgeBearing = 3,
    RedundantEchoCandidate = 4
}

public enum FilamentResolutionTargetKind
{
    OE = 0,
    SelfGEL = 1,
    CGoA = 2,
    WorkingSurfaceOnly = 3
}

public enum AntiEchoDispositionKind
{
    Preserve = 0,
    DeduplicateCarrier = 1,
    ThinAsEcho = 2,
    Defer = 3
}

public sealed record CommunicativeCarrierPacket(
    string CarrierHandle,
    string SourceRetainedWholeRecordHandle,
    PrimeRetainedWholeKind SourceRetainedWholeKind,
    CommunicativeCarrierClassKind CarrierClass,
    bool PreservedDistinctionVisible,
    bool SelfEchoDetected,
    bool GelSubstanceConsumptionRequested,
    IReadOnlyList<string> PointerHandles,
    IReadOnlyList<string> SymbolicCarrierHandles,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> PreservedResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredResidues,
    IReadOnlyList<string> Notes,
    DateTimeOffset TimestampUtc);

public sealed record CommunicativeFilamentResolutionReceipt(
    string ReceiptHandle,
    string CarrierHandle,
    string SourceRetainedWholeRecordHandle,
    PrimeRetainedWholeKind SourceRetainedWholeKind,
    CommunicativeCarrierClassKind CarrierClass,
    FilamentResolutionTargetKind ResolutionTarget,
    AntiEchoDispositionKind AntiEchoDisposition,
    bool PreservedDistinctionVisible,
    bool GelSubstanceConsumptionRequested,
    bool GelSubstanceConsumptionRefused,
    bool CarrierTransportOnly,
    IReadOnlyList<string> PointerHandles,
    IReadOnlyList<string> SymbolicCarrierHandles,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> PreservedResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class CommunicativeFilamentEvaluator
{
    public static CommunicativeFilamentResolutionReceipt Evaluate(
        CommunicativeCarrierPacket carrier,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(carrier);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var normalizedPointers = NormalizeTokens(carrier.PointerHandles);
        var normalizedCarriers = NormalizeTokens(carrier.SymbolicCarrierHandles);
        var preservedResidues = NormalizeResidues(carrier.PreservedResidues);
        var deferredResidues = NormalizeResidues(carrier.DeferredResidues);
        var resolutionTarget = DetermineResolutionTarget(carrier);
        var antiEchoDisposition = DetermineAntiEchoDisposition(carrier);
        var insufficientRetainedBasis = HasInsufficientRetainedBasis(carrier);

        return new CommunicativeFilamentResolutionReceipt(
            ReceiptHandle: receiptHandle,
            CarrierHandle: carrier.CarrierHandle,
            SourceRetainedWholeRecordHandle: carrier.SourceRetainedWholeRecordHandle,
            SourceRetainedWholeKind: carrier.SourceRetainedWholeKind,
            CarrierClass: carrier.CarrierClass,
            ResolutionTarget: resolutionTarget,
            AntiEchoDisposition: antiEchoDisposition,
            PreservedDistinctionVisible: carrier.PreservedDistinctionVisible,
            GelSubstanceConsumptionRequested: carrier.GelSubstanceConsumptionRequested,
            GelSubstanceConsumptionRefused: carrier.GelSubstanceConsumptionRequested,
            CarrierTransportOnly: true,
            PointerHandles: normalizedPointers,
            SymbolicCarrierHandles: normalizedCarriers,
            PreservedResidues: preservedResidues,
            DeferredResidues: deferredResidues,
            ConstraintCodes: DetermineConstraintCodes(
                carrier,
                resolutionTarget,
                antiEchoDisposition,
                insufficientRetainedBasis,
                deferredResidues.Count),
            ReasonCode: DetermineReasonCode(carrier, resolutionTarget, antiEchoDisposition),
            LawfulBasis: DetermineLawfulBasis(resolutionTarget, antiEchoDisposition),
            TimestampUtc: carrier.TimestampUtc);
    }

    public static FilamentResolutionTargetKind DetermineResolutionTarget(
        CommunicativeCarrierPacket carrier)
    {
        ArgumentNullException.ThrowIfNull(carrier);

        if (carrier.GelSubstanceConsumptionRequested ||
            !carrier.PreservedDistinctionVisible ||
            HasInsufficientRetainedBasis(carrier) ||
            carrier.CarrierClass == CommunicativeCarrierClassKind.RedundantEchoCandidate)
        {
            return FilamentResolutionTargetKind.WorkingSurfaceOnly;
        }

        return carrier.CarrierClass switch
        {
            CommunicativeCarrierClassKind.DecisionBearing => FilamentResolutionTargetKind.OE,
            CommunicativeCarrierClassKind.ChosenPathBearing => FilamentResolutionTargetKind.SelfGEL,
            _ => FilamentResolutionTargetKind.CGoA
        };
    }

    public static AntiEchoDispositionKind DetermineAntiEchoDisposition(
        CommunicativeCarrierPacket carrier)
    {
        ArgumentNullException.ThrowIfNull(carrier);

        if (carrier.GelSubstanceConsumptionRequested ||
            !carrier.PreservedDistinctionVisible ||
            HasInsufficientRetainedBasis(carrier) ||
            carrier.SourceRetainedWholeKind == PrimeRetainedWholeKind.StillDeferred ||
            carrier.CarrierClass == CommunicativeCarrierClassKind.DeferredEdgeBearing)
        {
            return AntiEchoDispositionKind.Defer;
        }

        if (carrier.CarrierClass == CommunicativeCarrierClassKind.RedundantEchoCandidate)
        {
            return AntiEchoDispositionKind.ThinAsEcho;
        }

        if (carrier.SelfEchoDetected)
        {
            return AntiEchoDispositionKind.DeduplicateCarrier;
        }

        return AntiEchoDispositionKind.Preserve;
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return [];
        }

        return tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> NormalizeResidues(
        IReadOnlyList<PrimeMembraneProjectedLineResidue>? residues)
    {
        if (residues is null || residues.Count == 0)
        {
            return [];
        }

        return residues
            .Where(static residue =>
                residue is not null &&
                !string.IsNullOrWhiteSpace(residue.LineHandle) &&
                !string.IsNullOrWhiteSpace(residue.SourceSurfaceHandle))
            .GroupBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool HasInsufficientRetainedBasis(
        CommunicativeCarrierPacket carrier)
    {
        return carrier.CarrierClass switch
        {
            CommunicativeCarrierClassKind.DecisionBearing =>
                carrier.SourceRetainedWholeKind is not (
                    PrimeRetainedWholeKind.RetainedWholeUnclosed or
                    PrimeRetainedWholeKind.ClosureCandidate),
            CommunicativeCarrierClassKind.ChosenPathBearing =>
                carrier.SourceRetainedWholeKind is not (
                    PrimeRetainedWholeKind.RetainedPartial or
                    PrimeRetainedWholeKind.RetainedWholeUnclosed or
                    PrimeRetainedWholeKind.ClosureCandidate),
            _ => false
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        CommunicativeCarrierPacket carrier,
        FilamentResolutionTargetKind resolutionTarget,
        AntiEchoDispositionKind antiEchoDisposition,
        bool insufficientRetainedBasis,
        int deferredResidueCount)
    {
        var constraints = new List<string>
        {
            "communicative-filament-pointer-and-symbolic-carrier-only",
            "communicative-filament-gel-substance-not-consumed"
        };

        constraints.Add(resolutionTarget switch
        {
            FilamentResolutionTargetKind.OE => "communicative-filament-routed-to-oe",
            FilamentResolutionTargetKind.SelfGEL => "communicative-filament-routed-to-selfgel",
            FilamentResolutionTargetKind.CGoA => "communicative-filament-routed-to-cgoa",
            _ => "communicative-filament-routed-to-working-surface-only"
        });

        constraints.Add(antiEchoDisposition switch
        {
            AntiEchoDispositionKind.DeduplicateCarrier => "communicative-filament-self-echo-deduplicated",
            AntiEchoDispositionKind.ThinAsEcho => "communicative-filament-redundant-echo-thinned",
            AntiEchoDispositionKind.Defer => "communicative-filament-deferred",
            _ => "communicative-filament-preserved"
        });

        if (!carrier.PreservedDistinctionVisible)
        {
            constraints.Add("communicative-filament-distinction-not-visible");
        }

        if (carrier.SelfEchoDetected)
        {
            constraints.Add("communicative-filament-self-echo-detected");
        }

        if (carrier.GelSubstanceConsumptionRequested)
        {
            constraints.Add("communicative-filament-gel-substance-consumption-refused");
        }

        if (insufficientRetainedBasis)
        {
            constraints.Add("communicative-filament-insufficient-retained-basis");
        }

        if (deferredResidueCount > 0)
        {
            constraints.Add("communicative-filament-deferred-residues-visible");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        CommunicativeCarrierPacket carrier,
        FilamentResolutionTargetKind resolutionTarget,
        AntiEchoDispositionKind antiEchoDisposition)
    {
        if (carrier.GelSubstanceConsumptionRequested)
        {
            return "communicative-filament-gel-substance-consumption-refused";
        }

        if (!carrier.PreservedDistinctionVisible)
        {
            return "communicative-filament-distinction-not-visible";
        }

        if (HasInsufficientRetainedBasis(carrier))
        {
            return "communicative-filament-insufficient-retained-basis";
        }

        if (antiEchoDisposition == AntiEchoDispositionKind.ThinAsEcho)
        {
            return "communicative-filament-redundant-echo-thinned";
        }

        if (antiEchoDisposition == AntiEchoDispositionKind.DeduplicateCarrier)
        {
            return "communicative-filament-self-echo-deduplicated";
        }

        if (antiEchoDisposition == AntiEchoDispositionKind.Defer)
        {
            return "communicative-filament-deferred";
        }

        return resolutionTarget switch
        {
            FilamentResolutionTargetKind.OE => "communicative-filament-decision-bearing-to-oe",
            FilamentResolutionTargetKind.SelfGEL => "communicative-filament-chosen-path-to-selfgel",
            FilamentResolutionTargetKind.CGoA => "communicative-filament-distributed-integration-to-cgoa",
            _ => "communicative-filament-working-surface-only"
        };
    }

    private static string DetermineLawfulBasis(
        FilamentResolutionTargetKind resolutionTarget,
        AntiEchoDispositionKind antiEchoDisposition)
    {
        if (antiEchoDisposition == AntiEchoDispositionKind.ThinAsEcho)
        {
            return "communication may thin redundant self-echo while preserving lawful distinction and without consuming GEL substance.";
        }

        if (antiEchoDisposition == AntiEchoDispositionKind.Defer)
        {
            return "communication may keep deferred, flattened, or insufficiently resolved matter on the working or distributed integration surfaces without forcing it into higher predicate or self-bearing stores.";
        }

        return resolutionTarget switch
        {
            FilamentResolutionTargetKind.OE =>
                "communication may route important decision-bearing matter toward OE while carrying only pointers, symbolic carriers, and preserved distinctions.",
            FilamentResolutionTargetKind.SelfGEL =>
                "communication may route the coherent body of the chosen path toward SelfGEL while preserving lawful residues and without consuming GEL substance.",
            FilamentResolutionTargetKind.CGoA =>
                "communication may route remaining integrable matter toward cGoA for wider distributed integration while preserving deferred edges and lawful distinction.",
            _ =>
                "communication may keep matter on the working surfaces when it must be thinned, deduplicated, or refused stronger resolution so cOE and cSelfGEL do not become final burden sinks."
        };
    }
}

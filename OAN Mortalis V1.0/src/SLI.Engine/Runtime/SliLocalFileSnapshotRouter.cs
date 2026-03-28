using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Oan.Common;

namespace SLI.Engine.Runtime;

/// <summary>
/// A governed snapshot router configured exclusively to drop <see cref="SliSnapshotRetentionPosture.DebugOnly"/>
/// payloads safely into the local untracked diagnostic store.
/// </summary>
internal sealed class SliLocalFileSnapshotRouter : ISliSnapshotRouter
{
    private readonly IManagedEgressRouter _egressRouter;

    public SliLocalFileSnapshotRouter(IManagedEgressRouter? egressRouter = null)
    {
        _egressRouter = egressRouter ?? NullEgressRouter.Instance;
    }
    private static string GetLocalDiagnosticDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "build.ps1")))
            {
                // Specifically route to `OAN Mortalis V1.0/.local/sli-snapshots` preventing repo root pollution
                return Path.Combine(current.FullName, "OAN Mortalis V1.0", ".local", "sli-snapshots");
            }

            current = current.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "OAN Mortalis V1.0", ".local", "sli-snapshots");
    }

    /// <inheritdoc />
    public async Task RouteAsync(SliExecutionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
        {
            return;
        }

        // This local diagnostic store refuses Governance artifacts defensively
        if (snapshot.RetentionPosture == SliSnapshotRetentionPosture.GovernanceArtifact)
        {
            return;
        }

        try
        {
            var envelope = new ManagedEgressEnvelope(
                EffectKind: SliEgressEffectKind.ArtifactWrite,
                RetentionPosture: SliEgressRetentionPosture.DebugOnly,
                JurisdictionClass: SliEgressJurisdictionClass.CoreRuntime,
                IdentityFormingAllowed: false,
                TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
                AuthorityReason: "Emitting transient inspection snapshot for local debugging"
            );

            await _egressRouter.TryRouteEgressAsync(envelope, async () =>
            {
                var directory = GetLocalDiagnosticDirectory();
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var filename = $"snapshot_{snapshot.TraceId}_{snapshot.RetentionPosture}.json";
                var filePath = Path.Combine(directory, filename);

                var json = SliExecutionSnapshotFactory.Serialize(snapshot);
                await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Emitter failures trace silently without impeding core runtime cognition loops
        }
    }
}

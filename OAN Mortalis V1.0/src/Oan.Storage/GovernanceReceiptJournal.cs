using System.Text.Json;
using System.Text.RegularExpressions;
using Oan.Common;

namespace Oan.Storage;

public sealed class NdjsonGovernanceReceiptJournal : IGovernanceReceiptJournal
{
    private static readonly Regex LoopKeyPattern = new("\"loopKey\"\\s*:\\s*\"(?<loopKey>[^\"]+)\"", RegexOptions.Compiled);
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public NdjsonGovernanceReceiptJournal(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public async Task AppendAsync(
        GovernanceJournalEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(entry, _serializerOptions);
        await File.AppendAllTextAsync(_filePath, json + Environment.NewLine, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GovernanceJournalReplayBatch> ReplayBatchAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new GovernanceJournalReplayBatch([], []);
        }

        var entries = new List<GovernanceJournalEntry>();
        var issues = new List<GovernanceJournalReplayIssue>();
        using var stream = File.OpenRead(_filePath);
        using var reader = new StreamReader(stream);
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var entry = JsonSerializer.Deserialize<GovernanceJournalEntry>(line, _serializerOptions);
                if (entry is not null)
                {
                    entries.Add(entry);
                }
                else
                {
                    issues.Add(new GovernanceJournalReplayIssue(
                        lineNumber,
                        "deserialized-null-entry",
                        line,
                        TryExtractLoopKey(line)));
                }
            }
            catch (JsonException ex)
            {
                issues.Add(new GovernanceJournalReplayIssue(
                    lineNumber,
                    $"json-error:{ex.GetType().Name}",
                    line,
                    TryExtractLoopKey(line)));
            }
        }

        return new GovernanceJournalReplayBatch(entries, issues);
    }

    public async Task<GovernanceJournalReplayBatch> ReplayLoopBatchAsync(
        string loopKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        var batch = await ReplayBatchAsync(cancellationToken).ConfigureAwait(false);
        return new GovernanceJournalReplayBatch(
            batch.Entries
                .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
                .ToArray(),
            batch.Issues
                .Where(issue => string.IsNullOrWhiteSpace(issue.LoopKey) ||
                                string.Equals(issue.LoopKey, loopKey, StringComparison.Ordinal))
                .ToArray());
    }

    public async Task<IReadOnlyList<GovernanceJournalEntry>> ReplayAsync(
        CancellationToken cancellationToken = default)
    {
        return (await ReplayBatchAsync(cancellationToken).ConfigureAwait(false)).Entries;
    }

    public async Task<IReadOnlyList<GovernanceJournalEntry>> ReplayLoopAsync(
        string loopKey,
        CancellationToken cancellationToken = default)
    {
        return (await ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false)).Entries;
    }

    private static string? TryExtractLoopKey(string rawLine)
    {
        var match = LoopKeyPattern.Match(rawLine);
        return match.Success ? match.Groups["loopKey"].Value : null;
    }
}

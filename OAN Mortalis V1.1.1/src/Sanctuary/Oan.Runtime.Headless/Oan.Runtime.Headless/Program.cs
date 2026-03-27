using System.Text.Json;

namespace Oan.Runtime.Headless;

internal static class Program
{
    private static readonly JsonSerializerOptions OutputJsonOptions = new()
    {
        WriteIndented = true
    };

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || !args[0].Equals("evaluate", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Usage: evaluate --input <text> [--agent-id <id>] [--theater-id <id>] [--return-surface-only] [--outbound-object-only] [--outbound-lane-only]");
            return 2;
        }

        var agentId = ReadOption(args, "--agent-id") ?? "agent-001";
        var theaterId = ReadOption(args, "--theater-id") ?? "theater-A";
        var input = ReadOption(args, "--input");
        var returnSurfaceOnly = args.Any(static arg => arg.Equals("--return-surface-only", StringComparison.OrdinalIgnoreCase));
        var outboundObjectOnly = args.Any(static arg => arg.Equals("--outbound-object-only", StringComparison.OrdinalIgnoreCase));
        var outboundLaneOnly = args.Any(static arg => arg.Equals("--outbound-lane-only", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(input) && Console.IsInputRedirected)
        {
            input = await Console.In.ReadToEndAsync().ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.Error.WriteLine("No evaluation input was supplied.");
            return 2;
        }

        var host = HeadlessRuntimeBootstrap.CreateHost();
        if (outboundLaneOnly)
        {
            var outboundLane = await host
                .EvaluateOutboundLaneAsync(agentId, theaterId, input)
                .ConfigureAwait(false);
            Console.WriteLine(JsonSerializer.Serialize(outboundLane, OutputJsonOptions));
            return 0;
        }

        if (outboundObjectOnly)
        {
            var outboundObject = await host
                .EvaluateOutboundObjectAsync(agentId, theaterId, input)
                .ConfigureAwait(false);
            Console.WriteLine(JsonSerializer.Serialize(outboundObject, OutputJsonOptions));
            return 0;
        }

        if (returnSurfaceOnly)
        {
            var returnSurface = await host
                .EvaluateReturnSurfaceAsync(agentId, theaterId, input)
                .ConfigureAwait(false);
            Console.WriteLine(JsonSerializer.Serialize(returnSurface, OutputJsonOptions));
            return 0;
        }

        var result = await host.EvaluateAsync(agentId, theaterId, input).ConfigureAwait(false);
        Console.WriteLine(JsonSerializer.Serialize(result, OutputJsonOptions));
        return 0;
    }

    private static string? ReadOption(IReadOnlyList<string> args, string optionName)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (args[index].Equals(optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }
}

using System.Text.Json;

namespace Oan.Runtime.Headless;

internal static class Program
{
    private static readonly JsonSerializerOptions EvaluateJsonOptions = new()
    {
        WriteIndented = true
    };

    private static async Task<int> Main(string[] args)
    {
        var runtimeRoot = Directory.GetCurrentDirectory();

        if (args.Length > 0 && args[0].Equals("evaluate", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("OAN Mortalis v1.0 Headless Host Booting...");
            var host = await HeadlessRuntimeBootstrap.CreateEvaluateHostAsync(runtimeRoot).ConfigureAwait(false);
            var result = await host.EvaluateAsync("agent-001", "theater-A", new { input = "CLI_TRIGGER" }).ConfigureAwait(false);
            Console.WriteLine("Evaluation Result:");
            Console.WriteLine(JsonSerializer.Serialize(result, EvaluateJsonOptions));
            return (int)RuntimeOperatorExitCode.Success;
        }

        var context = HeadlessRuntimeBootstrap.CreateOperatorContext(runtimeRoot);
        return await RuntimeOperatorCli.RunAsync(args, Console.Out, Console.Error, context).ConfigureAwait(false);
    }
}

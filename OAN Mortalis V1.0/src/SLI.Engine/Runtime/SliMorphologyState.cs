using GEL.Graphs;

namespace SLI.Engine.Runtime;

internal sealed class SliMorphologyState
{
    public List<string> ResolvedLemmaRoots { get; } = [];
    public List<(string Token, string Kind)> OperatorAnnotations { get; } = [];
    public List<(string Role, string RootKey)> ConstructorBodies { get; } = [];
    public List<ConstructorEdge> GraphEdges { get; } = [];
    public List<string> ContinuityAnchors { get; } = [];
    public List<string> BodyInvariants { get; } = [];
    public List<string> ClusterEntries { get; } = [];
    public string DiagnosticPredicateRender { get; set; } = string.Empty;
    public string? PredicateRoot { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? ScalarPayload { get; set; }
    public string BodySummary { get; set; } = string.Empty;
    public string Outcome { get; set; } = "OutOfScope";
}

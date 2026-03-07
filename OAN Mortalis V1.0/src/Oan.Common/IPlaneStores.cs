using System.Threading.Tasks;

namespace Oan.Common
{
    /// <summary>
    /// Legacy coarse-grain surface for public plane storage operations.
    /// Prefer Prime derivative publication contracts for new extension points.
    /// </summary>
    public interface IPublicPlaneStores
    {
        Task AppendToGoAAsync(string engramHash, object payload);
        Task AppendToGELAsync(string engramHash, object payload);
    }

    /// <summary>
    /// Legacy coarse-grain surface for cryptic plane storage operations.
    /// Prefer Cryptic custody and SoulFrame membrane contracts for new extension points.
    /// </summary>
    public interface ICrypticPlaneStores
    {
        Task AppendToCGoAAsync(string engramHash, object payload);
    }
}

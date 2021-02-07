using JetBrains.Annotations;

namespace AIoT.Core.Data
{
    public interface IConnectionStringResolver
    {
        [NotNull]
        string Resolve(string connectionStringName = null);
    }
}

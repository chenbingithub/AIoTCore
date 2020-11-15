using System;
using JetBrains.Annotations;

namespace AIoT.Core.Uow
{
    public interface ITransactionApiContainer
    {
        [CanBeNull]
        ITransactionApi FindTransactionApi([NotNull] string key);

        void AddTransactionApi([NotNull] string key, [NotNull] ITransactionApi api);

        [NotNull]
        ITransactionApi GetOrAddTransactionApi([NotNull] string key, [NotNull] Func<ITransactionApi> factory);
    }
}
using JetBrains.Annotations;
using Volo.Abp;

namespace AIoT.Core.Uow
{
    public static class UnitOfWorkExtensions
    {
        public static bool IsReservedFor([NotNull] this IUnitOfWork unitOfWork, string reservationName)
        {
            Check.NotNull(unitOfWork, nameof(unitOfWork));

            return unitOfWork.IsReserved && unitOfWork.ReservationName == reservationName;
        }
    }
}
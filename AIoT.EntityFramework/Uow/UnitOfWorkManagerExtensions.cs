﻿using System;
using System.Data;
using JetBrains.Annotations;
using Volo.Abp;

namespace AIoT.EntityFramework.Uow
{
    public static class UnitOfWorkManagerExtensions
    {
        [NotNull]
        public static IUnitOfWork Begin(
            [NotNull] this IUnitOfWorkManager unitOfWorkManager, 
            bool requiresNew = false,
            bool isTransactional = false,
            IsolationLevel? isolationLevel = null, 
            TimeSpan? timeout = null)
        {
            Check.NotNull(unitOfWorkManager, nameof(unitOfWorkManager));

            return unitOfWorkManager.Begin(new EntityFramework.Uow.AbpUnitOfWorkOptions
            {
                IsTransactional = isTransactional,
                IsolationLevel = isolationLevel,
                Timeout = timeout
            }, requiresNew);
        }

        public static void BeginReserved([NotNull] this IUnitOfWorkManager unitOfWorkManager, [NotNull] string reservationName)
        {
            Check.NotNull(unitOfWorkManager, nameof(unitOfWorkManager));
            Check.NotNull(reservationName, nameof(reservationName));

            unitOfWorkManager.BeginReserved(reservationName, new EntityFramework.Uow.AbpUnitOfWorkOptions());
        }

        public static void TryBeginReserved([NotNull] this IUnitOfWorkManager unitOfWorkManager, [NotNull] string reservationName)
        {
            Check.NotNull(unitOfWorkManager, nameof(unitOfWorkManager));
            Check.NotNull(reservationName, nameof(reservationName));

            unitOfWorkManager.TryBeginReserved(reservationName, new EntityFramework.Uow.AbpUnitOfWorkOptions());
        }
    }
}
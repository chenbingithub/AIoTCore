﻿using AIoT.Core.Repository;
using AIoT.RedisCache.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AIoTCoreWebTest.Service
{
    public interface IRoleCache : IEntityCache<RoleCacheItem, string>
    {

    }
    public class RoleCacheItem
    {

    }
    public class RoleCache : EntityCache<Role,RoleCacheItem, string>, IRoleCache, ITransientDependency
    {

        /// <inheritdoc />
        public RoleCache(IRepository<Role, string> repository, ICacheManager cacheManager) : base(repository, cacheManager)
        {
        }
         
    }
}

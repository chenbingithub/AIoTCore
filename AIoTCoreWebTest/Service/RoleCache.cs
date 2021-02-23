﻿using AIoT.Core.Repository;
using AIoT.RedisCache.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIoT.Core.Cache;
using Volo.Abp.DependencyInjection;

namespace AIoTCoreWebTest.Service
{
    public interface IRoleCache : IEntityCache<RoleCacheItem, string>
    {

    }
    public interface IRoleListCache : IEntityListCache<RoleCacheItem, string>
    {

    }
    public class RoleCacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class RoleCache : EntityCache<Role,RoleCacheItem, string>, IRoleCache, ITransientDependency
    {

        /// <inheritdoc />
        public RoleCache(IRepository<Role, string> repository, ICacheManager cacheManager) : base(repository, cacheManager)
        {
        }
         
    }

    public class RoleListCache : EntityListCache<Role, RoleCacheItem, string>, IRoleListCache, ITransientDependency
    {
        public RoleListCache(IRepository<Role, string> repository, ICacheManager cacheManager) : base(repository, cacheManager)
        {
        }

        protected override string GetKeyByCacheItem(RoleCacheItem data) => data.Id;
       
    }
}

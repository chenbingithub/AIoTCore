using System.Threading.Tasks;
using AIoT.Core.Runtime;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.Authorization
{
    /// <inheritdoc cref="IPermissionChecker" />
    public class DefaultPermissionChecker : IPermissionChecker, ITransientDependency
    {
        private readonly ICurrentUser _currentUser;
        

        /// <inheritdoc />
        public DefaultPermissionChecker(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        /// <inheritdoc />
        public async Task<bool> IsGrantedAsync(string permission)
        {
            //var userId = _currentUser.UserId;
            //if (userId != null)
            //{
            //    return await IsGrantedAsync(userId, permission);
            //}

            //return false;
            return await IsGrantedAsync(_currentUser.UserId, permission);
        }

        /// <inheritdoc />
        public  Task<bool> IsGrantedAsync(string userId, string permission)
        {
             return Task.FromResult(true);
        }

        public Task<bool> IsGrantedPathAsync(string path)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsGrantedPathAsync(string userId, string path)
        {
            return Task.FromResult(true);
        }
    }
}

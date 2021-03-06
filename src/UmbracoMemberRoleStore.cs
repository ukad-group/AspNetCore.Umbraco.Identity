using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Umbraco.Identity
{
    public class UmbracoMemberRoleStore : IRoleStore<string>
    {
        public Task<IdentityResult> CreateAsync(string role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(string role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        public Task<string> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return Task.FromResult(roleId);
        }

        public Task<string> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(normalizedRoleName);
        }

        public Task<string> GetNormalizedRoleNameAsync(string role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role);
        }

        public Task<string> GetRoleIdAsync(string role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role);
        }

        public Task<string> GetRoleNameAsync(string role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role);
        }

        public Task SetNormalizedRoleNameAsync(string role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(string role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(string role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

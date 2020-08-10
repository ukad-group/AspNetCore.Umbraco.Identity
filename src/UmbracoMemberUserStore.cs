using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AspNetCore.Umbraco.Identity.AppModels;
using AspNetCore.Umbraco.Identity.Models;
using AspNetCore.Umbraco.Identity.Utils;

using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Umbraco.Identity
{
    public class UmbracoMemberUserStore<TUser> :
        IUserPasswordStore<TUser>,
        IUserEmailStore<TUser>,
        IUserRoleStore<TUser>,
        IUserClaimStore<TUser>,
        IQueryableUserStore<TUser>
        where TUser : class, IUser, new()
    {
        private readonly IRepository<CmsMember> membersRepository;
        private readonly IRepository<CmsPropertyData> propertyDataRepository;
        private readonly IRepository<CmsPropertyType> propertyTypeRepository;
        private readonly IRepository<CmsMember2MemberGroup> member2MemberGroupRepository;
        private readonly IRepository<UmbracoNode> umbracoNodeRepository;
        private readonly IRepository<CmsContent> contentRepository;
        private readonly IRepository<CmsContentVersion> contentVersionRepository;
        private readonly IRepository<CmsContentXml> contentXmlRepository;
        private readonly IRepository<CmsContentType> contentTypeRepository;

        private const int DefaultMemberContentTypeId = 1095;

        public UmbracoMemberUserStore(
            IRepository<CmsContent> contentRepository,
            IRepository<CmsContentVersion> contentVersionRepository,
            IRepository<CmsContentXml> contentXmlRepository,
            IRepository<CmsContentType> contentTypeRepository,
            IRepository<CmsMember> membersRepository,
            IRepository<CmsPropertyData> propertyDataRepository,
            IRepository<CmsPropertyType> propertyTypeRepository,
            IRepository<CmsMember2MemberGroup> member2MemberGroupRepository,
            IRepository<UmbracoNode> umbracoNodeRepository
            )
        {
            this.membersRepository = membersRepository;
            this.propertyDataRepository = propertyDataRepository;
            this.propertyTypeRepository = propertyTypeRepository;
            this.member2MemberGroupRepository = member2MemberGroupRepository;
            this.umbracoNodeRepository = umbracoNodeRepository;
            this.contentRepository = contentRepository;
            this.contentVersionRepository = contentVersionRepository;
            this.contentXmlRepository = contentXmlRepository;
            this.contentTypeRepository = contentTypeRepository;
        }

        public IQueryable<TUser> Users => MappingUtils<TUser>.Mapper.ProjectTo<TUser>(membersRepository.GetAll());

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            var umbracoNode = new UmbracoNode
            {
                CreateDate = DateTime.UtcNow,
                Level = 1,
                NodeObjectType = Const.MemberObjectType,
                NodeUser = 0,
                ParentId = -1,
                SortOrder = 3,
                Text = user.Email,
                Trashed = false,
                UniqueId = Guid.NewGuid(),
                Path = "-1"
            };

            await umbracoNodeRepository.AddAsync(umbracoNode, cancellationToken);
            await umbracoNodeRepository.SaveChangesAsync(cancellationToken);

            umbracoNode.Path = $"-1,{umbracoNode.Id}";
            umbracoNodeRepository.Update(umbracoNode);

            user.Id = umbracoNode.Id;

            var content = new CmsContent
            {
                ContentType = DefaultMemberContentTypeId,
                NodeId = umbracoNode.Id
            };

            await contentRepository.AddAsync(content, cancellationToken);

            var xmlWriterSettings = new System.Xml.XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true
            };

            using (var sw = new StringWriter())
            using (var writer = System.Xml.XmlWriter.Create(sw, xmlWriterSettings))
            {
                var xmlNode = new XmlNode
                {
                    CreateDate = umbracoNode.CreateDate,
                    Email = user.Email,
                    Id = umbracoNode.Id,
                    LastPasswordReset = DateTime.UtcNow,
                    Level = umbracoNode.Level,
                    LoginName = user.Email,
                    NodeName = umbracoNode.Text,
                    NodeType = DefaultMemberContentTypeId,
                    NodeTypeAlias = "Default",
                    ParentId = umbracoNode.ParentId,
                    Path = umbracoNode.Path,
                    SortOrder = umbracoNode.SortOrder,
                    Template = 0,
                    Version = umbracoNode.UniqueId,
                    WriterId = 0,
                    WriterName = "kkadmin",
                    UrlName = umbracoNode.Text,
                    UpdateDate = umbracoNode.CreateDate

                };

                var emptyNamespaces = new XmlSerializerNamespaces(new[] { System.Xml.XmlQualifiedName.Empty });
                var umbracoNodeSerializer = new XmlSerializer(typeof(XmlNode));
                umbracoNodeSerializer.Serialize(writer, xmlNode, emptyNamespaces);

                var contentXml = new CmsContentXml
                {
                    NodeId = umbracoNode.Id,
                    Xml = sw.ToString()
                };

                await contentXmlRepository.AddAsync(contentXml, cancellationToken);
            }


            var member = new CmsMember
            {
                NodeId = umbracoNode.Id,
                Email = user.Email,
                LoginName = user.Alias,
                Password = user.PasswordHash
            };

            await membersRepository.AddAsync(member, cancellationToken);

            var propertyTypes = propertyTypeRepository
                .GetAll()
                .Where(pt => pt.ContentTypeId == DefaultMemberContentTypeId)
                .ToList();

            var properties = user.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(pt => pt.Name.ToLower(), pt => pt);


            var cmsContentVersion = new CmsContentVersion
            {
                ContentId = member.NodeId,
                VersionDate = DateTime.UtcNow,
                VersionId = Guid.NewGuid()
            };

            await contentVersionRepository.AddAsync(cmsContentVersion, cancellationToken);

            foreach (var pt in propertyTypes)
            {
                var cmsProp = new CmsPropertyData
                {
                    ContentNodeId = member.NodeId,
                    VersionId = cmsContentVersion.VersionId,
                    PropertyTypeId = pt.Id
                };

                if (properties.ContainsKey(pt.Alias.ToLower()))
                {
                    var p = properties[pt.Alias.ToLower()];
                    switch (p.PropertyType)
                    {
                        case Type boolType when boolType == typeof(bool):
                            cmsProp.DataInt = (bool)Convert.ChangeType(p.GetValue(user) ?? 0, p.PropertyType) ? 1 : 0;
                            break;
                        case Type dateType when dateType == typeof(DateTime):
                            cmsProp.DataDate = (DateTime?)Convert.ChangeType(p.GetValue(user) ?? 0, p.PropertyType);
                            break;
                        default:
                            cmsProp.DataNvarchar = p.GetValue(user)?.ToString();
                            break;
                    }
                }

                await propertyDataRepository.AddAsync(cmsProp, cancellationToken);
            }

            await propertyTypeRepository.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;

        }

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var query = from m in membersRepository.GetAllAsNoTracking()
                        join pd in propertyDataRepository.GetAllAsNoTracking() on m.NodeId equals pd.ContentNodeId
                        join pt in propertyTypeRepository.GetAllAsNoTracking() on pd.PropertyTypeId equals pt.Id
                        where m.Email.ToUpper() == normalizedEmail.ToUpper()
                        select new MemberProperty
                        {
                            Member = m,
                            PropertyData = pd,
                            PropertyType = pt
                        };

            var memberProperties = await membersRepository.ToListAsync(query, cancellationToken);

            return UmbracoUtils.CreateUserFromProperties<TUser>(memberProperties);
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!int.TryParse(userId, out int intUserId))
            {
                throw new ArgumentException(nameof(userId));
            }

            var query = from m in membersRepository.GetAllAsNoTracking()
                        join pd in propertyDataRepository.GetAllAsNoTracking() on m.NodeId equals pd.ContentNodeId
                        join pt in propertyTypeRepository.GetAllAsNoTracking() on pd.PropertyTypeId equals pt.Id
                        where m.NodeId == intUserId
                        select new MemberProperty
                        {
                            Member = m,
                            PropertyData = pd,
                            PropertyType = pt
                        };

            var memberProperties = await membersRepository.ToListAsync(query, cancellationToken);

            return UmbracoUtils.CreateUserFromProperties<TUser>(memberProperties);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var query = from m in membersRepository.GetAllAsNoTracking()
                        join pd in propertyDataRepository.GetAllAsNoTracking() on m.NodeId equals pd.ContentNodeId
                        join pt in propertyTypeRepository.GetAllAsNoTracking() on pd.PropertyTypeId equals pt.Id
                        where m.LoginName.ToUpper() == normalizedUserName.ToUpper()
                        select new MemberProperty
                        {
                            Member = m,
                            PropertyData = pd,
                            PropertyType = pt
                        };


            var memberProperties = await membersRepository.ToListAsync(query, cancellationToken);

            return UmbracoUtils.CreateUserFromProperties<TUser>(memberProperties);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Alias.ToUpper());
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Alias);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            await Task.Yield();
        }

        public async Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }

        public async Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }

        public async Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }

        public async Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            await Task.Yield();
        }

        public async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            user.Alias = userName;
            await Task.Yield();
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            var member = await membersRepository.GetByIdAsync(user.Id, cancellationToken);
            var propertyTypes = propertyTypeRepository
                .GetAllAsNoTracking()
                .Where(pt => pt.ContentTypeId == DefaultMemberContentTypeId)
                .ToList();

            var propertiesFromDb = await propertyDataRepository.ToListAsync(
                propertyDataRepository.GetAllAsNoTracking().Where(x => x.ContentNodeId == member.NodeId),
                cancellationToken);

            var properties = user.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(pt => pt.Name.ToLower(), pt => pt);

            var cmsContentVersion = await contentVersionRepository.FirstOrDefaultAsync(
                contentVersionRepository.GetAll().Where(x => x.ContentId == member.NodeId),
                cancellationToken);

            var propertiesToUpdate = new List<CmsPropertyData>();
            foreach (var pt in propertyTypes)
            {
                var ptDb = propertiesFromDb.FirstOrDefault(x => x.PropertyTypeId == pt.Id);

                if (ptDb != null && properties.TryGetValue(pt.Alias.ToLower(), out var p))
                {
                    bool changed;
                    switch (p.PropertyType)
                    {
                        case Type boolType when boolType == typeof(bool):
                            var boolTypeValue = (bool)Convert.ChangeType(p.GetValue(user) ?? 0, p.PropertyType) ? 1 : 0;
                            changed = boolTypeValue != ptDb.DataInt;
                            ptDb.DataInt = boolTypeValue;
                            break;
                        case Type dateType when dateType == typeof(DateTime):
                            var dateTypeValue = (DateTime?)Convert.ChangeType(p.GetValue(user) ?? 0, p.PropertyType);
                            changed = dateTypeValue != ptDb.DataDate;
                            ptDb.DataDate = dateTypeValue;
                            break;
                        default:
                            var value = p.GetValue(user)?.ToString();
                            changed = value != ptDb.DataNvarchar;
                            ptDb.DataNvarchar = value;
                            break;
                    }
                    if (changed)
                    {
                        propertiesToUpdate.Add(ptDb);
                    }
                }
            }

            cmsContentVersion.VersionDate = DateTime.UtcNow;

            member.Email = user.Email;
            member.LoginName = user.Alias;
            member.Password = user.PasswordHash;

            membersRepository.Update(member);
            propertyDataRepository.Update(propertiesToUpdate);
            await propertyDataRepository.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var q = umbracoNodeRepository
                .GetAll()
                .Where(n => n.NodeObjectType == Const.MemberGroupObjectType && n.Text == roleName);

            var roleNode = await umbracoNodeRepository.FirstOrDefaultAsync(q, cancellationToken);

            if (roleNode == null)
            {
                throw new ArgumentException("Role not found", nameof(roleName));
            }

            var member2memberGroup = new CmsMember2MemberGroup
            {
                Member = user.Id,
                MemberGroup = roleNode.Id
            };

            await member2MemberGroupRepository.AddAsync(member2memberGroup, cancellationToken);
            await member2MemberGroupRepository.SaveChangesAsync();
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            var query = from m in membersRepository.GetAll()
                        join c in contentRepository.GetAll() on m.NodeId equals c.NodeId
                        join ct in contentTypeRepository.GetAll() on c.ContentType equals ct.NodeId
                        join m2m in member2MemberGroupRepository.GetAll() on m.NodeId equals m2m.Member into grouping
                        from m2m in grouping.DefaultIfEmpty()
                        join un in umbracoNodeRepository.GetAll() on m2m.MemberGroup equals un.Id into grouping2
                        from un in grouping2.DefaultIfEmpty()
                        where m.NodeId == user.Id
                        select new
                        {
                            Role = un.Text,
                            ContentType = ct.Alias
                        };

            var results = await membersRepository.ToListAsync(query, cancellationToken);

            var roles = results
                .Where(x => x.Role != null)
                .GroupBy(x => x.Role)
                .Select(x => x.Key)
                .ToList();

            var contentTypes = results
                .GroupBy(x => x.ContentType)
                .Select(x => x.Key);

            roles.AddRange(contentTypes);

            return roles;
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var query = from m in membersRepository.GetAll()
                        join c in contentRepository.GetAll() on m.NodeId equals c.NodeId
                        join ct in contentTypeRepository.GetAll() on c.ContentType equals ct.NodeId
                        join m2m in member2MemberGroupRepository.GetAll() on m.NodeId equals m2m.Member into grouping
                        from m2m in grouping.DefaultIfEmpty()
                        join un in umbracoNodeRepository.GetAll() on m2m.MemberGroup equals un.Id
                        where m.NodeId == user.Id && un.Text.ToUpper() == roleName.ToUpper() || ct.Alias.ToUpper().Equals(roleName.ToUpper())
                        select un;

            var roles = await GetRolesAsync(user, cancellationToken);

            var umbracoRoleNode = await membersRepository.FirstOrDefaultAsync(query, cancellationToken);

            return umbracoRoleNode != null;
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var query = from m in membersRepository.GetAll()
                        join c in contentRepository.GetAll() on m.NodeId equals c.NodeId
                        join ct in contentTypeRepository.GetAll() on c.ContentType equals ct.NodeId
                        join m2m in member2MemberGroupRepository.GetAll() on m.NodeId equals m2m.Member into grouping
                        from m2m in grouping.DefaultIfEmpty()
                        join un in umbracoNodeRepository.GetAll() on m2m.MemberGroup equals un.Id into grouping2
                        from un in grouping2.DefaultIfEmpty()
                        join pd in propertyDataRepository.GetAll() on m.NodeId equals pd.ContentNodeId
                        join pt in propertyTypeRepository.GetAll() on pd.PropertyTypeId equals pt.Id
                        where un.Text.ToUpper() == roleName.ToUpper() || ct.Alias.ToUpper().Equals(roleName.ToUpper())
                        select new MemberProperty
                        {
                            Member = m,
                            PropertyData = pd,
                            PropertyType = pt
                        };

            var memberProperties = await membersRepository.ToListAsync(query, cancellationToken);

            var membersProp = memberProperties.GroupBy(x => x.Member.NodeId, x => x);

            return membersProp.Select(x => UmbracoUtils.CreateUserFromProperties<TUser>(x.ToList())).ToList();
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync(user, cancellationToken);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Alias.ToString()));
            return claims;
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Data;
using AspNetCore.Umbraco.Identity.Models;
using AutoMapper;

namespace AspNetCore.Umbraco.Identity.Utils
{
    internal static class MappingUtils<TUser>
        where TUser : class, IUser, new()
    {
        internal static readonly IMapper Mapper = BuildAutoMapperConfiguration().CreateMapper();

        private static MapperConfiguration BuildAutoMapperConfiguration()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CmsMember, TUser>()
                    .ForMember(x => x.Email, opt => opt.MapFrom(c => c.Email))
                    .ForMember(x => x.Alias, opt => opt.MapFrom(c => c.LoginName))
                    .ForMember(x => x.Id, opt => opt.MapFrom(c => c.NodeId))
                    .ForMember(x => x.PasswordHash, opt => opt.MapFrom(c => c.Password))
                    .ForAllOtherMembers(x => x.Ignore());
            });

            config.AssertConfigurationIsValid();

            return config;
        }
    }
}

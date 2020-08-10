using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AspNetCore.Umbraco.Identity.AppModels;

namespace AspNetCore.Umbraco.Identity.Utils
{
    public static class UmbracoUtils
    {
        public static TUser CreateUserFromProperties<TUser>(IList<MemberProperty> memberProperties)
            where TUser : class, IUser, new()
        {
            if (!memberProperties.Any())
            {
                return null;
            }

            var property = memberProperties.First();

            var member = MappingUtils<TUser>.Mapper.Map<TUser>(property.Member);

            foreach (var prop in memberProperties)
            {
                var propertyInfo = member.GetType().GetProperty(prop.PropertyType.Alias, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    switch (propertyInfo.PropertyType)
                    {
                        case Type boolType when boolType == typeof(bool):
                            propertyInfo.SetValue(member, Convert.ChangeType(prop.PropertyData.DataInt ?? 0, propertyInfo.PropertyType), null);
                            break;
                        case Type dateType when dateType == typeof(DateTime):
                            propertyInfo.SetValue(member, Convert.ChangeType(prop.PropertyData.DataDate, propertyInfo.PropertyType), null);
                            break;
                        default:
                            propertyInfo.SetValue(member, Convert.ChangeType(prop.PropertyData.DataNvarchar, propertyInfo.PropertyType), null);
                            break;
                    }
                }
            }

            return member;
        }
    }
}

using System.Reflection;

namespace Furesoft.Core.ObjectDB.Services;

internal interface IReflectionService
{
    IList<FieldInfo> GetFields(Type type);

    IList<PropertyInfo> GetProperties(Type type);

    IList<MemberInfo> GetFieldsAndProperties(Type type);
}
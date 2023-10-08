using Furesoft.Core.ObjectDB.Cache;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core;

public interface IInstanceBuilder
{
    object BuildOneInstance(NonNativeObjectInfo objectInfo, IOdbCache cache);

    object BuildOneInstance(NonNativeObjectInfo objectInfo);
}
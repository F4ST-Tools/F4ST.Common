using System;
using System.Collections.Generic;
using F4ST.Common.Containers;

namespace F4ST.Common.Tools
{
    public interface IAppSetting : ISingleton
    {
        void ClearCachedItems();
        string Get(string section, bool forceUpdate = false);
        Dictionary<string, string> GetSettings(string section, bool forceUpdate = false);
        T Get<T>(string section, bool forceUpdate = false);
        object Get(Type type, string section, bool forceUpdate = false);
    }
}
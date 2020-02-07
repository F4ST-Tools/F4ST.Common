using System.Collections.Generic;
using F4St.Common.Containers;

namespace F4St.Common.Tools
{
    public interface IAppSetting : ISingleton
    {
        void ClearCachedItems();
        string Get(string section, bool forceUpdate = false);
        Dictionary<string, string> GetSettings(string section, bool forceUpdate = false);
        T Get<T>(string section, bool forceUpdate = false);
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace F4ST.Common.Tools
{
    public class AppSetting : IAppSetting
    {
        private readonly IConfiguration _configuration;

        private readonly ConcurrentDictionary<string, object> _internalCache =
            new ConcurrentDictionary<string, object>();

        public AppSetting(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Internal setting cacche

        private object GetCacheItem(string section)
        {
            return _internalCache.ContainsKey(section)
                ? _internalCache[section]
                : null;
        }

        private void AddToCache<T>(string section, T item)
        {
            _internalCache.AddOrUpdate(section, item, (s, o) => item);
        }

        /*private void RemoveCacheItem(string section)
        {
            if (!_internalCache.ContainsKey(section))
                return;


            _internalCache.Remove(section, out _);
        }*/

        public void ClearCachedItems()
        {
            _internalCache.Clear();
        }

        #endregion

        public string Get(string section, bool forceUpdate = false)
        {
            var res = string.Empty;
            if (!forceUpdate)
            {
                res = GetCacheItem(section) as string;
            }

            if (!forceUpdate && !string.IsNullOrWhiteSpace(res))
                return res;

            res = _configuration.GetSection(section).Value;
            AddToCache(section, res);

            return res;
        }

        public Dictionary<string, string> GetSettings(string section, bool forceUpdate = false)
        {
            var res = GetCacheItem(section) as Dictionary<string, string>;

            if (!forceUpdate && res != null)
                return res;

            res = _configuration.GetSection(section).GetChildren()
                .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                .ToDictionary(x => x.Key, x => x.Value);
            AddToCache(section, res);

            return res;
        }

        public T Get<T>(string section, bool forceUpdate = false)
        {
            /*T res = default;
            if (!forceUpdate)
            {
                res = (T)GetCacheItem(section);
            }

            if (!forceUpdate && res != null)
                return res;

            var str = _configuration.GetSection(section);
            res = str.Get<T>();

            AddToCache(section, res);*/

            return (T)Get(typeof(T), section, forceUpdate);
        }

        public object Get(Type type, string section, bool forceUpdate = false)
        {
            object res = null;
            if (!forceUpdate)
            {
                res = GetCacheItem(section);
            }

            if (!forceUpdate && res != null)
                return res;

            var str = _configuration.GetSection(section);
            res = str.Get(type);

            AddToCache(section, res);

            return res;
        }
    }
}
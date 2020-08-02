using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace BusinessCodeGenerator.Configuration
{
    public class InMemoryBusinessCodeConfigManager : IBusinessCodeConfigManager
    {
        private readonly Dictionary<Guid, List<BusinessCodeConfig>> _cache = null;
        private readonly object _locker = new object();

        public InMemoryBusinessCodeConfigManager()
        {
            _cache = new Dictionary<Guid, List<BusinessCodeConfig>>();
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public BusinessCodeConfig Get(Guid applicationId, byte codeType)
        {
            lock (_locker)
            {
                if (_cache.ContainsKey(applicationId))
                {
                    var subCache = _cache[applicationId];
                    return subCache.FirstOrDefault(m => m.CodeType == codeType);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取配置列表
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public List<BusinessCodeConfig> GetList(Guid applicationId)
        {
            lock (_locker)
            {
                if (_cache.ContainsKey(applicationId))
                {
                    return _cache[applicationId];
                }
                else
                {
                    return new List<BusinessCodeConfig>();
                }
            }
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        /// <param name="codePrefix"></param>
        /// <param name="sequenceMinLength"></param>
        /// <param name="remark"></param>
        public bool Add(Guid applicationId, byte codeType, string codePrefix, byte sequenceMinLength, string remark)
        {
            _cache.TryAdd(applicationId, new List<BusinessCodeConfig>());
            _cache[applicationId].Add(new BusinessCodeConfig
            {
                ApplicationId = applicationId,
                CodeType = codeType,
                CodePrefix = codePrefix,
                SequenceMinLength = sequenceMinLength,
                Remark = remark
            });
            return true;
        }

        /// <summary>
        /// 移除配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        public bool Remove(Guid applicationId, byte codeType)
        {
            lock (_locker)
            {
                if (_cache == null || !_cache.ContainsKey(applicationId))
                {
                    return true;
                }
                var subCache = _cache[applicationId];
                var config = subCache.FirstOrDefault(m => m.CodeType == codeType);
                if (config != null)
                {
                    subCache.Remove(config);
                }
            }
            return true;
        }
    }
}

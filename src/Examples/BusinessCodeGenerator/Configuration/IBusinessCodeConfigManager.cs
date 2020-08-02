﻿using System;
using System.Collections.Generic;

namespace BusinessCodeGenerator.Configuration
{
    public interface IBusinessCodeConfigManager
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        /// <returns></returns>
        BusinessCodeConfig Get(Guid applicationId, byte codeType);

        /// <summary>
        /// 获取配置列表
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        List<BusinessCodeConfig> GetList(Guid applicationId);

        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        /// <param name="codePrefix"></param>
        /// <param name="sequenceMinLength"></param>
        /// <param name="remark"></param>
        bool Add(Guid applicationId, byte codeType, string codePrefix, byte sequenceMinLength, string remark);

        /// <summary>
        /// 移除配置
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        bool Remove(Guid applicationId, byte codeType);
    }
}

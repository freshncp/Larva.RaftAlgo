using System;

namespace BusinessCodeGenerator.Configuration
{
    /// <summary>
    /// 业务编号配置
    /// </summary>
    public class BusinessCodeConfig
    {
        /// <summary>
        /// 应用程序ID
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// 编号类型
        /// </summary>
        public byte CodeType { get; set; }

        /// <summary>
        /// 编号前缀
        /// </summary>
        public string CodePrefix { get; set; }

        /// <summary>
        /// 序列最短长度
        /// </summary>
        public byte SequenceMinLength { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}

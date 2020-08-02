using BusinessCodeGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BusinessCodeGenerator.Storage
{
    /// <summary>
    /// 业务编号数据存储格式
    /// </summary>
    /// <remarks>
    /// 二进制协议：
    /// ApplicationID | CodeType | UnixTime | Sequence | (blank) | VersionID
    /// 16 | 4 | 8 | 4 | 8 | 8
    /// </remarks>
    public class BusinessCode
    {
        public const int BYTES_LENGTH = 48;// 取16字节的倍数
        private readonly object _locker = new object();

        private long _unixTime = 0;
        private long _versionId = 1;

        /// <summary>
        /// 初始化创建
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="codeType"></param>
        public BusinessCode(Guid applicationId, int codeType)
        {
            ApplicationId = applicationId;
            CodeType = codeType;
            Sequence = 1;
        }

        /// <summary>
        /// 从字节数组中加载
        /// </summary>
        /// <param name="bytes">48字节</param>
        /// <exception cref="InvalidCastException"></exception>
        public BusinessCode(byte[] bytes)
        {
            if (bytes != null && bytes.Length == BYTES_LENGTH)
            {
                ApplicationId = new Guid(bytes.Take(16).ToArray());
                CodeType = BitConverter.ToInt32(bytes, 16);
                _unixTime = BitConverter.ToInt64(bytes, 20);
                Infix = DateTime2UnixTime.FromUnixTime(_unixTime).ToString("yyMMddHH");
                Sequence = BitConverter.ToInt32(bytes, 28);
                if (Sequence < 1)
                {
                    throw new InvalidCastException("Sequence is invalid");
                }
                _versionId = BitConverter.ToInt64(bytes, 40);
                if (_versionId < 1)
                {
                    throw new InvalidCastException("Version is invalid");
                }
            }
            else
            {
                throw new InvalidCastException("Data must 48 bytes");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid ApplicationId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int CodeType { get; private set; }

        ///// <summary>
        ///// Infix
        ///// </summary>
        public string Infix { get; private set; }
       
        public int Sequence { get; private set; }

        public long VersionId => Interlocked.Read(ref _versionId);

        /// <summary>
        /// 获取新序号
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public void Generate(DateTime dateTime)
        {
            lock (_locker)
            {
                Interlocked.Increment(ref _versionId);
                var unixTime = DateTime2UnixTime.ToUnixTime(dateTime.Date.AddHours(dateTime.Hour));
                if (unixTime <= _unixTime)
                {
                    ++Sequence;
                }
                else
                {
                    _unixTime = unixTime;
                    Infix = DateTime2UnixTime.FromUnixTime(_unixTime).ToString("yyMMddHH");
                    Sequence = 1;
                }
            }
        }

        public byte[] ToBytes()
        {
            lock (_locker)
            {
                List<byte> buffer = new List<byte>(BYTES_LENGTH);
                buffer.AddRange(ApplicationId.ToByteArray());
                buffer.AddRange(BitConverter.GetBytes(CodeType));
                buffer.AddRange(BitConverter.GetBytes(_unixTime));
                buffer.AddRange(BitConverter.GetBytes(Sequence));
                buffer.AddRange(BitConverter.GetBytes(0L));// 占位字节，用于确保是16字节的倍数
                buffer.AddRange(BitConverter.GetBytes(_versionId));
                return buffer.ToArray();
            }
        }

        public string GetUniqueKeyString()
        {
            return GetUniqueKeyString(ApplicationId, CodeType);
        }

        public static string GetUniqueKeyString(Guid applicationId, int codeType)
        {
            return $"{applicationId:D}|{codeType}";
        }
    }
}

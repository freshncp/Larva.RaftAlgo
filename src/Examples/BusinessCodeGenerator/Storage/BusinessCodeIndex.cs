using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessCodeGenerator.Storage
{
    /// <summary>
    /// 业务编号索引存储格式
    /// </summary>
    /// <remarks>
    /// 二进制协议：
    /// ApplicationID | CodeType | (blank) | Position
    /// 16 | 4 | 4 | 8
    /// </remarks>
    public class BusinessCodeIndex
    {
        public const int BYTES_LENGTH = 32;// 取16字节的倍数

        public BusinessCodeIndex() { }

        /// <summary>
        /// 从字节数组中加载
        /// </summary>
        /// <param name="bytes">32字节</param>
        public BusinessCodeIndex(byte[] bytes)
        {
            if (bytes != null && bytes.Length == BYTES_LENGTH)
            {
                ApplicationID = new Guid(bytes.Take(16).ToArray());
                CodeType = BitConverter.ToInt32(bytes, 16);
                Position = BitConverter.ToInt64(bytes, 24);
                if (Position < 0)
                {
                    throw new InvalidCastException("Position is invalid");
                }
            }
            else
            {
                throw new InvalidCastException("Index must 32 bytes");
            }
        }

        public Guid ApplicationID { get; set; }

        public int CodeType { get; set; }

        public long Position { get; set; }

        public byte[] ToBytes()
        {
            List<byte> buffer = new List<byte>(BYTES_LENGTH);
            buffer.AddRange(ApplicationID.ToByteArray());
            buffer.AddRange(BitConverter.GetBytes(CodeType));
            buffer.AddRange(BitConverter.GetBytes(0));// 占位字节，用于确保是16字节的倍数
            buffer.AddRange(BitConverter.GetBytes(Position));
            return buffer.ToArray();
        }
    }
}

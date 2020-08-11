using System;

namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// Serializer for command
    /// </summary>
    public interface ICommandSerializer
    {
        /// <summary>
        /// Serialize object to type name and data
        /// </summary>
        /// <param name="type"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        (string typeName, byte[] data) Serialize(Type type, object o);

        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        object Deserialize(string typeName, byte[] data);
    }
}
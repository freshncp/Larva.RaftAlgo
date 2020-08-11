using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Larva.RaftAlgo.Log;
using Newtonsoft.Json;

namespace BusinessCodeGenerator.Raft
{
    public class JsonCommandSerializer : ICommandSerializer
    {
        private readonly ConcurrentDictionary<string, Type> _cache;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private Assembly[] _assemblies;

        public JsonCommandSerializer()
        {
            _cache = new ConcurrentDictionary<string, Type>();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public void Initialize(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            _assemblies = assemblies;
        }

        public (string typeName, byte[] data) Serialize(Type type, object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return (type.FullName, Encoding.UTF8.GetBytes(json));
        }

        public object Deserialize(string typeName, byte[] data)
        {
            if (!_cache.TryGetValue(typeName, out Type type))
            {
                foreach (var assembly in _assemblies)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        break;
                    }
                }
                if (type == null)
                {
                    throw new ApplicationException($"Type {typeName} not found");
                }
                _cache.TryAdd(typeName, type);
            }
            var o = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type);
            return o;
        }
    }
}
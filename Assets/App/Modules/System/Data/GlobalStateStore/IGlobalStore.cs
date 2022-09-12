using System;

namespace ZFramework
{
    public interface IGlobalStore
    {
        public object Get<T>(string key);
        public object Get(string key);


        public void ProcessData(TypeCode typeCode);
        public void Set<T>(string key, T value);
    }
}

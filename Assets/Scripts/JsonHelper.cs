using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Since JsonUtility doesn't allow to serialize/deserialize a list or an array of serializable objects, this auxiliar class makes it easier, with the Wrapper class
    /// </summary>
    public static class JsonHelper
    {
        public static T[] FromJsonArray<T>(string json)
        {
            ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
            
            if (wrapper == null) return null;
            return wrapper.Items;
        }

        public static List<T> FromJsonList<T>(string json)
        {
            ListWrapper<T> wrapper = JsonUtility.FromJson<ListWrapper<T>>(json);

            if (wrapper == null) return null;
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            ArrayWrapper<T> wrapper = new ArrayWrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            ArrayWrapper<T> wrapper = new ArrayWrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        public static string ToJson<T>(List<T> list)
        {
            ListWrapper<T> wrapper = new ListWrapper<T>();
            wrapper.Items = list;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(List<T> list, bool prettyPrint)
        {
            ListWrapper<T> wrapper = new ListWrapper<T>();
            wrapper.Items = list;

            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class ArrayWrapper<T>
        {
            public T[] Items;
        }

        [Serializable]
        private class ListWrapper<T>
        {
            public List<T> Items;
        }
    }
}

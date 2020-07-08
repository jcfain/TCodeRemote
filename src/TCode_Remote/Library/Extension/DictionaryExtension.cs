using System.Collections.Generic;

namespace TCode_Remote.Library.Extension
{
	public static class DictionaryExtension
	{
		public static V GetValue<K, V>(this Dictionary<K, V> dictionary, K key)
		{
			V value = default;
			if (dictionary == null)
				return value;
			try
			{
				dictionary.TryGetValue(key, out value);
				return value;
			} catch
			{
				return value;
			}
		}
	}
}

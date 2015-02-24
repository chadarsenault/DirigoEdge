using System;
using AutoMapper;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DirigoEdge
{
	public static class Caching
	{
		private static ConnectionMultiplexer _redis;

		public static ConnectionMultiplexer Redis
		{
			get { return _redis ?? (_redis = ConnectionMultiplexer.Connect("localhost")); }
		}

		public static IDatabase Cache
		{
			get { return Redis.GetDatabase(); }
		}
	}

	public abstract class CacheableObject
	{
		public abstract void Load();

		public String Key;
		private Boolean Loaded = false;

		protected CacheableObject(object key)
		{
			

			try
			{
				if (key == null)
				{
					key = GetType() + "_undefined_key";
				}

				Key = key.ToString();

				var cachedString = Caching.Cache.StringGet(Key);

				if (String.IsNullOrEmpty(cachedString))
				{
					Load();
					Loaded = true;
					var value = Serialize();
					Caching.Cache.StringSet(Key, value);
				}
				else
				{
					var type = GetType();
					var deserializedObject = Deserialize(cachedString);

					Mapper.CreateMap(type, type);
					Mapper.Map(deserializedObject, this);
				}
			}
			catch (Exception ex)
			{
				if (!Loaded)
				{
					Load();
				}

			}
		}

		protected String Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		protected object Deserialize(String serializedString)
		{
			return JsonConvert.DeserializeObject(serializedString, GetType());
		}
	}
}
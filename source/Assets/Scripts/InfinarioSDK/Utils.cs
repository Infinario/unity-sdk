using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Infinario
{
	public class Utils
	{
		public static double GetCurrentTimestamp()
		{
			var t0 = DateTime.UtcNow;
            
            var tEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var tTime = t0.Subtract(tEpoch).TotalMilliseconds / 1000.0;
           
            var tSanityDateTime = new DateTime(2020, 1, 1, 0, 0, 0);
            var tSanity = (tSanityDateTime.Subtract(tEpoch)).TotalMilliseconds / 1000.0;
            while(tTime > tSanity)
            {
                tTime = tTime / 10.0;
            }
            
            return tTime;
		}
		
		public static string GenerateCookieId()
		{
			return SystemInfo.deviceUniqueIdentifier.ToString ();
		}

		public static bool IsDoubleDefined(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value);
		}

		public static void ExtendDictionary<K, V>(Dictionary<K, V> destination, Dictionary<K, V> source)
		{
			foreach (KeyValuePair<K, V> pair in source)
			{
				destination[pair.Key] = pair.Value;
			}
		}
	}
}


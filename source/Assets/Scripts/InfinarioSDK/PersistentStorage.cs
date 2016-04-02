using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Infinario.MiniJSON;
using System;
using System.Linq;
using System.Globalization;

namespace Infinario.Storage 
{
	class PersistentStorage
	{
		private object lockAccess = new object ();
		private static PersistentStorage instance;

		private PersistentStorage()
		{
		}

		public static PersistentStorage GetInstance()
		{
			if (instance == null)
				instance = new PersistentStorage ();
			return instance;
		}

		public void SaveCookieId(string cookieId)
		{
			lock (lockAccess)
			{
				PlayerPrefs.SetString (Constants.ID_COOKIE, cookieId);
			}
		}

		public string GetCookieId()
		{
			lock (lockAccess)
			{
				var cookieId = PlayerPrefs.GetString (Constants.ID_COOKIE);
				return (String.IsNullOrEmpty(cookieId) ? null : cookieId);
			}
		}

		public void SaveRegisteredId(string registeredId)
		{
			lock (lockAccess)
			{
				PlayerPrefs.SetString (Constants.ID_REGISTERED, registeredId);
			}
		}
		
		public string GetRegisteredId()
		{
			lock (lockAccess)
			{
				var registeredId = PlayerPrefs.GetString (Constants.ID_REGISTERED);
				return (String.IsNullOrEmpty(registeredId) ? null : registeredId);
			}
		}

        public void SaveUserId(string userId)
        {
            lock (lockAccess)
            {
                PlayerPrefs.SetString(Constants.ID_USER, userId);
            }
        }

        public string GetUserId()
        {
            lock (lockAccess)
            {
                var userId = PlayerPrefs.GetString(Constants.ID_USER);
                return (String.IsNullOrEmpty(userId) ? null : userId);
            }
        }

        public void SaveSessionStart(double timestamp, Dictionary<string, object> properties)
		{
			lock (lockAccess)
			{
				PlayerPrefs.SetString (Constants.PROPERTY_SESSION_START_TIMESTAMP, timestamp.ToString("R", CultureInfo.InvariantCulture));
				PlayerPrefs.SetString (Constants.PROPERTY_SESSION_START_PROPERTIES, Json.Serialize(properties));
			}
		}
		
		public double GetSessionStartTimestamp()
		{
			lock (lockAccess)
			{
				var timestamp = PlayerPrefs.GetString (Constants.PROPERTY_SESSION_START_TIMESTAMP);
				return (String.IsNullOrEmpty(timestamp) ? double.NaN : double.Parse(timestamp));
			}
		}

		public Dictionary<string, object> GetSessionStartProperties()
		{
			lock (lockAccess)
			{
				return Json.Deserialize(PlayerPrefs.GetString (Constants.PROPERTY_SESSION_START_PROPERTIES)) as Dictionary<string, object>;
			}
		}

		public void SaveSessionEnd(double timestamp, Dictionary<string, object> properties)
		{
			lock (lockAccess)
			{
				PlayerPrefs.SetString (Constants.PROPERTY_SESSION_END_TIMESTAMP, timestamp.ToString("R", CultureInfo.InvariantCulture));
				PlayerPrefs.SetString (Constants.PROPERTY_SESSION_END_PROPERTIES, Json.Serialize(properties));
			}
		}
		
		public double GetSessionEndTimestamp()
		{
			lock (lockAccess)
			{
				var timestamp = PlayerPrefs.GetString (Constants.PROPERTY_SESSION_END_TIMESTAMP);
				return (String.IsNullOrEmpty(timestamp) ? double.NaN : double.Parse(timestamp));
			}
		}

		public Dictionary<string, object> GetSessionEndProperties()
		{
			lock (lockAccess)
			{
				return Json.Deserialize(PlayerPrefs.GetString (Constants.PROPERTY_SESSION_END_PROPERTIES)) as Dictionary<string, object>;
			}
		}

		public Dictionary<string, object> GetIds()
		{
			Dictionary<string, object> ids = new Dictionary<string, object>();
			
			var cookie = GetCookieId ();
			
			if (string.IsNullOrEmpty (cookie))
			{
				var new_cookie = Utils.GenerateCookieId();
				ids.Add(Constants.ID_COOKIE, new_cookie);
				SaveCookieId(new_cookie);
			} 
			else
			{
				ids.Add(Constants.ID_COOKIE, cookie);
			}
			
			var registered = GetRegisteredId ();
			if (!string.IsNullOrEmpty (registered)) 
				ids.Add(Constants.ID_REGISTERED, registered);

            var userid = GetUserId();
            if (!string.IsNullOrEmpty(userid))
                ids.Add(Constants.ID_USER, userid);
            return ids;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using Infinario.Interface;
using Infinario.SDK;

namespace Infinario
{
	class Infinario
	{
		private IInfinario implementation;
		private volatile static Infinario instance; 
		private static object lockAccess = new object();
		
		private Infinario()
		{
			//Prepare for wrappers
			implementation = new SDK.Unity ();
		}
		
		public static Infinario GetInstance()
		{
			if (instance == null)
				lock (lockAccess)
				{
					instance = new Infinario();
				}
				
			return instance;
		}
		
		public void Initialize(string projectToken)
		{
			Initialize (projectToken, null, null);
		}
		
		public void Initialize(string projectToken, string appVersion)
		{
			Initialize (projectToken, appVersion, null);
		}
		
		public void Initialize(string projectToken, string appVersion, string target)
		{
			implementation.Initialize (projectToken, appVersion, target);
		}
		
		public void Identify(string registeredId)
		{
			Identify(new Dictionary<string, object>() { { Constants.ID_REGISTERED, registeredId } }, new Dictionary<string, object>());
		}
		
		public void Identify(string registeredId, Dictionary<string, object> properties)
		{
			Identify(new Dictionary<string, object>() { { Constants.ID_REGISTERED, registeredId } }, properties);
		}

		public void Identify(Dictionary<string, object> customerIds)
		{
			Identify (customerIds, new Dictionary<string, object>());
		}

		public void Identify(Dictionary<string, object> customerIds, Dictionary<string, object> properties)
		{
			implementation.Identify (customerIds, properties);
		}

		public void Track(string type)
		{
			Track (type, null, double.NaN);
		}
		
		public void Track(string type, Dictionary<string, object> properties)
		{
			Track (type, properties, double.NaN);
		}
		
		public void Track(string type, Dictionary<string, object> properties, double timeStamp)
		{
			implementation.Track (type, properties, timeStamp);
		}
		
		public void Update(Dictionary<string, object> properties)
		{
			implementation.Update (properties);
		}
		
		public void TrackSessionStart()
		{
			TrackSessionStart (null);
		}

		public void TrackSessionStart(Dictionary<string, object> properties)
		{
			implementation.TrackSessionStart (properties);
		}
		
		public void TrackSessionEnd()
		{
			TrackSessionEnd (null);
		}
		
		public void TrackSessionEnd(Dictionary<string, object> properties)
		{
			implementation.TrackSessionEnd (properties);
		}
		
		public void TrackVirtualPayment(string currency, long amount, string itemName, string itemType)
		{
			implementation.TrackVirtualPayment (currency, amount, itemName, itemType);
		}

		public void SessionStatus(bool paused)
		{
			SessionStatus (paused, null);
		}

		public void SessionStatus(bool paused, Dictionary<string, object> properties)
		{
			if (paused)
			{
				TrackSessionEnd(properties);
			} 
			else
			{
				TrackSessionStart(properties);
			}
		}

	}
}


using System.Text;
using System.Collections;
using System.Collections.Generic;
using Infinario.Interface;
using Infinario.Commands;
using Infinario.Storage;
using Infinario.Sender;
using Infinario.MiniJSON;

namespace Infinario.SDK
{
	class Unity : IInfinario
	{
		private string projectToken = null;
		private string appVersion = null;
		private Dictionary<string, object> deviceProperties = null;
		Dictionary<string, object> customerIds = null;

		private static object initializeFinalizeLock = new object();
		private static object sessionLock = new object();

		private PersistentBulkCommandQueue commandQueue = null;
		private PersistentStorage storage = PersistentStorage.GetInstance();

		public override void Initialize(string projectToken, string appVersion, string target)
		{
			this.projectToken = projectToken;
			this.appVersion = appVersion;

			deviceProperties = Device.GetProperties ();
			customerIds = storage.GetIds ();

			commandQueue = new PersistentBulkCommandQueue ("events", Constants.BULK_LIMIT);

			new Sender.Sender ((target != null) ? target : Constants.DEFAULT_TARGET, commandQueue);
		}

		public override void Identify(Dictionary<string, object> customer, Dictionary<string, object> properties)
		{
			if ((customer.ContainsKey (Constants.ID_REGISTERED) && customer[Constants.ID_REGISTERED] != null) ||
				(customer.ContainsKey(Constants.ID_USER) && customer[Constants.ID_USER] != null))
            {
                if (customer.ContainsKey(Constants.ID_REGISTERED)) 
                {
                	customerIds[Constants.ID_REGISTERED] = customer[Constants.ID_REGISTERED];
                }
                else
                {
                    if (customerIds.ContainsKey(Constants.ID_REGISTERED)) 
                    {
                    	customerIds.Remove(Constants.ID_REGISTERED);
                    }
                }
				if (customer.ContainsKey(Constants.ID_USER)) 
				{
					customerIds[Constants.ID_USER] = customer[Constants.ID_USER];
				}
                else
                {
                    if (customerIds.ContainsKey(Constants.ID_USER)) 
                    {
                    	customerIds.Remove(Constants.ID_USER);
                    }
                }

                if ((customer.ContainsKey(Constants.ID_REGISTERED) && !customer[Constants.ID_REGISTERED].Equals(storage.GetRegisteredId())) ||
            		(customer.ContainsKey(Constants.ID_USER) && !customer[Constants.ID_USER].Equals(storage.GetUserId())))
                {
                    if (customer.ContainsKey(Constants.ID_REGISTERED)) 
                    {
                    	storage.SaveRegisteredId(customer[Constants.ID_REGISTERED].ToString());
                    }
                    else 
                    {
                    	storage.SaveRegisteredId(null);
                    }
                    if (customer.ContainsKey(Constants.ID_USER)) 
                    {
                    	storage.SaveUserId(customer[Constants.ID_USER].ToString());
                    }
                    else 
                    {
                    	storage.SaveUserId(null);
                    }
                    Dictionary<string, object> mergedProperties = MergeAutomaticProperties(customer);
					Track (Constants.EVENT_IDENTIFICATION, mergedProperties, double.NaN);
					
					if (properties != null) 
					{
						Update(properties);
					}
				}
			}
		}

		public override void Track(string type, Dictionary<string, object> properties, double timeStamp)
		{
			ScheduleCommand (new TrackCommand (type, properties, timeStamp, projectToken, customerIds));
		}

		public override void Update(Dictionary<string, object> properties)
		{
			ScheduleCommand (new UpdateCommand (properties, projectToken, customerIds));
		}

		public override void TrackSessionStart(Dictionary<string, object> properties)
		{
			lock (sessionLock)
			{
				double now = Utils.GetCurrentTimestamp();
				double sessionEnd = storage.GetSessionEndTimestamp();
				double sessionStart = storage.GetSessionStartTimestamp();

				if (Utils.IsDoubleDefined(sessionEnd))
				{
					if (now - sessionEnd > Constants.SESSION_TIMEOUT)
					{
						SessionEnd(sessionEnd, (sessionEnd - sessionStart), storage.GetSessionEndProperties());
						SessionStart(now, properties);
					}
				}
				else if (!Utils.IsDoubleDefined(sessionStart))
				{
					SessionStart(now, properties);
				} 
				else if (now - sessionStart > Constants.SESSION_TIMEOUT)
				{
					SessionStart(now, properties);
				}
			}
		}

		public override void TrackSessionEnd(Dictionary<string, object> properties)
		{
			lock (sessionLock)
			{
				storage.SaveSessionEnd(Utils.GetCurrentTimestamp(), properties);
			}
		}

		private void SessionStart(double timestamp, Dictionary<string, object> properties)
		{
			storage.SaveSessionStart (timestamp, properties);
			Dictionary<string, object> mergedProperties = MergeAutomaticProperties(properties);

			Track(Constants.EVENT_SESSION_START, mergedProperties, timestamp);
		}

		private void SessionEnd(double timestamp, double duration, Dictionary<string, object> properties)
		{
			Dictionary<string, object> mergedProperties = MergeAutomaticProperties(properties);
			mergedProperties.Add(Constants.PROPERTY_DURATION, duration);

			Track(Constants.EVENT_SESSION_END, mergedProperties, timestamp);

			storage.SaveSessionStart (double.NaN, null);
			storage.SaveSessionEnd (double.NaN, null);
		}

		public override void TrackVirtualPayment(string currency, long amount, string itemName, string itemType)
		{
			Dictionary<string, object> properties = new Dictionary<string, object>(deviceProperties);
			properties.Add(Constants.PROPERTY_CURRENCY, currency);
			properties.Add(Constants.PROPERTY_AMOUNT, amount);
			properties.Add(Constants.PROPERTY_ITEM_NAME, itemName);
			properties.Add(Constants.PROPERTY_ITEM_TYPE, itemType);
			this.Track(Constants.EVENT_VIRTUAL_PAYMENT, properties, double.NaN);
		}

		private Dictionary<string, object> MergeAutomaticProperties(Dictionary<string, object> properties)
		{
			lock (initializeFinalizeLock)
			{
				Dictionary<string, object> mergedProperties = new Dictionary<string, object>(deviceProperties);
				if (appVersion != null)
				{
					mergedProperties.Add(Constants.PROPERTY_APP_VERSION, appVersion);
				}
				if (properties != null)
				{
					Utils.ExtendDictionary(mergedProperties, properties);
				}
				return mergedProperties;
			}
		}

		private void ScheduleCommand(Command command)
		{
			List<object> lst = new List<object> ();
			lst.Add (command.Execute());
			commandQueue.MultiEnqueue(lst);
		}
	}
}

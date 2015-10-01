using System.Collections.Generic;

namespace Infinario.Interface
{
	abstract class IInfinario
	{
		public abstract void Initialize (string projectToken, string appVersion, string target);
		public abstract void Identify (Dictionary<string, object> customer, Dictionary<string, object> properties);
		public abstract void Track (string type, Dictionary<string, object> properties, double timeStamp);
		public abstract void Update(Dictionary<string, object> properties);
		public abstract void TrackSessionStart (Dictionary<string, object> properties);
		public abstract void TrackSessionEnd (Dictionary<string, object> properties);
		public abstract void TrackVirtualPayment (string currency, long amount, string itemName, string itemType);
	}
}
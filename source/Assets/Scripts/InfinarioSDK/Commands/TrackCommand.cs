using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Infinario.Commands
{
	class TrackCommand: Command
	{
		private string eventType;
		private Dictionary<string, object> properties;
		private double timestamp;
		private Dictionary<string, object> ids;
		private string projectToken;

		public TrackCommand(string eventType, Dictionary<string, object> properties, double timestamp, string projectToken, Dictionary<string, object> ids)
		{
			this.eventType = eventType;
			this.properties = properties;
			this.timestamp = Utils.IsDoubleDefined(timestamp) ? timestamp : Utils.GetCurrentTimestamp();
			this.ids = ids;
			this.projectToken = projectToken;
		}

		public object Execute()
		{
			return new Dictionary<string, object>() {
				{"name", Constants.ENDPOINT_TRACK},
				{"data", new Dictionary<string, object>() {
						{"customer_ids", ids},
						{"project_id", projectToken},
						{"type", eventType},
						{"properties", (properties != null) ? properties : new Dictionary<string, object>()},
						{"timestamp", timestamp} 
					}}
			};
		}
	}
}

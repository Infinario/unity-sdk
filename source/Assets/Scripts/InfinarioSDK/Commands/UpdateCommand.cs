using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Infinario.Commands
{
	internal class UpdateCommand: Command
	{		
		private Dictionary<string, object> properties;
		private string projectToken;
		private Dictionary<string, object> ids;
		
		public UpdateCommand(Dictionary<string, object> properties, string projectToken, Dictionary<string, object> ids)
		{
			this.properties = properties;
			this.projectToken = projectToken;
			this.ids = ids;
		}
		
		public object Execute()
		{
		   return new Dictionary<string, object>() {
				{"name", Constants.ENDPOINT_UPDATE},
				{"data", new Dictionary<string, object>() {
						{"ids", ids},
						{"project_id", projectToken},                    
						{"properties", (properties != null) ? properties : new Dictionary<string, object>()}                    
					}}
			};
		}
	}
}

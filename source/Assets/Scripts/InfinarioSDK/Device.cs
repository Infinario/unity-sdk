using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Infinario
{
	class Device
	{
		public static Dictionary<string, object> GetProperties()
		{
			return new Dictionary<string, object>() {
				{"sdk", Constants.SDK},
				{"sdk_version", Constants.VERSION},
				{"device_model", SystemInfo.deviceModel},
				{"device_type", ""},
				{"device_name", SystemInfo.deviceModel},
				{"os_version", SystemInfo.operatingSystem},
				{"os_name", GetPlatform()}                
			};
		}

		private static string GetPlatform() 
		{
			var platform = Application.platform;
			var dict = new Dictionary<RuntimePlatform, string> {
				{RuntimePlatform.Android, "Android"},
				{RuntimePlatform.IPhonePlayer, "iOS"},
				{RuntimePlatform.LinuxPlayer, "Linux"},
				{RuntimePlatform.WSAPlayerARM, "Windows Phone"},
				{RuntimePlatform.WSAPlayerX64, "Windows Store"},
				{RuntimePlatform.WSAPlayerX86, "Windows Store"},
				{RuntimePlatform.OSXDashboardPlayer, "Mac OS X"},
				{RuntimePlatform.OSXEditor, "Mac OS X"},
				{RuntimePlatform.OSXPlayer, "Mac OS X"},
				{RuntimePlatform.OSXWebPlayer, "Mac OS X"},
				{RuntimePlatform.WindowsEditor, "Unity Editor"},
				{RuntimePlatform.WindowsPlayer, "Windows Standalone"},
				{RuntimePlatform.WP8Player, "Windows Phone"},
				{RuntimePlatform.WindowsWebPlayer, "Web Player"}
			};
			return (dict.ContainsKey (platform) ? dict [platform] : "Other");
		}
	}
}

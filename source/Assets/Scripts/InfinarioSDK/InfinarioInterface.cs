using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Infinario.Interface{
	#region Interfaces
	public interface IInfinarioApi{
		void Identify (String name);
		void Identify (String name, object properties);
		void Track (String type);
		void Track (String type, object properties, long time);		
		void Track (String type, long time);
		void Track (String type, object properties);
		void TrackVirtualPayment (String currency, long amount, String itemName, String itemType);
		void TrackAndroidSessionEnd ();
		void Update (object properties);
		void ClearStoredData ();
		void EnablePushNotifications (String senderId,String iconName);
		void EnablePushNotifications (String senderId);
		void DisablePushNotifications ();
		void EnableAutomaticFlushing ();
		void DisableAutomaticFlushing();
		void Flush ();
		void SetAppleDeviceToken ();
	}
	#endregion
}

using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Infinario.Interface;
using Infinario.MiniJSON;
using System.Runtime.InteropServices;
using NotificationServices = UnityEngine.iOS.NotificationServices;


namespace Infinario.iOS {
#if UNITY_IPHONE || UNITY_IOS
	
	public class Infinario :IInfinarioApi{

		[DllImport ("__Internal")]
		private static extern void _shareInstanceWithToken(string companyToken, string target);

		[DllImport("__Internal")]
		private static extern void _identifyWithCustomer(string customerId, string properties);

		[DllImport ("__Internal")]
		private static extern void _track(string type, string properties, string timeStamp);

		[DllImport ("__Internal")]
		private static extern void _trackVirtualPayment(string currency, string amount, string itemName, string itemType);

		[DllImport ("__Internal")]
		private static extern void _update(string properties);

		[DllImport("__Internal")]
		private static extern void _registrationPushNotification();

		[DllImport("__Internal")]
		private static extern void _addPushNotificationToken(string deviceToken);

		[DllImport("__Internal")]
		private static extern void _enableAutomaticFlushing();

		[DllImport("__Internal")]
		private static extern void _disableAutomaticFlushing();

		[DllImport("__Internal")]
		private static extern void _flush();

		bool hasToken;

		public Infinario(string companyToken, string target){
			hasToken = false;
			_shareInstanceWithToken(companyToken, target);
		}

		#region Public API
		public void Identify(String name, object properties){
			_identifyWithCustomer(name, Json.Serialize(properties));
		}

		public void Identify(String name){
			this.Identify(name, null);
		}

		public void Track(String type){
			_track(type, null, null);
		}

		public void Track(String type, object properties, long time){
			_track(type, Json.Serialize(properties), time.ToString());
		}

		public void Track(String type, long time){
			_track(type, null, time.ToString());
		}

		public void Track(String type, object properties){
			_track(type, Json.Serialize(properties), null);
		}

		public void TrackVirtualPayment(String currency, long amount, String itemName, String itemType){
			_trackVirtualPayment(currency, amount.ToString(), itemName, itemType);
		}

		public void TrackAndroidSessionEnd(){
			Debug.Log("Method TrackAndroidSessionEnd works just for Android devices");
		}

		public void Update(object properties){
			_update(Json.Serialize(properties));
		}

		public void ClearStoredData(){
			Debug.Log("Method ClearStoredData works just for Android devices");
		}

		public void EnablePushNotifications(String senderId,string iconName){
			_registrationPushNotification();
		}

		public void EnablePushNotifications(String senderId){
			this.EnablePushNotifications(senderId, null);
		}

		public void DisablePushNotifications(){
			Debug.Log("Method DisablePushNotifications works just for Android devices");
		}

		public void EnableAutomaticFlushing(){
			_enableAutomaticFlushing();
		}

		public void DisableAutomaticFlushing(){
			_disableAutomaticFlushing();
		}

		public void Flush(){	
			_flush();
		}

		public void SetAppleDeviceToken(){
			if (!hasToken) {
				byte[] token = NotificationServices.deviceToken;
				if (token != null) {
					string hexToken = System.BitConverter.ToString(token).Replace("-","");
					_addPushNotificationToken(hexToken);
					hasToken = true;
				}
			}
		}
		#endregion
	}	
#endif
}

using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Infinario.Interface;

namespace Infinario.Android {
#if UNITY_ANDROID

	public class Infinario : IInfinarioApi {
		private AndroidJavaObject androidSDK;
		private AndroidJavaObject activity;

		public Infinario(string companyToken, string target){
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");	
			if (target == null) {
				androidSDK = new AndroidJavaClass("com.infinario.android.infinariosdk.Infinario").CallStatic<AndroidJavaObject>("getInstance", new object[]{activity, companyToken});
			} else {
				androidSDK = new AndroidJavaClass("com.infinario.android.infinariosdk.Infinario").CallStatic<AndroidJavaObject>("getInstance", new object[]{activity, companyToken, target});
			}
		}

		#region Public API
		public void Identify(String name, object properties){
			if (properties != null){
				androidSDK.Call("identify", new object[]{name, CrossPlatformHelper.DictionaryToHashMap(properties)});
			} else {
				androidSDK.Call("identify", name);
			}
		}

		public void Identify(String name){
			androidSDK.Call("identify", name);
		}

		public void Track(String type){
			androidSDK.Call<bool>("track", new object[]{type});
		}

		public void Track(String type, object properties, long time){
			androidSDK.Call<bool>("track", new object[]{type, CrossPlatformHelper.DictionaryToHashMap(properties), time});
		}

		public void Track(String type, long time){
			androidSDK.Call<bool>("track", new object[]{type, time});
		}

		public void Track(String type, object properties){
			androidSDK.Call<bool>("track", new object[]{type, CrossPlatformHelper.DictionaryToHashMap(properties)});
		}

		public void TrackVirtualPayment(String currency, long amount, String itemName, String itemType){
			androidSDK.Call("trackVirtualPayment", new object[]{currency, amount, itemName, itemType});
		}

		public void TrackAndroidSessionEnd(){
			androidSDK.Call("trackSessionEnd");
		}

		public void Update(object properties){
			androidSDK.Call<bool>("update", new object[]{CrossPlatformHelper.DictionaryToHashMap(properties)});
		}

		public void ClearStoredData(){
			androidSDK.Call("clearStoredData");
		}

		public void EnablePushNotifications(String senderId,string iconName){
			androidSDK.Call("enablePushNotifications", new object[]{senderId, iconName});
		}

		public void EnablePushNotifications(String senderId){
			this.EnablePushNotifications (senderId, "infinario_notification_icon");
		}

		public void DisablePushNotifications(){
			androidSDK.Call("disablePushNotifications");
		}

		public void EnableAutomaticFlushing(){
			androidSDK.Call("enableAutomaticFlushing");
		}

		public void DisableAutomaticFlushing(){
			androidSDK.Call("disableAutomaticFlushing");
		}

		public void Flush(){
			androidSDK.Call("flush");
		}

		public void SetAppleDeviceToken (){
		}
		#endregion
	}

	public class CrossPlatformHelper{
		static public AndroidJavaObject DictionaryToHashMap(object parameters) {
			AndroidJavaObject JNIMap = new AndroidJavaObject ("java.util.HashMap");			
			IntPtr method_Put = AndroidJNIHelper.GetMethodID(JNIMap.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");			
			object[] args = new object[2];
			foreach(KeyValuePair<string, object> kvp in parameters as Dictionary<string, object>) {		
				using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key))
				{
					using(AndroidJavaObject v = convertToAndroidObject(kvp.Value))
					{
						args[0] = k;
						args[1] = v;
						AndroidJNI.CallObjectMethod(JNIMap.GetRawObject(), method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
					}
				}
			}			
			
			return JNIMap;
		}
		
		static public AndroidJavaObject convertToAndroidObject(object value){
			if (value.GetType() == typeof(bool)){
				return new AndroidJavaObject("java.lang.Boolean", value);
			} 
			if (value.GetType() == typeof(byte)){
				return new AndroidJavaObject("java.lang.Byte", value);
			} 
			if (value.GetType() == typeof(short)){
				return new AndroidJavaObject("java.lang.Short", value);
			} 
			if (value.GetType() == typeof(int)){
				return new AndroidJavaObject("java.lang.Integer", value);
			} 
			if (value.GetType() == typeof(long)){
				return new AndroidJavaObject("java.lang.Long", value);
			}
			if (value.GetType() == typeof(float)){
				return new AndroidJavaObject("java.lang.Float", value);
			} 
			if (value.GetType() == typeof(double)){
				return new AndroidJavaObject("java.lang.Double", value);
			} 
			if (value.GetType() == typeof(char)){
				return new AndroidJavaObject("java.lang.Character", value);
			} 
			if (value.GetType() == typeof(string)){
				return new AndroidJavaObject("java.lang.String", value);
			}
			
			return new AndroidJavaObject("java.lang.String", "Unknown type");			
		}
	}
#endif
}

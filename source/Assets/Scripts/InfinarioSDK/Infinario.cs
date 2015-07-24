using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Infinario.Interface;
using Infinario.Unity;

namespace Infinario {

	public class Infinario : IInfinarioApi{

		IInfinarioApi implementation;
	
		public Infinario (String companyToken) : this(companyToken, null){
		}

		public Infinario (String companyToken, String target){
			#if UNITY_ANDROID
			try {
				implementation = (IInfinarioApi) Activator.CreateInstance(Type.GetType("Infinario.Android.Infinario"), new object[]{companyToken, target});
				Debug.Log("Found Android Plugin");
			}
			catch  {
				// log exception
				Debug.Log("Couldn't instantiate native Android Plugin, falling back to Unity-only implementation");
				implementation = new Unity.Infinario(companyToken, target);
			}
			#elif UNITY_IPHONE || UNITY_IOS
			try{
				implementation = (IInfinarioApi) Activator.CreateInstance(Type.GetType("Infinario.iOS.Infinario"), new object[]{companyToken, target});
				Debug.Log("Found iOS Plugin");
			} catch {
				Debug.Log("Couldn't instantiate native iOS Plugin, falling back to Unity-only implementation");
				implementation = new Unity.Infinario(companyToken, target);
			}
			#else
			Debug.Log("Unity Infinario SDK");
			implementation = new Unity.Infinario(companyToken, target);
			#endif
		}

		public void Identify (String name){
			this.Identify(name, null);
		}

		public void Identify (String name, object properties){
			implementation.Identify(name, properties);
		}

		public void Track (String type){
			implementation.Track(type);
		}

		public void Track (String type, object properties, long time){
			implementation.Track(type, properties, time);
		}

		public void Track (String type, long time){
			implementation.Track(type, time);
		}

		public void Track (String type, object properties){
			implementation.Track (type, properties);
		}

		public void TrackVirtualPayment (String currency, long amount, String itemName, String itemType){
			implementation.TrackVirtualPayment (currency, amount, itemName, itemType);
		}

		public void TrackAndroidSessionEnd (){
			implementation.TrackAndroidSessionEnd();
		}

		public void Update (object properties){
			implementation.Update (properties);
		}

		public void ClearStoredData (){
			implementation.ClearStoredData ();
		}

		public void EnablePushNotifications (String senderId,string iconName){
			implementation.EnablePushNotifications (senderId, iconName);
		}

		public void EnablePushNotifications (String senderId){
			implementation.EnablePushNotifications (senderId);
		}

		public void DisablePushNotifications (){
			implementation.DisablePushNotifications ();
		}

		public void EnableAutomaticFlushing (){
			implementation.EnableAutomaticFlushing ();
		}

		public void DisableAutomaticFlushing (){
			implementation.DisableAutomaticFlushing ();
		}

		public void Flush (){
			implementation.Flush ();
		}

		public void SetAppleDeviceToken (){
			implementation.SetAppleDeviceToken();
		}
	}		
}

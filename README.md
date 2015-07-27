# Getting started with Infinario Unity SDK (in 6 minutes)

Infinario Unity SDK is available in this Git repository: <a href="https://github.com/infinario/unity-sdk">https://github.com/infinario/unity-sdk</a>. It provides tracking capabilities for your application.

## Installation

* Download or clone this repository from your command line: ```git clone https://github.com/Infinario/unity-sdk.git```
* For Unity C# use the Unity Editor to import the provided ```C_SHARP_UNITY_SDK.unitypackage``` (Assets->Import Package->Custom Package...).
* For Unity JS use the Unity Editor to import the provided ```JS_UNITY_SDK.unitypackage``` (Assets->Import Package->Custom Package...).

## Plugins

* <strong>Android Plugin</strong> - use the Unity Editor to import the provided ```C_SHARP_ANDROID_PLUGIN.unitypackage``` for C# or ```JS_ANDROID_PLUGIN.unitypackage``` for JS (Assets->Import Package->Custom Package...).<br>Plugin contains Infinario Android SDK v1.1.1, Google Play Services and Android Support v4 libraries.
* <strong>iOS Plugin</strong> - use the Unity Editor to import the provided ```C_SHARP_IOS_PLUGIN.unitypackage``` for C# or ```JS_IOS_PLUGIN.unitypackage``` for JS (Assets->Import Package->Custom Package...).<br>For this plugin you need Store.framework, AdSupport.framework and sqlite3.dylib. If they are missing in your project, you can add them manually in xcode throught Build Phases -> Link Binary With Libraries

## Usage

### Basic Tracking

To start tracking, you need to know your ```company_token```. To initialize the tracking, simply create an instance of the ```Infinario``` class:

```
var infinario = new Infinario.Infinario(“your_company_token”);
```

Now you can track events by calling the ```Track``` method:
```
infinario.Track("my_user_action");
```
What happens now, is that an event called ```my_user_action``` is recorded for the current player.

### Identifying Players
To control the identity of the current player use the ```Identify``` method. By calling
```
infinario.Identify("player@example.com");
```

you can register a new player in Infinario. All events you track by the ```Track``` method from now on will belong to this player. To switch to an existing player, simply call ```Identify``` with his name. You can switch the identity of the current player as many times as you need to.

### Anonymous Players
Up until you call ```Identify``` for the first time, all tracked events belong to an anonymous player (internally identified with a cookie). Once you call ```Identify```, the previously anonymous player is automatically merged with the newly identified player.

### Adding Properties
Both ```Identify``` and ```Track``` accept an optional dictionary parameter that can be used to add custom information (properties) to the respective entity. Usage is straightforward:

C#
```
infinario.Track("my_player_action", new Dictionary<string,object> {
                                                          {"daily_score", 4700}
                                                        });                                       

infinario.Identify("player@example.com", new Dictionary<string,object> {
                                                          {"first_name", "John"},
                                                          { "last_name", "Doe" }
                                                        }); 
infinario.Update(new Dictionary<string,object> {{"level", 1}}); // A shorthand for adding properties to the current customer
```
JS
```
infinario.Track("my_player_action", {"daily_score": 4700});

infinario.Identify("player@example.com", {"first_name": "John",
                                          "last_name": "Doe"});

infinario.Update({"level": 1}); // A shorthand for adding properties to the current customer

```
### Virtual payment
If you use virtual payments (e.g. purchase with in-game gold, coins, ...) in your project, you can track them with a call to TrackVirtualPayment.
```
infinario.TrackVirtualPayment ("gold", 3, "sword", "sword_type”);
```
### Timestamps
The SDK automatically adds timestamps to all events. To specify your own timestamp, use one of the following method overloads:
```
infinario.Track("my_player_action", <long_your_tsp>);
infinario.Track("my_player_action", <properties> , <long_your_tsp>);	
```
*Tip:* To obtain the current UNIX timestamp, you can use  ```Infinario.Command.Epoch()```.

### Player Sessions
Infinario automatically manages player sessions. Each session starts with a ```session_start``` event and ends with ```session_end```. Sessions are terminated by either timeout (currently 20 minutes of inactivity) or on player logout (caused by calling ```Identify``` on a different player).

Once started, the SDK tries to recreate the previous session from its persistent cache. If it fails to, or the session has already expired it automatically creates a new one.

if you use Android plugin, please call TrackAndroidSessionEnd somewhere at the end of the game loop.

### Plugin features

#### Advertising IDs
Plugins can track google or apple advertising ids of devices.

#### Push notifications

Infinario web application allows you to easily create complex scenarios which you can use to send push notifications directly to your customers. The following section explains how to enable receiving push notifications in the Android plugin.

For push notifications to work, you need a working Google API project. The following steps show you how to create one. If you already have created a Google API project and you have your <strong>project number (or sender ID)</strong> and <strong>Google Cloud Messaging API key</strong>, you may skip this part of the tutorial and proceed directly to enabling of the push notifications in the Infinario Android SDK.

#### Google API project
* In your preferred browser, navigate to <a href="https://console.developers.google.com/">https://console.developers.google.com/</a>
* Click on <strong>Create Project</strong> button
* Fill in preferred project name and click <strong>Create</strong> button
* Please wait for the project to create, it usually takes only a few seconds
* After the project has been created you will be redirected to the <strong>Project Dashboard</strong> page where you'll find <strong>Project Number</strong> which is needed in the Infinario Android SDK
* In the left menu, navigate to <strong>APIs &amp; auth -&gt; APIs</strong> and find <strong>Google Cloud Messaging for Android</strong>
* Please make sure the Google Cloud Messaging for Android is <strong>turned ON</strong>
* In the left menu, navigate to <strong>APIs &amp; auth -&gt; Credentials</strong> and click on <strong>Create new Key</strong> button
* Click on <strong>Server key</strong> button and the click on <strong>Create</strong> button
* Copy the API key which is needed for the Infinario web application

#### Infinario web application
Once you have obtained <strong>Google Cloud Messaging API key</strong>, you need to enter it in the input field on the <strong>Company / Settings / Notifications</strong> in the Infinario web application.

#### Apple Push certificate

For push notifications to work, you need a push notifications certificate with a corresponding private key in a single file in PEM format. The following steps show you how to export one from the Keychain Access application on your Mac:

* Launch Keychain Access application on your Mac
* Find Apple Push certificate for your app in <em>Certificates</em> or <em>My certificates</em> section (it should start with <strong>&quot;Apple Development IOS Push Services:&quot;</strong> for development certificate or <strong>&quot;Apple Production IOS Push Services:&quot;</strong> for production certificate)
* The certificate should contain a <strong>private key</strong>, select both certificate and its corresponding private key, then right click and click <strong>Export 2 items</strong>
* In the saving modal window, choose a filename and saving location which you prefer and select the file format <strong>Personal Information Exchange (.p12)</strong> and then click <strong>Save</strong>
* In the next modal window, you will be prompted to choose a password, leave the password field blank and click <strong>OK</strong>. Afterwards, you will be prompted with you login password, please enter it.
* Convert p12 file format to PEM format using OpenSSL tools in terminal. Please launch <strong>Terminal</strong> and navigate to the folder, where the .p12 certificate is saved (e.g. <code>~/Desktop/</code>)
* Run the following command <code>openssl pkcs12 -in certificate.p12 -out certificate.pem -clcerts -nodes</code>, where <strong>certificate.p12</strong> is the exported certificate from Keychain Access and <strong>certificate.pem</strong> is the converted certificate in PEM format containing both Apple Push certificate and its private key
* The last step is to upload the Apple Push certificate to the INFINARIO web application. In the INFINARIO web application, navigate to <strong>Project management -&gt; Settings -&gt; Notifications</strong>
* Copy the content of <strong>certificate.pem</strong> into <strong>Apple Push Notifications Certificate</strong> and click <strong>Save</strong>

Now you are ready to implement Push Notifications into your iOS application.

#### Implementation
By default, receiving of push notifications is disabled. You can enable them by calling the simple method ```EnablePushNotifications()```. 
```
//for google you need to have your project number
infinario.EnablePushNotifications ("123456"); // this method enable ios push notification as well. If you build apps just for iOS devices, you dont need to have project number. Then just call this method with empty string ""

//if you want to have custom icon for google push notification, use this method with name of the icon
//location of the icon should be in res/drawable... folder in plugin
infinario.EnablePushNotifications ("123456", "ic_push"); // enable ios push notification as well. (iOS push notification cannot use custom icons)

//if you build apps for iOS devices, you must call infinario.SetAppleDeviceToken in Update method
//It is very important, because this method will track ios device token
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EXAMPLE : MonoBehaviour {

	Infinario.Infinario infinario;
	void Start () {
		infinario = new Infinario.Infinario("XXX");
		infinario.EnablePushNotifications ("");
	}
	
	// Update is called once per frame
	void Update () {
		infinario.SetAppleDeviceToken ();
	}
}

//
```

#### Flushing events
All tracked events are stored in the internal SQL database. By default, Infinario SDK automagically takes care of flushing events to the Infinario API. This feature can be turned off with method ```DisableAutomaticFlushing()``` which takes no arguments. Please be careful with turning automatic flushing off because if you turn it off, you need to manually call ```Flush();``` to flush the tracked events manually everytime there is something to flush.

### Offline Behavior

Once instantized, the SDK collects and sends all tracked events continuously to the Infinario servers. 

However, if your application goes offline, the SDK guarantees you to re-send the events once online again (up to a approximately 5k offline events). This synchronization is transparent to you and happens in the background.

## Final Remarks
- Make sure you create at most one instance of ```Infinario``` during your application lifetime.
- If you wish to override some of the capabilities (e.g. session management), please note that we will not be able to give you any guarantees.
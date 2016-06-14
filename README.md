# Getting started with Infinario Unity SDK (in 6 minutes)
[![Slack Status](http://community.exponea.com/badge.svg)](https://community.exponea.com/)

Infinario Unity SDK is available in this Git repository: <a href="https://github.com/infinario/unity-sdk">https://github.com/infinario/unity-sdk</a>. It provides tracking capabilities for your application.

## Installation

Download or clone this repository from your command line: ```git clone https://github.com/Infinario/unity-sdk.git```

## Usage

### Basic Tracking

To start tracking, you need to know your `projectToken`. To initialize the tracking, simply get an instance of the `Infinario` class and call just once `Initialize`:

```
var infinario = Infinario.Infinario.GetInstance();
infinario.Initialize("projectToken");

//or if you want to track app version as well
infinario.Initilize("projectToken", "1.0.0");
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

### Player Sessions
Session is a real time spent in the game, it starts when the game is launched and ends when the game goes to background. But if the player returns to game in 60 seconds, game will continue in current session. Tracking of sessions produces two events, ```session_start``` and ```session_end```. Put below code to your ```MonoBehaviour``` object like MainCamera.  

C#
```
Infinario.Infinario infinario;

void Start () {
    infinario = Infinario.Infinario.GetInstance();
    infinario.Initialize("projectToken");

    infinario.TrackSessionStart();
}

void OnApplicationPause(bool pauseStatus) {
    infinario.SessionStatus(pauseStatus);
}
```
JS
```
Infinario.Infinario infinario;

function Start () {
    infinario = Infinario.Infinario.GetInstance();
    infinario.Initialize("projectToken");

    infinario.TrackSessionStart();
}

function OnApplicationPause(pauseStatus: boolean) {
    infinario.SessionStatus(pauseStatus);
}
```
Both events contain the timestamp of the occurence together with basic attributes about the device (OS, OS version, SDK, SDK version and device model). Event `session_end` contains also the duration of the session in seconds.

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


### Offline Behavior

Once instantized, the SDK collects and sends all tracked events continuously to the Infinario servers. 

However, if your application goes offline, the SDK guarantees you to re-send the events once online again (up to a approximately 5k offline events). This synchronization is transparent to you and happens in the background.

## Final Remarks
- Make sure you create at most one instance of ```Infinario``` during your application lifetime.
- If you wish to override some of the capabilities (e.g. session management), please note that we will not be able to give you any guarantees.
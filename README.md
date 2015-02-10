# Infinario Unity SDK

Infinario Android SDK is available in this Git repository: <a href="https://github.com/infinario/unity-sdk">https://github.com/infinario/unity-sdk</a>.

## Installation

The most straightforward way to install the SDK is to copying the contents of the source/Assets/Scripts directory to your Unity project's Scripts/ directory.

## Usage

### Basic Interface

To start tracking, you need to know your ```company_token``` and create an instance of the ```InfinarioApi``` class using it. You can also pass a second (optional) parameter to override the URL of the Infinario API.

```
var infinario = new Infinario.Infinario(<your_company_token>);
```

The SDK automatically creates a new anonymous player whenever started, or loads the last player it has seen on the device. You can control the identity of the current user by calling the ```Identify``` method with a string parameter denoting his registration name.

The communication between the SDK and the Infinario servers are wrapped in a coroutine, so they won't block the processing in your main application.

Here is how you could start tracking a new registered customer (along with passing an email property to his profile):
```
infinario.Identify(customer_id, new Dictionary<string, object> () {{"email","someone@example.com"}});
```

There is a similar method called ```Update```, that takes only one parameter with a dictionary of properties you wish to set to the current player:
```
infinario.Update(new Dictionary<string, object> () {{"gender","f"}, {"age",1990}});
```

## Tracking

The client collects and sends all tracked events continuously to the Infinario servers. However, if your application goes offline, the SDK guarantees you to re-send the events once online again. This synchronization is transparent to you and happens in the background.

You track various events by utilizing the ```Track``` function.
The only required field is a string describing the type of your event.
By default the time of the event is tracked as the epoch time,
and diffed against the server time of Infinario before sending.

As with players, you can attach properties to each event by leveraging the ```properties``` argument of the ```Track``` method. 

```
infinario.Track("login");
```

## Session Management

The client manages sessions automatically and transparently, with a 20 minute inactivity timeout. Session events ```session_start``` and ```session_end``` are always tracked to the current player and you can find them in your Infinario application in the Customer screen.

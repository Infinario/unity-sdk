# Infinario Unity SDK

Infinario Android SDK is available in this Git repository: <a href="https://github.com/infinario/unity-sdk">https://github.com/infinario/unity-sdk</a>.

## Installation

Sources are readily aviable, as well as an exported unitypackage in the root of this repository.

## Usage

### Basic Interface

To start tracking, you need to know the uri of your Infinario API instance, usually ```https://api.infinario.com/```, your ```company_token``` and generate a unique ```customer_id``` for the customer you are about to track. The unique ```customer_id``` can either be string, or an object representing the ```customer_ids``` as referenced in [the API guide](http://guides.infinario.com/technical-guide/rest-client-api/#Detailed_key_descriptions).
Setting ```customer_id = "123-asdf"``` is equivalent to ```customer_id = new Dictionary<String, String> () {{"registered","123-adf"}};```


```
var infinario = new Infinario(company_token, server_uri, customer_id);
```

If this is the beginning of tracking of a new user, you need to first ```Identify``` him against the server.

All of the commands spawn a coroutine, so that they won't block the processing in your main application.

```
infinario.Identify (customer_id, new Dictionary<String, Object> () {{"email","asdf@asdf.com"}});
```

For serialization we utilize small embedded MiniJSON parser. 

You track various events by utilizing the Track function.
It assumes you want to track the customer you identified by ```Identify```.
The only required field is a string describing the type of your event.
By default the time of the event is tracked as the epoch time,
and diffed against the server time of Infinario before sending.

```
infinario.Track ("login");
```

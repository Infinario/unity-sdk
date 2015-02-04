using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using MiniJSON;

namespace Infinario {

	/// <summary>
	/// Abstract class representing an Infinario API command.
	/// </summary>
	public abstract class Command {	
		private InfinarioIdentity _identity;

		public static long Epoch() {
			var t0 = DateTime.UtcNow;
			var tEpoch = new DateTime (1970, 1, 1, 0, 0, 0);
			return (long)Math.Truncate (t0.Subtract (tEpoch).TotalSeconds);
		}
		
		public abstract String Endpoint {
			get ;
		}

		/// <summary>
		/// Holds the identity of the player (at the moment when the command was created).
		/// </summary>
		public InfinarioIdentity Identity {
			set {
				this._identity = value;
			}
			get {
				return this._identity;
			}
		}

		public abstract object JsonPayload {
			get ;
		}

		public virtual object JsonSerialize {
			get { 
				return JsonPayload;
			}
		}

		public override String ToString() {
			return "curl -X \'POST\' https://api.infinario.com/" + Endpoint + "  -H \"Content-type: application/json\" -d \'" +
				Json.Serialize (JsonPayload) + "\'";
		}

		public object BulkSerialization() {
			return new Dictionary<String,object> () {
				{ "name",this.Endpoint },
				{ "data",this.JsonSerialize }
			};
		}

		public object BulkRepresentation() {
			return new Dictionary<String,object> () {
				{ "name",this.Endpoint },
				{ "data",this.JsonPayload }
			};
		}
		
		public String SerializeToJson() {
			return Json.Serialize (BulkRepresentation());
		} 
	}
	
	public class CustomerCommand : Command {
		private const string CUSTOMERS_API_ROUTE = "crm/customers";

		private String Company;
		private object Properties;

		public CustomerCommand (String company, InfinarioIdentity identity, object properties) {
			this.Company = company;
			this.Identity = identity;
			this.Properties = properties;
		}

		public override String Endpoint {
			get { 
				return CUSTOMERS_API_ROUTE;
			}
		}

		public override object JsonPayload {
			get { 
					var dict = new Dictionary<object,object>() {
						{"ids",  this.Identity.ToDictionary("cookie", "registered")},
						{"company_id",  Company}
					};
					if (this.Properties != null){
						dict.Add("properties",this.Properties);
					}
					return dict;
				}
		}
	}
	
	public class EventCommand : Command {
		private const string EVENTS_API_ROUTE = "crm/events";

		private String Type;
		private long Time;
		private String Company;
		private object Properties;
		
		public EventCommand (String company, InfinarioIdentity identity, String type, object properties, long time) {
			this.Company = company;
			this.Identity = identity;
			this.Properties = properties;
			this.Type = type;
			this.Time = time;
		}
		
		public override String Endpoint {
			get { 
				return EVENTS_API_ROUTE;
			}
		}

		public override object JsonSerialize {
			get { 
				var dict = new Dictionary<object,object>(){
					{"customer_ids", this.Identity.ToDictionary()},
					{"company_id",  this.Company}, 
					{"type",  this.Type}, 
					{"time",  this.Time}
				};

				if (this.Properties != null) {
					dict.Add ("properties",this.Properties);
				}
				return dict;
			}
		}

		public override object JsonPayload {
			get { 
				var dict = new Dictionary<object,object>(){
					{"customer_ids", this.Identity.ToDictionary()},
					{"company_id", this.Company}, 
					{"type", this.Type}, 
					{"age", Epoch () - this.Time},
				};
				if (this.Properties != null) {
					dict.Add ("properties", this.Properties);
				}
				return dict;
			}
		}
	}
	
	public interface IInfinarioApi{
		/// <summary>
		/// Identifies the current player with a name (corresponding to the field 'registered' in the customer's profile in Infinario). You can use this method to:
		/// 	* identify an anonymous player/customer
		/// 	* switch players/customer for which the subsequent tracking events apply.
		/// 
		/// Note on anonymous players/customers: 
		/// 	Whenever you start tracking and Infinario SDK fails to load the current player's identity from your local cache, an anonymous player is created and all events are tracked for him. Once you call Identify, 
		/// 	this player will keep all his events from back when he was anonymous (and be merged with an existing player if their name parameters match.)
		/// </summary>
		/// <param name="name">Any string by which you wish to identify the player ().</param>
		/// <param name="properties">An optional dictionary of (new) properties to add to this player.</param>
		void Identify (String name, object properties);		
		void Track (String type, object properties, long time);		
		void Track (String type, long time);
		void Track (String type, object properties);
		void Update (object properties);
		void NewAnonymous();
	}

	/// <summary>
	/// A common interface to classes that implement player/customer identity persistence.
	/// </summary>
	public interface IPlayerPersistenceAdapter {
		InfinarioIdentity LoadIdentity();
		void SaveIdentity (InfinarioIdentity identity);
	}

	/// <summary>
	/// A driver to the Unity's PlayerPref storage.
	/// </summary>
	public class PlayerPrefPersistenceAdapter:IPlayerPersistenceAdapter{
		const string IDENTITY_PREF = "infinario_identity";
		const string COOKIE_KEY = "cookie";
		const string REGISTERED_KEY = "registered";

		/// <summary>
		/// Loads the identity of the current (previously saved) customer.
		/// </summary>
		/// <returns>An InfinarioIdentity object.</returns>
		public InfinarioIdentity LoadIdentity() {

			if (!PlayerPrefs.HasKey (IDENTITY_PREF)) {
				return new InfinarioIdentity(InfinarioIdentity.GenerateGUID(),String.Empty);
			}

			string identityString = PlayerPrefs.GetString (IDENTITY_PREF);
			InfinarioIdentity identity = new InfinarioIdentity ();

			if (identityString != String.Empty) {
				Dictionary<string,object> identityInfo;
				identityInfo = Json.Deserialize(identityString) as Dictionary<string,object>;
				object cookie;

				if(identityInfo.TryGetValue(COOKIE_KEY, out cookie)) {
					identity.Cookie = (string)cookie;
				} else {
					identity.Cookie = InfinarioIdentity.GenerateGUID();
				}

				object registered;
				if(identityInfo.TryGetValue(REGISTERED_KEY, out registered)) {
					identity.Registered = (string)registered;
				}
			}
//			Debug.Log ("Recovered the following identity: " + identity.ToString ());
			return identity;
		}

		/// <summary>
		/// Persistently saves the current customer's identity.
		/// </summary>
		/// <param name="identity">InfinarioIdentity object.</param>
		public void SaveIdentity(InfinarioIdentity identity) {
			PlayerPrefs.SetString(IDENTITY_PREF, identity.ToJson());
			PlayerPrefs.Save ();
//			Debug.Log ("Saved the following identity: " + identity.ToString ());
		}

	}
			
	/// <summary>
	/// Represents a single customer (anonymous or not).
	/// </summary>
	public struct InfinarioIdentity {
		public string Cookie;
		public string Registered;
		
		public InfinarioIdentity(String cookie=null, String registered=null) {
			this.Cookie = (cookie == null || cookie == String.Empty ? GenerateGUID() : cookie);
			this.Registered = (registered == null || cookie == String.Empty ? null: registered);
		}

		public string ToJson(string cookieKey = "cookie", string registeredKey = "registered") {
			return Json.Serialize(ToDictionary(cookieKey, registeredKey));
		}

		public static string GenerateGUID() {
			var random = new System.Random();                     		
			return  Application.systemLanguage                            				   //Language
				+"-"+String.Format("{0:X}", Convert.ToInt32(Command.Epoch()))              //Time
					+"-"+String.Format("{0:X}", Convert.ToInt32(Time.time*1000000))        //Time in game
					+"-"+String.Format("{0:X}", random.Next(1000000000));
		}

		public Dictionary<string,object> ToDictionary(string cookieKey = "cookie", string registeredKey = "registered") {
			if (this.Registered != String.Empty && this.Registered != null) {
				return new Dictionary<string,object> {
					{cookieKey, Cookie},
					{registeredKey, Registered}
				};
			} else {
				return new Dictionary<string,object> {
					{cookieKey, Cookie}				
				};
			}
		}

		public override string ToString() {
			return String.Format ("<cookie:'{0}',registered:'{1}'>", this.Cookie, this.Registered);
		}

		public override bool Equals(System.Object o) {
			if (!(o is InfinarioIdentity)) {
				return false;
			} else {
				try{
					InfinarioIdentity obj = (InfinarioIdentity)o;
					bool cookieEq = (obj.Cookie == null && this.Cookie == null) || (obj.Cookie != null && this.Cookie !=null && obj.Cookie.Equals (this.Cookie));
					bool regEq = (obj.Registered == null && this.Registered == null) || (obj.Registered != null && this.Registered !=null && obj.Registered.Equals (this.Registered));
					return cookieEq && regEq;
				} catch(NullReferenceException) {
					return false;
				}
			}
		}

		public override int GetHashCode() {
			return (this.Cookie!=null?this.Cookie.GetHashCode():1234) ^ (this.Registered!=null?this.Registered.GetHashCode():1234);
		}
	}
	
	public class InfinarioApi : IInfinarioApi {
		private const string INFINARIO_API_URI = "https://api.infinario.com/";
		private const string INFINARIO_API_BULK_ROUTE = "bulk";
		private const int BULK_SIZE = 49;
	
		protected readonly String CompanyToken;
		protected readonly String Target;
		protected object Customer;
		protected InfinarioIdentity CurrentIdentity;
		protected IPlayerPersistenceAdapter PersistenceAdapter = new PlayerPrefPersistenceAdapter();
		protected static PersistentBulkCommandQueue CommandQueue = new PersistentBulkCommandQueue ("events",BULK_SIZE);

		#region Contructors
		/// <summary>
		/// Initializes and starts a new tracking session.
		/// </summary>
		/// <param name="companyToken">Your company token.</param>
		/// <param name="target">The base URI to the Infinario API (you usually do not have to set this parameter).</param>
		public InfinarioApi(String companyToken, String target = INFINARIO_API_URI) {
			this.CompanyToken = companyToken;
			this.Target = target;
			this.AdjustCurrentIdentity (true); // retrieve the saved identity
			this.StartSendLoop ();
		}
		#endregion

		#region identity management
		/// <summary>
		/// Adjusts the current customer identity and persistently stores it.
		/// </summary>
		/// <param name="newCookie">New cookie_id of the customer.</param>
		/// <param name="newRegistered">New registered_id of the customer.</param>
		protected void AdjustCurrentIdentity(string newCookie, string newRegistered, object properties=null) {
			if (newCookie != String.Empty) {
				this.CurrentIdentity.Cookie = newCookie;
			}
			if (newRegistered != String.Empty) {
				this.CurrentIdentity.Registered = newRegistered;
			}

			if (!this.PersistenceAdapter.LoadIdentity ().Equals(this.CurrentIdentity) || properties!=null) {
//				Debug.Log ("Changing identity");
				this.PersistenceAdapter.SaveIdentity (this.CurrentIdentity);
				this.ScheduleCustomer(this.CompanyToken, this.CurrentIdentity, properties);
			}	
		}

		protected void AdjustCurrentIdentity(Boolean loadPersistent = true) {
			var recoveredIdentity = this.PersistenceAdapter.LoadIdentity ();
			this.AdjustCurrentIdentity (recoveredIdentity.Cookie, recoveredIdentity.Registered);
		}
		#endregion

		#region Identify(...)
		public void Identify(String name, object properties=null){
			this.AdjustCurrentIdentity(String.Empty, name, properties);
		}

		public void Identify(){
			this.Identify(String.Empty);
		}
		#endregion		
	
		#region Track(...)
		public void Track(String EventType, object Properties, long Time){
			ScheduleCommand(new EventCommand(this.CompanyToken, this.CurrentIdentity, EventType, Properties, Time));
		}
		
		public void Track(String EventType, long Time=long.MinValue){
			this.Track(EventType, null, Time == long.MinValue ? Command.Epoch() : Time);
		}
				
		public void Track(String EventType,object Properties){
			this.Track(EventType,Properties, Command.Epoch());
		}
		#endregion(...)

		/// <summary>
		/// Changes the current player's identity to a new anonymous player.
		/// </summary>
		public void NewAnonymous() {
			this.ClearIdentity ();
		}

		private void ClearIdentity() {
			this.CurrentIdentity = new InfinarioIdentity (String.Empty,String.Empty);
			this.AdjustCurrentIdentity (this.CurrentIdentity.Cookie, this.CurrentIdentity.Registered);
		}

		#region Schedule
		public virtual void ScheduleCommand(Command command){
			List<object> lst = new List<object> ();
			lst.Add (command.BulkSerialization());
			CommandQueue.MultiEnqueue(lst);
		}

		protected void ScheduleCustomer(String Company, InfinarioIdentity Identity, object Properties){
			this.ScheduleCommand(new CustomerCommand(Company, Identity, Properties));
		}
		#endregion

		private void StartSendLoop() {
			this.StartCoroutine (APISendLoop(this.Target));
		}

		#region API communication
		/// <summary>
		/// Periodically performs the following steps:
		// 	 1. collect the freshest command bulk
		//	 2. try-send bulk
		// 	 3. validate response
		// 	 4. parse response and enqueue for retry if needed
		// 	 5. wait for N seconds (N grows with the number of consecutive connection failures)
		/// </summary>
		/// <returns>The send loop enumerator.</returns>
		private static IEnumerator APISendLoop(string apiTarget) {		
			const int WAIT_FOR_DEFAULT = 3;
			var httpTarget = apiTarget+INFINARIO_API_BULK_ROUTE;
			int consecutiveFailedRequests = 0;
		
			while (true) {
				// Decide if we process retry commands or new commands in this round
				List<object> commands = CommandQueue.BulkDequeue(true);
//				Debug.Log (String.Format ("CommandQ: {0}", CommandQueue.ElementCount));

				if (commands.Count > 0){
//					Debug.Log (String.Format ("-----{0} elements in execution queue, prefs is {1}", commands.Count, PlayerPrefs.GetString("infinario_event_queue")));

					// 1B: Prepare the http components
					var httpBody = Json.Serialize(new Dictionary<string,object> {{"commands", commands}});
					byte[] httpBodyBytes = Encoding.UTF8.GetBytes(httpBody);
					Dictionary<string,string> httpHeaders = new Dictionary<string,string>{ {"Content-type", "application/json"} };

					// 2. Send the bulk API request
					WWW req = new WWW(httpTarget, httpBodyBytes, httpHeaders); //TODO: we could add a timeout functionality
					yield return req;

					// 3A: Check response for errors
					if (!String.IsNullOrEmpty(req.error)){
						consecutiveFailedRequests++;					
//						Debug.Log ("[SendLoop] 2. Connection error " + req.error);
//						Debug.Log (String.Format ("Retrying {0} commands.", commands.Count));
					} else{
						// 3B. Parse the API response
						var responseBody = req.text;
//						Debug.Log("[Infinario]: API responded to: " + httpBody + " @ " + apiTarget+INFINARIO_API_BULK_ROUTE + " with: " + responseBody);

						Dictionary<string, object> apiResponse = (Dictionary<string, object>) Json.Deserialize(responseBody);
						bool success = (bool) apiResponse ["success"];
						if(success){
							consecutiveFailedRequests = 0;

							// 4A: extract retry-commands and queue them back (if any)
							var retryCommands = ExtractRetryCommands(apiResponse,commands);
							CommandQueue.MultiDequeue(commands.Count); //remove every command from this request
							CommandQueue.MultiPush(retryCommands);     //re-add failed commands with the highest priority
//							Debug.Log (String.Format ("Retrying {0} commands.", retryCommands.Count));
						} else {
							consecutiveFailedRequests++;
						}
					}

				} else {
//					Debug.Log ("[Infinario] No commands to send.");
				}

				// 5. Detemine wait time and go idle.
				float waitSeconds = (float)Math.Pow(WAIT_FOR_DEFAULT, Math.Sqrt(consecutiveFailedRequests+1));
				if(consecutiveFailedRequests == 0 && CommandQueue.ElementCount > 0){
					waitSeconds = 0f;
				}

//				Debug.Log (String.Format ("[Infinario] Waiting for {0} seconds.", waitSeconds));
				yield return new WaitForSeconds(waitSeconds);
			}
		}
	
		/// <summary>
		/// Walks through the API response and returns all commands that should be retried.
		/// </summary>
		/// <returns>A list of retry command objects.</returns>
		/// <param name="response">API response dictionary object.</param>
		/// <param name="sentCommands">Request dictionary object.</param>
		private static List<object> ExtractRetryCommands(Dictionary<string,object> response,List<object> sentCommands) {
			List<object> commandResponses = response ["results"] as List<object>;
			if (commandResponses.Count != sentCommands.Count) {
//				Debug.LogError (String.Format ("Failed assertion. Response from server contains {0} records, but the original request contained {1}.", commandResponses.Count, sentCommands.Count));
			}

			List<object> retryCommands = new List<object> ();
			int idx = 0;
			foreach (var cmdResponse in commandResponses) {
				var cmdResponseDict = (Dictionary<string,object>)cmdResponse;
				string status = (cmdResponseDict ["status"] as String).ToLower ();
				if (status == "error") {
//					Debug.LogError (String.Format ("API Request {0} failed with errors {1}.", sentCommands [idx], Json.Serialize (cmdResponseDict ["errors"])));
				} else if (status == "retry") {
					retryCommands.Add (sentCommands[idx]);
				}
				idx++;
			}
			return retryCommands;
		}

		protected MonoBehaviour _coroutineObject;
		protected void StartCoroutine(IEnumerator coroutine){
			if (_coroutineObject == null) {
				var go = new GameObject("Infinario Coroutines");
				UnityEngine.Object.DontDestroyOnLoad(go);
				_coroutineObject = go.AddComponent<MonoBehaviour>();
			}		
			_coroutineObject.StartCoroutine (coroutine);
		}

		#endregion

		//todo
		public void Update(object properties){
			ScheduleCustomer(CompanyToken, this.CurrentIdentity, properties);
		}
	}
	
	public class PersistentCommandQueue {
		const string PERSISTENT_QUEUE_KEY = "infinario_command_queue";
		private string QueueName;

		public PersistentCommandQueue(string name = PERSISTENT_QUEUE_KEY) {
			this.QueueName = name;
		}

		public void MultiPush(IEnumerable<object> commands){
			var queue = this.GetQueue ();
			foreach (var cmd in commands) { 
				queue.Add (cmd);
			}
			this.SetQueue (queue);
		}

		public List<object> MultiPop(int Elements=1, bool peek=false){
			var queue = this.GetQueue ();
			List<object> results = new List<object>();
			for(int i=0;i<Elements && queue.Count > 0;i++){
				results.Add (queue.Count-1);
				queue.RemoveAt(queue.Count-1);
			}

			if(!peek){
				this.SetQueue(queue);
			}
			return queue;
		}

//		public void MultiEnqueue(IEnumerable<Command> commands) {
//			var queue = this.GetQueue ();
//			foreach (var cmd in commands) {
//				object serialization = cmd.BulkSerialization();
//				queue.Add(serialization);
//			}
//			this.SetQueue (queue);
//		}

		public void MultiEnqueue(IEnumerable<object> commands) {
			var queue = this.GetQueue();
			foreach (var cmd in commands) {
				queue.Add(cmd);
			}
			this.SetQueue (queue);
		}

		public List<object> MultiDequeue(int Elements=1,bool peek=false) {
			var queue = this.GetQueue();
			List<object> result = new List<object> ();

			int queueSize = queue.Count;
			if (queueSize == 0) {
				return new List<object>();
			}

			for(int i=0;i<Math.Min(Elements, queueSize);i++) {
				result.Add (queue.ElementAt(0));
				queue.RemoveAt(0);
			}

			if(!peek) {
				this.SetQueue(queue);
			}

			return result;
		}

		private List<object> GetQueue() {
			if (!PlayerPrefs.HasKey (this.QueueName)) {
				return new List<object>();
			}
			string serializedQueue = PlayerPrefs.GetString (this.QueueName);
			List<object> queue = Json.Deserialize (serializedQueue) as List<object>;
			return queue == null ? new List<object> () : queue;
		}

		private void SetQueue(List<object> queue) {
			PlayerPrefs.SetString (this.QueueName, Json.Serialize(queue));
			PlayerPrefs.Save ();
		}

		public void PushAll(IEnumerable<object> commands) {
			if (commands.Count() == 0) {
				return;
			}

			List<object> queue = new List<object>(commands);
			List<object> oldQueue = this.GetQueue ();
			foreach (var itm in oldQueue) {
				queue.Add(itm);
			}
			this.SetQueue(queue);
		}

		public int ElementCount {
			get  { return GetQueue().Count; }
		}
	}

	public class PersistentBulkCommandQueue: PersistentCommandQueue {
		private int BulkSize;
		public PersistentBulkCommandQueue(string name, int bulkSize):base(name) {
			this.BulkSize = bulkSize;
		}

		public List<object> BulkDequeue(bool peek = true) {
			int elements = Math.Min(this.ElementCount, this.BulkSize);
			return this.MultiDequeue(elements,peek);
		}

	}
}

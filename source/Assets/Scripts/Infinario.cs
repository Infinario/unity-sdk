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

	public enum BulkResult {Ok, Error, Retry};

	public abstract class Command {
		
		public static long Epoch() {
			var t0 = DateTime.UtcNow;
			var tEpoch = new DateTime (1970, 1, 1, 0, 0, 0);
			return (long)Math.Truncate (t0.Subtract (tEpoch).TotalSeconds);
		}
		
		public abstract String Endpoint {
			get ;
		}

		public BulkResult Result;
		public String Response;

		public abstract object JsonPayload {
			get ;
		}

		public virtual object JsonSerialize {
			get { 
				return JsonPayload;
			}
		}

		public override String ToString() {
			String s = "curl -X \'POST\' https://api.infinario.com/" + Endpoint + "  -H \"Content-type: application/json\" -d \'" +
				Json.Serialize (JsonPayload) + "\'";

			return s;
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
	
	public class DeserializedCommand : Command {
		private readonly String StoredEndpoint;
		private readonly object StoredPayload;
		
		public DeserializedCommand (String endpoint, object payload) {
			StoredEndpoint = endpoint;
			StoredPayload = payload;
		}
		
		public DeserializedCommand (String serialized) {
			Dictionary<String,object> dict = (Dictionary<String,object>) Json.Deserialize(serialized);
			StoredEndpoint = (String)dict ["name"];
			StoredPayload = dict ["data"];
		}
		
		public override String Endpoint {
			get { 
				return StoredEndpoint;
			}
		}
		
		public override object JsonPayload {
			get { 
				return StoredPayload;
			}
		}


	}	
	
	public class Customer : Command {
		private String Company;
		private object CustomerIds;
		private object Properties;
		public Customer (String company, object customerId, object properties) {
			Company = company;
			CustomerIds = customerId;
			Properties = properties;
		}
		
		
		public override String Endpoint {
			get { 
				return "crm/customers";
			}
		}



		public override object JsonPayload {
			get { 
				if (Properties != null) {
					return new Dictionary<object,object>() {
						{"ids", CustomerIds },
						{"company_id",  Company}, 
						{"properties", Properties} 
					};
				} else {
					return new Dictionary<object,object>() {
						{"ids",  CustomerIds },
						{"company_id",  Company}, 
					};
				}
			}
		}
	}
	
	public class Event : Command {
		private String Type;
		private long Time;
		private String Company;
		private object Customer;
		private object Properties;
		
		public Event (String company, object customer, String type, object properties, long time) {
			Company = company;
			Customer = customer;
			Properties = properties;
			Type = type;
			Time = time;
		}
		
		public override String Endpoint {
			get { 
				return "crm/events";
			}
		}

		public override object JsonSerialize {
			get { 
				if (Properties != null) {
					return new Dictionary<object,object>(){
						{"customer_ids", Customer},
						{"company_id",  Company}, 
						{"properties", Properties}, 
						{"type",  Type}, 
						{"time", this.Time}
					};
				} else {
					return  new Dictionary<object,object>(){
						{"customer_ids", Customer },
						{"company_id",  Company}, 
						{"type",  Type}, 
						{"time",  this.Time},
					};
				}
			}
		}

		public override object JsonPayload {
			get { 
				if (Properties != null) {
					return new Dictionary<object,object>(){
						{"customer_ids", Customer},
						{"company_id",  Company}, 
						{"properties", Properties}, 
						{"type",  Type}, 
						{"age",  Epoch () - this.Time}
					};
				} else {
					return  new Dictionary<object,object>(){
						{"customer_ids", Customer },
						{"company_id",  Company}, 
						{"type",  Type}, 
						{"age",  Epoch () - this.Time},
					};
				}
			}
		}
	}
	
	public interface InfinarioApi {

		void Update (object properties);
		void Identify(object customer);		
		void Identify(object customer,object properties);		
		void Identify (String customer, object properties);
		
		void Track (String Type, object Properties, long Time);		
		void Track (String Type, long Time);		
		void Track (String Type);		
		void Track (String Type, object Properties);

		void ScheduleCommand (Command command);		
	}
	
	public class Infinario : InfinarioApi {

		protected MonoBehaviour _coroutineObject;

		protected void saveUserToPrefs(){
			PlayerPrefs.SetString ("infinario_current_user", Json.Serialize (this.Customer));
		}
		protected void loadUserFromPrefs(){
			string s = PlayerPrefs.GetString ("infinario_current_user");
			if (s != String.Empty) {
				this.Customer = Json.Deserialize (s);
			}
		}

		protected static IEnumerator PostJsonCoroutine(Uri url, string postdata) {
			byte[] data = Encoding.UTF8.GetBytes(postdata);
			Dictionary<string,string> t = new Dictionary<string,string>();
			t.Add ("Content-type", "application/json");

			WWW req = new WWW(url.ToString(), data, t);
			Debug.Log("Infinario: Posting " + postdata + " to " + url.ToString());
			yield return req;

			Dictionary<string, object> data_result = (Dictionary<string, object>) Json.Deserialize(req.text);
			if (((bool) data_result ["success"]) == true) {
				Debug.Log("Infinario: Posting " + postdata + " to " + url.ToString() + " resulted in " + req.text);
			} else {
				Debug.LogError("Infinario: Posting " + postdata + " to " + url.ToString() + " resulted in " + req.text);
			}
		}

		protected void PostJson(Uri url, string postdata){
			StartCoroutine(PostJsonCoroutine(url, postdata));

		}

		protected void StartCoroutine(IEnumerator coroutine){
			if (_coroutineObject == null) {
				var go = new GameObject("Infinario Coroutines");
				UnityEngine.Object.DontDestroyOnLoad(go);
				_coroutineObject = go.AddComponent<MonoBehaviour>();
			}		
			_coroutineObject.StartCoroutine (coroutine);
		}
		
		protected readonly String CompanyToken;
		protected readonly Uri Target;
		protected object Customer;

		private string GenerateGUID() {
			var random = new System.Random();                     

			 return  Application.systemLanguage                            //Language
				+"-"+String.Format("{0:X}", Convert.ToInt32(Command.Epoch()))                //Time
					+"-"+String.Format("{0:X}", Convert.ToInt32(Time.time*1000000))        //Time in game
					+"-"+String.Format("{0:X}", random.Next(1000000000));
		}

		public Infinario(String companyToken, String target, object customer) {
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;

			}
			CompanyToken = companyToken;
			Target = new  Uri(target);
		}

		public Infinario(String companyToken) {
			CompanyToken = companyToken;
			Target = new Uri("https://api.infinario.com/");
			Customer =  new Dictionary<String, String> () {{"registered",""}};
		}

		public Infinario(String companyToken, String target) {
			CompanyToken = companyToken;
			Target = new Uri(target);
			Customer =  new Dictionary<String, String> () {{"registered",GenerateGUID()}};
			loadUserFromPrefs ();
			saveUserToPrefs ();
		}


		public Infinario(String companyToken, String target, string customer) {
			CompanyToken = companyToken;
			Target = new Uri(target);
			Customer =  new Dictionary<String, String> () {{"registered",customer}};
			saveUserToPrefs ();
		}

		public Infinario(String companyToken, Uri target, object customer) {
			CompanyToken = companyToken;
			Target = target;
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;
				
			}
			saveUserToPrefs ();

		}
		
		public Infinario(String companyToken, Uri target, string customer) {
			CompanyToken = companyToken;
			Target = target;
			Customer =  new Dictionary<String, String> () {{"registered",customer}};
			saveUserToPrefs ();

		}
		
		public void Update(object properties){
			ScheduleCustomer(CompanyToken, Customer, properties);
		}

		public void Identify(){
			ScheduleCustomer(CompanyToken, Customer, null);
		}

		public void Identify(object customer){
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;
				
			}
			saveUserToPrefs ();

			ScheduleCustomer(CompanyToken, Customer, null);
		}

		public void Identify(object customer, object properties){
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;
				
			}
			saveUserToPrefs ();

			ScheduleCustomer(CompanyToken, Customer, properties);
		}
		
		public void Identify(String customer, object properties){
			Customer = new Dictionary<String, String> () {{"registered",customer}};
			saveUserToPrefs ();
			ScheduleCustomer (CompanyToken, Customer, properties);
		}
		
		protected void ScheduleCustomer(String Company, object Customer, object Properties){
			ScheduleCommand(new Customer(Company, Customer, Properties));
		}
		
		public void Track(String Type, object Properties, long Time){
			ScheduleCommand(new Event(CompanyToken, Customer, Type, Properties, Time));
		}
		
		public void Track(String Type, long Time){
			ScheduleCommand(new Event(CompanyToken, Customer, Type, null, Time));
		}
		
		
		public void Track(String Type){
			ScheduleCommand(new Event(CompanyToken, Customer, Type, null, Command.Epoch()));
		}
		
		public void Track(String Type,object Properties){
			ScheduleCommand(new Event(CompanyToken, Customer, Type, Properties, Command.Epoch()));
		}
		
		public virtual void ScheduleCommand(Command command){
			PostJson(new Uri(Target.ToString() + command.Endpoint), Json.Serialize(command.JsonPayload));
		}
		
	}

	public class InfinarioAutomatic : Infinario
	{
		protected static string bulkPayload(List<Command> bulk){

			return Json.Serialize( new Dictionary<String,object>() {
				{"commands", (from item in bulk	select item.BulkRepresentation()).ToList()}});
		}

		protected static string bulkSerialize(List<Command> bulk){
			
			return Json.Serialize( new Dictionary<String,object>() {
				{"commands", (from item in bulk	select item.BulkSerialization()).ToList()}});
		}


		protected static List<Command> deserializeBulkManualy(List<Command> bulk, String input){
			Dictionary<String,object> o = Json.Deserialize (input) as Dictionary<String,object>;

			List<Dictionary<String,object>> l = o ["results"] as List<Dictionary<String,object>>;
			if (l == null) {
				foreach(var item in bulk){
					item.Result = BulkResult.Error;
					item.Response = input;
				}
				return bulk;
			}

			int i = 0;
			for(i=0;(i<bulk.Count) && (i<l.Count);i++){
				BulkResult tmpBulkResult = BulkResult.Error;
				var status = l[i]["status"];
				if (status != null) {
					String strstat = status as String;
					if (strstat == "ok") {
						tmpBulkResult = BulkResult.Ok;
					}
					if (strstat == "retry") {
						tmpBulkResult = BulkResult.Retry;
					}
				}
				bulk[i].Response = l[i].ToString();
				bulk[i].Result = tmpBulkResult;
			}
			return bulk;

		}
		
		
		private readonly Queue<Command> commands;
		private readonly Queue<List<Command>> bulkCommandsInProgress;
		private readonly Queue<Command> retryCommands;


		void loadCommands(){
			String saved = PlayerPrefs.GetString ("infinario_event_queue");
			if (saved == null)
								return;
			if (saved == String.Empty)
								return;
			Dictionary<String,object> cmds = Json.Deserialize (saved) as Dictionary<String,object>;
			if (cmds == null)
								return;
			if (cmds.ContainsKey ("commands") == false)
								return;

			List<Dictionary<String,object>> lst = cmds ["commands"] as List<Dictionary<String,object>>;
			if (lst == null)
								return;
			if (lst.Count == 0)
								return;

			foreach (Dictionary<String,object> i in lst) {
				String s = i["name"] as String;
				Dictionary<String,object> d = i["data"] as Dictionary<String,object>;
				if(s.Contains("customer")){
					Command c = new DeserializedCommand(s,d);
					commands.Enqueue(c);
				}else{
					String company = d["company"] as String;
					object customer = d["customer"];
					String type = d["type"] as String;
					object properties = d["properties"];
					long time = Convert.ToInt64(d["time"]);
					Command c = new Event(company,customer,type,properties,time);
					commands.Enqueue(c);
				}
			}
		}

		public InfinarioAutomatic (String companyToken, Uri target, String customer) : base(companyToken,target,customer) {
			commands = new Queue<Command>();
			retryCommands = new Queue<Command>();
			loadCommands ();
			StartCoroutine (BulkUpload ());
		}

	
		public InfinarioAutomatic(String companyToken):base(companyToken) {
			commands = new Queue<Command>();
			retryCommands = new Queue<Command>();
			loadCommands ();

			StartCoroutine (BulkUpload ());
		}
		
		public InfinarioAutomatic(String companyToken, String target):base(companyToken,target) {
			commands = new Queue<Command>();
			retryCommands = new Queue<Command>();
			loadCommands ();

			StartCoroutine (BulkUpload ());
		}
		



		public override void ScheduleCommand(Command command){
			commands.Enqueue(command);
		}

		private Command processFinishedCommand(Command item){

			if(item.Result == BulkResult.Ok){
				Debug.Log("Infinario: Posting " + item.JsonPayload + " to " + item.Endpoint + " resulted in " + item.Response);

			}else if(item.Result == BulkResult.Retry){
				Debug.Log("Infinario: Posting " + item.JsonPayload + " to " + item.Endpoint + " resulted in " + item.Response);

				retryCommands.Enqueue(item);
			}else {
				Debug.LogError("Infinario: Posting " + item.JsonPayload + " to " + item.Endpoint + " resulted in " + item.Response);

			}
			return item;
		}
			
		public IEnumerator BulkUpload(){
			ConnectionTesterStatus status = ConnectionTesterStatus.Undetermined;

			while (true) {

				status = ConnectionTesterStatus.Undetermined;
				yield return new WaitForSeconds(60);
				PlayerPrefs.SetString("infinario_event_queue", BulkSerialize());
				status = Network.TestConnection ();

				if(status != ConnectionTesterStatus.Error && (retryCommands.Count>0 || commands.Count>0)){
					List<Command> tmpList = new List<Command> ();

					while (tmpList.Count < 49 && retryCommands.Count>0) {
						Command c = retryCommands.Dequeue ();
						if(c!=null){
							tmpList.Add (c);
						}
					}

					while (tmpList.Count < 49 && commands.Count>0) {
						Command c = commands.Dequeue ();
						if(c!=null){
							tmpList.Add (c);
						}
					}

					if (tmpList.Count > 0) {
					    var postdata = bulkPayload(tmpList);
						Uri url = new Uri(Target.ToString()+"/bulk");

						byte[] data = Encoding.UTF8.GetBytes(postdata);
						Dictionary<string,string> t = new Dictionary<string,string>();
						t.Add ("Content-type", "application/json");
						
						WWW req = new WWW(url.ToString(), data, t);
						Debug.Log("Infinario: Posting " + postdata + " to " + url.ToString());
						yield return req;
												
						List<Command> result = deserializeBulkManualy (tmpList,req.text);

						foreach(var cmd in result){
							processFinishedCommand (cmd);
						}
					 
					}
					PlayerPrefs.SetString("infinario_event_queue", BulkSerialize());

				}

			}
		} 
		
		
		public String BulkSerialize(){
			
			List<Command> tmpList = new List<Command> ();
			foreach (var i in retryCommands) {
				tmpList.Add (i);
			}
			foreach (var i in commands) {
				tmpList.Add (i);
			}
			
			return bulkSerialize(tmpList);
		}
		
		
	}

}

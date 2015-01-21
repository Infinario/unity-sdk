using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;

namespace Infinario {
	public abstract class Command {
		
		public static long Epoch() {
			var t0 = DateTime.UtcNow;
			var tEpoch = new DateTime (1970, 1, 1, 0, 0, 0);
			return (long)Math.Truncate (t0.Subtract (tEpoch).TotalSeconds);
		}
		
		public abstract String Endpoint {
			get ;
		}
		
		public abstract object JsonPayload {
			get ;
		}
		
		public override String ToString() {
			String s = "curl -X \'POST\' https://api.infinario.com/" + Endpoint + "  -H \"Content-type: application/json\" -d \'" +
				Json.Serialize (JsonPayload) + "\'";
			
			return s;
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
		
		private MonoBehaviour _coroutineObject;
		
		protected static IEnumerator PostJsonCoroutine(Uri url, string postdata) {
			byte[] data = Encoding.UTF8.GetBytes(postdata);
			Dictionary<string,string> t = new Dictionary<string,string>();
			t.Add ("Content-type", "application/json");
			WWW req = new WWW(url.ToString(), data, t);
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
		
		private void StartCoroutine(IEnumerator coroutine){
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
			Customer =  new Dictionary<String, String> () {{"registered",""}};
		}
		
		
		public Infinario(String companyToken, String target, string customer) {
			CompanyToken = companyToken;
			Target = new Uri(target);
			Customer =  new Dictionary<String, String> () {{"registered",customer}};
		}
		
		public Infinario(String companyToken, Uri target, object customer) {
			CompanyToken = companyToken;
			Target = target;
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;
				
			}
		}
		
		public Infinario(String companyToken, Uri target, string customer) {
			CompanyToken = companyToken;
			Target = target;
			Customer =  new Dictionary<String, String> () {{"registered",customer}};
		}
		
		public void Update(object properties){
			ScheduleCustomer(CompanyToken, Customer, properties);
		}
		
		public void Identify(object customer){
			Identify (customer, null);
		}
		
		public void Identify(object customer, object properties){
			if (customer is String) {
				Customer =  new Dictionary<string, string>(){{"registered",(string) customer}};
			} else {
				Customer = customer;
				
			}
			ScheduleCustomer(CompanyToken, customer, properties);
		}
		
		public void Identify(String customer, object properties){
			Customer = new Dictionary<String, String> () {{"registered",customer}};
			ScheduleCustomer (CompanyToken, customer, properties);
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
	
}

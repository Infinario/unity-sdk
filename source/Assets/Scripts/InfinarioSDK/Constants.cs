namespace Infinario
{
	internal class Constants
	{	
		/**
         * SDK
         */
		public static string SDK = "Unity SDK";
		public static string VERSION = "2.0.3";
		
		/**
         * Tracking ids, events and properties
         */
		public static string ID_REGISTERED = "registered";
		public static string ID_COOKIE = "cookie";
        public static string ID_USER = "user_id";

        public static string EVENT_SESSION_START = "session_start";
		public static string EVENT_SESSION_END = "session_end";
		public static string EVENT_IDENTIFICATION = "identification";
		public static string EVENT_VIRTUAL_PAYMENT = "virtual_payment";
		
		public static string PROPERTY_APP_VERSION = "app_version";
		public static string PROPERTY_DURATION = "duration";
		public static string PROPERTY_REGISTRATION_ID = "registration_id";
		public static string PROPERTY_CURRENCY = "currency";
		public static string PROPERTY_AMOUNT = "amount";
		public static string PROPERTY_ITEM_NAME = "item_name";
		public static string PROPERTY_ITEM_TYPE = "item_type";
		public static string PROPERTY_SESSION_START_TIMESTAMP = "session_start_timestamp";
		public static string PROPERTY_SESSION_START_PROPERTIES = "session_start_properties";
		public static string PROPERTY_SESSION_END_TIMESTAMP = "session_end_timestamp";
		public static string PROPERTY_SESSION_END_PROPERTIES = "session_end_properties";
		
		/**
         * Sending
         */
		public static int BULK_LIMIT = 49;
		public static int BULK_TIMEOUT_MS = 10000;
		public static int BULK_INTERVAL_MS = 1000;
		public static int BULK_MAX_RETRIES = 20;
		public static int BULK_MAX_RETRY_WAIT_MS = 60 * 1000;
		public static long BULK_MAX = 60 * 20;

		public static double SESSION_TIMEOUT = 60;
		
		public static string DEFAULT_TARGET = "https://api.infinario.com";
		public static string BULK_URL = "/bulk";
		
		public static string ENDPOINT_UPDATE = "crm/customers";
		public static string ENDPOINT_TRACK = "crm/events";
	}
}

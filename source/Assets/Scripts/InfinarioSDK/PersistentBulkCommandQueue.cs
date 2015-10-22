using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Infinario.MiniJSON;
using System;
using System.Linq;

namespace Infinario.Storage
{
	public class PersistentCommandQueue {
		const string PERSISTENT_QUEUE_KEY = "infinario_command_queue";
		private string QueueName;
		private int MaxQueueBytes;

		private object lockPlayerPrefAccess;
		private object lockAccess;
		
		public PersistentCommandQueue(int maxBytes=1024*1024,string name = PERSISTENT_QUEUE_KEY)
		{
			this.QueueName = name;
			this.lockPlayerPrefAccess = new object ();
			this.lockAccess = new object();
			this.SetMaxQueueBytes(maxBytes);
		}
		
		public void SetMaxQueueBytes(int bytes)
		{
			lock (lockAccess)
			{
				this.MaxQueueBytes = bytes;
				List<object> queue = this.GetQueue ();
				List<object> newQueue = new List<object>();
				int sumBytes = 0;
				int i = 0;
				while(i < queue.Count)
				{
					sumBytes += 2 * (Json.Serialize (queue.ElementAt(i))).Length;
					if (sumBytes > bytes)
					{
						break;
					}
					newQueue.Add (queue.ElementAt(i));
				}
				this.SetQueue (newQueue);
			}
		}
		
		public void MultiPush(IEnumerable<object> commands)
		{
			lock (lockAccess)
			{
				var queue = this.GetQueue ();
				foreach (var cmd in commands)
				{ 
					queue.Add (cmd);
				}
				this.SetQueue (queue);
			}
		}
		
		public List<object> MultiPop(int Elements = 1, bool peek = false)
		{
			lock (lockAccess)
			{
				var queue = this.GetQueue ();
				List<object> results = new List<object>();
				for(int i=0;i<Elements && queue.Count > 0;i++)
				{
					results.Add (queue.Count - 1);
					queue.RemoveAt(queue.Count - 1);
				}
				
				if(!peek)
				{
					this.SetQueue(queue);
				}
				return queue;
			}
		}
		
		public void MultiEnqueue(IEnumerable<object> commands)
		{
			lock (lockAccess)
			{
				var queue = this.GetQueue();
				foreach (var cmd in commands)
				{
					queue.Add(cmd);
				}
				this.SetQueue (queue);
			}
		}
		
		public List<object> MultiDequeue(int Elements=1,bool peek=false)
		{
			lock (lockAccess)
			{
				var queue = this.GetQueue();
				List<object> result = new List<object> ();
				
				int queueSize = queue.Count;
				if (queueSize == 0)
				{
					return new List<object>();
				}
				
				for(int i=0;i<Math.Min(Elements, queueSize);i++)
				{
					result.Add (queue.ElementAt(0));
					queue.RemoveAt(0);
				}
				
				if(!peek)
				{
					this.SetQueue(queue);
				}
				
				return result;
			}
		}
		
		private List<object> GetQueue()
		{
			lock (lockPlayerPrefAccess)
			{
				if (!PlayerPrefs.HasKey (this.QueueName))
				{
					return new List<object>();
				}
				string serializedQueue = PlayerPrefs.GetString (this.QueueName);
				if (serializedQueue == "")
					return new List<object>();
				List<object> queue = Json.Deserialize (serializedQueue) as List<object>;
				return queue == null ? new List<object> () : queue;
			}
		}
		
		private void SetQueue(List<object> queue)
		{
			lock (lockPlayerPrefAccess)
			{
				string str = Json.Serialize (queue);
				if (str.Length * 2 <= this.MaxQueueBytes)
				{
					PlayerPrefs.SetString (this.QueueName, str);
					PlayerPrefs.Save ();
				}
			}
		}
		
		public void PushAll(IEnumerable<object> commands)
		{
			lock (lockAccess)
			{
				if (commands.Count() == 0)
				{
					return;
				}
				
				List<object> queue = new List<object>(commands);
				List<object> oldQueue = this.GetQueue ();
				foreach (var itm in oldQueue)
				{
					queue.Add(itm);
				}
				this.SetQueue(queue);
			}
		}
		
		public int ElementCount
		{
			get 
			{ 
				lock(lockAccess)
				{
					return GetQueue().Count;
				} 
			}
		}
	}
	
	public class PersistentBulkCommandQueue: PersistentCommandQueue
	{
		private int BulkSize;
		public PersistentBulkCommandQueue(string name, int bulkSize):base(1024*1024,name)
		{
			this.BulkSize = bulkSize;
		}
		
		public List<object> BulkDequeue(bool peek = true)
		{
			int elements = Math.Min(this.ElementCount, this.BulkSize);
			return this.MultiDequeue(elements,peek);
		}
		
	}
}
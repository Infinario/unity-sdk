﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Infinario.Storage;
using Infinario.MiniJSON;
using System.Text;
using System;

namespace Infinario.Sender
{
    class InfinarioMonoBehaviour : MonoBehaviour
    {
    }

    class Sender
    {
        private string _target;

        public Sender(string target, PersistentBulkCommandQueue commandQueue)
        {
            this.StartCoroutine(APISendLoop(target, commandQueue));
            _target = target;
        }

        protected MonoBehaviour _coroutineObject;

        protected void StartCoroutine(IEnumerator coroutine)
        {
            if (_coroutineObject == null)
            {
                var go = new GameObject("Infinario Coroutines");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _coroutineObject = go.AddComponent<InfinarioMonoBehaviour>();
            }
            _coroutineObject.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Periodically performs the following steps:
        ///  0. keep alive the session
        // 	 1. collect the freshest command bulk
        //	 2. try-send bulk
        // 	 3. validate response
        // 	 4. parse response and enqueue for retry if needed
        // 	 5. wait for N seconds (N grows with the number of consecutive connection failures)
        /// </summary>
        /// <returns>The send loop enumerator.</returns>
        private IEnumerator APISendLoop(string target, PersistentBulkCommandQueue commandQueue)
        {
            const int WAIT_FOR_DEFAULT = 3;
            var httpTarget = target + Constants.BULK_URL;
            int consecutiveFailedRequests = 0;

            while (true)
            {
                // Decide if we process retry commands or new commands in this round
                List<object> commands = commandQueue.BulkDequeue(true);

                if (commands.Count > 0)
                {
                    // 1B: Prepare the http components
                    var httpBody = Json.Serialize(new Dictionary<string, object> {{"commands", commands}});
                    byte[] httpBodyBytes = Encoding.UTF8.GetBytes(httpBody);
                    Dictionary<string, string> httpHeaders = new Dictionary<string, string>
                    {
                        {"Content-type", "application/json"}
                    };

                    // 2. Send the bulk API request
                    WWW req = new WWW(httpTarget, httpBodyBytes, httpHeaders);
                        //TODO: we could add a timeout functionality
                    yield return req;

                    // 3A: Check response for errors
                    if (!string.IsNullOrEmpty(req.error))
                    {
                        consecutiveFailedRequests++;
                    }
                    else
                    {
                        // 3B. Parse the API response
                        var responseBody = req.text;
                        Dictionary<string, object> apiResponse =
                            (Dictionary<string, object>) Json.Deserialize(responseBody);
                        bool success = (bool) apiResponse["success"];
                        if (success)
                        {
                            consecutiveFailedRequests = 0;

                            // 4A: extract retry-commands and queue them back (if any)
                            var retryCommands = ExtractRetryCommands(apiResponse, commands);
                            commandQueue.MultiDequeue(commands.Count); //remove every command from this request
                            commandQueue.MultiPush(retryCommands); //re-add failed commands with the highest priority
                        }
                        else
                        {
                            consecutiveFailedRequests++;
                        }
                    }

                }

                // 5. Detemine wait time and go idle.
                float waitSeconds =
                    (float) System.Math.Pow(WAIT_FOR_DEFAULT, System.Math.Sqrt(consecutiveFailedRequests + 1));
                if (consecutiveFailedRequests == 0 && commandQueue.ElementCount > 0)
                {
                    waitSeconds = 0f;
                }
                waitSeconds = System.Math.Min(waitSeconds, Constants.BULK_MAX - 3f);

                yield return new WaitForSeconds(waitSeconds);
            }
        }

        /// <summary>
        /// Walks through the API response and returns all commands that should be retried.
        /// </summary>
        /// <returns>A list of retry command objects.</returns>
        /// <param name="response">API response dictionary object.</param>
        /// <param name="sentCommands">Request dictionary object.</param>
        private List<object> ExtractRetryCommands(Dictionary<string, object> response, List<object> sentCommands)
        {
            List<object> commandResponses = response["results"] as List<object>;

            List<object> retryCommands = new List<object>();
            int idx = 0;
            foreach (var cmdResponse in commandResponses)
            {
                var cmdResponseDict = (Dictionary<string, object>) cmdResponse;
                string status = (cmdResponseDict["status"] as string).ToLower();
                if (status.Equals("retry"))
                {
                    retryCommands.Add(sentCommands[idx]);
                }
                idx++;
            }
            return retryCommands;
        }


        private IEnumerator GetCurrentSegmentCoroutine(Dictionary<string, object> customerIds, string projectSecret,
            string segmentationId, Action<bool, InfinarioSegment, string> onSegmentReceiveCallback)
        {
           

            var httpTarget = (_target ?? Constants.DEFAULT_TARGET) + Constants.GET_SEGMENT_URL;

            //	        var httpBody = Json.Serialize(new Dictionary<string, object> {{"commands", commands}});


            var body = new Dictionary<string, object>()
            {
                {"customer_ids", customerIds},
                {"analysis_id", segmentationId}
            };

            var httpBody = Json.Serialize(body);

            byte[] httpBodyBytes = Encoding.UTF8.GetBytes(httpBody);
            Dictionary<string, string> httpHeaders = new Dictionary<string, string>
            {
                {"Accept", "application/json"},
                {"Content-type", "application/json"},
                {Constants.DEFAULT_SECRET, projectSecret} // this needs to be project secret not project token 
            };

            WWW req = new WWW(httpTarget, httpBodyBytes, httpHeaders); //TODO: we could add a timeout functionality
            yield return req;

            // Check response for errors
            if (!string.IsNullOrEmpty(req.error))
            {
                onSegmentReceiveCallback(false, null, req.error + "\n " + req.text);
            }
            else
            {
                // Parse the API response
                var responseBody = req.text;
                Dictionary<string, object> apiResponse = (Dictionary<string, object>) Json.Deserialize(responseBody);
                bool success = (bool) apiResponse["success"];
                if (success)
                {
                    var segmentName = apiResponse["segment"] as string;
                    var segmentationName = apiResponse["analysis_name"] as string;
                    var c = apiResponse["segment_index"];
                    string error = "";
                    int segmentIndex = -1;
                    try
                    {
                        segmentIndex = Convert.ToInt32(c);
                    }
                    catch (Exception exception)
                    {
                        error += exception.Message;
                    }
	                onSegmentReceiveCallback(true, new InfinarioSegment(segmentName, segmentationName, segmentIndex), error);
	            }
	            else
	            {
                    onSegmentReceiveCallback(false, null, "Unsuccessful segmentation request. Response text:\n"+req.text);
                }
	        }
	    }

	    public void GetCurrentSegment(Dictionary<string, object> customerIds, string projectSecret, string segmentaionId, Action<bool, InfinarioSegment, string> onSegmentReceiveCallback)
	    {
	        StartCoroutine(GetCurrentSegmentCoroutine(customerIds,projectSecret,segmentaionId,onSegmentReceiveCallback));
	    }
	}
}

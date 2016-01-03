
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

using UnityEngine;

// I ADDED FUNCTIONALITY FOR UPLOADING IMAGES TO TWITTER, BUT STARTED WITH THE BASE BY PATRICK SMITH,
// BELOW IS HIS LICENSE WHICH ALLOWS FOR MODIFIED SHARING. ENJOY!
// - Chris Wade @cjacobwade

// HERE'S SOME EXAMPLE USAGE:

// STEP 1: Use this to take a screenshot with the main camera and encode it as a png
//byte[] GetScreenshotBytes()
//{
//	Camera cam = Camera.main;
//	int resWidth = cam.pixelWidth;
//	int resHeight = cam.pixelHeight;
//	
//	RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
//	cam.targetTexture = rt;
//	Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
//	cam.Render();
//	RenderTexture.active = rt;
//	screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
//	cam.targetTexture = null;
//	RenderTexture.active = null; // JC: added to avoid errors
//	Destroy(rt);
//	return screenShot.EncodeToPNG();
//}

// STEP 2: Pass those bytes into our PostImageAndTextTweet
// YOU'LL NEED TO SET THE CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN and ACCESS_TOKEN_SECRET SOMEWHERE TO REFERENCE HERE
// YOU GET THAT FROM TWITTER WHEN YOU REGISTER YOUR APPLICATION!

//StartCoroutine(ImageToTwitter.API.PostTextTweet("SOME COOL TEXT", CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET, OnPostTweet));
//StartCoroutine(ImageToTwitter.API.PostImageTweet(GetScreenshotBytes(), CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET, OnPostTweet));
//StartCoroutine(ImageToTwitter.API.PostImageAndTextTweet(GetScreenshotBytes(), "SOME COOL TEXT", CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET, OnPostTweet));

// The below help methods are modified from "WebRequestBuilder.cs" in Twitterizer(http://www.twitterizer.net/).
// Here is its license.

//-----------------------------------------------------------------------
// <copyright file="WebRequestBuilder.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net/)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Ricky Smith</author>
// <summary>Provides the means of preparing and executing Anonymous and OAuth signed web requests.</summary>
//-----------------------------------------------------------------------

namespace ImageToTwitter
{
	public class API
	{
		public delegate void PostTweetCallback(bool success);

		private static string GetHeaderWithAccessToken(string httpRequestType, string apiURL, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Dictionary<string, string> parameters)
		{
			AddDefaultOAuthParams(parameters, consumerKey, consumerSecret);

			parameters.Add("oauth_token", accessToken);
			parameters.Add("oauth_token_secret", accessTokenSecret);

			return GetFinalOAuthHeader(httpRequestType, apiURL, parameters);
		}

		#region Twitter API Methods

		private const string UploadMediaURL = "https://upload.twitter.com/1.1/media/upload.json";
		private const string PostTweetURL = "https://api.twitter.com/1.1/statuses/update.json";

		public static IEnumerator PostTextTweet(string text, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Action<bool> callback)
		{
			if (text.Length == 0)
			{
				callback(false);
			}
			else
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("status", text);
				
				// Add data to the form to post.
				WWWForm form = new WWWForm();
				form.AddField("status", text);
				
				// HTTP header
				Dictionary<string, string> headers = new Dictionary<string, string>();
				headers["Authorization"] = GetHeaderWithAccessToken("POST", PostTweetURL, consumerKey, consumerSecret, accessToken, accessTokenSecret, parameters);
				
				WWW web = new WWW(PostTweetURL, form.data, headers);
				yield return web;
				
				if (!string.IsNullOrEmpty(web.error))
				{
					Debug.Log(string.Format("PostTweet - failed. {0}\n{1}", web.error, web.text));
					callback(false);
				}
				else
				{
					string error = Regex.Match(web.text, @"<error>([^&]+)</error>").Groups[1].Value;
					
					if (!string.IsNullOrEmpty(error))
					{
						Debug.Log(string.Format("PostTweet - failed. {0}", error));
						callback(false);
					}
					else
					{
						callback(true);
						
						// Now that the image is uploaded, we need to send a tweet and incorporate the image ID...
					}
				}
			}
		}

		public static IEnumerator PostImageTweet(byte[] imageInBytes, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Action<bool> callback)
		{
			if (imageInBytes.Length == 0)
			{
				callback(false);
			}
			else
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string encoded64ImageData = System.Convert.ToBase64String(imageInBytes);
				parameters.Add("media_data", encoded64ImageData );

				// Add data to the form to post.
				WWWForm form = new WWWForm();
				form.AddField("media_data", encoded64ImageData);

				// HTTP header
				var headers = new Dictionary<string, string>();
				headers["Authorization"] = GetHeaderWithAccessToken("POST", UploadMediaURL, consumerKey, consumerSecret, accessToken, accessTokenSecret, parameters);
				headers["Content-Transfer-Encoding"] = "base64";

				WWW web = new WWW(UploadMediaURL, form.data, headers);
				yield return web;

				if (!string.IsNullOrEmpty(web.error))
				{
					Debug.Log(string.Format("PostTweet - failed. {0}\n{1}", web.error, web.text));
					callback(false);
				}
				else
				{
					string error = Regex.Match(web.text, @"<error>([^&]+)</error>").Groups[1].Value;

					if (!string.IsNullOrEmpty(error))
					{
						Debug.Log(string.Format("PostTweet - failed. {0}", error));
						callback(false);
					}
					else
					{
						callback(true);

						// NOW THAT MEDIA IS UPLOADED, TWEET WITH THE ASSOCIATED MEDIA ID
						string parsedMediaID = web.text.Substring(12,18);
						string tweetString = "";

						parameters = new Dictionary<string, string>();
						parameters.Add("status", tweetString );
						parameters.Add("media_ids", parsedMediaID);
						
						// Add data to the form to post.
						form = new WWWForm();
						form.AddField("status", tweetString);
						form.AddField("media_ids", parsedMediaID);
						
						// HTTP header
						headers = new Dictionary<string, string>();
						headers["Authorization"] = GetHeaderWithAccessToken("POST", PostTweetURL, consumerKey, consumerSecret, accessToken, accessTokenSecret, parameters);

						WWW web2 = new WWW(PostTweetURL, form.data, headers);
						yield return web2;

						if (!string.IsNullOrEmpty(web2.error))
						{
							Debug.Log(string.Format("PostTweet - failed. {0}\n{1}", web2.error, web2.text));
							callback(false);
						}
						else
						{
							error = Regex.Match(web2.text, @"<error>([^&]+)</error>").Groups[1].Value;
							
							if (!string.IsNullOrEmpty(error))
							{
								Debug.Log(string.Format("PostTweet - failed. {0}", error));
								callback(false);
							}
							else
							{
								callback(true);
								
								// Now that the image is uploaded, we need to send a tweet and incorporate the image ID...
							}
						}
					}
				}
			}
		}

		public static IEnumerator PostImageAndTextTweet(byte[] imageInBytes, string text, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Action<bool> callback)
		{
			if (imageInBytes.Length == 0)
			{
				callback(false);
			}
			else
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string encoded64ImageData = System.Convert.ToBase64String(imageInBytes);
				parameters.Add("media_data", encoded64ImageData );
				
				// Add data to the form to post.
				WWWForm form = new WWWForm();
				form.AddField("media_data", encoded64ImageData);
				
				// HTTP header
				var headers = new Dictionary<string, string>();
				headers["Authorization"] = GetHeaderWithAccessToken("POST", UploadMediaURL, consumerKey, consumerSecret, accessToken, accessTokenSecret, parameters);
				headers["Content-Transfer-Encoding"] = "base64";
				
				WWW web = new WWW(UploadMediaURL, form.data, headers);
				yield return web;
				
				if (!string.IsNullOrEmpty(web.error))
				{
					Debug.Log(string.Format("PostTweet - failed. {0}\n{1}", web.error, web.text));
					callback(false);
				}
				else
				{
					string error = Regex.Match(web.text, @"<error>([^&]+)</error>").Groups[1].Value;
					
					if (!string.IsNullOrEmpty(error))
					{
						Debug.Log(string.Format("PostTweet - failed. {0}", error));
						callback(false);
					}
					else
					{
						callback(true);
						
						// NOW THAT MEDIA IS UPLOADED, TWEET WITH THE ASSOCIATED MEDIA ID
						string parsedMediaID = web.text.Substring(12,18);
						
						parameters = new Dictionary<string, string>();
						parameters.Add("status", text );
						parameters.Add("media_ids", parsedMediaID);
						
						// Add data to the form to post.
						form = new WWWForm();
						form.AddField("status", text);
						form.AddField("media_ids", parsedMediaID);
						
						// HTTP header
						headers = new Dictionary<string, string>();
						headers["Authorization"] = GetHeaderWithAccessToken("POST", PostTweetURL, consumerKey, consumerSecret, accessToken, accessTokenSecret, parameters);
						
						WWW web2 = new WWW(PostTweetURL, form.data, headers);
						yield return web2;
						
						if (!string.IsNullOrEmpty(web2.error))
						{
							Debug.Log(string.Format("PostTweet - failed. {0}\n{1}", web2.error, web2.text));
							callback(false);
						}
						else
						{
							error = Regex.Match(web2.text, @"<error>([^&]+)</error>").Groups[1].Value;
							
							if (!string.IsNullOrEmpty(error))
							{
								Debug.Log(string.Format("PostTweet - failed. {0}", error));
								callback(false);
							}
							else
							{
								callback(true);
								
								// Now that the image is uploaded, we need to send a tweet and incorporate the image ID...
							}
						}
					}
				}
			}
		}
		#endregion

		#region OAuth Help Methods
		private static readonly string[] OAuthParametersToIncludeInHeader = new[]
														{
															"oauth_version",
															"oauth_nonce",
															"oauth_timestamp",
															"oauth_signature_method",
															"oauth_consumer_key",
															"oauth_token",
															"oauth_verifier"
															// Leave signature omitted from the list, it is added manually
															// "oauth_signature",
														};

		private static readonly string[] SecretParameters = new[]
																{
																	"oauth_consumer_secret",
																	"oauth_token_secret",
																	"oauth_signature"
																};

		private static void AddDefaultOAuthParams(Dictionary<string, string> parameters, string consumerKey, string consumerSecret)
		{
			parameters.Add("oauth_version", "1.0");
			parameters.Add("oauth_nonce", GenerateNonce());
			parameters.Add("oauth_timestamp", GenerateTimeStamp());
			parameters.Add("oauth_signature_method", "HMAC-SHA1");
			parameters.Add("oauth_consumer_key", consumerKey);
			parameters.Add("oauth_consumer_secret", consumerSecret);
		}

		private static string GetFinalOAuthHeader(string HTTPRequestType, string URL, Dictionary<string, string> parameters)
		{
			// Add the signature to the oauth parameters
			string signature = GenerateSignature(HTTPRequestType, URL, parameters);

			parameters.Add("oauth_signature", signature);

			StringBuilder authHeaderBuilder = new StringBuilder();
			authHeaderBuilder.AppendFormat("OAuth realm=\"{0}\"", "Twitter API");

			var sortedParameters = from p in parameters
									where OAuthParametersToIncludeInHeader.Contains(p.Key)
									orderby p.Key, UrlEncode(p.Value)
									select p;

			foreach (var item in sortedParameters)
			{
			authHeaderBuilder.AppendFormat(",{0}=\"{1}\"", UrlEncode(item.Key), UrlEncode(item.Value));
			}

			authHeaderBuilder.AppendFormat(",oauth_signature=\"{0}\"", UrlEncode(parameters["oauth_signature"]));

			return authHeaderBuilder.ToString();
		}

		private static string GenerateSignature(string httpMethod, string url, Dictionary<string, string> parameters)
		{
			var nonSecretParameters = (from p in parameters
										where !SecretParameters.Contains(p.Key)
										select p);

			// Create the base string. This is the string that will be hashed for the signature.
			string signatureBaseString = string.Format(CultureInfo.InvariantCulture,
														"{0}&{1}&{2}",
														httpMethod,
														UrlEncode(NormalizeUrl(new Uri(url))),
														UrlEncode(nonSecretParameters));

			// Create our hash key (you might say this is a password)
			string key = string.Format(CultureInfo.InvariantCulture,
										"{0}&{1}",
										UrlEncode(parameters["oauth_consumer_secret"]),
										parameters.ContainsKey("oauth_token_secret") ? UrlEncode(parameters["oauth_token_secret"]) : string.Empty);


			// Generate the hash
			HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(key));
			byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
			return Convert.ToBase64String(signatureBytes);
		}

		private static string GenerateTimeStamp()
		{
			// Default implementation of UNIX time of the current UTC time
			TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
		}

		private static string GenerateNonce()
		{
			// Just a simple implementation of a random number between 123400 and 9999999
			return new System.Random().Next(123400, int.MaxValue).ToString("X", CultureInfo.InvariantCulture);
		}

		private static string NormalizeUrl(Uri url)
		{
			string normalizedUrl = string.Format(CultureInfo.InvariantCulture, "{0}://{1}", url.Scheme, url.Host);
			if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
			{
				normalizedUrl += ":" + url.Port;
			}

			normalizedUrl += url.AbsolutePath;
			return normalizedUrl;
		}

		private static string UrlEncode(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			value = BigEscapeString(value);

			// UrlEncode escapes with lowercase characters (e.g. %2f) but oAuth needs %2F
			value = Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());

			// these characters are not escaped by UrlEncode() but needed to be escaped
			value = value
				.Replace("(", "%28")
				.Replace(")", "%29")
				.Replace("$", "%24")
				.Replace("!", "%21")
				.Replace("*", "%2A")
				.Replace("'", "%27");

			// these characters are escaped by UrlEncode() but will fail if unescaped!
			value = value.Replace("%7E", "~");

			return value;
		}

		private static string BigEscapeString(string originalString)
		{
			int limit = 2000;
			
			StringBuilder sb = new StringBuilder();
			int loops = originalString.Length / limit;
			
			for (int i = 0; i <= loops; i++)
			{
				if (i < loops)
				{
					sb.Append(Uri.EscapeDataString(originalString.Substring(limit * i, limit)));
				}
				else
				{
					sb.Append(Uri.EscapeDataString(originalString.Substring(limit * i)));
				}
			}
			return sb.ToString();
		}

		private static string UrlEncode(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			StringBuilder parameterString = new StringBuilder();

			var paramsSorted = from p in parameters
								orderby p.Key, p.Value
								select p;

			foreach (var item in paramsSorted)
			{
				if (parameterString.Length > 0)
				{
					parameterString.Append("&");
				}

				parameterString.Append(
					string.Format(
						CultureInfo.InvariantCulture,
						"{0}={1}",
						UrlEncode(item.Key),
						UrlEncode(item.Value)));
			}

			return UrlEncode(parameterString.ToString());
		}

		#endregion
	}
}

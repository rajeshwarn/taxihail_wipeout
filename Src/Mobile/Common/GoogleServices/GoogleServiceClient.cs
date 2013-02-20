using System;
using System.IO;
using RestSharp;
using System.Collections.Generic;
using RestSharp.Deserializers;
using Microsoft.Practices.ServiceLocation;
using RestSharp.Serializers;
using ServiceStack.Text;

namespace TaxiMobileApp.Lib.GoogleServices
{
	public class GoogleServiceClient
	{
		public static string DefaultUrl {
			get{ return "https://maps.googleapis.com/"; }
		}
		
		private string _baseUrl = "https://maps.googleapis.com/";

		public GoogleServiceClient ()
		{
			
		}

		public GoogleServiceClient ( string uri )//AuthenticationToken token)
		{
			_baseUrl = Path.Combine( DefaultUrl, uri );
//			_token = token;
		}

//		private AuthenticationToken _token;

		protected string BaseUrl { get { return _baseUrl; } }

		protected RestClient GetClient ()
		{			
			var client = new RestClient ( BaseUrl );			

			client.Timeout =  30 * 1000;
			
			client.RemoveHandler ("application/json");			
			client.AddDefaultHeader ("Accept-Encoding", "gzip, deflate");
			var deserializer = new JsonDeserializer();

			client.AddHandler ("application/json", deserializer);
			return client;
		}

//		protected void SetOAuthToken (IRestRequest request)
//		{
//			if ((_token != null) && (_token.Error.IsNullOrEmpty ()))
//			{
//				request.AddHeader ("Authorization", "Bearer " + _token.AccessToken);
//			}
//		}

		protected IEnumerable<W> ExecuteList<W> (RestRequest request, int timeout = 30000)
		{			
			var response = ExecuteServiceCall (request, true, client => client.Execute<List<W>> (request), timeout);
			return response.Data;
		}

		protected T Execute<T> (RestRequest request, int timeout = 30000) where T : new()
		{		
			var response = ExecuteServiceCall (request, true, client => client.Execute<T> (request), timeout);
			return response.Data;
		}

		public string ExecuteRaw (RestRequest request, bool needAuth, int timeout = 30000)
		{
			var response = ExecuteServiceCall (request, needAuth, client => client.Execute (request), timeout);
			return response.Content;
		}

		private TResponse ExecuteServiceCall<TResponse> (RestRequest request, bool needAuth, Func<RestClient , TResponse>  action, int timeout ) where TResponse : IRestResponse
		{			
			ServiceClientLogger.Start ("Google Service: Start ExecuteServiceCall");			
			
			var client = GetClient ();
			
//			if (needAuth)
//			{	
//				SetOAuthToken (request);
//			}

			request.Timeout = timeout;
			
			var response = action (client);
            			
			ServiceClientLogger.Stop ("Google Service: Done ExecuteServiceCall");
						
			ValidateResponse (response);
			
			return response;
		}
		
		protected void ValidateResponse (IRestResponse response)
		{
			Console.WriteLine( response.Content ); 
			if (response.ErrorException != null)
			{
				ServiceClientLogger.LogStack ();
				throw new Exception( response.ErrorMessage );
			}
			
			if ((int)response.StatusCode >= 400)
			{
//								
//				if (response.Content.Contains ("errorCategory") && response.Content.Contains ("errorCode"))
//				{
//					var errorResponse = JsonConvert.DeserializeObject< ResponseData > (response.Content);
//					throw new BusinessException (errorResponse.Message, ex);
//									
//				}
//				else
//				{
//					throw new BusinessException ("Google Server error.");
//				}
				throw new Exception( "Google server error" );
			}
			else
			if ((int)response.StatusCode == 0)
			{
				
//				throw new ConnectionErrorException ("Cannot connect to Google server", null);
				throw new Exception( "Cannot connect to Google server" );
			}
						
			
		}

		public RestRequest GetRequest (Method method)
		{
			return new RestRequest (method) { RequestFormat = DataFormat.Json, JsonSerializer = new JsonSerializer () };
		}

	}

	public class GoogleServiceClient<T> : GoogleServiceClient where T : new()
	{
		public GoogleServiceClient ()
		{

		}

		public GoogleServiceClient (string uri) : base( uri )
		{
        
		}

		protected T Execute (RestRequest request, int timeout = 30000)
		{
			return base.Execute<T> (request, timeout);
		}

		public IEnumerable<T> ExecuteList (RestRequest request)
		{ 
			
			return base.ExecuteList<T> (request);
		}

		public new  RestRequest GetRequest (Method method)
		{
			return new RestRequest (method) { RequestFormat = DataFormat.Json, JsonSerializer = new JsonSerializer () };
		}

	}

}


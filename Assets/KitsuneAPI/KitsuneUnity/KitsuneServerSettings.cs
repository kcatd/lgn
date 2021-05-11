using KitsuneCore.Server;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	public class KitsuneServerSettings : ScriptableObject, IServerSettings
	{
		[SerializeField] private string _name;
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		
		
		[SerializeField] private string _webServerUrl;
		public string WebServerUrl
		{
			get { return _webServerUrl; }
			set { _webServerUrl = value; }
		}
		
		[SerializeField] private int _webServerPort = 8080;
		public int WebServerPort
		{
			get { return _webServerPort; }
			set { _webServerPort = value; }
		}

		[SerializeField] private int _secureWebServerPort = 8443;
		public int SecureWebServerPort
		{
			get { return _secureWebServerPort; }
			set { _secureWebServerPort = value; }
		}

		public bool IsLocalHost => Name == "Localhost";

		public string FullyResolvedWebServerURL
		{
			get
			{
				if (_webServerUrl == "localhost" ||
				    _webServerUrl == "http://localhost" ||
				    _webServerUrl == "https://localhost")
				{
					return "http://" + _webServerUrl + ":" + _webServerPort + "/fx/";
				}
				
				return "https://" + _webServerUrl + "/fx/";
			}
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
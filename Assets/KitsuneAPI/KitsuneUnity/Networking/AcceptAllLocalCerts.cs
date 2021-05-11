// using System;
// using System.Security.Cryptography.X509Certificates;
// using UnityEngine;
using UnityEngine.Networking;

namespace KitsuneAPI.KitsuneUnity.Networking
{
	public class AcceptAllLocalCerts : CertificateHandler
	{
		// private static string PUB_KEY = ""; // *.kitsuneapi.com public key
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			// if (KitsuneManager.GameSettings.ServerSettings.IsLocalHost)
			// 	return true;
			
			// X509Certificate2 certificate = new X509Certificate2(certificateData);
			// bool valid = certificate.Verify();
			// X509Chain chain = new X509Chain();
			//
			// try
			// {
			// 	var chainBuilt = chain.Build(certificate );
			// 	Debug.LogFormat("Chain building status: {0}", chainBuilt);
			//
			// 	if (chainBuilt == false)
			// 		foreach (X509ChainStatus chainStatus in chain.ChainStatus)
			// 			Debug.LogErrorFormat("Chain error: {0} {1}", chainStatus.Status, chainStatus.StatusInformation);
			// }
			// catch (Exception ex)
			// {
			// 	Debug.LogError(ex.ToString());
			// }
			// Debug.LogFormat("Validating Certificate value={0}", valid);
			// string pk = certificate.GetPublicKeyString();
			// if (pk != null)
			// {
			// 	string pkLower = pk.ToLower();
			// 	string pubLower = PUB_KEY.ToLower();
			// 	if (pkLower == pubLower)
			// 		return true;
			// }

			return true;
		}
	}
}
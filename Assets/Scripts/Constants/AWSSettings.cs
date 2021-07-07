using KitsuneAPI.KitsuneUnity;

namespace Constants
{
	public class AWSSettings
	{
		public const string S3_BUCKET_ASSET_BUNDLES_BUCKET = "asset_bundles";
		public const string S3_WEBGL_BUILDS_BUCKET = "html";

		public const string S3_PROD_BUCKET = "livegamenight-prod";
		public const string S3_PLAYTEST_BUCKET = "livegamenight-playtest";
		public const string S3_QATEST_BUCKET = "livegamenight-qatest";
		// Change this to the current game using the naming convetion 'lgn_gamename'
		public const string S3_GAME_DIR = "lgn_yatzy"; 

		public static string GetBucketFromServerSettings(string serverEnvironment)
		{
			switch (serverEnvironment)
			{
				case EKitsuneEnvironment.Playtest:
					return S3_PLAYTEST_BUCKET;
				case EKitsuneEnvironment.QATest:
					return S3_QATEST_BUCKET;
				default:
					return S3_PROD_BUCKET;
			}
		}
	}
}
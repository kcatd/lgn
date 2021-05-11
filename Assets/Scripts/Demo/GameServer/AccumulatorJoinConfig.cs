using KitsuneCommon.GameServer;
using KitsuneCommon.Net.Messages;
using KitsuneCore.Services.Places;

namespace Demo.GameServer
{
	public class AccumulatorJoinConfig : IJoinGameConfig
	{
		public Place Place => Place.Mk(PlaceId.Mk(183), "kitsune-server-sample");
		public void AddJoinDetails(MessageBody messageBody)
		{
			// no join details
		}
	}
}
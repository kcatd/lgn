using KitsuneCommon.Net.Messages;
using KitsuneCore.Services.Players;

namespace Demo.GameServer
{
	public class PlayerState : IMessageData
	{
		public PlayerId PlayerId { get; private set; }
		public string Name { get; private set; }

		public void ParseMessage(MessageBody msgBody)
		{
			PlayerId = msgBody.GetPlayerId(Tag.kDBUserId);
			Name = msgBody.GetString(Tag.kDBName);
		}
	}
}
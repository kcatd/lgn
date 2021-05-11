using System.Collections.Generic;
using KitsuneCommon.Net.Messages;
using KitsuneCore.Services.Players;

namespace Demo.GameServer
{
	public class GameState : IMessageData
	{
		public PlayerId HostId { get; private set; }
		public bool GameStarted { get; private set; }
		public PlayerId ActivePlayerId { get; private set; }
		public int Accumulator { get; private set; }
		public Dictionary<PlayerId, PlayerState> Players { get; private set; }

		public void ParseMessage(MessageBody msgBody)
		{
			HostId = msgBody.GetPlayerId(GameTag.HostPlayer);
			GameStarted = msgBody.GetBoolean(GameTag.GameStarted);
			ActivePlayerId = msgBody.GetPlayerId(GameTag.ActivePlayer);
			Accumulator = msgBody.GetInteger(GameTag.Accumulator);

			Players = new Dictionary<PlayerId, PlayerState>();
			MessageBody playersMessageBody = msgBody.GetChildByTag(GameTag.PlayersInGame);
			List<MessageBody> playersChildren = playersMessageBody.GetChildrenByTag(Tag.kTagUser);
			for (int i = 0; i < playersChildren.Count; ++i)
			{
				PlayerState player = new PlayerState();
				player.ParseMessage(playersChildren[i]);
				Players.Add(player.PlayerId, player);
			}
		}
	}
}
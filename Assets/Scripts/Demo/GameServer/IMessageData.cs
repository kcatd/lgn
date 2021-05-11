using KitsuneCommon.Net.Messages;

namespace Demo.GameServer
{
	/// <summary>
	/// Parses a <see cref="MessageBody"/> to create a type safe data object
	/// </summary>
	public interface IMessageData
	{
		void ParseMessage(MessageBody msgBody);
	}
}
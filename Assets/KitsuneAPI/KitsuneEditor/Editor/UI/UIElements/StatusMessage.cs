namespace KitsuneAPI.KitsuneEditor.Editor.UI.UIElements
{
	public class StatusMessage
	{
		public enum EStatusType
		{
			Info,
			Warn,
			Error,
			Success
		}
		/// <summary>
		///   <para>Text to display inside the status bar</para>
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		///   <para>Type of status message.</para>
		/// </summary>
		public EStatusType Type { get; set; }
	}
}
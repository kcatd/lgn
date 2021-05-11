using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneCore.Developer;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	public abstract class KitsuneBaseInspector : UnityEditor.Editor
	{
		protected VisualElement _rootElement;

		protected virtual void OnEnable()
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_ERROR>(OnError);
		}

		protected virtual void OnDisable()
		{
			Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_ERROR>(OnError);
		}

		protected void SetStatus(string message,
			StatusMessage.EStatusType type = StatusMessage.EStatusType.Info)
		{
			StatusMessage msg = new StatusMessage
			{
				Status = message,
				Type = type
			};
			using (ChangeEvent<StatusMessage> pooled =
				ChangeEvent<StatusMessage>.GetPooled(null, msg))
			{
				pooled.target = _rootElement;
				_rootElement?.SendEvent(pooled);
			}
		}

		protected void ClearStatus()
		{
			SetStatus("");
		}

		protected void DispatchEvent<T>(T newValue)
		{
			using (ChangeEvent<T> changeEvent =
				ChangeEvent<T>.GetPooled(default, newValue))
			{
				changeEvent.target = _rootElement;
				_rootElement?.SendEvent(changeEvent);
			}
		}
		
		protected void DispatchEvent<T>(T oldValue, T newValue)
		{
			using (ChangeEvent<T> changeEvent =
				ChangeEvent<T>.GetPooled(default, newValue))
			{
				changeEvent.target = _rootElement;
				_rootElement?.SendEvent(changeEvent);
			}
		}

		protected virtual void OnError(string errorMessage)
		{
			_rootElement?.SetEnabled(true);
			SetStatus(errorMessage, StatusMessage.EStatusType.Error);
		}
	}
}
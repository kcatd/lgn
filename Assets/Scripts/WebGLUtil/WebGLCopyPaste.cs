using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WebGLUtil
{
    public class WebGLCopyPaste : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TMP_InputField _input;
#if UNITY_WEBGL
        void Update()
        {
            if (_input.isFocused)
            {
                if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                    Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        System.PasteTextFromClipboard();
                    }
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        OnCopy();
                    }
                }
            }
        }
#endif

        public void OnSelect(BaseEventData data)
        {
#if UNITY_WEBGL
            System.OnPasteFromClipboard = OnPaste;
#endif
        }
        
        public void OnDeselect(BaseEventData data)
        {
#if UNITY_WEBGL
            System.OnPasteFromClipboard = null;
#endif
        }
        
#if UNITY_WEBGL
        private void OnPaste(string value)
        {
            if (!_input.readOnly)
            {
                if (_input.characterLimit > 0)
                {
                    _input.text = value.Substring(0, _input.characterLimit);    
                }
                else
                {
                    _input.text = value;
                }
            }
        }

        private void OnCopy()
        {
            System.CopyTextToClipboard(!string.IsNullOrEmpty(_input.text) ? _input.text : "");
        }
#endif
    }
}
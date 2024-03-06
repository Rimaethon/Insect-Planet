using UnityEngine;

namespace Rimaethon.Utility
{
    public class LogSystem : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private bool _showLogs = true;

        [SerializeField] private Color textColor;
        [HideInInspector] public Object sender;
        private string colorHex;

        private void Awake()
        {
            colorHex = ColorUtility.ToHtmlStringRGB(textColor);
        }

        public void PromptLog(object message)
        {
            if (_showLogs)
            {
                var formattedMessage = $"<color=#{colorHex}>[{sender.name}]</color> {message}";
                Debug.Log(formattedMessage, sender);
            }
        }

        public void LogWarning(object message)
        {
            if (_showLogs) Debug.LogWarning(message, sender);
        }

        public void LogError(object message)
        {
            if (_showLogs) Debug.LogError(message, sender);
        }
    }
}
using UnityEditor.Sequences;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimaethon._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private AudioClip buttonForward;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip buttonBack;
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        
        public void ClosePanel(GameObject panel)
        {
            panel.SetActive(false);
        }
        public void OpenPanel(GameObject panel)
        {
            panel.SetActive(true);
        }
        
    }
}
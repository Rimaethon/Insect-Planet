using System.Collections;
using TMPro;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    
    /// <summary> It works, but blinking cursor allocates 142bytes every frame, char array would probably solve it </summary>
    public class TypeWriter : MonoBehaviour
    {
     
        [SerializeField] private Texture2D noiseTexture;
        [SerializeField] private AudioClip typingSound;

        private TMP_Text _textMeshPro;
        private int _cursorIndex;
        private bool _isCursorVisible = true;
        private bool _isTyping;
        private AudioSource _audioSource;
        
        private const string TextToWrite = "year:2458\nSolar system:aurelıa majorıs\nmonıtorıng deep space\nno anomalıes detected.";
        private const string Space = " ";
        private const string Cursor = "|";

        //Glitch variables
        [SerializeField] private bool triggerGlitch;
        [SerializeField] private float glitchTime;
        private bool _isGlitching;
        private Color _currentColor;

     

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _textMeshPro = GetComponent<TMP_Text>();
            _textMeshPro.text = ""; 

            StartCoroutine(BlinkCursor());
            StartCoroutine(StartTyping());
        }

    
        private void Update()
        {
            _textMeshPro.text = InsertCursorCharacter(_cursorIndex-1);

            if (triggerGlitch&& !_isGlitching)
            {
                StartCoroutine(Glitch());

            }
        }

   
        
        IEnumerator Glitch()
        {
          
                _isGlitching = true;
                _textMeshPro.fontMaterial.SetTexture("_FaceTex",noiseTexture);
                _textMeshPro.fontMaterial.SetFloat("_FaceDilate",0.4f);
                _textMeshPro.fontMaterial.SetFloat("_OutlineSoftness",0.25f);
                _textMeshPro.fontMaterial.SetFloat("_OutlineThickness",0.2f);
                _currentColor=_textMeshPro.fontMaterial.GetColor("_FaceColor");
                _currentColor*=3f;
                _textMeshPro.fontMaterial.SetColor("_FaceColor",_currentColor);
                yield return new WaitForSeconds(glitchTime);
                _currentColor/=3f;
                _textMeshPro.fontMaterial.SetColor("_FaceColor",_currentColor);
                _textMeshPro.fontMaterial.SetTexture("_FaceTex",null);
                _textMeshPro.fontMaterial.SetFloat("_FaceDilate", -0.2f);
                _textMeshPro.fontMaterial.SetFloat("_OutlineSoftness",0f);
                _isGlitching = false;
        }
    
        
        IEnumerator StartTyping()
        {
            if (_isTyping)
                yield break;

            _isTyping = true;

            foreach (char c in TextToWrite)
            {
                if (_cursorIndex <= TextToWrite.Length)
                {
                    _textMeshPro.text = InsertCursorCharacter(_cursorIndex); 
                    _audioSource.PlayOneShot(typingSound);
                }

                if (c == '\n')
                {
                    _cursorIndex = _textMeshPro.text.Length;
                    yield return new WaitForSeconds(1f);
                }

           
                _cursorIndex = _textMeshPro.text.Length;

                yield return new WaitForSeconds(0.1f); 
            }
        }

        string InsertCursorCharacter(int index)
        {
            return TextToWrite.Substring(0, index) + (_isCursorVisible ? Cursor : Space);
        }

        IEnumerator BlinkCursor()
        {
            while (true)
            {
                _isCursorVisible = !_isCursorVisible;
                if (_cursorIndex == TextToWrite.Length)
                {
                    _textMeshPro.text = InsertCursorCharacter(_cursorIndex - 1);
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}

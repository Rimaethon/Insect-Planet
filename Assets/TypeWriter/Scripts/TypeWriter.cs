using System.Collections;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    [SerializeField]
    public GameObject letter;

    [SerializeField]
    public GameObject cursor;

    [SerializeField]
    public AudioClip typeSound;

    [SerializeField]
    public AudioClip endOfLineSound;

    [SerializeField]
    public Sprite[] characters;

    Vector3 pos;
    readonly float lineSpacing = 0.5f;   
    readonly float charSpacing = 0.35f;
    char[] convertedString;
    GameObject spriteChar;
    GameObject spriteCursor;
    AudioSource audioPlayer;

    readonly string[] data = { "terminal or", "typewriter effect", " ", "check out description", "for source code" };

    void Start()
    {
        pos = transform.localPosition;
        audioPlayer = GetComponent<AudioSource>();
        StartCoroutine(PrintOut());
    }

    IEnumerator PrintOut()
    {
        int charToPrint;
        Vector3 lastPos = new Vector3(0, 0,0);

        spriteCursor = Instantiate(cursor, pos, Quaternion.identity);
        spriteCursor.transform.parent = gameObject.transform;
        foreach (string singleLine in data) 
        {
            convertedString = singleLine.ToCharArray();
            pos.x = ((singleLine.Length * charSpacing) / 2) * -1;
            foreach (char singleChar in singleLine) 
            {
                charToPrint = SelectChar(singleChar);
                if (charToPrint >= 0 && charToPrint <= 26)  
                {
                    spriteChar = Instantiate(letter,pos, Quaternion.identity);
                    spriteChar.transform.parent = gameObject.transform;
                    spriteChar.transform.localPosition = pos;

                    spriteChar.GetComponent<SpriteRenderer>().sprite = characters[charToPrint];
                    spriteCursor.transform.localPosition = pos + new Vector3(charSpacing, 0,0);
                    audioPlayer.PlayOneShot(typeSound, 1f);
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.35f));   
                }
                pos.x += charSpacing; 
            }
            lastPos = pos;
            pos.y -= lineSpacing;
            pos.x = transform.localPosition.x;
        }
        spriteCursor.transform.localPosition = lastPos;
    }

    int SelectChar(char selChar) 
    {
        return selChar - 97; 
    }
}

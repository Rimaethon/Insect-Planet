using System.Collections;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    [SerializeField] private GameObject letter;
    [SerializeField] private GameObject cursor;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip endOfLineSound;
    [SerializeField] private Sprite[] characters;
    [SerializeField] private Sprite[] numbers;
    [SerializeField] private Sprite spacingSprite;

    private Vector3 pos;
    private readonly float lineSpacing = 0.5f;
    private readonly float charSpacing = 0.2f;
    private char[] convertedString;
    private GameObject spriteChar;
    private GameObject spriteCursor;
    private AudioSource audioPlayer;

    private readonly string[] data = { "year 2458             ", "aurelia majoris       ", "monitoring deep space ", "no anomalies detected." };

    private void Start()
    {
        
        audioPlayer = GetComponent<AudioSource>();
        StartCoroutine(PrintOut());
    }

    private IEnumerator PrintOut()
    {
        int charToPrint;
        Vector3 lastPos = new Vector3(0, 0, 0);

        spriteCursor = Instantiate(cursor, pos, Quaternion.identity);
        spriteCursor.transform.parent = gameObject.transform;
        spriteCursor.transform.localRotation=Quaternion.identity;
        foreach (string singleLine in data)
        {
            convertedString = singleLine.ToCharArray();
            pos.x = ((singleLine.Length * charSpacing) / 2) * -1;
            foreach (char singleChar in singleLine)
            {
                charToPrint = SelectChar(singleChar);
                if (charToPrint >= 0)
                {
                    if (charToPrint < characters.Length)
                    {
                        spriteChar = Instantiate(letter, pos, Quaternion.identity);
                        spriteChar.transform.parent = gameObject.transform;
                        spriteChar.transform.localPosition = pos;
                        spriteChar.transform.localRotation = Quaternion.identity;
                        spriteChar.GetComponent<SpriteRenderer>().sprite = characters[charToPrint];
                    }
                    else if (charToPrint == -2)  // Spacing character
                    {
                        spriteChar = Instantiate(letter, pos, Quaternion.identity);
                        spriteChar.transform.parent = gameObject.transform;
                        spriteChar.transform.localPosition = pos;
                        spriteChar.transform.localRotation = Quaternion.identity;
                        spriteChar.GetComponent<SpriteRenderer>().sprite = spacingSprite;
                    }
                    else
                    {
                        spriteChar = Instantiate(letter, pos, Quaternion.identity);
                        spriteChar.transform.parent = gameObject.transform;
                        spriteChar.transform.localPosition = pos;
                        spriteChar.transform.localRotation = Quaternion.identity;
                        spriteChar.GetComponent<SpriteRenderer>().sprite = numbers[charToPrint - characters.Length];
                    }
                    spriteCursor.transform.localPosition = pos + new Vector3(charSpacing, 0, 0);
                    audioPlayer.PlayOneShot(typeSound, 1f);
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.35f));
                }
                pos.x += charSpacing;
            }
            lastPos = pos;
            pos.x = transform.localPosition.x;
            pos.y -= lineSpacing;
        }
        spriteCursor.transform.localPosition = lastPos;
    }

    private int SelectChar(char selChar)
    {
        if (char.IsLetter(selChar))
        {
            return selChar - 'a';
        }

        if (char.IsNumber(selChar))
        {
            return selChar - '0' + characters.Length;
        }

        if (selChar == ' ')
        {
            return -2;  // Spacing character
        }

        return -1;
    }

}

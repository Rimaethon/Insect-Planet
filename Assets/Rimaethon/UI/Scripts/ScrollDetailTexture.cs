using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ScrollDetailTexture : MonoBehaviour
{
    public bool uniqueMaterial = false;
    public Vector2 scrollPerSecond = Vector2.zero;

    private Matrix4x4 m_Matrix;
    private Material mCopy;
    private Material mOriginal;
    private Image mSprite;
    private Material m_Mat;

    private void OnEnable()
    {
        mSprite = GetComponent<Image>();
        mOriginal = mSprite.material;

        if (uniqueMaterial && mSprite.material != null)
        {
            mCopy = new Material(mOriginal);
            mCopy.name = "Copy of " + mOriginal.name;
            mCopy.hideFlags = HideFlags.DontSave;
            mSprite.material = mCopy;
        }
    }

    private void OnDisable()
    {
        if (mCopy != null)
        {
            mSprite.material = mOriginal;
            if (Application.isEditor)
                DestroyImmediate(mCopy);
            else
                Destroy(mCopy);
            mCopy = null;
        }

        mOriginal = null;
    }

    private void Update()
    {
        var mat = mCopy != null ? mCopy : mOriginal;

        if (mat != null)
        {
            var tex = mat.GetTexture("_DetailTex");

            if (tex != null) mat.SetTextureOffset("_DetailTex", scrollPerSecond * Time.time);
            // TODO: It would be better to add support for MaterialBlocks on UIRenderer,
            // because currently only one Update() function's matrix can be active at a time.
            // With material block properties, the batching would be correctly broken up instead,
            // and would work with multiple widgets using this detail shader.
        }
    }
}
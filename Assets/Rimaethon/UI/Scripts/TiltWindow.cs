using UnityEngine;

public class TiltWindow : MonoBehaviour
{
    public Vector2 range = new(5f, 3f);

    private Transform mTrans;
    private Quaternion mStart;
    private Vector2 mRot = Vector2.zero;

    private void Start()
    {
        mTrans = transform;
        mStart = mTrans.localRotation;
    }

    private void Update()
    {
        var pos = Input.mousePosition;
        var halfWidth = Screen.width * 0.5f;
        var halfHeight = Screen.height * 0.5f;
        var x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);
        var y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
        mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * 5f);

        mTrans.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);
    }
}
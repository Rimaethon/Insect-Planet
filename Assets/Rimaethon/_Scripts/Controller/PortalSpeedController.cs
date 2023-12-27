using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PortalSpeedController : MonoBehaviour
{
    [Header("Time settings")]

    [SerializeField] private int accelerationStartDelayInMilliseconds;
    [SerializeField] private float accelerationEndTime;
    [SerializeField] private float decelerationStartTime;
    [SerializeField] private float decelerationEndTime;


    [Header("Speed settings")]
    [SerializeField] private float maxRotationSpeed;
    [SerializeField] private float minRotationSpeed;

    private float _currentSpeed;
    private float _rotationSpeed;
    private float _timer;

    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;
    private void Awake()
    {
    cancellationTokenSource = new CancellationTokenSource();
    cancellationToken = cancellationTokenSource.Token;
    }

    private void Start()
    {
        StartPortal(cancellationToken).Forget();
    }

    private void OnDisable()
    {
        if (cancellationTokenSource is { Token: { IsCancellationRequested: false } })
        {
            cancellationTokenSource.Cancel();
        }
    }


    private async UniTaskVoid StartPortal(CancellationToken cancellationTokenReference )
    {

        _timer = 0;


            while (_timer < accelerationEndTime&&!cancellationTokenReference.IsCancellationRequested)
            {
                _timer += Time.deltaTime;
                _currentSpeed = Mathf.Lerp(0f, maxRotationSpeed, _timer / accelerationEndTime);
                gameObject.transform.Rotate(0, 0, _currentSpeed * Time.deltaTime);
                await UniTask.Yield();

            }

            while (decelerationStartTime > _timer&&!cancellationTokenReference.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
            while (_timer < decelerationEndTime&&!cancellationTokenReference.IsCancellationRequested)
            {
                _timer += Time.deltaTime;

                _currentSpeed = Mathf.Lerp(_currentSpeed, minRotationSpeed, _timer / decelerationEndTime);
                gameObject.transform.Rotate(0, 0, _currentSpeed * Time.deltaTime);
                await UniTask.Yield();
            }



    }


}

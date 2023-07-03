using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalHandler : MonoBehaviour
{
    private PlayableDirector playableDirector;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        playableDirector.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {
        playableDirector.stopped -= OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("Timeline Stopped");
        EventManager.Instance.Broadcast(GameEvents.OnSceneChange, 1);
    }
}
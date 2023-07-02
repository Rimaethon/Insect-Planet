using System.Collections;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    [CreateAssetMenu(fileName = "New UI Manager", menuName = "UI/Manager", order = 1)]
    public class UIManager : ScriptableObject
    {
        #region Variables

        [SerializeField] private bool containsLoopGroup = false;
        [SerializeField] private UIPanel initialPanel = null;
        [SerializeField] private float loopTransitionSpeed = 1;
        [SerializeField] private UITransition prevTransition = null;
        [SerializeField] private UITransition nextTransition = null;
        [SerializeField] private UIPanel[] loopGroup = null;

        private MonoBehaviour rootComponent;
        private Animator animatorComponent;
        private Transform inParent;
        private Transform outParent;
        private GameObject clickBlocker;
        private GameObject passOverBlocker;
        private bool transitioning = false;
        private UIPanel currentPanel;
        private int loopIndex = 0;

        #endregion

        #region Initialization

        public void Initialize(RectTransform parent)
        {
            rootComponent = ((GameObject)Instantiate(Resources.Load("UIManager/UI Root"), parent)).GetComponent<MonoBehaviour>();
            animatorComponent = rootComponent.GetComponentInChildren<Animator>();
            inParent = animatorComponent.transform.Find("Parent_In");
            outParent = animatorComponent.transform.Find("Parent_Out");
            clickBlocker = animatorComponent.transform.Find("ClickBlocker").gameObject;
            passOverBlocker = animatorComponent.transform.Find("PassOverBlocker").gameObject;
            rootComponent.GetComponent<UIFitter>().Fit();

            UIPanel panel = initialPanel;
            RectTransform panelRoot = (RectTransform)Instantiate(panel.panelPrefab, inParent).transform;
            animatorComponent.Play("Instant");
            panel.Initialize(this, panelRoot);
            panel.OnPanelTransitionedIn(this, panelRoot);
            currentPanel = panel;
            transitioning = false;
        }

        public void Clear()
        {
            if (rootComponent != null)
            {
                Destroy(rootComponent.gameObject);
            }
        }

        #endregion

        #region Panel Transition

        public void SetPanel(UIPanel input)
        {
            if (TryPreparePanel(input, input.transition, input.animationSpeed))
            {
                input.PlayTransition(this, input.transition, input.animationSpeed);
                transitioning = true;
            }
        }

        public void NextPanel()
        {
            if (CheckIsValidLoopGroup("NextPanel()"))
            {
                loopIndex++;
                if (TryPreparePanel(loopGroup[loopIndex], nextTransition, loopTransitionSpeed))
                {
                    loopGroup[loopIndex].PlayTransition(this, nextTransition, containsLoopGroup ? loopTransitionSpeed : loopGroup[loopIndex].animationSpeed);
                    transitioning = true;
                }
            }
        }

        public void PreviousPanel()
        {
            if (CheckIsValidLoopGroup("PreviousPanel()"))
            {
                loopIndex--;
                if (TryPreparePanel(loopGroup[loopIndex], prevTransition, loopTransitionSpeed))
                {
                    loopGroup[loopIndex].PlayTransition(this, prevTransition, containsLoopGroup ? loopTransitionSpeed : loopGroup[loopIndex].animationSpeed);
                    transitioning = true;
                }
            }
        }

        private bool CheckIsValidLoopGroup(string functionAttempt)
        {
            if (!containsLoopGroup)
            {
                Debug.LogError("Trying to Call " + functionAttempt + " on a UIManager that doesn't contain a loop group");
                return false;
            }
            if (loopGroup.Length == 0)
            {
                Debug.LogError("No panels listed in loop group of " + name);
                return false;
            }
            return true;
        }

        private bool TryPreparePanel(UIPanel input, UITransition transition, float speed)
        {
            if (transitioning)
            {
                Debug.LogError("Wait for transition to end before setting a new panel");
                return false;
            }
            if (currentPanel != null)
            {
                currentPanel.OnPanelTransitionOutStarted((RectTransform)rootComponent.transform);
            }

            clickBlocker.SetActive(true);
            passOverBlocker.SetActive(input.blockPassOver);

            inParent.SetSiblingIndex(transition.panelOnTop == UITransition.ParentSelection.inComingPanel ? 2 : 1);

            foreach (Transform child in inParent)
            {
                child.SetParent(outParent);
                RectTransform rt = (RectTransform)child;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            rootComponent.StartCoroutine(WaitForAnimationEnd(input, transition, speed));
            input.Initialize(this, (RectTransform)Instantiate(input.panelPrefab, inParent).transform);
            return true;
        }

        private IEnumerator WaitForAnimationEnd(UIPanel panel, UITransition transition, float speed)
        {
            yield return new WaitForSeconds(transition.inAnimation.length * speed);
            ClearOutParent();
            transitioning = false;
            animatorComponent.Play("Idle");
            currentPanel.OnPanelTransitionedOut(currentPanel.GetPanel());
            currentPanel = panel;
            clickBlocker.SetActive(false);
        }

        private void ClearOutParent()
        {
            foreach (Transform child in outParent)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion
    }
}

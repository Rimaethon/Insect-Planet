using UnityEngine;

namespace Rimaethon._Scripts.UI
{
	public class UIPanelOpenerButton: UIButton
	{
		[SerializeField] private RectTransform _panelToOpen;
		[SerializeField] private RectTransform _panelToClose;
		private void OpenPanel()
		{
			StartCoroutine(UIAnimationHelperMethods.SlideIn(_panelToOpen, UIDirection.UP, 2f));
			StartCoroutine(UIAnimationHelperMethods.SlideOut(_panelToClose, UIDirection.UP, 2f));
		}
		protected override void DoOnClicked()
		{
			OpenPanel();
		}
	}
}

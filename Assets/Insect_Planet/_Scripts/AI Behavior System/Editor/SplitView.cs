using UnityEngine.UIElements;

namespace AI_Behavior_System.Editor {
    public class SplitView : TwoPaneSplitView {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TheKiwiCoder
{
    [System.Serializable]
    public class Parallel : CompositeNode
    {
        private List<State> childrenLeftToExecute = new();

        protected override void OnStart()
        {
            childrenLeftToExecute.Clear();
            children.ForEach(a => { childrenLeftToExecute.Add(State.Running); });
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var stillRunning = false;
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i)
                if (childrenLeftToExecute[i] == State.Running)
                {
                    var status = children[i].Update();
                    if (status == State.Failure)
                    {
                        AbortRunningChildren();
                        return State.Failure;
                    }

                    if (status == State.Running) stillRunning = true;

                    childrenLeftToExecute[i] = status;
                }

            return stillRunning ? State.Running : State.Success;
        }

        private void AbortRunningChildren()
        {
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i)
                if (childrenLeftToExecute[i] == State.Running)
                    children[i].Abort();
        }
    }
}
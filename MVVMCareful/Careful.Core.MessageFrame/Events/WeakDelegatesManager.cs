using System;
using System.Collections.Generic;
using System.Linq;

namespace Careful.Core.MessageFrame.Events
{
    public class WeakDelegatesManager
    {
        private readonly List<DelegateReference> listeners = new List<DelegateReference>();

        public void AddListener(Delegate listener)
        {
            this.listeners.Add(new DelegateReference(listener, false));
        }

        public void RemoveListener(Delegate listener)
        {
            //Remove the listener, and prune collected listeners
            this.listeners.RemoveAll(reference => reference.TargetEquals(null) || reference.TargetEquals(listener));
        }

        public void Raise(params object[] args)
        {
            this.listeners.RemoveAll(listener => listener.TargetEquals(null));

            foreach (Delegate handler in this.listeners.Select(listener => listener.Target).Where(listener => listener != null).ToList())
            {
                handler.DynamicInvoke(args);
            }
        }
    }
}

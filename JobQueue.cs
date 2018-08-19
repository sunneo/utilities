using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class JobQueue
    {
        Locker mLocker = new Locker();
        LinkedList<Action> Queue = new LinkedList<Action>();
        LinkedList<Action> Notifier = new LinkedList<Action>();
        public void Push(Action action, Action notifier=null)
        {
            mLocker.Synchronized(() =>
            {
                Queue.AddLast(action);
                Notifier.AddLast(new Action(() =>
                {
                    if (notifier != null)
                    {
                        notifier();
                    }
                }));
            });
        }
        public void Clear()
        {
            mLocker.Synchronized(() =>
            {
                Queue.Clear();
                Notifier.Clear();
            });
        }
        public bool Launch()
        {
            Action action = null;
            Action notifier = null;
            mLocker.Synchronized(() =>
            {
                if (Queue.Count > 0)
                {
                    action = Queue.First.Value;
                    notifier = Notifier.First.Value;
                    Queue.RemoveFirst();
                    Notifier.RemoveFirst();
                }

            });
            if(action != null) action();
            if(notifier != null) notifier();
            return (action != null);
        }

        public int Count
        {
            get
            {
                return mLocker.Synchronized(() =>
                {
                    return Queue.Count;
                });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class UndoRedoStack<T>
    {
        bool doTrackTextChange = true;
        List<T> StatusStack = new List<T>();
        int StackPoint = -1;
        int m_TrackFrequency = 100;
        bool TrackUndoRedo = true;
        DateTime TextChangeTime;
        public event EventHandler<T> OnUndoRedo;
        public event EventHandler<bool> OnCanUndoChanged;
        public event EventHandler<bool> OnCanRedoChanged;
        /// <summary>
        /// frequency to track undo/redo, default to 100 ms
        /// </summary>
        [Description("Frequency to track text change")]
        public int TrackFrequency
        {
            get
            {
                return m_TrackFrequency;
            }
            set
            {
                m_TrackFrequency = value;
            }
        }

        /// <summary>
        /// Track text change
        /// </summary>
        [Description("是否追蹤文字變動到堆疊(預設:false)")]
        public bool IsTextChangeTracked
        {
            get
            {
                return doTrackTextChange;
            }
            set
            {
                doTrackTextChange = value;
            }
        }

        bool m_CanUndo;
        bool m_CanRedo;

        public bool CanUndo
        {
            get
            {
                return m_CanUndo;
               
            }
            set
            {
                bool oldValue = m_CanUndo;
                m_CanUndo = value;
                if (oldValue != m_CanUndo)
                {
                    if (OnCanRedoChanged != null)
                    {
                        OnCanRedoChanged(this, value);
                    }
                }
            }
        }
        public bool CanRedo
        {
            get
            {
                return m_CanRedo;  
            }
            set
            {
                bool oldValue = m_CanRedo;
                m_CanRedo = value;
                if (oldValue != m_CanRedo)
                {
                    if (OnCanRedoChanged != null)
                    {
                        OnCanRedoChanged(this, value);
                    }
                }
            }
        }
        private bool CheckCanUndo()
        {
            return StackPoint > 0;
        }
        private bool CheckCanRedo()
        {
            return StackPoint + 1 < StatusStack.Count;
        }

        public void Redo()
        {
            TrackUndoRedo = false;
            CanRedo = CheckCanRedo();
            if (!CanRedo)
            {
                TrackUndoRedo = true;
                return;
            }
            T content = StatusStack[StackPoint + 1];
            ++StackPoint;
            if (OnUndoRedo != null)
            {
                OnUndoRedo(this, content);
            }
            CanUndo = CheckCanUndo();
            CanRedo = CheckCanRedo();
            TrackUndoRedo = true;
        }
        public void Undo()
        {
            TrackUndoRedo = false;
            CanUndo = CheckCanUndo();
            if (!CanUndo)
            {
                TrackUndoRedo = true;
                return;
            }
            T content = StatusStack[StackPoint - 1];
            --StackPoint;
            if (OnUndoRedo != null)
            {
                OnUndoRedo(this, content);
            }
            CanUndo = CheckCanUndo();
            CanRedo = CheckCanRedo();
            TrackUndoRedo = true;
        }
        public void PerformPushChange(T content)
        {
            if (doTrackTextChange)
            {
                if (!TrackUndoRedo)
                {
                    return;
                }
                DateTime now = DateTime.Now;
                if (now.Subtract(TextChangeTime).TotalMilliseconds < m_TrackFrequency)
                {
                    return;
                }
                TextChangeTime = now;
                if (StackPoint != -1 && StackPoint != StatusStack.Count - 1)
                {
                    StatusStack.RemoveRange(StackPoint, (StatusStack.Count - StackPoint));
                }
                StatusStack.Add(content);
                ++StackPoint;
                CanUndo = CheckCanUndo();
                CanRedo = CheckCanRedo();
            }
        }
    }
}

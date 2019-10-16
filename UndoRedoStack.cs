using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public interface IUndoRedo<T>
    {
        event EventHandler<T> OnUndoRedo;
        event EventHandler<bool> OnCanUndoChanged;
        event EventHandler<bool> OnCanRedoChanged;

        /// <summary>
        /// frequency to track undo/redo, default to 100 ms
        /// </summary>
        [Description("Frequency to track change")]
        int TrackFrequency { get; set; }

        /// <summary>
        /// Track text change
        /// </summary>
        [Description("Track Change To Stack(default:false)")]
        bool IsChangeTracked { get; set; }

        /// <summary>
        /// Test whether Can Undo
        /// </summary>
        bool CanUndo { get; }
       
        /// <summary>
        /// Test whether Can Redo
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// undo change
        /// </summary>
        void Undo();

        /// <summary>
        /// redo change
        /// </summary>
        void Redo();

        /// <summary>
        /// push change into stack
        /// </summary>
        /// <param name="content">content to change/track</param>
        void PushChange(T change);
        
        /// <summary>
        /// empty undo/redo buffer
        /// </summary>
        void EmptyUndoRedoBuffer();
    }
    public class StackPointMaintainer
    {
        protected virtual int Count()
        {
            return 0;
        }
        protected int StackPoint = -1;
        protected virtual bool CheckCanUndo()
        {
            return StackPoint > 0;
        }
        protected virtual bool CheckCanRedo()
        {
            return StackPoint + 1 <= Count();
        }
        protected virtual int PeekUndo()
        {
            return StackPoint - 1;
        }
        protected virtual int PeekRedo()
        {
            return StackPoint;
        }
        protected virtual void MoveUndo()
        {
            --StackPoint;
        }
        protected virtual void MoveRedo()
        {
            ++StackPoint;
        }
    }
    public class UndoRedoStack<T>: StackPointMaintainer, IUndoRedo<T>
    {
#region PRIVATE fields
        bool m_IsTrackChange = false;
        List<T> StatusStack = new List<T>();
        int m_TrackFrequency = 100;
        bool shouldTrackUndoRedo = true;
        DateTime TextChangeTime;
        bool m_CanUndo;
        bool m_CanRedo;
        protected override int Count()
        {
            return StatusStack.Count;
        }
#endregion

#region PUBLIC events
        public event EventHandler<T> OnUndoRedo;
        public event EventHandler<bool> OnCanUndoChanged;
        public event EventHandler<bool> OnCanRedoChanged;
#endregion

#region PROPERTIES
        /// <summary>
        /// frequency to track undo/redo, default to 100 ms
        /// </summary>
        [Description("Frequency to track change")]
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
        [Description("Track Change To Stack(default:false)")]
        public bool IsChangeTracked
        {
            get
            {
                return m_IsTrackChange;
            }
            set
            {
                m_IsTrackChange = value;
            }
        }

      
        /// <summary>
        /// Test whether Can Undo
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return m_CanUndo;

            }
            private set
            {
                bool oldValue = m_CanUndo;
                m_CanUndo = value;
                if (oldValue != m_CanUndo)
                {
                    if (OnCanUndoChanged != null)
                    {
                        OnCanUndoChanged(this, value);
                    }
                }
            }
        }
        /// <summary>
        /// Test whether Can Redo
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return m_CanRedo;
            }
            private set
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
#endregion
#region PUBLIC methods
        /// <summary>
        /// redo change
        /// </summary>
        public void Redo()
        {
            shouldTrackUndoRedo = false;
            CanRedo = CheckCanRedo();
            if (!CanRedo)
            {
                shouldTrackUndoRedo = true;
                return;
            }
            T content = StatusStack[PeekRedo()];
            MoveRedo();
            if (OnUndoRedo != null)
            {
                OnUndoRedo(this, content);
            }
            CanUndo = CheckCanUndo();
            CanRedo = CheckCanRedo();
            shouldTrackUndoRedo = true;
        }
        /// <summary>
        /// undo change
        /// </summary>
        public void Undo()
        {
            shouldTrackUndoRedo = false;
            CanUndo = CheckCanUndo();
            if (!CanUndo)
            {
                shouldTrackUndoRedo = true;
                return;
            }
            T content = StatusStack[PeekUndo()];
            MoveUndo();
            if (OnUndoRedo != null)
            {
                OnUndoRedo(this, content);
            }
            CanUndo = CheckCanUndo();
            CanRedo = CheckCanRedo();
            shouldTrackUndoRedo = true;
        }
        /// <summary>
        /// push change into stack
        /// </summary>
        /// <param name="content">content to change/track</param>
        public void PushChange(T content)
        {
            if (m_IsTrackChange)
            {
                if (!shouldTrackUndoRedo)
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
        /// <summary>
        /// empty undo/redo buffer
        /// </summary>
        public void EmptyUndoRedoBuffer()
        {
            this.StackPoint = -1;
            this.StatusStack.Clear();
            this.CanRedo = false;
            this.CanRedo = false;
        }
#endregion
    }
}

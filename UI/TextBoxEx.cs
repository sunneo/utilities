using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class TextBoxEx:TextBox, IUndoRedo<KeyValuePair<int,String>>
    {
        Locked<Color> mMouseEnterBackColor = Color.FromArgb(255, 255, 255, 128);
        Locked<Color> mNormalBackColor = Color.FromArgb(255, 255, 255, 255);
        Locked<Color> mFocusBackColor = Color.FromArgb(255, 255, 255, 128);
        volatile bool Focused = false;
        UndoRedoStack<KeyValuePair<int, string>> TextBoxUndoRedoTracker = new UndoRedoStack<KeyValuePair<int, string>>();

     
        
        
        protected override void OnTextChanged(EventArgs e)
        {
            if (this.IsChangeTracked)
            {
                if (doTrack)
                {
                    RedoStack.PushChange(new KeyValuePair<int, String>(this.SelectionStart, this.Text));
                }
            }
            base.OnTextChanged(e);
        }
        
        
        [Browsable(true)]
        public Color MouseEnterBackColor
        {
            get
            {
                return mMouseEnterBackColor;
            }
            set
            {
                mMouseEnterBackColor = value;
            }
        }
        
        [Browsable(true)]
        public Color NormalColor
        {
            get
            {
                return mNormalBackColor;
            }
            set
            {
                mNormalBackColor = value;
            }
        }

        [Browsable(true)]
        public Color FocusBackColor
        {
            get
            {
                return mFocusBackColor;
            }
            set
            {
                mFocusBackColor = value;
            }
        }
        public TextBoxEx()
        {
            RedoStack.OnUndoRedo+=RedoStack_OnUndoRedo;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        
        protected override void OnGotFocus(EventArgs e)
        {
            Focused = true;
            this.BackColor = FocusBackColor;
            base.OnGotFocus(e);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Focused = false;
            this.BackColor = NormalColor;
            base.OnLostFocus(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!Focused)
            {
                this.BackColor = NormalColor;
            }
            else
            {
                this.BackColor = FocusBackColor;
            }
            base.OnMouseLeave(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!Focused)
            {
                this.BackColor = MouseEnterBackColor;
            }
            base.OnMouseEnter(e);
        }

        #region UNDO/REDO
        private UndoRedoStack<KeyValuePair<int, String>> RedoStack = new UndoRedoStack<KeyValuePair<int, String>>();
        bool doTrack = true;
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            if (IsChangeTracked)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (keyData.HasFlag(Keys.Z))
                    {
                        if (ModifierKeys.HasFlag(Keys.Shift))
                        {
                            Redo();
                        }
                        else
                        {
                            Undo();
                        }
                        return true;
                    }
                    else if (keyData.HasFlag(Keys.Y))
                    {
                        Redo();
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void RedoStack_OnUndoRedo(object sender, KeyValuePair<int, String> e)
        {
            this.Text = e.Value;
            this.SelectionStart = e.Key;
        }
        public event EventHandler<KeyValuePair<int, String>> OnUndoRedo
        {
            add
            {
                RedoStack.OnUndoRedo += value;
            }
            remove
            {
                RedoStack.OnUndoRedo -= value;
            }
        }

        public event EventHandler<bool> OnCanUndoChanged
        {
            add
            {
                RedoStack.OnCanUndoChanged += value;
            }
            remove
            {
                RedoStack.OnCanUndoChanged -= value;
            }
        }

        public event EventHandler<bool> OnCanRedoChanged
        {
            add
            {
                RedoStack.OnCanRedoChanged += value;
            }
            remove
            {
                RedoStack.OnCanRedoChanged -= value;
            }
        }

        public int TrackFrequency
        {
            get
            {
                return RedoStack.TrackFrequency;
            }
            set
            {
                RedoStack.TrackFrequency = value;
            }
        }

        public bool IsChangeTracked
        {
            get
            {
                return RedoStack.IsChangeTracked;
            }
            set
            {
                RedoStack.IsChangeTracked = value;
                PushChange(new KeyValuePair<int, string>(this.SelectionStart, this.Text));

            }
        }

        public bool CanRedo
        {
            get { return RedoStack.CanRedo; }
        }

        public void Redo()
        {
            doTrack = false;
            RedoStack.Redo();
            doTrack = true;
        }
        public void Undo()
        {
            doTrack = false;
            RedoStack.Undo();
            doTrack = true;
        }

        public void PushChange(KeyValuePair<int, String> change)
        {
            RedoStack.PushChange(change);
        }


        public void EmptyUndoRedoBuffer()
        {
            RedoStack.EmptyUndoRedoBuffer();
            RedoStack.PushChange(new KeyValuePair<int, String>(this.SelectionStart, this.Text));
        }
        #endregion

    }
}

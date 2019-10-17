using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class TextBoxEx:TextBox
    {
        Locked<Color> mMouseEnterBackColor = Color.FromArgb(255, 255, 255, 128);
        Locked<Color> mNormalBackColor = Color.FromArgb(255, 255, 255, 255);
        Locked<Color> mFocusBackColor = Color.FromArgb(255, 255, 255, 128);
        volatile bool Focused = false;
        UndoRedoStack<String> TextBoxUndoRedoTracker = new UndoRedoStack<string>();

        public event EventHandler<bool> OnCanUndoChanged
        {
            add
            {
                TextBoxUndoRedoTracker.OnCanRedoChanged += value;
            }
            remove
            {
                TextBoxUndoRedoTracker.OnCanRedoChanged += value;
            }
        }
        public event EventHandler<bool> OnCanRedoChanged
        {
            add
            {
                TextBoxUndoRedoTracker.OnCanRedoChanged += value;
            }
            remove
            {
                TextBoxUndoRedoTracker.OnCanRedoChanged += value;
            }
        }
        /// <summary>
        /// frequency to track undo/redo, default to 100 ms
        /// </summary>
        [Description("Frequency to track text change")]
        public int TrackFrequency
        {
            get
            {
                return TextBoxUndoRedoTracker.TrackFrequency;
            }
            set
            {
                TextBoxUndoRedoTracker.TrackFrequency = value;
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
                return TextBoxUndoRedoTracker.IsChangeTracked;
            }
            set
            {
                TextBoxUndoRedoTracker.IsChangeTracked = value;
                if (value)
                {
                    TextBoxUndoRedoTracker.PushChange(this.Text);
                }
            }
        }


        public bool CanUndo
        {
            get
            {
                return TextBoxUndoRedoTracker.CanRedo;
            }
        }
        public bool CanRedo
        {
            get
            {
                return TextBoxUndoRedoTracker.CanRedo;
            }
        }


        public void Redo()
        {
            TextBoxUndoRedoTracker.Redo();
        }
        public void Undo()
        {
            TextBoxUndoRedoTracker.Undo();
        }
     
        
        
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            TextBoxUndoRedoTracker.PushChange(this.Text);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (keyData.HasFlag( Keys.Z))
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
            return base.ProcessCmdKey(ref msg, keyData);
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
            TextBoxUndoRedoTracker.OnUndoRedo += TextBoxUndoRedoTracker_OnUndoRedo;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        void TextBoxUndoRedoTracker_OnUndoRedo(object sender, string e)
        {
            this.Text= e;
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
    
    }
}

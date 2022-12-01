/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.OptionParser.Attributes;
using Utilities.UI;

namespace Utilities
{
    public class IniConfigurationUI<T>
    {
        String TargetFileName;
        public T Value = default(T);
        List<Control> DataFieldControl = new List<Control>();
        Dictionary<String, IniReader.OnSerializeNotificationEventArgs> FieldDeserializeMap = new Dictionary<string, IniReader.OnSerializeNotificationEventArgs>();
        Dictionary<String, List<String>> FieldCategoryMap = new Dictionary<string, List<String>>();
       
        
        public class CustomFormEditEventArg : EventArgs
        {
            public IniReader.OnSerializeNotificationEventArgs SerializeArg;
            public string DisplayName="";
            public string Description = "";
            public String FieldName = "";
            public Var<object> Value = new Var<object>();
            public Control CustomControl;
            public bool Handled;
            public CustomFormEditEventArg()
            {
                Value.ValueChanged += Value_ValueChanged;
            }

            void Value_ValueChanged(object sender, object e)
            {
                if (SerializeArg != null)
                {
                    SerializeArg.Field.SetValue(SerializeArg.Target, e);
                }
            }
        }
        public event EventHandler<CustomFormEditEventArg> OnCustomForm;
        public event EventHandler<CustomFormEditEventArg> OnCustomEditorNeeded;
        bool fromFile = false;
        public IniConfigurationUI(String filename)
        {
            this.TargetFileName = filename;
            fromFile = true;
        }
        public IniConfigurationUI(T val)
        {
            fromFile = false;
            this.Value = val;
        }
        public bool ShowAll = false;
        public Form BuildForm(String title="")
        {
            UI.SaveConfigurationTemplateForm ret = new UI.SaveConfigurationTemplateForm();
            DataFieldControl.Clear();
            ToolTip tip = new ToolTip();
            if (fromFile)
            {
                this.Value = IniReader.Deserialize<T>(this.TargetFileName, HandleDeserializeField);
            }
            else
            {
                IniWriter writer = new IniWriter();
                writer.Serialize(Value);
                IniReader.DeserializeString<T>(writer.ToString(), HandleDeserializeField);
            }
            
            List<GroupBox> groupBoxs = new List<GroupBox>();
            if (String.IsNullOrEmpty(title))
            {
                title = this.TargetFileName;
            }
            ret.Text = title;
            ret.TipText = "設定(點選欄位/標籤可以看到欄位敘述)";


            foreach (String g in FieldCategoryMap.Keys)
            {

                GroupBox gbox = new GroupBox();
                groupBoxs.Add(gbox);
                gbox.Margin = new Padding(3);
                List<String> names = FieldCategoryMap[g];
                TableLayoutPanel tablelayout = new TableLayoutPanel();
                tablelayout.RowCount = names.Count;
                tablelayout.ColumnCount = 2;
                tablelayout.AutoScroll = true;
                for (int i = 0; i < 2; ++i)
                {
                    ColumnStyle rs = new ColumnStyle();
                    tablelayout.ColumnStyles.Add(rs);
                    tablelayout.ColumnStyles[i].SizeType = SizeType.Percent;
                    tablelayout.ColumnStyles[i].Width = 0.5f;
                }

                if (String.IsNullOrEmpty(g))
                {
                    gbox.Text = "Default";
                }
                else
                {
                    gbox.Text = g;
                }

                int row = 0;

                foreach (String name in names)
                {
                    IniReader.OnSerializeNotificationEventArgs arg = FieldDeserializeMap[name];
                    if (arg.Field.FieldType.IsClass)
                    {
                        if (arg.Field.FieldType != typeof(String)
                           && arg.Field.FieldType != typeof(Rectangle)
                            && arg.Field.FieldType != typeof(Size)
                            && arg.Field.FieldType != typeof(Point)
                            && arg.Field.FieldType != typeof(Color)
                            && arg.Field.FieldType != typeof(int[]))
                        {
                            continue;
                        }
                    }

                    FieldDisplayNameAttribute displayName = (FieldDisplayNameAttribute)arg.Field.GetCustomAttribute(typeof(FieldDisplayNameAttribute), true);
                    DescriptionAttribute description = (DescriptionAttribute)arg.Field.GetCustomAttribute(typeof(DescriptionAttribute), true);
                    CategoryAttribute category = (CategoryAttribute)arg.Field.GetCustomAttribute(typeof(CategoryAttribute), true);
                    AttributeConfigureUIVisible visible = (AttributeConfigureUIVisible)arg.Field.GetCustomAttribute(typeof(AttributeConfigureUIVisible), true);

                    RowStyle rs = new RowStyle();
                    if (ShowAll || visible == null || visible.Visible)
                    {
                        tablelayout.RowStyles.Add(rs);
                        rs.SizeType = SizeType.Absolute;
                        rs.Height = 30;
                    }
                    String descriptionTxt = "";
                    if (description != null)
                    {
                        descriptionTxt = description.Description;
                    }
                    else
                    {
                        descriptionTxt = name;
                    }
                    EventHandler focusHandler = new EventHandler((object s, EventArgs e) =>
                    {
                        ret.TipText = descriptionTxt;
                    });
                    Panel left = new Panel();
                    Panel right = new Panel();
                    left.Dock = DockStyle.Fill;
                    right.Dock = DockStyle.Fill;
                    left.Margin = new Padding(0);
                    right.Margin = new Padding(0);
                    Label nameLabel = new Label();
                    String strname = GetPropertyDisplayName(arg.Field);
                    if (String.IsNullOrEmpty(strname))
                    {
                        strname = arg.FullName;
                    }
                    if (displayName != null)
                    {
                        nameLabel.Text = String.Format("{0}({1})", displayName.DisplayName, strname);
                    }
                    else
                    {
                        nameLabel.Text = strname;
                    }
                    nameLabel.Dock = DockStyle.Fill;
                    nameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    nameLabel.Click += focusHandler;
                    left.Controls.Add(nameLabel);
                    IniReader reader = arg.Reader;
                    Type fieldType = arg.Field.FieldType;
                    bool customEditorHandled = false;
                    if (OnCustomEditorNeeded != null)
                    {
                        CustomFormEditEventArg args = new CustomFormEditEventArg();
                        args.FieldName = name;
                        args.DisplayName = strname;
                        args.Description = descriptionTxt;
                        args.Value.Value = arg.FieldValue;
                        args.SerializeArg = arg;
                        OnCustomEditorNeeded(this, args);
                        if (args.Handled)
                        {
                            if (args.CustomControl != null)
                            {
                                Control ctrl = args.CustomControl;
                                ctrl.Size = new Size(100, (int)(rs.Height));
                                ctrl.Dock = DockStyle.Fill;
                                ctrl.Tag = arg.FullName;
                                ctrl.GotFocus += focusHandler;
                                right.Controls.Add(ctrl);
                                customEditorHandled = true;
                            }
                            
                        }
                        
                    }
                    if (!customEditorHandled)
                    {
                        if (fieldType == typeof(Color))
                        {
                            Button btn = new Button();
                            btn.Size = new Size(100, (int)(rs.Height));
                            btn.BackColor = (Color)arg.FieldValue;
                            btn.Dock = DockStyle.Fill;
                            btn.Click += btn_colorPickerClick;
                            right.Controls.Add(btn);
                            btn.Tag = arg.FullName;
                            btn.GotFocus += focusHandler;
                            this.DataFieldControl.Add(btn);
                        }
                        else if (fieldType == typeof(bool))
                        {
                            ComboBox cbox = new ComboBox();
                            cbox.DropDownStyle = ComboBoxStyle.DropDownList;
                            cbox.Items.Add("false");
                            cbox.Items.Add("true");
                            cbox.Dock = DockStyle.Fill;
                            cbox.Text = (String)arg.FieldValue.ToString().ToLower();
                            right.Controls.Add(cbox);
                            cbox.Tag = arg.FullName;
                            cbox.GotFocus += focusHandler;
                            this.DataFieldControl.Add(cbox);
                        }
                        else
                        {
                            if (fieldType.IsEnum)
                            {
                                ComboBox cbox = new ComboBox();
                                cbox.DropDownStyle = ComboBoxStyle.DropDownList;
                                var enumNames = fieldType.GetEnumNames();
                                cbox.Items.AddRange(enumNames);
                                cbox.Dock = DockStyle.Fill;
                                String val = "";
                                if(arg.FieldValue != null) val = arg.FieldValue.ToString();
                                cbox.Text = val;
                                right.Controls.Add(cbox);
                                cbox.Tag = arg.FullName;
                                cbox.GotFocus += focusHandler;
                                this.DataFieldControl.Add(cbox);
                            }
                            else
                            {
                                FieldAvailableValueAttribute availableValueAttribute = (FieldAvailableValueAttribute)arg.Field.GetCustomAttribute(typeof(FieldAvailableValueAttribute), true);
                                if (availableValueAttribute != null && availableValueAttribute.FieldValues != null && availableValueAttribute.FieldValues.Length > 0)
                                {
                                    ComboBox cbox = new ComboBox();
                                    cbox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    var enumNames = availableValueAttribute.FieldValues;
                                    cbox.Items.AddRange(enumNames);
                                    cbox.Dock = DockStyle.Fill;
                                    String val = "";
                                    if (arg.FieldValue != null) val = arg.FieldValue.ToString();
                                    cbox.Text = val;
                                    right.Controls.Add(cbox);
                                    cbox.Tag = arg.FullName;
                                    cbox.GotFocus += focusHandler;
                                    this.DataFieldControl.Add(cbox);
                                }
                                else
                                {
                                    TextBoxEx tbox = new TextBoxEx();
                                    tbox.Size = new Size(100, (int)(rs.Height));
                                    tbox.Dock = DockStyle.Fill;
                                    tbox.IsChangeTracked = true;
                                    right.Controls.Add(tbox);
                                    tbox.Tag = arg.FullName;
                                    tbox.Text = (String)arg.FieldValue.ToString();
                                    tbox.GotFocus += focusHandler;
                                    tbox.Click += (s, e) =>
                                    {
                                        if (OnCustomForm != null)
                                        {
                                            CustomFormEditEventArg args = new CustomFormEditEventArg();
                                            args.FieldName = name;
                                            args.DisplayName = strname;
                                            args.Description = descriptionTxt;
                                            args.Value = tbox.Text;
                                            args.SerializeArg = arg;
                                            OnCustomForm(this, args);
                                            if (args.Handled)
                                            {
                                                tbox.Text = args.Value.ToString();
                                            }
                                        }
                                    };
                                    this.DataFieldControl.Add(tbox);
                                }
                            }
                        }
                    }
                    if (ShowAll || visible == null || visible.Visible)
                    {
                        tablelayout.Height += ((int)(rs.Height));

                        tablelayout.Controls.Add(left);
                        tablelayout.Controls.Add(right);

                        tablelayout.SetCellPosition(left, new TableLayoutPanelCellPosition(0, row));
                        tablelayout.SetCellPosition(right, new TableLayoutPanelCellPosition(1, row));
                        ++row;
                    }
                }
                tablelayout.Dock = DockStyle.Fill;
                gbox.Height = tablelayout.Height;
                gbox.Controls.Add(tablelayout);
            }

            foreach (var g in groupBoxs)
            {

                if (ret.MainPanel.Controls.Count == 0)
                {
                    g.Location = new Point(0, 0);
                }
                else
                {
                    Control lastCtrl=ret.MainPanel.Controls[ret.MainPanel.Controls.Count - 1];
                    g.Location = new Point(0, lastCtrl.Bottom);
                }
                g.Width = ret.MainPanel.Width;
                g.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                ret.MainPanel.Controls.Add(g);
            }
            ret.OKClicked += ret_OKClicked;
            return ret;
        }

        void ret_OKClicked(object sender, EventArgs e)
        {
            Dictionary<String, Object> givenValue = new Dictionary<string, object>();
            
            foreach (Control c in DataFieldControl)
            {
                String fullName =(String)c.Tag;
                IniReader.OnSerializeNotificationEventArgs arg = FieldDeserializeMap[fullName];
                Object value = null;

                bool textBased = false;
                if (c is TextBox)
                {
                    value = ((TextBox)c).Text;
                    textBased=true;
                }
                else if(c is ComboBox)
                {
                    value = ((ComboBox)c).Text;
                    textBased=true;
                }
                if(textBased)
                { 
                    if (arg.Field.FieldType == typeof(int))
                    {
                        int ival = default(int);
                        int.TryParse((String)value, out ival);
                        value = ival;
                    }
                    else if (arg.Field.FieldType == typeof(double))
                    {
                        double ival = default(double);
                        double.TryParse((String)value, out ival);
                        value = ival;
                    }
                    else if (arg.Field.FieldType == typeof(bool))
                    {
                        bool ival = default(bool);
                        bool.TryParse((String)value, out ival);
                        value = ival;
                    }
                    else if (arg.Field.FieldType == typeof(Size))
                    {
                        var list= IniReader.IntListFromString((String)value);
                        Size sz = new Size();
                        if (list.Count == 2)
                        {
                            sz.Width = list[0];
                            sz.Height = list[1];
                        }
                        value = sz;
                    }
                    else if (arg.Field.FieldType.IsEnum)
                    {
                        try
                        {
                            String sval = (String)value.ToString();
                            object enumVal = Enum.Parse(arg.Field.FieldType, sval);
                            value = sval;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else if (arg.Field.FieldType == typeof(Point))
                    {
                        var list = IniReader.IntListFromString((String)value);
                        Point sz = new Point();
                        if (list.Count == 2)
                        {
                            sz.X = list[0];
                            sz.Y = list[1];
                        }
                        value = sz;
                    }
                    else if (arg.Field.FieldType == typeof(Rectangle))
                    {
                        var list = IniReader.IntListFromString((String)value);
                        Rectangle sz = new Rectangle();
                        if (list.Count == 4)
                        {
                            sz.X = list[0];
                            sz.Y = list[1];
                            sz.Width = list[2];
                            sz.Height = list[3];
                        }
                        value = sz;
                    }
                    else if (arg.Field.FieldType == typeof(int[]))
                    {
                        var list = IniReader.IntListFromString((String)value);
                        value = list;
                    }
                }
                else
                {
                    value = c.BackColor;
                }
                givenValue[fullName] = value;
            }
            if (fromFile)
            {
                IniWriter writer = IniWriter.Open(this.TargetFileName);
                writer.GivenValue = givenValue;
                writer.Serialize(this.Value);
                writer.Close();
            }
            else
            {
                IniWriter writer = new IniWriter();
                writer.GivenValue = givenValue;
                writer.Serialize(this.Value);
                this.Value = IniReader.DeserializeString<T>(writer.ToString());
            }
        }

        void btn_colorPickerClick(object sender, EventArgs e)
        {
            ColorDialog picker = new ColorDialog();
            Button btn = (Button)sender;
            picker.Color = btn.BackColor;
            if (picker.ShowDialog() == DialogResult.OK)
            {
                btn.BackColor = picker.Color;
            }
        }


        public static T GetAttribute<T>(MemberInfo member, bool isRequired)
    where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The {0} attribute must be defined on member {1}",
                        typeof(T).Name,
                        member.Name));
            }

            return (T)attribute;
        }

        public static string GetPropertyDisplayName(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return "";
            }

            var attr = GetAttribute<DisplayNameAttribute>(memberInfo,false);
            if (attr == null)
            {
                return "";
            }

            return attr.DisplayName;
        }
        public static string GetPropertyCategory(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return "";
            }

            var attr = GetAttribute<CategoryAttribute>(memberInfo, false);
            if (attr == null)
            {
                return "";
            }

            return attr.Category;
        }

        private void HandleDeserializeField(object sender, IniReader.OnSerializeNotificationEventArgs args)
        {
            if (String.IsNullOrEmpty(args.FullName))
            {
                return;
            }
            FieldDeserializeMap[args.FullName] = args;
            String category = GetPropertyCategory(args.Field);
            if (!String.IsNullOrEmpty(args.Section))
            {
                category = args.Section;
            }
            List<String> fullNameList = new List<string>();
            if (!FieldCategoryMap.ContainsKey(category))
            {
                FieldCategoryMap[category] = fullNameList;
            }
            else
            {
                fullNameList = FieldCategoryMap[category];
            }
           
            fullNameList.Add(args.FullName);
        }
        
    }
}

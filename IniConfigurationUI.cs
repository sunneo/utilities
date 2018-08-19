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
        public IniConfigurationUI(String filename)
        {
            this.TargetFileName = filename;
        }
        public Form BuildForm(String title="")
        {
            UI.SaveConfigurationTemplateForm ret = new UI.SaveConfigurationTemplateForm();
            DataFieldControl.Clear();
            
            this.Value = IniReader.Deserialize<T>(this.TargetFileName,HandleDeserializeField);
            List<GroupBox> groupBoxs = new List<GroupBox>();
            if (String.IsNullOrEmpty(title))
            {
                title = this.TargetFileName;
            }
            ret.Text = title;
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
                        if(arg.Field.FieldType != typeof(String) 
                           && arg.Field.FieldType != typeof(Rectangle)
                            && arg.Field.FieldType != typeof(Size)
                            && arg.Field.FieldType != typeof(Point)
                            && arg.Field.FieldType != typeof(Color)
                            && arg.Field.FieldType != typeof(int[]))
                        {
                            continue;
                        }
                    }
                    RowStyle rs = new RowStyle();
                    tablelayout.RowStyles.Add(rs);
                    rs.SizeType = SizeType.Absolute;
                    rs.Height = 30;
                    
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
                    nameLabel.Text = strname;
                    nameLabel.Dock = DockStyle.Fill;
                    nameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    left.Controls.Add(nameLabel);
                    IniReader reader = arg.Reader;
                    Type fieldType = arg.Field.FieldType;
                 
                    if (fieldType == typeof(Color))
                    {
                        Button btn = new Button();
                        btn.Size = new Size(100, (int)(rs.Height));
                        btn.BackColor = (Color)arg.FieldValue;
                        btn.Dock = DockStyle.Fill;
                        btn.Click += btn_colorPickerClick;
                        right.Controls.Add(btn);
                        btn.Tag = arg.FullName;
                        this.DataFieldControl.Add(btn);
                    }
                    else
                    {
                        TextBoxEx tbox = new TextBoxEx();
                        tbox.Size = new Size(100, (int)(rs.Height));
                        tbox.Dock = DockStyle.Fill;
                        right.Controls.Add(tbox);
                        tbox.Tag = arg.FullName;
                        tbox.Text = (String)arg.FieldValue.ToString();
                        this.DataFieldControl.Add(tbox);
                    }
                    tablelayout.Height += ((int)(rs.Height));
                    tablelayout.Controls.Add(left);
                    tablelayout.Controls.Add(right);
                    
                    tablelayout.SetCellPosition(left, new TableLayoutPanelCellPosition(0, row));
                    tablelayout.SetCellPosition(right, new TableLayoutPanelCellPosition(1, row));
                    ++row;
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
            IniWriter writer = IniWriter.Open(this.TargetFileName);
            foreach (Control c in DataFieldControl)
            {
                String fullName =(String)c.Tag;
                IniReader.OnSerializeNotificationEventArgs arg = FieldDeserializeMap[fullName];
                Object value = null;

                if (c is TextBox)
                {
                    value = ((TextBox)c).Text;
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
           
            writer.GivenValue = givenValue;
            writer.Serialize(this.Value);
            writer.Close();
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

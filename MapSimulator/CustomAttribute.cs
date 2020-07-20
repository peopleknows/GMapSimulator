using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace InfoEdit
{ 
    /// <summary>
    /// 自定义属性类
    /// </summary>
    public class CustomAttribute
    {
        /// <summary>
        /// 用于存放属性英文-中文的键值对
        /// </summary>
        public static Dictionary<string, string> dic = new Dictionary<string, string>();
        
        //CustomAttribute1 _ca1;
        
        ///
        private GMap.NET.WindowsForms.Markers.GMarkerGoogleType _LineStyle= GMap.NET.WindowsForms.Markers.GMarkerGoogleType.none;
        private string _LineName = "Unknown";
        private int _PointCount = 0;
        private bool _IsVisible = false;
        private Color _LineColor = Color.Blue;
        private bool _IsEditable = false;


        public CustomAttribute()
        {
            dic.Add("LineStyle", "线路标注样式");
            dic.Add("PointCount", "线路点个数");
            dic.Add("LineName", "线路名称");
            dic.Add("IsVisible", "是否隐藏");
            dic.Add("LineColor", "线路颜色");
            dic.Add("IsEditable", "是否可移动");
        }
        public CustomAttribute(string LineName,int PointCount,bool IsVisible,GMap.NET.WindowsForms.Markers.GMarkerGoogleType type,Color lineColor,bool isEditable)
        {
            this.LineName = LineName;
            this.PointCount = PointCount;
            this.IsVisible = IsVisible;
            this.LineStyle = type;
            this.LineColor = lineColor;
            this.IsEditable = isEditable;
        }
        //public CustomAttribute()
        /// <summary>
        /// 线路标注样式
        /// </summary>
        [Browsable(true), Category("线路")]
        public GMap.NET.WindowsForms.Markers.GMarkerGoogleType LineStyle
        {
            get {return _LineStyle; }
            set {_LineStyle=value; }
        }
        
        /// <summary>
        /// 线路名称
        /// </summary>
        [Browsable(true), Category("线路")]
        public string LineName
        {
            get {return _LineName; }
            set {_LineName=value; }
        }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        [Browsable(true), Category("线路")]
        public bool IsVisible
        {
            get { return _IsVisible; }
            set
            {
                _IsVisible = value;
            }
        }
        /// <summary>
        /// 线路点个数
        /// </summary>
        [Browsable(true), Category("线路")]
        public int PointCount
        {
            get {return _PointCount; }
            set {_PointCount=value; }
        }

        [Browsable(true), Category("线路")]
        public Color LineColor
        {
            get { return _LineColor; }
            set { _LineColor = value; }
        }


        [Browsable(true), Category("线路")]
        public bool IsEditable
        {
            get { return _IsEditable; }
            set { _IsEditable = value; }
        }

        ///// <summary>
        ///// 可展开属性
        ///// </summary>
        ///// TypeConverter(typeof(ExpandableObjectConverter)):将CustomAttribute1类型的对象转为可扩展对象
        //[Browsable(true), Category("自定义属性"), TypeConverter(typeof(ExpandableObjectConverter))]
        //public CustomAttribute1 CA1
        //{
        //    get { return _ca1; }
        //    set
        //    {
        //        _ca1 = value;
        //    }
        //}
    }

    ///// <summary>
    ///// 自定义属性类1
    ///// </summary>
    //class CustomAttribute1
    //{
    //    public CustomAttribute1()
    //    {
    //        CustomAttribute.dic.Add("CustomDisplayFormart", "显示类型");
    //        CustomAttribute.dic.Add("CustomFormartString", "类型格式");
    //    }
    //    private CustomAttribute2 _CustomDisplayFormart;
    //    private string _CustomFormartString = "";
    //    /// <summary>
    //    /// 显示类型
    //    /// </summary>
    //    [Browsable(true), TypeConverter(typeof(ExpandableObjectConverter))]
    //    public CustomAttribute2 CustomDisplayFormart
    //    {
    //        get { return _CustomDisplayFormart; }
    //        set
    //        {
    //            _CustomDisplayFormart = value;
    //        }
    //    }
    //    /// <summary>
    //    /// 类型格式
    //    /// </summary>
    //    public string CustomFormartString
    //    {
    //        get { return _CustomFormartString; }
    //        set
    //        {
    //            _CustomFormartString = value;
    //        }
    //    }
    //}
    ///// <summary>
    ///// 自定义属性类2
    ///// </summary>
    //class CustomAttribute2
    //{
    //    public CustomAttribute2()
    //    {
    //        CustomAttribute.dic.Add("CustomDisplayFormartValue", "显示格式值");
    //        CustomAttribute.dic.Add("AllowUseCustomDisplayFormart", "是否启用显示格式");
    //    }
    //    private string _CustomDisplayFormartValue = "";
    //    private CustomAttribute.VisibleStatus _AllowUseCustomDisplayFormart = CustomAttribute.VisibleStatus.False;
    //    /// <summary>
    //    /// 值
    //    /// </summary>
    //    public string CustomDisplayFormartValue
    //    {
    //        get { return _CustomDisplayFormartValue; }
    //        set
    //        {
    //            _CustomDisplayFormartValue = value;
    //        }
    //    }
    //    /// <summary>
    //    /// 是否启用
    //    /// </summary>
    //    public CustomAttribute.VisibleStatus AllowUseCustomDisplayFormart
    //    {
    //        get { return _AllowUseCustomDisplayFormart; }
    //        set
    //        {
    //            _AllowUseCustomDisplayFormart = value;
    //        }
    //    }
    //}
}

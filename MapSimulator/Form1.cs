using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using InfoEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapSimulator
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private GMapOverlay gMapOverlay = new GMapOverlay("Examle");
        private OpenFileDialog oFD = new OpenFileDialog();
        private List<string> FilePaths = new List<string>();
        private DataTable chooseFiles = new DataTable();
        public List<GMapOverlay> gMapOverlays = new List<GMapOverlay>();//经纬度文件的GmapOverlay
        //Custom Marker
        private GMarkerGoogleExt Train = null;
        private List<PointLatLng> points;//All points of currentOverlay;
        //当前图层
        private GMapOverlay currentOverlay;
        //自定义属性
        public List<CustomAttribute> customAttributes = new List<CustomAttribute>();//自定义属性
        public CustomAttribute currentAttribute = null;//当前属性
        public Form1()
        {
            InitializeComponent();
            //Initialize the filemanager's datasource and PropertyGridControl
            InitialBindingDataTable();
            InitialSetPropCtrl();
        }
        private void gmap_Load(object sender, EventArgs e)
        {
            //加载谷歌中国地图 Load GooglChinaMap
            GMapProvider.TimeoutMs = 0;
            gmap.MapProvider = GMapProviders.GoogleChinaMap;
            gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance;

            //加载谷歌卫星地图 Load Google SatelliteMap
            //gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            gmap.DragButton = System.Windows.Forms.MouseButtons.Left;
            gmap.Zoom = 12;
            gmap.ShowCenter = false;
            gmap.MaxZoom = 24;
            gmap.MinZoom = 2;
            this.gmap.Position = new PointLatLng(39.923518, 116.539009);
            this.gmap.IsAccessible = false;
            GMapProvider.TimeoutMs = 1000;

            ////Example
            PointLatLng point = new PointLatLng(39.93, 116.53);
            Bitmap b = global::MapSimulator.Properties.Resources.car1;
            Train = new GMarkerGoogleExt(point, b);

            ////设置Train的ToolTip:  You can set Train's ToolTip with a string variable。
            
            //Train.ToolTipText = "Car1";
            //Train.ToolTip.Foreground = Brushes.Black;
            //Train.ToolTip.TextPadding = new Size(20, 10);
            //Train.ToolTipMode = MarkerTooltipMode.OnMouseOver;

            gMapOverlay.Markers.Add(Train);
            this.gMapOverlays.Add(gMapOverlay);
            //The gMapOverlay must be added to the GMapControl;
            this.gmap.Overlays.Add(gMapOverlay);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void btnOpenFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetOFD(true, "文本文件(*.txt)|*.txt");
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] filenames = oFD.FileNames;
                    foreach (string s in filenames)
                    {
                        if (!IsHaveFile(s))
                        {
                            List<string[]> latlngs = new List<string[]>();
                            ReadFormatTxt(s, ref latlngs);//读格式的文件返回经纬度
                            //之后想添加约简算法
                            ShowFileLatLng(latlngs, s, true);//在地图控件上显示文件
                        }
                    }
                    OFDSaveLastFilePath(filenames[filenames.Count() - 1]);//根据最后一个文件设置下一次打开的路径
                    var overlay = gMapOverlays.Last();
                    SetPerfectPos(false, false, overlay);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("地图文件格式不正确！");
                }
            }
        }

        private Bitmap GetBitmapFromString(string name)
        {
            Bitmap b= global::MapSimulator.Properties.Resources.train1;
            switch (name)
            {
                case "train1":
                    b = global::MapSimulator.Properties.Resources.train1;
                    break;
                case "train2":
                    b = global::MapSimulator.Properties.Resources.train2;
                    break;
                case "train3_1":
                    b = global::MapSimulator.Properties.Resources.train3_1;
                    break;
                case "train3_2":
                    b = global::MapSimulator.Properties.Resources.train3_2;
                    break;
                case "train4":
                    b = global::MapSimulator.Properties.Resources.train4;
                    break;
                case "train5":
                    b = global::MapSimulator.Properties.Resources.train5;
                    break;
                case "car1":
                    b = global::MapSimulator.Properties.Resources.car1;
                    break;
                case "car2":
                    b = global::MapSimulator.Properties.Resources.car2;
                    break;
                case "airplane1":
                    b = global::MapSimulator.Properties.Resources.airplane1;
                    break;
                default:
                    break;
            }
            return b;
        }


        #region Initialize the PropertyGridControl 设置属性框
        /// <summary>
        /// 初始设置PropertyGridControl
        /// </summary>
        void InitialSetPropCtrl()
        {
            propertyGridControl1.ExpandAllRows();
            propertyGridControl1.OptionsBehavior.PropertySort = DevExpress.XtraVerticalGrid.PropertySort.NoSort;

            CustomAttribute ca1 = new CustomAttribute();//初始的自定义属性
            propertyGridControl1.SelectedObject = ca1;
            //
            DevExpress.XtraVerticalGrid.Rows.BaseRow br = propertyGridControl1.GetRowByCaption("线路");
            //通过循环遍历设置属性的中文名称 
            foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per in br.ChildRows)
            {
                if (per.ChildRows.Count > 0)
                {                    //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per);
                }
                string dicKey = per.Properties.FieldName;
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per.Properties.Caption = CustomAttribute.dic[dicKey];
                per.Height = 23;//设置属性行高度                          
            }
        }
        

        #region Add Custom Attributes.添加或者设置PropCtrl中的内容

        /// <summary>
        /// 添加到全局变量的CustomAttributes
        /// </summary>
        /// <param name="overlay">图层</param>
        /// <param name="IsSetPropCtrl">是否添加到属性框里</param>
        /// <param name="isVisible">是否隐藏这一栏</param>
        private void AddCustomAttribute(GMapOverlay overlay, bool IsSetPropCtrl, bool isVisible, System.Drawing.Color color, bool isEditable)
        {
            CustomAttribute attribute = new CustomAttribute(overlay.Id, overlay.Markers.Count, isVisible, ((GMarkerGoogle)overlay.Markers[0]).Type, color, isEditable);
            customAttributes.Add(attribute);
            //if (!isEditable)//不可编辑即锁定状态
            //{ chooseLocked.Add(overlay.Id); }
            currentAttribute = attribute;
            if (IsSetPropCtrl)
            {
                SetCurrentCustomAttribute(overlay.Id);
            }
        }

        /// <summary>
        /// 设置当前的自定义属性
        /// </summary>
        /// <param name="overlayname">当前图层的名称</param>
        private void SetCurrentCustomAttribute(string overlayname)
        {
            var currentCustom = customAttributes.Find(a => a.LineName == overlayname);
            this.propertyGridControl1.SelectedObject = currentCustom;
            currentAttribute = currentCustom;
            //
            DevExpress.XtraVerticalGrid.Rows.BaseRow br = propertyGridControl1.GetRowByCaption("线路");
            //通过循环遍历设置属性的中文名称       
            //foreach (DevExpress.XtraVerticalGrid.Rows.PGridEditorRow per in br.ChildRows)
            foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per in br.ChildRows)
            {
                if (per.ChildRows.Count > 0)
                {   //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per);
                }
                string dicKey = per.Properties.FieldName;
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per.Properties.Caption = CustomAttribute.dic[dicKey];
                per.Height = 23;//设置属性行高度                          
            }
        }


        /// <summary>        
        /// 设置自定义属性的描述        
        /// </summary>        
        private void SetCustomAttributeCaption(DevExpress.XtraVerticalGrid.Rows.EditorRow EditorRow)
        {
            foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per_child in EditorRow.ChildRows)
            {
                if (per_child.ChildRows.Count > 0)
                {
                    //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per_child);
                }
                //FieldName属性包含了该属性的父属性FieldName;通过 . 分割                
                string[] per_child_FieldName = per_child.Properties.FieldName.Split('.');
                string dicKey = per_child_FieldName[per_child_FieldName.GetLength(0) - 1];
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per_child.Properties.Caption = CustomAttribute.dic[dicKey];
                per_child.Height = 23;//设置属性行高度           
            }
        }
        #endregion
        #endregion

        #region Set the file manager.设置文件管理框

        /// <summary>
        /// 文件管理框的初始化.Intialize the filemanager's data source.
        /// </summary>
        void InitialBindingDataTable()
        {
            chooseFiles.Columns.Add("IsChoose", typeof(bool)).SetOrdinal(0);
            chooseFiles.Columns.Add("IsOverlayVisible", typeof(bool)).SetOrdinal(1);
            chooseFiles.Columns.Add("IsLine", typeof(bool)).SetOrdinal(2);
            chooseFiles.Columns.Add("IsLocked", typeof(bool)).SetOrdinal(3);
            chooseFiles.Columns.Add("Name");
            this.gridControl1.DataSource = chooseFiles;
        }

        #region  Add a datarow to the file manager while you open a new file.地图文件框添加文件
        /// <summary>
        /// 在地图文件添加新栏
        /// </summary>
        /// <param name="filename">文件名称</param>
        private void AddFileRow(string filename)
        {
            DataRow dr = chooseFiles.NewRow();
            dr["IsChoose"] = false;//选择状态
            dr["IsOverlayVisible"] = true;//显示
            dr["IsLine"] = true;//连线状态
            dr["IsLocked"] = true;//锁定状态即不可编辑
            dr["Name"] = filename.Substring(filename.LastIndexOf("\\") + 1);
            chooseFiles.Rows.Add(dr);
        }
        /// <summary>
        /// 在文件管理栏添加新栏
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <param name="isChoose">图层是否选择</param>
        /// <param name="isOverlayVisible">图层是否显示</param>
        /// <param name="isLine">是否连线</param>
        /// <param name="isLocked">是否锁定</param>
        private void AddFileRow(string filename, bool isChoose, bool isOverlayVisible, bool isLine, bool isLocked)
        {
            DataRow dr = chooseFiles.NewRow();
            dr["IsChoose"] = isChoose;//选择状态
            dr["IsOverlayVisible"] = isOverlayVisible;//显示
            dr["IsLine"] = isLine;//连线状态
            dr["IsLocked"] = isLocked;//锁定状态即不可编辑
            dr["Name"] = filename.Substring(filename.LastIndexOf("\\") + 1);
            chooseFiles.Rows.Add(dr);
        }
        #endregion

        #endregion

        #region GPS纠偏算法--GPS2GCJ02
        /**
        * gps纠偏算法，适用于google,高德体系的地图
        */
        static double pi = Math.PI;
        //public static double pi = 3.1415926535897932384626;
        public static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        public static double a = 6378245.0;
        public static double ee = 0.00669342162296594323;
        public static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }
        public static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0
                    * pi)) * 2.0 / 3.0;
            return ret;
        }
        /// <summary>
        /// 纠偏GCJ02之后
        /// </summary>
        /// <param name="lat">WGS84下纬度</param>
        /// <param name="lon">WGS84下经度</param>
        /// <returns>double[0]=lat,double[1]=lng</returns>
        public static double[] transform(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { mgLat, mgLon };
        }
        public static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }
        /** 
         * 84 to 火星坐标系 (GCJ-02) World Geodetic System ==> Mars Geodetic System 
         * 
         * @param lat 
         * @param lon 
         * @return 
         */
        public static double[] gps84_To_Gcj02(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { mgLat, mgLon };
        }
        /** 
         * * 火星坐标系 (GCJ-02) to 84 * * @param lon * @param lat * @return 
         * */
        public static double[] gcj02_To_Gps84(double lat, double lon)
        {
            double[] gps = transform(lat, lon);
            double lontitude = lon * 2 - gps[1];
            double latitude = lat * 2 - gps[0];
            return new double[] { latitude, lontitude };
        }
        /** 
         * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 将 GCJ-02 坐标转换成 BD-09 坐标 
         * 
         * @param lat 
         * @param lon 
         */
        public static double[] gcj02_To_Bd09(double lat, double lon)
        {
            double x = lon, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta) + 0.0065;
            double tempLat = z * Math.Sin(theta) + 0.006;
            double[] gps = { tempLat, tempLon };
            return gps;
        }
        /** 
         * * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 * * 将 BD-09 坐标转换成GCJ-02 坐标 * * @param 
         * bd_lat * @param bd_lon * @return 
         */
        public static double[] bd09_To_Gcj02(double lat, double lon)
        {
            double x = lon - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta);
            double tempLat = z * Math.Sin(theta);
            double[] gps = { tempLat, tempLon };
            return gps;
        }
        /**将gps84转为bd09 
         * @param lat 
         * @param lon 
         * @return 
         */
        public static double[] gps84_To_bd09(double lat, double lon)
        {
            double[] gcj02 = gps84_To_Gcj02(lat, lon);
            double[] bd09 = gcj02_To_Bd09(gcj02[0], gcj02[1]);
            return bd09;
        }
        public static double[] bd09_To_gps84(double lat, double lon)
        {
            double[] gcj02 = bd09_To_Gcj02(lat, lon);
            double[] gps84 = gcj02_To_Gps84(gcj02[0], gcj02[1]);
            //保留小数点后六位  
            gps84[0] = retain6(gps84[0]);
            gps84[1] = retain6(gps84[1]);
            return gps84;
        }
        /**保留小数点后六位 
         * @param num 
         * @return 
         */
        private static double retain6(double num)
        {
            string result = String.Format("%.6f", num);
            return Convert.ToDouble(result);
        }
        #endregion
        
        #region Close the Checked files and Delete these files from FilePaths,Navigate to the current overlay.定位至当前图层、关闭且删除当前文件
        private void btnCloseCurrentFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<string> files = GetSelectFiles();
            DoDelete(files);
            this.gmap.Refresh();
        }
        /// <summary>
        /// 关闭且删除当前文件(从FilePaths中和图层).
        /// </summary>
        public void DoDelete(List<string> selectedFiles)
        {
            if (selectedFiles != null && selectedFiles.Count != 0)
            {
                foreach (string s in selectedFiles)
                {
                    var obj = FilePaths.Find(a => a.Contains(s));
                    FilePaths.Remove(obj);//从保存文件路径的全局变量中删除文件
                    
                    var map = gMapOverlays.FindAll(a => a.Id == s);
                    var attribute = customAttributes.Find(a => a.LineName == s.Trim());

                    customAttributes.Remove(attribute);//丛自定义属性中删除该文件的属性
                    foreach (GMapOverlay o in map)
                    {
                        o.Dispose();
                        this.gmap.Overlays.Remove(o);
                        gMapOverlays.Remove(o);
                    }
                }
            }
            this.gridView1.DeleteSelectedRows();
            gridView1.RefreshData();
            this.gridView1.OptionsBehavior.Editable = true;
        }

        /// <summary>
        /// 获取地图文件选择列的文件
        /// </summary>
        /// <returns></returns>
        private List<string> GetSelectFiles()
        {
            List<string> selectedFiles = new List<string>();
            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                var value = gridView1.GetDataRow(i)["IsChoose"].ToString().Trim();
                if (value == "True")//选择
                {
                    gridView1.SelectRow(i);
                    selectedFiles.Add(gridView1.GetDataRow(i)["Name"].ToString().Trim());
                }
                else if (value == "False")
                {
                    gridView1.UnselectRow(i);
                    continue;
                }
            }
            return selectedFiles;
        }

        /// <summary>
        /// 点击GridControl行定位至当前文件图层同时设置当前图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string st = "";
                DevExpress.XtraGrid.Views.Base.ColumnView cv = (DevExpress.XtraGrid.Views.Base.ColumnView)gridControl1.FocusedView;//重新获取此ID 否则无法从表头连删获取不到id
                int focusedhandle = cv.FocusedRowHandle;
                object rowIdObj = gridView1.GetRowCellValue(focusedhandle, "Name");
                st = rowIdObj.ToString().Trim();
                //var currentmap = gMapOverlays.Find(a => a.Id == st);
                var currentmap = this.gmap.Overlays.First(a => a.Id == st);
                SetPerfectPos(true, true, currentmap);
                currentOverlay = currentmap;
                txtCurrentOverlay.Caption = string.Format("当前图层{0}", currentmap.Id);
                SetCurrentCustomAttribute(st);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            if (gridView1.RowCount == 0)
            {
                this.gmap.Overlays.Clear();
            }
        }

        #endregion

        #region  Click the checkbox to change the values.点击复选框改变数据表值
        //选择
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView1.IsNewItemRow(gridView1.FocusedRowHandle))
            {
                gridView1.CloseEditor();
                gridView1.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            if (check == CheckState.Checked)
            {
                //事件
            }
        }
        //连线
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView1.IsNewItemRow(gridView1.FocusedRowHandle))
            {
                gridView1.CloseEditor();
                gridView1.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            int index = this.gridView1.FocusedRowHandle;
            string name = this.gridView1.GetRowCellDisplayText(index, "Name");
            if (check == CheckState.Checked)
            {
                //事件
                SetIsLine(name, true);
            }
            else
            {
                SetIsLine(name, false);
            }
        }
        //锁定
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView1.IsNewItemRow(gridView1.FocusedRowHandle))
            {
                gridView1.CloseEditor();
                gridView1.UpdateCurrentRow();
            }
            ///
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            int index = this.gridView1.FocusedRowHandle;
            string name = this.gridView1.GetRowCellDisplayText(index, "Name");
            //
            var overlay = gMapOverlays.Find(a => a.Id == name.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            //if (check == CheckState.Checked)
            //{
            //    //事件
            //    if (chooseLocked.Contains(name))
            //    {
            //        attribute.IsEditable = false;
            //    }
            //    else
            //    {
            //        chooseLocked.Add(name);
            //    }
            //}
            //else
            //{
            //    if (chooseLocked.Contains(name))
            //    {
            //        chooseLocked.Remove(name);
            //        attribute.IsEditable = true;
            //    }
            //}
            SetCurrentCustomAttribute(overlay.Id);
            this.propertyGridControl1.Refresh();
        }
        //显示
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView1.IsNewItemRow(gridView1.FocusedRowHandle))
            {
                gridView1.CloseEditor();
                gridView1.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            int index = this.gridView1.FocusedRowHandle;
            string name = this.gridView1.GetRowCellDisplayText(index, "Name");
            //
            var overlay = gMapOverlays.Find(a => a.Id == name.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            if (check == CheckState.Checked)
            {
                //事件
                overlay.IsVisibile = true;
                attribute.IsVisible = false;
            }
            else
            {
                overlay.IsVisibile = false;
                attribute.IsVisible = true;
            }
            SetCurrentCustomAttribute(overlay.Id);
            this.propertyGridControl1.Refresh();
        }
        
        public void SetCheckStateValue(DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            string val = "";
            if (e.Value != null)
            {
                val = e.Value.ToString();
            }
            else
            {
                val = "True";//默认为选中 
            }
            switch (val)
            {
                case "True":
                    e.CheckState = CheckState.Checked;
                    break;
                case "False":
                    e.CheckState = CheckState.Unchecked;
                    break;
                case "Yes":
                    goto case "True";
                case "No":
                    goto case "False";
                case "1":
                    goto case "True";
                case "0":
                    goto case "False";
                default:
                    e.CheckState = CheckState.Checked;
                    break;
            }
            e.Handled = true;
        }
        private void repositoryItemCheckEdit1_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            SetCheckStateValue(e);
        }
        private void repositoryItemCheckEdit2_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            SetCheckStateValue(e);
        }
        private void repositoryItemCheckEdit3_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            SetCheckStateValue(e);
        }
        private void repositoryItemCheckEdit4_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            SetCheckStateValue(e);
        }
        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            try
            {
                if (gMapOverlays.Count != 0)
                {
                    string key = this.gridView1.GetRowCellValue(e.FocusedRowHandle, "Name").ToString();
                    var Overlay = gMapOverlays.Find(a => a.Id == key.Trim());
                    SetPerfectPos(true, false, Overlay);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        private void gridView1_Click(object sender, EventArgs e)
        {
            try
            {
                Point pt = gridControl1.PointToClient(Control.MousePosition);
                DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo info = gridView1.CalcHitInfo(pt);
                if (info.InColumn && info.Column != null)
                {
                    string s = info.Column.FieldName.ToString();
                    switch (s)
                    {
                        case "IsChoose":
                            SetAllCheck("IsChoose", GetIsAllCheck("IsChoose"));
                            break;
                        case "IsOverlayVisible":
                            SetAllCheck("IsOverlayVisible", GetIsAllCheck("IsOverlayVisible"));
                            SetAllIsVisible();//全选/全不选的显示操作
                            break;
                        case "IsLine":
                            SetAllCheck("IsLine", GetIsAllCheck("IsLine"));
                            SetAllIsLine();//全选/全不选的连线操作
                            break;
                        case "IsLocked":
                            SetAllCheck("IsLocked", GetIsAllCheck("IsLocked"));

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置gridView1某列全选
        /// </summary>
        /// <param name="columnFieldName">列的FieldName</param>
        /// <param name="checkState">选中状态</param>
        private void SetAllCheck(string columFieldName, bool checkState)
        {
            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                gridView1.SetRowCellValue(i, gridView1.Columns[columFieldName], checkState);
            }
            gridControl1.Refresh();
            gridView1.RefreshData();
        }
      
        /// <summary>
        /// gridView1判断某列是否全选
        /// </summary>
        private bool GetIsAllCheck(string columnFieldName)
        {
            DataTable dt = (DataTable)this.gridControl1.DataSource;
            //如果不是全选，则返回全选，是全选返回未选
            List<bool> states = new List<bool>();
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                var state = dt.Rows[j][columnFieldName];
                states.Add(Convert.ToBoolean(state));
            }
            if (states.TrueForAll(a => a))//判断是否全为false(全未选)-true;不是全选(即全为false)的情况下就返回false
            {
                //全为true返回false
                return false;
            }
            else
            {
                //否则返回true
                return true;
            }
        }
        private void SetAllIsVisible()
        {
            bool state = Convert.ToBoolean(this.gridView1.GetRowCellValue(0, "IsOverlayVisible"));
            foreach (GMapOverlay overlay in gMapOverlays)
            {
                SetIsVisible(overlay.Id, state);
            }
        }
        private void SetIsVisible(string overlayname, bool state)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            if (state)
            {
                overlay.IsVisibile = true;
                attribute.IsVisible = false;
            }
            else
            {
                overlay.IsVisibile = false;
                attribute.IsVisible = true;
            }
        }
        private void SetAllIsLine()
        {
            bool state = Convert.ToBoolean(this.gridView1.GetRowCellValue(0, "IsLine"));
            foreach (GMapOverlay overlay in gMapOverlays)
            {
                SetIsLine(overlay.Id, state);
            }
        }
        private void SetIsLine(string overlayname, bool state)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname.Trim());
            if (state)
            {
                //事件
                var color = customAttributes.Find(a => a.LineName == overlay.Id).LineColor;
                List<PointLatLng> points = new List<PointLatLng>();
                foreach (GMarkerGoogleExt m in overlay.Markers)
                {
                    points.Add(m.Position);
                }
                GMapRouteExt route = new GMapRouteExt(points, overlay.Id);
                route.Stroke = new Pen(color, 3);
                overlay.Routes.Add(route);
                gmap.Refresh();
            }
            else
            {
                overlay.Routes.Clear();
            }
        }
        private void SetAllIsLocked()
        {
            bool state = Convert.ToBoolean(this.gridView1.GetRowCellValue(0, "IsLocked"));
            foreach (GMapOverlay overlay in gMapOverlays)
            {
                SetIsLocked(overlay.Id, state);
            }
        }
        private void SetIsLocked(string overlayname, bool state)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            if (state)
            {
                overlay.IsVisibile = true;
                attribute.IsVisible = false;
            }
            else
            {
                overlay.IsVisibile = false;
                attribute.IsVisible = true;
            }
        }
        private int GetRowFromName(string overlayname)
        {
            int index = 0;
            DataTable dt = (DataTable)this.gridControl1.DataSource;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["Name"].ToString() == overlayname)
                {
                    index = dt.Rows.IndexOf(dr);
                    break;
                }
            }
            return index;
        }

        #endregion

        #region PropertyGridControl value changed。属性框事件
        
        //属性框cell的值改变
        private void propertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            try
            {
                var rowFieldName = e.Row.Name.ToString();
                switch (rowFieldName)
                {
                    case "LineStyle":
                        ChangeLineStyle((GMarkerGoogleType)e.Value);
                        break;
                    case "IsVisible":
                        currentAttribute.IsVisible = (bool)e.Value;
                        break;
                    case "LineColor":
                        ChangeLineMarkerColor((System.Drawing.Color)e.Value);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Change the linestyle. 更改线路标注或者颜色

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void ChangeLineStyle(GMarkerGoogleType type)
        {
            var overlay = gMapOverlays.Find(a => a.Id == currentAttribute.LineName);
            currentOverlay = overlay;
            //改变customAttribute的属性
            var customattribute = customAttributes.Find(a => a.LineName == currentAttribute.LineName);
            customattribute.LineStyle = type;
            if (overlay.Id.Contains(".xml"))//
            {
                List<GMarkerGoogleExt> points = new List<GMarkerGoogleExt>();
                foreach (GMapMarker marker in overlay.Markers)
                {
                    GMarkerGoogleExt newmarker = UpdateMarker(marker, type);
                    points.Add(newmarker);
                }
                overlay.Markers.Clear();
                foreach (GMarkerGoogleExt a in points)
                {
                    overlay.Markers.Add(a);
                }
                this.gmap.Refresh();
            }
            else
            {
                GMarkerGoogleExt pfirst = UpdateMarker(overlay.Markers[0], type);
                GMarkerGoogleExt plast = UpdateMarker(overlay.Markers.Last(), type);
                overlay.Markers.RemoveAt(0);
                overlay.Markers.Insert(0, pfirst);
                overlay.Markers.RemoveAt(overlay.Markers.IndexOf(overlay.Markers.Last()));
                overlay.Markers.Add(plast);
                this.gmap.Refresh();
            }
        }

        /// <summary>
        /// 改变线路文件的起始点和终点的Marker样式
        /// </summary>
        /// <param name="c">颜色</param>
        private void ChangeLineMarkerColor(System.Drawing.Color c)
        {
            var overlay = gMapOverlays.Find(a => a.Id == currentAttribute.LineName);
            var customattribute = customAttributes.Find(a => a.LineName == currentAttribute.LineName);
            customattribute.LineColor = c;
            if (!(overlay.Id.Contains(".xml")))//不是xml文件
            {
                for (int i = 1; i < overlay.Markers.Count - 1; i++)
                {
                    GMarkerGoogleExt newmarker = UpdateMarker(overlay.Markers[i], c);
                    overlay.Markers.RemoveAt(i);
                    overlay.Markers.Insert(i, newmarker);
                }
                foreach (GMapRouteExt route in overlay.Routes)
                {
                    route.Stroke = new Pen(c, 3);
                }
                this.gmap.Refresh();
            }
            else
            {
                foreach (GMapRouteExt route in overlay.Routes)
                {
                    route.Stroke = new Pen(c, 3);
                }
                this.gmap.Refresh();
            }
        }


        /// <summary>
        /// 更新Marker的标注类型
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMapMarker marker, GMarkerGoogleType type)
        {
            GMarkerGoogleExt newmarker = new GMarkerGoogleExt(marker.Position, type);
            newmarker.ToolTipText = marker.ToolTipText;
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return newmarker;
        }

        /// <summary>
        /// 更新Marker的颜色
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMapMarker marker, System.Drawing.Color c)
        {
            GMarkerGoogleExt newmarker = new GMarkerGoogleExt(marker.Position, ((GMarkerGoogle)marker).Type, c, 5, true);
            newmarker.ToolTipText = marker.ToolTipText;
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return newmarker;
        }

        /// <summary>
        /// 更新Marker的位置和ToolTipText
        /// </summary>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMarkerGoogleExt marker, int index, PointLatLng point)
        {
            GMarkerGoogleExt newmarker = marker;
            newmarker.Position = point;
            newmarker.ToolTipText = string.Format("点编号{0}:经度{1},纬度{2}", index, point.Lng, point.Lat);
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return marker;
        }
        #endregion

        #region  Rotate Bitmap.旋转图像 
        private static Bitmap RotateImage(Image image, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            const double pi2 = Math.PI / 2.0;
            // Why can't C# allow these to be const, or at least readonly
            // *sigh*  I'm starting to talk like Christian Graus :omg:
            double oldWidth = (double)image.Width;
            double oldHeight = (double)image.Height;
            // Convert degrees to radians
            double theta = ((double)angle) * Math.PI / 180.0;
            double locked_theta = theta;
            // Ensure theta is now [0, 2pi)
            while (locked_theta < 0.0)
                locked_theta += 2 * Math.PI;
            double newWidth, newHeight;
            int nWidth, nHeight; // The newWidth/newHeight expressed as ints
            #region Explaination of the calculations
            /*
			 * The trig involved in calculating the new width and height
			 * is fairly simple; the hard part was remembering that when 
			 * PI/2 <= theta <= PI and 3PI/2 <= theta < 2PI the width and 
			 * height are switched.
			 * 
			 * When you rotate a rectangle, r, the bounding box surrounding r
			 * contains for right-triangles of empty space.  Each of the 
			 * triangles hypotenuse's are a known length, either the width or
			 * the height of r.  Because we know the length of the hypotenuse
			 * and we have a known angle of rotation, we can use the trig
			 * function identities to find the length of the other two sides.
			 * 
			 * sine = opposite/hypotenuse
			 * cosine = adjacent/hypotenuse
			 * 
			 * solving for the unknown we get
			 * 
			 * opposite = sine * hypotenuse
			 * adjacent = cosine * hypotenuse
			 * 
			 * Another interesting point about these triangles is that there
			 * are only two different triangles. The proof for which is easy
			 * to see, but its been too long since I've written a proof that
			 * I can't explain it well enough to want to publish it.  
			 * 
			 * Just trust me when I say the triangles formed by the lengths 
			 * width are always the same (for a given theta) and the same 
			 * goes for the height of r.
			 * 
			 * Rather than associate the opposite/adjacent sides with the
			 * width and height of the original bitmap, I'll associate them
			 * based on their position.
			 * 
			 * adjacent/oppositeTop will refer to the triangles making up the 
			 * upper right and lower left corners
			 * 
			 * adjacent/oppositeBottom will refer to the triangles making up 
			 * the upper left and lower right corners
			 * 
			 * The names are based on the right side corners, because thats 
			 * where I did my work on paper (the right side).
			 * 
			 * Now if you draw this out, you will see that the width of the 
			 * bounding box is calculated by adding together adjacentTop and 
			 * oppositeBottom while the height is calculate by adding 
			 * together adjacentBottom and oppositeTop.
			 */
            #endregion
            double adjacentTop, oppositeTop;
            double adjacentBottom, oppositeBottom;
            // We need to calculate the sides of the triangles based
            // on how much rotation is being done to the bitmap.
            //   Refer to the first paragraph in the explaination above for 
            //   reasons why.
            if ((locked_theta >= 0.0 && locked_theta < pi2) ||
                (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2)))
            {
                adjacentTop = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
                oppositeTop = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                adjacentBottom = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                oppositeBottom = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
            }
            else
            {
                adjacentTop = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
                oppositeTop = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                adjacentBottom = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                oppositeBottom = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
            }
            newWidth = adjacentTop + oppositeBottom;
            newHeight = adjacentBottom + oppositeTop;
            nWidth = (int)Math.Ceiling(newWidth);
            nHeight = (int)Math.Ceiling(newHeight);
            Bitmap rotatedBmp = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // This array will be used to pass in the three points that 
                // make up the rotated image
                Point[] points;
                /*
                 * The values of opposite/adjacentTop/Bottom are referring to 
                 * fixed locations instead of in relation to the
                 * rotating image so I need to change which values are used
                 * based on the how much the image is rotating.
                 * 
                 * For each point, one of the coordinates will always be 0, 
                 * nWidth, or nHeight.  This because the Bitmap we are drawing on
                 * is the bounding box for the rotated bitmap.  If both of the 
                 * corrdinates for any of the given points wasn't in the set above
                 * then the bitmap we are drawing on WOULDN'T be the bounding box
                 * as required.
                 */
                if (locked_theta >= 0.0 && locked_theta < pi2)
                {
                    points = new Point[] {
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( 0, (int) adjacentBottom )
                                         };
                }
                else if (locked_theta >= pi2 && locked_theta < Math.PI)
                {
                    points = new Point[] {
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( (int) oppositeBottom, 0 )
                                         };
                }
                else if (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2))
                {
                    points = new Point[] {
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( nWidth, (int) oppositeTop )
                                         };
                }
                else
                {
                    points = new Point[] {
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( (int) adjacentTop, nHeight )
                                         };
                }
                g.DrawImage(image, points);
            }
            return rotatedBmp;
        }
        

        private Bitmap RotateImage(Bitmap bmp, double angle)
        {
            Graphics g = null;
            Bitmap tmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            tmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            g = Graphics.FromImage(tmp);
            try
            {
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
                g.RotateTransform((float)angle);
                g.DrawImage(bmp, 0, 0);
            }
            finally
            {
                g.Dispose();
            }
            return tmp;
        }
        public static Bitmap KiRotate(Bitmap bmp, float angle, Color bkColor)
        {
            int w = bmp.Width + 2;
            int h = bmp.Height + 2;

            PixelFormat pf;

            if (bkColor == Color.Transparent)
            {
                pf = PixelFormat.Format32bppArgb;
            }
            else
            {
                pf = bmp.PixelFormat;
            }

            Bitmap tmp = new Bitmap(w, h, pf);
            Graphics g = Graphics.FromImage(tmp);
            g.Clear(bkColor);
            g.DrawImageUnscaled(bmp, 1, 1);
            g.Dispose();

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(0f, 0f, w, h));
            Matrix mtrx = new Matrix();
            mtrx.Rotate(angle);
            RectangleF rct = path.GetBounds(mtrx);

            Bitmap dst = new Bitmap((int)rct.Width, (int)rct.Height, pf);
            g = Graphics.FromImage(dst);
            g.Clear(bkColor);
            g.TranslateTransform(-rct.X, -rct.Y);
            g.RotateTransform(angle);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(tmp, 0, 0);
            g.Dispose();

            tmp.Dispose();

            return dst;
        }
        #endregion

        #region Some Methods。一些计算角度的方法

        private double GetBear(double LatA, double LatB, double LngA, double LngB)
        {

            double bear = 0.0;
            //
            double E = LngB - LngA;
            double N = LatB - LatA;

            bear = Math.Atan2(E, N);//得到弧度-3.14~3.14
            return bear;
        }
        private double GetBear2(double LatA, double LatB, double LngA, double LngB)
        {

            double bear = 0.0;
            //
            double E = LngB - LngA;
            double N = LatB - LatA;

            bear = Math.Atan2(E, N);//得到弧度-3.14~3.14
            if (bear < 0)
            {
                bear += 6.28;//弧度范围0~6.28
            }

            //改成角度
            bear = bear * 180 / Math.PI;
            if (bear < 0)
            {
                bear += 360;//
            }
            return bear;

        }
        /// <summary>
        /// Get distance between two points
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = lat1 * Math.PI / 180;
            double radLat2 = lat2 * Math.PI / 180;
            double a = radLat1 - radLat2;
            double b = lng1 * Math.PI / 180 - lng2 * Math.PI / 180;
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1)
                    * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * 6378137.0;// 取WGS84标准参考椭球中的地球长半径(单位:m)
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        /// <summary>
        /// Get Heading between two points
        /// </summary>
        /// <param name="Alat"></param>
        /// <param name="Alon"></param>
        /// <param name="Blat"></param>
        /// <param name="Blon"></param>
        /// <returns></returns>
        private double GetAngle(double Alat, double Alon, double Blat, double Blon)
        {
            double length = GetDistance(Alat, Alon, Blat, Blon);
            double hudu = Math.Asin(Math.Abs(Alon - Blon) / length);
            double bear = hudu * 180 / Math.PI;
            // = GetBear(Alat, Blat, Alon, Blon);
            if ((Blat - Alat) <= 0 && (Blon - Alon) >= 0)
            {
                bear = 90 - bear;
            }
            else
            {
                if ((Blat - Alat) <= 0 && (Blon - Alon) <= 0)
                {
                    bear = bear + 90;
                }
                else
                {
                    if ((Blat - Alat) >= 0 && (Blon - Alon) <= 0)
                    {
                        bear = 270 - bear;
                    }
                    else
                    {
                        if ((Blat - Alat) >= 0 && (Blon - Alon) >= 0)
                        {
                            bear = bear + 270;
                        }
                    }
                }
            }
            bear -= 235;
            return bear;
            //return 360 - bear;
        }


        #endregion

        #region OpenFile/Read Format route file(longtitude,latitude,height,milieage,tag),文件操作，线路文件格式必须是(经度，纬度，高度，公里标，备注)

        private void SetOFD(bool isMultiSelect, string fileType)
        {
            oFD.Multiselect = isMultiSelect;
            oFD.Filter = fileType;//"Xml文件(*.xml)|*.xml";
        }
        private bool IsHaveFile(string filename)
        {
            if (FilePaths.Contains(filename))
            {
                return true;
            }
            else//不含当前文件则添加路径
            {
                FilePaths.Add(filename);
                return false;
            }
        }
        private void ReadFormatTxt(string filePath, ref List<string[]> list)
        {
            //try
            //{
            list.Clear();//首先清空list
            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.Default);
                foreach (var line in lines)
                {
                    string temp = line.Trim();
                    if (temp != "")
                    {
                        string[] arr = temp.Split(new char[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (arr.Length > 0)
                        //{
                        //    list.Add(arr);
                        //}
                        if (arr.Length >= 3)
                        {
                            list.Add(arr);
                        }
                        else
                        {
                            throw new Exception("线路文件格式不正确");
                        }
                    }
                }
            }
        }

        void OFDSaveLastFilePath(string filepath)
        {
            oFD.InitialDirectory = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);
        }

        #endregion

        #region Set Markers/Overlays 设置图标/添加图层/动车车头的位置

        /// <summary>
        /// Set TrainMarker's Postion, Angle
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="bear"></param>
        private void SetTrainMarker(double lat, double lon, bool iscorrect=false,double bear = 0)
        {
            PointLatLng point = new PointLatLng();
            if (iscorrect)
            {
                double[] correctlatlng = transform(lat, lon);
                point = new PointLatLng(correctlatlng[0], correctlatlng[1]);
            }
            else
            {
                point = new PointLatLng(lat, lon);
            }
            Bitmap b = null;
            if (btnChooseImage.EditValue == null)
            {
                b = global::MapSimulator.Properties.Resources.train1;
            }
            else
            {
                b = GetBitmapFromString(btnChooseImage.EditValue.ToString());
            }
            Train = new GMarkerGoogleExt(point, b);
            Train.IsHitTestVisible = true;
        }
        private void InitialMarker()
        {
            if(Train!=null)
            {
                return;
            }
            if (currentOverlay == null || points.Count == 0)
            {
                return;
            }
            var point = points[0];
            SetTrainMarker(point.Lat, point.Lng);
            this.gMapOverlay.Markers.Add(Train);
            this.gmap.Overlays.Add(gMapOverlay);
        }
        
        public GMarkerGoogleType GetRandomMarkerGoogleType()
        {
            int[] markerColor = new int[] { 0, 2, 8, 13, 18, 21, 24, 27, 31 };
            Random random = new Random();
            int index = random.Next(0, 9);
            GMarkerGoogleType type = (GMarkerGoogleType)markerColor[index];
            return type;
        }
        public System.Drawing.Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //  对于C#的随机数，没什么好说的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;
            return System.Drawing.Color.FromArgb(int_Red, int_Green, int_Blue);
        }
        /// <summary>
        /// 根据点坐标，索引和类型转GMarkerGoogleExt
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private GMarkerGoogleExt PointToCustomMarker(PointLatLng point, int index, GMarkerGoogleType t)
        {
            GMarkerGoogleExt gMapMarker = new GMarkerGoogleExt(point, t);
            gMapMarker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            //
            gMapMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return gMapMarker;
        }
        private PointLatLng GetTxtPoint(double[] latlon)
        {
            double[] correctlatlng = transform(latlon[1], latlon[0]);
            PointLatLng point = new PointLatLng(correctlatlng[0], correctlatlng[1]);
            return point;
        }

        private GMarkerGoogleExt PointToGoogleExt(PointLatLng point, int index, GMarkerGoogleType t, System.Drawing.Color c, bool isCircle)
        {
            GMarkerGoogleExt marker = new GMarkerGoogleExt(point, t, c, 5, isCircle);
            marker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            marker.ToolTip.Foreground = Brushes.Black;
            marker.ToolTip.TextPadding = new Size(20, 10);
            //
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return marker;
        }
        private void ShowFileLatLng(List<string[]> lnglat, string filename, bool isShowPerfectView)
        {
            try
            {
                GMapOverlay fileOverlay = new GMapOverlay();
                fileOverlay.Id = filename.Substring(filename.LastIndexOf("\\") + 1);
                List<PointLatLng> points = new List<PointLatLng>();
                var t = GetRandomMarkerGoogleType();
                var c = GetRandomColor();
                //第一个点
                double[] first = new double[2] { Convert.ToDouble(lnglat[0][0]), Convert.ToDouble(lnglat[0][1]) };//经度//纬度
                PointLatLng pfirst = GetTxtPoint(first);
                fileOverlay.Markers.Add(PointToCustomMarker(pfirst, 0, t));
                points.Add(pfirst);
                //
                for (int j = 1; j < lnglat.Count - 1; j++)
                {
                    double[] ll = new double[2] { Convert.ToDouble(lnglat[j][0]), Convert.ToDouble(lnglat[j][1]) };//经度//纬度
                    PointLatLng p = GetTxtPoint(ll);
                    fileOverlay.Markers.Add(PointToGoogleExt(p, j, t, c, true));
                    points.Add(p);
                }
                //最后一个点
                double[] last = new double[2] { Convert.ToDouble(lnglat[lnglat.Count - 1][0]), Convert.ToDouble(lnglat[lnglat.Count - 1][1]) };//经度//纬度
                PointLatLng plast = GetTxtPoint(last);
                fileOverlay.Markers.Add(PointToCustomMarker(plast, lnglat.Count - 1, t));
                points.Add(plast);
                //
                //GMapRoute route = new GMapRoute(points, fileOverlay.Id);
                //fileOverlay.Routes.Add(route);
                gMapOverlays.Add(fileOverlay);
                this.gmap.Overlays.Add(fileOverlay);

                //添加自定义属性
                AddCustomAttribute(fileOverlay, false, false, c, false);
                
                //如果设置最佳视图
                if (isShowPerfectView)
                {
                    SetPerfectPos(points);
                }
                AddFileRow(fileOverlay.Id, true, true, false, true);
                currentOverlay = fileOverlay;
                this.points = points;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        #endregion
        
        #region TimeLine/Replaying of historical running data。轨迹回放
        private void trackBarControl1_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void btnStop_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            t.Enabled = false;
        }
        private System.Timers.Timer t;

        private void btnStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if(currentOverlay==null&&this.points.Count==0)
                {
                    return;
                }
                if (t == null)
                {
                    t = new System.Timers.Timer(10);
                }
                t.Elapsed += new System.Timers.ElapsedEventHandler(theout);
                t.AutoReset = true;
                t.Enabled = true;
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        private int index = 0;
        private int maxIndex = 0;
        private PointLatLng GetPointLatLng(int index)
        {
            if (this.points == null||this.points.Count==0)
            {
                return new PointLatLng(38.0, 122.0);
            }
            else
            {
                var point = this.points.ElementAt(index);
                Console.WriteLine("Lat:"+point.Lat+"Lon"+point.Lng);
                return point;
            }
        }

        private void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            if(maxIndex == 0)
            {
                return;
            }
            if(index <= maxIndex-1)
            {
                Train.Position = GetPointLatLng(index);
                index++;
            }
            else
            {
                t.Enabled = false;
                return;
            }
        }

        private void btnSpeedUp_ItemClick(object sender, ItemClickEventArgs e)
        {
            t.Interval = t.Interval / 10;
        }

        private void btnSpeedCut_ItemClick(object sender, ItemClickEventArgs e)
        {
            t.Interval = t.Interval * 10;
        }
        private void btnRestart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            index = 0;
            Train.Position = this.points.ElementAt(index);
        }

        #endregion

        #region Change current Map.切换地图源
        private void barEditItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }


        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeMap(barEditItem3.EditValue.ToString());
        }
        /// <summary>
        /// 执行Visio的命令
        /// </summary>
        /// <param name="s"></param>
        private void ChangeMap(string s)
        {
            switch (s)
            {
                case "2DMap":
                    gmap.MapProvider = GMapProviders.GoogleChinaMap;
                    gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance; GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    break;
                case "GoogleChinaSatelliteMap":
                    gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
                    GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    break;
                case "Other":
                    gmap.MapProvider = GMapProviders.EmptyProvider;
                    GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    break;
                case "GoogleChinaHybridMap":
                    gmap.MapProvider = GMapProviders.GoogleChinaHybridMap;
                    GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    break;
                case "GoogleChinaTerrainMap":
                    gmap.MapProvider = GMapProviders.GoogleChinaTerrainMap;
                    GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    break;
                default:
                    break;
            }
            GMapProvider.TimeoutMs = 1000;
            gmap.ReloadMap();
        }
        #endregion

        #region Set the Overlay to fit on the full screen.适应全屏幕

        private void SetPerfectPos(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapMarker> gMarkers)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (GMapMarker m in gMarkers)
            {
                points.Add(m.Position);
            }
            SetPerfectPos(points);
        }
        private void SetPerfectPos(List<PointLatLng> gpoints)
        {

            if (gpoints.Count != 0)
            {
                double minlat = gpoints.Min(a => a.Lat);
                double maxlat = gpoints.Max(a => a.Lat);
                double minlng = gpoints.Min(a => a.Lng);
                double maxlng = gpoints.Max(a => a.Lng);
                PointLatLng lefttop = new PointLatLng(minlat, minlng);
                PointLatLng center = new PointLatLng((minlat + maxlat) / 2.0, (minlng + maxlng) / 2.0);
                lefttop.Lat += maxlat - minlat;
                RectLatLng area = new RectLatLng();
                area.LocationTopLeft = lefttop;
                area.Size = new SizeLatLng(maxlat - minlat, maxlng - minlng);
                this.gmap.SelectedArea = area;
                this.gmap.SetZoomToFitRect(area);
            }
        }
        private void SetPerfectPos(bool isCurrentOverlay, bool isSetPerfectPos, GMapOverlay overlay)
        {
            if (isCurrentOverlay)
            {
                currentOverlay = overlay;
                txtCurrentOverlay.Caption = string.Format("当前图层:{0}", overlay.Id);

            }
            if (isSetPerfectPos)
            {
                SetPerfectPos(overlay.Markers);
            }
        }

        private void btnSetPerfectPos_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                ///
                if (gMapOverlays.Count != 0)//如果没有打开地图文件
                {
                    List<PointLatLng> points = new List<PointLatLng>();
                    foreach (GMapOverlay overlay in gMapOverlays)
                    {
                        if (overlay.Markers.Count != 0)
                        { points.AddRange(SearchCertainPoint(overlay)); }
                    }
                    SetPerfectPos(points);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 找到图层中的左上和右下的地理点
        /// </summary>
        /// <returns></returns>
        private List<PointLatLng> SearchCertainPoint(GMapOverlay overlay)
        {
            GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapMarker> markers = overlay.Markers;
            var minlat = markers.Min(a => a.Position.Lat);
            var maxlat = markers.Max(a => a.Position.Lat);
            var minlng = markers.Min(a => a.Position.Lng);
            var maxlng = markers.Max(a => a.Position.Lng);
            PointLatLng lefttop = new PointLatLng(maxlat, minlng);
            PointLatLng rightbottom = new PointLatLng(minlat, maxlng);
            return new List<PointLatLng>() { lefttop, rightbottom };
        }


        #endregion

        #region Drag and drop the focused row of the gridControl1.文件管理框拖拽(上下移动)
        //公共变量,存放要拖拽的对象
        GridHitInfo downHitInfo = null;

        //鼠标按下事件
        private void gridView1_MouseDown(object sender, MouseEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            downHitInfo = null;
            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None) return;
            if (e.Button == MouseButtons.Left && hitInfo.RowHandle >= 0)
                downHitInfo = hitInfo;
        }

        //MouseMove 鼠标移动事件
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    object row = view.GetRow(downHitInfo.RowHandle);
                    view.GridControl.DoDragDrop(row, DragDropEffects.Move);
                    downHitInfo = null;
                    DevExpress.Utils.DXMouseEventArgs.GetMouseArgs(e).Handled = true;
                }
            }
        }

        private void gridControl1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void gridControl1_DragDrop(object sender, DragEventArgs e)
        {
            GridControl grid = sender as GridControl;
            GridView view = grid.MainView as GridView;
            GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
            int sourceRow = downHitInfo.RowHandle;
            int targetRow = hitInfo.RowHandle;
            MoveRow(sourceRow, targetRow, ref chooseFiles);
            index = 0;
            currentOverlay = null;
        }
        private void MoveRow(int sourceRow, int targetRow, ref DataTable sourceTable)
        {
            if (sourceRow == targetRow) return;
            GridView view = gridView1;
            GridControl gc = gridControl1;
            //
            sourceTable = UpdateChooseFileLine(sourceRow, targetRow, ref sourceTable);
            gridControl1.DataSource = sourceTable;
            gridView1.RefreshData();

        }
        private DataTable UpdateChooseFileLine(int sourceRow, int targetRow, ref DataTable sourceTable)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IsChoose", typeof(bool)).SetOrdinal(0);
            dt.Columns.Add("IsOverlayVisible", typeof(bool)).SetOrdinal(1);
            dt.Columns.Add("IsLine", typeof(bool)).SetOrdinal(2);
            dt.Columns.Add("IsLocked", typeof(bool)).SetOrdinal(3);
            dt.Columns.Add("Name");
            for (int j = 0; j < sourceTable.Rows.Count; j++)
            {
                DataRow dr;
                if (j == targetRow)
                {
                    dr = dt.NewRow();
                    dr.ItemArray = sourceTable.Rows[sourceRow].ItemArray;
                    Console.WriteLine(dr["Name"].ToString());
                    dt.Rows.Add(dr);
                }
                if (j == sourceRow)
                {
                    continue;
                }
                dr = dt.NewRow();
                dr.ItemArray = sourceTable.Rows[j].ItemArray;
                dt.Rows.Add(dr);

            }

            return dt;
        }



        #endregion
        
        private void btnSetMarker_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(chooseFiles.Rows.Count==0)
            {
                return;
            }
            else
            {
                string overlayname = chooseFiles.Rows[0]["Name"].ToString();
                currentOverlay = this.gMapOverlays.Find(a => a.Id == overlayname);
                Console.WriteLine(overlayname);
                List<PointLatLng> pointts = new List<PointLatLng>();
                foreach (GMapMarker marker in currentOverlay.Markers)
                {
                    pointts.Add(marker.Position);
                }
                this.points = pointts;
                index = 0;
                maxIndex = pointts.Count;
                float angle = (float)(GetBear2(points[0].Lat, points[1].Lat, points[0].Lng, points[1].Lng) + 90);
                if (Train == null)
                {
                    //Initialize the Train Marker.
                    Bitmap b = null;
                    if (btnChooseImage.EditValue == null)
                    {
                        b = global::MapSimulator.Properties.Resources.train1;
                    }
                    else
                    {
                        b = GetBitmapFromString(btnChooseImage.EditValue.ToString());
                    }
                    Train = new GMarkerGoogleExt(this.points[0], b);
                    Train._angle = 0;
                    this.gMapOverlay.Markers.Add(Train);
                    this.gMapOverlays.Add(gMapOverlay);
                }
                else if (Train != null)
                {
                    Train.IsHitTestVisible = true;
                    Train.Position = points[0];
                    Train._angle = (float)angle + 90;//方向
                }
            }
        }

        private void btnChooseImage_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
        }

        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Train == null)
            {
                return;
            }
            if (btnChooseImage.EditValue == null)
            {
                return;
            }
            Train.Bitmap = GetBitmapFromString(btnChooseImage.EditValue.ToString());
            this.gmap.Refresh();

        }

        private void gmap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)//上移
            {
                this.gmap.Offset(0, 10);
            }
            else if (e.KeyCode == Keys.A)//左移
            {
                this.gmap.Offset(10, 0);
            }
            else if (e.KeyCode == Keys.S)//下移
            {
                this.gmap.Offset(0, -10);
            }
            else if (e.KeyCode == Keys.D)//右移动
            {
                this.gmap.Offset(-10, 0);
            }
        }
    }
}

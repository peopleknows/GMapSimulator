using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    /// GMapRoute扩展
    /// </summary>
    public class GMapRouteExt:GMapRoute
    { 
        public GMapRouteExt(string name):base(name)
        {
            Markers = new List<GMarkerGoogleExt>();
            this.Stroke = new Pen(GetRandomColor(), 5);
            PointsInterval = 1;
        }

        public void updateMakers(int index)
        {
            if(this.Overlay.Markers.Contains((this.Markers[index]))) this.Overlay.Markers.Remove(this.Markers[index]); 
        }
        public GMapRouteExt(List<PointLatLng> points, string name) : base(points,name)
        {
            this.Stroke = new Pen(GetRandomColor(), 5);
            Markers = new List<GMarkerGoogleExt>();
            PointsInterval = 1;
        }
        public GMapRouteExt(PointLatLng[] points,string name) : base(points,name)
        {
            this.Stroke = new Pen(GetRandomColor(), 5);
            initialRouteMarker();
            PointsInterval = 1;
        }
        private void initialRouteMarker ()
        {
            if ( Markers == null)  Markers = new List<GMarkerGoogleExt>(); ;  
            System.Drawing.Color color;
            while ((color = GetRandomColor()) == this.Stroke.Color) ; 
            foreach (PointLatLng marker in this.Points)
            {
                GMarkerGoogleExt markerInRoute = new GMarkerGoogleExt(marker,GMarkerGoogleType.blue_dot,color,5,true);
                markerInRoute.Size = new Size(8, 8);
                markerInRoute.BindRoute = this;
                Markers.Add(markerInRoute);
            };

        }

        /// <summary>
        /// 返回随机颜色
        /// </summary>
        /// <returns></returns>
        public Color GetRandomColor()
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
            return Color.FromArgb(int_Red, int_Green, int_Blue);
        }
        /// <summary>
        /// 初始化加入指定的图层中
        /// </summary>
        /// <param name="points"></param>
        /// <param name="name"></param>
        /// <param name="overlay"></param>
        public GMapRouteExt(GMap.NET.PointLatLng[] points, string name,GMapOverlay overlay) : base(points, name)
        {
            initialRouteMarker();
            overlay.Routes.Add(this);
            PointsInterval = 1;
            this.markersVisible = false; 
        }
        public List<GMarkerGoogleExt> Markers
        { 
            get; 
            set; 
        }
        private bool markersVisible;
        public bool MarkersVisible
        {
            get
            {
                return this.markersVisible;
            }
            set
            {
                if (value) ShowMarkers(false);
                else HideMarkers(); 
                this.markersVisible = value;
            }
        }
        /// <summary>
        /// 显示标注
        /// </summary>
        public void ShowMarkers(bool isForced)
        {
            if (this.MarkersVisible&&!isForced) return;
            if (this.Markers.Count != this.Points.Count)
            {
                this.RemoveMarkers();
                this.Markers.Clear();
                initialRouteMarker();
                this.ShowMarkers(true);
            }
            else
            {
                foreach (GMarkerGoogleExt marker in Markers)
                {
                    if (!this.Overlay.Markers.Contains(marker)) this.Overlay.Markers.Add(marker);
                    marker.IsVisible = true;
                }
            }
        }
        /// <summary>
        /// 隐藏标注
        /// </summary>
        public void HideMarkers()
        {
            if (!markersVisible) return;
            foreach (GMapMarker marker in Markers)
            {
                marker.IsVisible = false;
            }
        }
        /// <summary>
        /// 移除标注
        /// </summary>
        public void RemoveMarkers()
        {
            foreach (GMapMarker marker in Markers)
            {
                if (this.Overlay.Markers.Contains(marker)) this.Overlay.Markers.Remove(marker);
            }
        }
        /// <summary>
        /// 公里标间隔
        /// </summary>
        public float PointsInterval { get; set; }
        /// <summary>
        /// 公里标开始位置
        /// </summary>
        public double StartKiloPos { get; set; }
        /// <summary>
        /// 绑定的开始标注
        /// </summary>
        public GMarkerGoogleExt BindeStartMarker { get; set; }
        /// <summary>
        /// 绑定的结束标注
        /// </summary>
        public GMarkerGoogleExt BindeEndMarker { get; set; }
        /// <summary>
        /// 获取以分隔符划分的第一部分
        /// </summary>
        /// <param name="splitter">分隔符</param>
        /// <returns>名字的第一部分</returns>
        public string GetFirstPartOfName(char splitter)
        {
            return this.Name.Split(splitter)[0];
        }
        /// <summary>
        /// 编号
        /// </summary>
        public int RouteID { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public List<double> Height { get; set; }
        public List<double> KiloPos { get; set; }



    }

    
}

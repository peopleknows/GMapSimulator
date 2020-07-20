using InfoEdit;
using Microsoft.Office.Interop.Visio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoEdits
{
    public class Point
    {
        public int ID { get; set; }//点id
        public int TrackID { get; set; }//直股轨道编号
        public int Track2ID { get; set; }//弯股轨道编号
        public double X { get; set; }//X坐标
        public double Y { get; set; }//Y坐标
        public double KiloPos { get; set; } = 0.0;//公里标
        public double Lat { get; set; } = 0.0;//纬度(数字度单位)
        public double Lon { get; set; } = 0.0;//经度（数字度）
        public double Hgt { get; set; } = 0.0;//高度(m)
        public double DeltaPos { get; set; }//距离轨道起点的距离
        public int ForwardID { get; set; } = 999; //前向指针(按照下行方向)，虚拟终点的下行方向为999
        public int ReverseID { get; set; } = 999;//后向指针(上行方向)，虚拟起点的上行方向为999
        public int SideID { get; set; } = 999;//侧向--主要针对道岔点的弯轨，无岔区段设置为999
        public DeviceType pointType { get; set; } = DeviceType.PiecePoint;
        public SwitchDirection SwitchDirection { get; set; } = SwitchDirection.None;//一般情况下点不为岔尖点
        public string Tag { get; set; }//特殊备注，如道岔的编号
        public int VisioIndex { get; set; }
        public string VisioShapeName { get; set; }
        public bool IsConnected { get; set; }
        public string FileName { get; set; } = "";
        public double Bear { get; set; } = 0.0;
        public double DeltaBear { get; set; } = 0.0;

        public int index { get; set; }//垂距限值算法的直线索引index
        public Point()
        {

        }
        public Point(double lat,double lon)
        {
            this.Lat = lat;
            this.Lon = lon;
        }
        public Point(double lat, double lon,int ID)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.ID = ID;
        }
        public Point(double lat, double lon, int ID,DeviceType type)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.ID = ID;
            this.pointType = type;
            if(type==DeviceType.StartPoint)//如果是起点
            {
                this.ReverseID = 999;
                this.ForwardID = ID + 1;
            }
            else if(type==DeviceType.EndPoint)//如果是终点
            {
                this.ReverseID = ID - 1;
                this.ForwardID = 999;
            }
            else if(type == DeviceType.PiecePoint)
            {
                this.ReverseID = ID - 1;
                this.ForwardID = ID + 1;
            }
        }
        
        public Point(double lat, double lon, int ID,int TrackId, DeviceType type)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.TrackID = TrackId;
            this.Track2ID = TrackId;
            this.ID = ID;
            this.pointType = type;
            if (type == DeviceType.StartPoint)//如果是起点
            {
                this.ReverseID = 999;
                this.ForwardID = ID + 1;
            }
            else if (type == DeviceType.EndPoint)//如果是终点
            {
                this.ReverseID = ID - 1;
                this.ForwardID = 999;
            }
            else if (type == DeviceType.PiecePoint)
            {
                this.ReverseID = ID - 1;
                this.ForwardID = ID + 1;
            }
        }
        public Point(double lat, double lon, int ID,int Trackid, DeviceType type, int reverseID, int forwardID)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.TrackID = Trackid;
            this.Track2ID = Trackid;
            this.ID = ID;
            this.pointType = type;
            this.ReverseID = reverseID;
            this.ForwardID = forwardID;
        }
        public Point(double lat, double lon, int ID, DeviceType type,int reverseID,int forwardID)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.ID = ID;
            this.pointType = type;
            this.ReverseID = reverseID;
            this.ForwardID = forwardID;
        }
        public bool Equals(Point p)
        {
            return Lat == p.Lat && Lon == p.Lon;
        }
        override
        public string ToString()
        {
            return "[" + Lat + "," + Lon + "]";
        }
    }

    

    public enum DeviceType
    {
        PiecePoint=0,//轨道点
        StartPoint=1,//虚拟起点
        Signal=2,//信号机
        Switch=3,//道岔
        Crossline=4,//渡线
        SingleTrack=5,//轨道区段
        Side=6,//侧线
        EndPoint=7,//虚拟终点
        Balise=8,//应答器
        UpsideTrack=9,
        DownsideTrack=10,
        Other=11

    }
    

    //按照正线的下行方向
    public enum Direction
    {
        LatAscend=0,//纬度升序,站型1
        LatDescend=1,//纬度降序,站型2
        LngAscend =2,//经度升序,站型3
        LngDesecnd =3,//经度降序,站型4

    }

    public enum SwitchDirection
    {
        Reverse=0,//岔尖所在轨道的位置偏移减小的方向（上行方向）
        Forward = 1,//岔尖所在轨道的位置偏移增长方向（下行方向）
        None=999//非道岔
    }

    public enum BaliseDirection
    {
        bothway=0,
        reverse=1,
        forward=2,
        temp=3
    }
    public enum BaliseProperty
    {
        VB=0,//Virtual Balise
        Entity=1,//实体应答器
        VB_2=2,//有区间数据的虚拟应答器
        temp=3//预留

    }
}

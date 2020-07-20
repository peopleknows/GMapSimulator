/// 来自官方网站

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Registrator;
using System.ComponentModel;

namespace Ydl.ECDP.WinMonitorStudio.Utility
{
    public class RepositoryItemYdlTrackBar : DevExpress.XtraEditors.Repository.RepositoryItemTrackBar
    {
        protected internal new BaseEditViewInfo CreateViewInfo()
        {
            return new YdlTrackBarViewInfo(this);
        }
        static RepositoryItemYdlTrackBar()
        {
            RegisterMyColorEdit();
        }
        public RepositoryItemYdlTrackBar()
        { }
        public static void RegisterMyColorEdit()
        {
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo("YdlTrackBarControl",
                typeof(YdlTrackBarControl), typeof(RepositoryItemYdlTrackBar), typeof(YdlTrackBarViewInfo),
                new DevExpress.XtraEditors.Drawing.TrackBarPainter(), true, null,
                typeof(DevExpress.Accessibility.PopupEditAccessible)));
        }
        public override string EditorTypeName { get { return "YdlTrackBarControl"; } }
    }


    public class YdlTrackBarControl : DevExpress.XtraEditors.TrackBarControl
    {
        static YdlTrackBarControl()
        {
            RepositoryItemYdlTrackBar.RegisterMyColorEdit();
        }
        public override string EditorTypeName
        {
            get
            { return "YdlTrackBarControl"; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new RepositoryItemYdlTrackBar Properties
        {
            get { return base.Properties as RepositoryItemYdlTrackBar; }
        }
    }

    public class YdlTrackBarViewInfo : TrackBarViewInfo
    {
        public YdlTrackBarViewInfo(DevExpress.XtraEditors.Repository.RepositoryItem item)
            : base(item)
        {
        }
        public override bool DrawFocusRect
        {
            get { return false; }
        }
    }
}

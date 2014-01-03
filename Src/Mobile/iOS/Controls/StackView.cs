
using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public enum StackPanelAlignement
    {
        Center,
        Top,
    }

    [Register ("StackView")]
    public class StackView : UIView
    {
        private float _offset = 2;

        public StackView ()
        {
            Initialize();
        }
        
        public StackView (IntPtr handle) : base(  handle )
        {
            Initialize();
        }

        
        private void Initialize()
        {
            VerticalAlignement = StackPanelAlignement.Center;    
        }

        public float Offset {
            get{return _offset;}
            set{_offset = value;}
        }

        public float TopOffset {
            get;set;
        }


        public StackPanelAlignement VerticalAlignement {
            get;
            set;
        }

        public void Layout( params StackRow[] rows )
        {

        }

        private float GetStartYPosition (float ctlTotalHeight)
        {
            if ( VerticalAlignement == StackPanelAlignement.Center )
            {
                return ( Bounds.Height - (  ctlTotalHeight < Bounds.Height ? ctlTotalHeight : Bounds.Height )) / 2;
            }
            if ( VerticalAlignement == StackPanelAlignement.Top )
            {
                return TopOffset;
            }
            return 0;
        }

        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();

            var grouppedCtl = Subviews.Where ( v=>!v.Hidden ).OrderBy ( v=>v.Frame.Top ).GroupBy ( v =>v.Frame.Top ).ToArray();

            var ctlTotalHeight = grouppedCtl.Sum ( g => g.Max ( v=>v.Bounds.Height ) );

            ctlTotalHeight += ( grouppedCtl.Count() - 1 ) * _offset;

            var currentYPosition = GetStartYPosition( ctlTotalHeight );


            foreach (var item in grouppedCtl) {

                foreach (var ctl in item) {
                    ctl.SetPosition( null , currentYPosition);             
                }

                currentYPosition = currentYPosition + item.Max ( v=>v.Bounds.Height ) + _offset  ;
            }
        }




    }

    public class StackRow
    {

    }
}


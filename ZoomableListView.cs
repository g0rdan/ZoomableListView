using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Application
{
    [Register ("ZoomableListView")]
    public class ZoomableListView : ListView
    {
        const int INVALID_POINTER_ID = -1;
        int mActivePointerId = INVALID_POINTER_ID;
        ScaleGestureDetector mScaleDetector;

        float mLastTouchX;
        float mLastTouchY;
        float mPosX;
        float mPosY;

        public float MScaleFactor { get; set; } = 1f;
        public float MaxWidth { get; set; } = 0.0f;
        public float MaxHeight { get; set; } = 0.0f;
        public float ZoomableViewWidth { get; set; }
        public float ZoomableViewHeight { get; set; }

        public ZoomableListView (Context context) : base (context)
        {
            mScaleDetector = new ScaleGestureDetector (Context, new ScaleListener (this));
        }

        public ZoomableListView (Context context, IAttributeSet attrs) : base (context, attrs)
        {
            mScaleDetector = new ScaleGestureDetector (Context, new ScaleListener (this));
        }

        public ZoomableListView (Context context, IAttributeSet attrs, int defStyleAttr) : base (context, attrs, defStyleAttr)
        {
            mScaleDetector = new ScaleGestureDetector (Context, new ScaleListener (this));
        }

        protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
        {
            ZoomableViewWidth = MeasureSpec.GetSize (widthMeasureSpec);
            ZoomableViewHeight = MeasureSpec.GetSize (heightMeasureSpec);
            base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
        }

        public override bool OnTouchEvent (MotionEvent e)
        {
            base.OnTouchEvent (e);
            var action = e.Action;
            mScaleDetector.OnTouchEvent (e);
            switch (action & MotionEventActions.Mask) {
            case MotionEventActions.Down:
                float x = e.GetX ();
                float y = e.GetY ();
                mLastTouchX = x;
                mLastTouchY = y;
                mActivePointerId = e.GetPointerId (0);
                break;
            case MotionEventActions.Move: 
                int pointerIndex = e.FindPointerIndex (mActivePointerId);
                float x1 = e.GetX (pointerIndex);
                float y1 = e.GetY (pointerIndex);
                float dx = x1 - mLastTouchX;
                float dy = y1 - mLastTouchY;

                mPosX += dx;
                mPosY += dy;

                if (mPosX > 0.0f)
                    mPosX = 0.0f;
                else if (mPosX < MaxWidth)
                    mPosX = MaxWidth;

                if (mPosY > 0.0f)
                    mPosY = 0.0f;
                else if (mPosY < MaxHeight)
                    mPosY = MaxHeight;

                mLastTouchX = x1;
                mLastTouchY = y1;

                Invalidate ();
                break;
            case MotionEventActions.Up:
                mActivePointerId = INVALID_POINTER_ID;
                break;
            case MotionEventActions.Cancel:
                mActivePointerId = INVALID_POINTER_ID;
                break;
            case MotionEventActions.PointerUp:
                int pointerIndex1 = ((int)action & (int)MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                int pointerId = e.GetPointerId (pointerIndex1);
                if (pointerId == mActivePointerId) {
                    int newPointerIndex = pointerIndex1 == 0 ? 1 : 0;
                    mLastTouchX = e.GetX (newPointerIndex);
                    mLastTouchY = e.GetY (newPointerIndex);
                    mActivePointerId = e.GetPointerId (newPointerIndex);
                }
                break;
            }
            return true;
        }

        protected override void OnDraw (Canvas canvas)
        {
            base.OnDraw (canvas);
            canvas.Save (SaveFlags.Matrix);
            canvas.Translate (mPosX, mPosY);
            canvas.Scale (MScaleFactor, MScaleFactor);
            canvas.Restore ();
        }

        protected override void DispatchDraw (Canvas canvas)
        {
            canvas.Save (SaveFlags.Matrix);
            if (MScaleFactor == 1.0f) {
                mPosX = 0.0f;
                mPosY = 0.0f;
            }
            canvas.Translate (mPosX, mPosY);
            canvas.Scale (MScaleFactor, MScaleFactor);
            base.DispatchDraw (canvas);
            canvas.Restore ();
            Invalidate ();
        }

        class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            ZoomableListView _zListView;

            public ScaleListener (ZoomableListView zListView)
            {
                _zListView = zListView;
            }

            public override bool OnScale (ScaleGestureDetector detector)
            {
                _zListView.MScaleFactor *= detector.ScaleFactor;
                _zListView.MScaleFactor = Math.Max (1.0f, Math.Min (_zListView.MScaleFactor, 3.0f));
                _zListView.MaxWidth = _zListView.ZoomableViewWidth - (_zListView.ZoomableViewWidth * _zListView.MScaleFactor);
                _zListView.MaxHeight = _zListView.ZoomableViewHeight - (_zListView.ZoomableViewHeight * _zListView.MScaleFactor);
                _zListView.Invalidate ();
                return true;
            }
        }
    }
}
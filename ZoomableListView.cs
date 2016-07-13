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

        public float mScaleFactor = 1f;
        public float maxWidth = 0.0f;
        public float maxHeight = 0.0f;
        float mLastTouchX;
        float mLastTouchY;
        float mPosX;
        float mPosY;
        public float width;
        public float height;

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
            width = MeasureSpec.GetSize (widthMeasureSpec);
            height = MeasureSpec.GetSize (heightMeasureSpec);
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
                else if (mPosX < maxWidth)
                    mPosX = maxWidth;

                if (mPosY > 0.0f)
                    mPosY = 0.0f;
                else if (mPosY < maxHeight)
                    mPosY = maxHeight;

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
            default:
                break;
            }
            return true;
        }

        protected override void OnDraw (Canvas canvas)
        {
            base.OnDraw (canvas);
            canvas.Save (SaveFlags.Matrix);
            canvas.Translate (mPosX, mPosY);
            canvas.Scale (mScaleFactor, mScaleFactor);
            canvas.Restore ();
        }

        protected override void DispatchDraw (Canvas canvas)
        {
            canvas.Save (SaveFlags.Matrix);
            if (mScaleFactor == 1.0f) {
                mPosX = 0.0f;
                mPosY = 0.0f;
            }
            canvas.Translate (mPosX, mPosY);
            canvas.Scale (mScaleFactor, mScaleFactor);
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
                _zListView.mScaleFactor *= detector.ScaleFactor;
                _zListView.mScaleFactor = Math.Max (1.0f, Math.Min (_zListView.mScaleFactor, 3.0f));
                _zListView.maxWidth = _zListView.width - (_zListView.width * _zListView.mScaleFactor);
                _zListView.maxHeight = _zListView.height - (_zListView.height * _zListView.mScaleFactor);
                _zListView.Invalidate ();
                return true;
            }
        }
    }
}


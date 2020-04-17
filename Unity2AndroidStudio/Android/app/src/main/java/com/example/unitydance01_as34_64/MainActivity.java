package com.example.unitydance01_as34_64;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.PixelFormat;
import android.graphics.Rect;
import android.graphics.SurfaceTexture;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.TextureView;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.FrameLayout;
import android.widget.ImageView;

import com.unity3d.player.*;

import org.opencv.android.BaseLoaderCallback;
import org.opencv.android.CameraBridgeViewBase;
import org.opencv.android.LoaderCallbackInterface;
import org.opencv.android.OpenCVLoader;
import org.opencv.core.Core;
import org.opencv.core.Mat;


import java.lang.reflect.Field;
import java.util.ArrayList;

public class MainActivity extends Activity implements CameraBridgeViewBase.CvCameraViewListener2 {

    private static String TAG="MainActivity";

    protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code

    protected Surface mSurface;
    protected SurfaceHolder.Callback mProxyCallback;
    protected ProxySurfaceHolder mProxySurfaceHolder = new ProxySurfaceHolder();

    private CameraBridgeViewBase mOpenCvCameraView;
    private BaseLoaderCallback mLoaderCallback = new BaseLoaderCallback(this) {
        @Override
        public void onManagerConnected(int status) {
            switch (status) {
                case LoaderCallbackInterface.SUCCESS:
                {
                    Log.i(TAG, "OpenCV loaded successfully");
                    mOpenCvCameraView.enableView();
                } break;
                default:
                {
                    super.onManagerConnected(status);
                } break;
            }
        }
    };
    private Mat mRgba;  //buffer for processing camera frames
    private Mat mFlipRgba;


    class ProxySurfaceHolder implements SurfaceHolder {
        @Override
        public void addCallback(Callback callback) {
        }

        @Override
        public void removeCallback(Callback callback) {
        }

        @Override
        public boolean isCreating() {
            return false;
        }

        @Override
        public void setType(int type) {
        }

        @Override
        public void setFixedSize(int width, int height) {
        }

        @Override
        public void setSizeFromLayout() {
        }

        @Override
        public void setFormat(int format) {
        }

        @Override
        public void setKeepScreenOn(boolean screenOn) {
        }

        @Override
        public Canvas lockCanvas() {
            return null;
        }

        @Override
        public Canvas lockCanvas(Rect dirty) {
            return null;
        }

        @Override
        public void unlockCanvasAndPost(Canvas canvas) {
        }

        @Override
        public Rect getSurfaceFrame() {
            return null;
        }

        @Override
        public Surface getSurface() {
            return mSurface;
        }
    }

    static{if (!OpenCVLoader.initDebug()){Log.e(TAG, "openCV Library load failed");}};

    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {

        requestWindowFeature(Window.FEATURE_NO_TITLE);

        super.onCreate(savedInstanceState);

        mUnityPlayer = new UnityPlayer(this);
        setContentView(R.layout.activity_main);

        ImageView imgLeft = (ImageView) findViewById(R.id.ImageLeft);
        ImageView imgRight = (ImageView) findViewById(R.id.ImageRight);

        Bitmap bmpTop = BitmapFactory.decodeResource(getResources(),R.drawable.star1_top);
        imgLeft.setImageBitmap(bmpTop);
        Bitmap bmpBottom = BitmapFactory.decodeResource(getResources(),R.drawable.star1_bottom);
        imgRight.setImageBitmap(bmpBottom);

        mOpenCvCameraView = (CameraBridgeViewBase) findViewById(R.id.tutorial1_activity_java_surface_view);
        mOpenCvCameraView.setVisibility(SurfaceView.VISIBLE);
        mOpenCvCameraView.setCvCameraViewListener(this);
        mOpenCvCameraView.setCameraIndex(CameraBridgeViewBase.CAMERA_ID_FRONT);


        for (int i = 0; i < mUnityPlayer.getChildCount(); i++)
        {
            View child = mUnityPlayer.getChildAt(i);
            if (child instanceof SurfaceView)
            {
                SurfaceView surfaceView = (SurfaceView)child;

                ViewGroup.LayoutParams layoutParams = surfaceView.getLayoutParams();
                layoutParams.width = 0;
                layoutParams.height = 0;
                surfaceView.setLayoutParams(layoutParams);

                try {
                    Field field = SurfaceView.class.getDeclaredField("mCallbacks");
                    field.setAccessible(true);
                    ArrayList<SurfaceHolder.Callback> callbacks = (ArrayList<SurfaceHolder.Callback>)field.get(surfaceView);
                    mProxyCallback = callbacks.get(0);
                    synchronized (callbacks) {
                        callbacks.clear();
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }

                final TextureView view = new TextureView(this);
                view.setOpaque(false);
                view.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
                    @Override
                    public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
                        mSurface = new Surface(surface);
                        mProxyCallback.surfaceCreated(mProxySurfaceHolder);
                    }

                    @Override
                    public void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {
                        mProxyCallback.surfaceChanged(mProxySurfaceHolder, 0, width, height);
                    }

                    @Override
                    public boolean onSurfaceTextureDestroyed(SurfaceTexture surface) {
                        mProxyCallback.surfaceDestroyed(mProxySurfaceHolder);
                        return false;
                    }

                    @Override
                    public void onSurfaceTextureUpdated(SurfaceTexture surface) {
                    }
                });


                FrameLayout layout = (FrameLayout)findViewById(R.id.FrameInside);
                layout.addView(view);
                break;
            }
        }
    }


    @Override protected void onNewIntent(Intent intent)
    {
        // To support deep linking, we need to make sure that the client can get access to
        // the last sent intent. The clients access this through a JNI api that allows them
        // to get the intent set on launch. To update that after launch we have to manually
        // replace the intent with the one caught here.
        setIntent(intent);
    }

    // Quit Unity
    @Override protected void onDestroy ()
    {
        mUnityPlayer.quit();
        super.onDestroy();
    }

    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
        mUnityPlayer.pause();
        if (mOpenCvCameraView != null)
            mOpenCvCameraView.disableView();
    }

    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        mLoaderCallback.onManagerConnected(LoaderCallbackInterface.SUCCESS);
        mUnityPlayer.resume();
    }

    // Low Memory Unity
    @Override public void onLowMemory()
    {
        super.onLowMemory();
        mUnityPlayer.lowMemory();
    }

    // Trim Memory Unity
    @Override public void onTrimMemory(int level)
    {
        super.onTrimMemory(level);
        if (level == TRIM_MEMORY_RUNNING_CRITICAL)
        {
            mUnityPlayer.lowMemory();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);
        mUnityPlayer.windowFocusChanged(hasFocus);
    }

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    @Override public boolean dispatchKeyEvent(KeyEvent event)
    {
        if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
            return mUnityPlayer.injectEvent(event);
        return super.dispatchKeyEvent(event);
    }

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    @Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
    /*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }


    public void onCameraViewStarted(int width, int height) {
        mRgba = new Mat();
        mFlipRgba = new Mat();
    }

    public void onCameraViewStopped() {
        mRgba.release();
    }

    public Mat onCameraFrame(CameraBridgeViewBase.CvCameraViewFrame inputFrame) {
        mRgba = inputFrame.rgba();
        //Core.rotate(mRgba.submat(0, mRgba.height(), 0, mRgba.width()), mFlipRgba,Core.ROTATE_90_CLOCKWISE);
        //Core.flip(mRgba, mFlipRgba, 1);
        return mRgba;
    }

}

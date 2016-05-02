using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Hardware;
using Android.Views;
using Android.OS;
using Android.Views.Animations;
using Android.Widget;
using Color = Android.Graphics.Color;
using Thread = System.Threading.Thread;
using View = Android.Views.View;

namespace XamarinExercise.Droid
{
    /*
     * @author VenkataKrishna Vanparthi
     * 
     * This is Main screen.
     * 
     */

    [Activity(Label = "XamarinExercise", Icon = "@drawable/icon", MainLauncher = true,
        Theme = "@style/NoActionBarTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity, View.IOnTouchListener, ISensorEventListener, Animation.IAnimationListener
    {

        private FrameLayout _rootLayout;
        private TextView _title;

        private bool _childScrolling;
        private bool _hasUpdated;

        private long _lastClickTime;

        private float _lastX;
        private float _lastY;
        private float _lastZ;

        private const int ShakeDetectionTimeLapse = 250;
        private const double ShakeThreshold = 800;
        private const long DoubleClickTimeDelta = 300; //milliseconds

        private DateTime _lastUpdate;

        private MemoryLimitedLruCache _memoryCache;

        /*
         * 
         * Initilization method for android activity.
         * 
         */

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main);

            #region getting View ids

            _rootLayout = FindViewById<FrameLayout>(Resource.Id.root_layout);
            _title = FindViewById<TextView>(Resource.Id.title);
            _title.SetTextColor(GlobalMethods.GetRandomColor());
            _title.SetBackgroundColor(Color.Transparent);
            _rootLayout.SetBackgroundColor(Color.White);
            _rootLayout.SetOnTouchListener(this);

            #endregion

            #region Creating animation for actionbar title

            var animation = AnimationUtils.LoadAnimation(this, Resource.Animation.slide_in_left
                );
            animation.Duration = 5000;
            animation.SetAnimationListener(this);
            _title.Text = Title;
            _title.StartAnimation(animation);

            #endregion

            #region Register this as a listener with the underlying service for detecting Shaking of device.

            var sensorManager = GetSystemService(SensorService) as SensorManager;
            if (sensorManager != null)
            {
                var sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
                sensorManager.RegisterListener(this, sensor, SensorDelay.Game);
            }

            #endregion


            #region   Get max available VM memory, exceeding this amount will throw an OutOfMemory exception. Stored in kilobytes as LruCache takes an int in its constructor.

            var maxMemory = (int) (Java.Lang.Runtime.GetRuntime().MaxMemory()/1024);

            // Use 1/8th of the available memory for this memory cache.
            int cacheSize = maxMemory/8;

            _memoryCache = new MemoryLimitedLruCache(cacheSize);

            #endregion
        }

        /*
         * 
         * This method was used to Adding views in the screen whenever user touches the screen.
         * 
         */

        private void AddView(int x, int y)
        {
            var rnd = new Random();
            var shapeType = rnd.Next(1, 3); // random logic for drawing circle or square.
            var size = rnd.Next(100, 200); // random logic for size of view.

            FrameLayout.LayoutParams rParams = new FrameLayout.LayoutParams(size,
                size)
            {
                LeftMargin = x - (size/2),
                TopMargin = y - (size/2)
            };

            if (shapeType == 1) // 1 means drawing Circle view
            {
                if (GlobalMethods.IsOnline(this))
                {
                    new Thread(() =>
                    {
                        // reading hex color code from server URL.
                        var randomData = ReadXml.Instance.ReadHexColor();

                        RunOnUiThread(() =>
                        {
                            var circleView = GetCircleView(rParams, randomData);
                            var bgShape = (GradientDrawable) circleView.Background;
                            bgShape.SetColor(Color.ParseColor(randomData.Data));
                        });
                    }).Start();
                }
                else
                {
                    var circleView = GetCircleView(rParams, null);
                    var bgShape = (GradientDrawable) circleView.Background;
                    bgShape.SetColor(GlobalMethods.GetRandomColor());
                }
            }
            else
            {
                if (GlobalMethods.IsOnline(this))
                {
                    var progressDialog = GlobalMethods.CreateProgressDialog(this, "Drawing");
                    new Thread(() =>
                    {
                        // reading imageUrl from server URL.
                        var randomData = ReadXml.Instance.ReadImageUrl();
                        var bitmap = DownloadAndCache(randomData.Data);

                        RunOnUiThread(() =>
                        {
                            var squareView = GetSquareView(rParams, randomData);
                            squareView.SetImageBitmap(bitmap);
                            progressDialog.Hide();

                        });
                    }).Start();
                }
                else
                {
                    var squareView = GetSquareView(rParams, null);
                    squareView.SetBackgroundColor(GlobalMethods.GetRandomColor());
                }
            }
        }

        /*
         * This method was used to create the circle view with animation. 
         *  
         */

        private View GetCircleView(FrameLayout.LayoutParams rParams, RandomData randomData)
        {
            var circleView = new View(this);
            circleView.SetBackgroundResource(Resource.Drawable.circle);
            circleView.Clickable = true;
            circleView.Focusable = true;
            circleView.FocusableInTouchMode = true;
            circleView.SetOnTouchListener(this);
            _rootLayout.AddView(circleView, rParams);

            circleView.StartAnimation(GetAnimationForNewViews());

            if (randomData != null)
            {
                ChangeTitle(randomData.Title);
            }
            return circleView;
        }

        /*
         * 
         * This method was used to create the square view with animation. 
         * 
         */

        private ImageView GetSquareView(FrameLayout.LayoutParams rParams, RandomData randomData)
        {
            var squareView = new ImageView(this)
            {
                Clickable = true,
                Focusable = true,
                FocusableInTouchMode = true
            };
            squareView.SetOnTouchListener(this);
            _rootLayout.AddView(squareView, rParams);

            squareView.StartAnimation(GetAnimationForNewViews());

            if (randomData != null)
            {
                ChangeTitle(randomData.Title);
            }
            return squareView;
        }

        /*
         * 
         * This method was used to change the actiionbar title whenver there is a change in views state.
         * 
         */

        private void ChangeTitle(string title)
        {
            _title.Text = title;
            _title.SetTextColor(GlobalMethods.GetRandomColor());
        }

        /*
         * 
         * This method was used to create Fab_In Animation for newly created views.
         * 
         */

        private Animation GetAnimationForNewViews()
        {
            var animation = AnimationUtils.LoadAnimation(this, Resource.Animation.design_fab_in
                );
            animation.Duration = 1000;

            return animation;
        }

        /*
         * 
         * This method was used to create Fad_in_from_bottom Animation for updating views.
         * 
         */

        private Animation GetAnimationForExistingViews()
        {
            var animation = AnimationUtils.LoadAnimation(this, Resource.Animation.abc_grow_fade_in_from_bottom
                );
            animation.Duration = 2000;

            return animation;
        }

        /*
         * 
         * This method was used to download image from server and store in cache memory.
         * 
         */

        private Bitmap DownloadAndCache(string imageUrl)
        {
            if (_memoryCache.Get(imageUrl) == null)
            {
                var bitmap = GlobalMethods.DownLoadImageFromUrl(imageUrl);
                _memoryCache.Put(imageUrl, bitmap);
            }
            return (Bitmap) _memoryCache.Get(imageUrl);
        }

        /*
         * 
         * This is a callback method for Touch view. This method will be called when user move the view or click on the screen or doubleTap on the screen.
         * 
         */

        public bool OnTouch(View v, MotionEvent e)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            var action = e.Action & MotionEventActions.Mask;

            switch (action)
            {
                // Called when user click on screen.
                case MotionEventActions.Down:
                    if (v.Id == Resource.Id.root_layout && !_childScrolling)
                    {
                        AddView((int) e.GetX(), (int) e.GetY());
                    }
                    else
                    {
                        long clickTime = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
                        if (clickTime - _lastClickTime < DoubleClickTimeDelta) // Called when user doubleTap the view.
                        {
                            if (v is ImageView)
                            {
                                ImageView imageView = (ImageView) v;

                                if (GlobalMethods.IsOnline(this))
                                {
                                    var progressDialog = GlobalMethods.CreateProgressDialog(this, "Changing");

                                    new Thread(() =>
                                    {
                                        var randomData = ReadXml.Instance.ReadImageUrl();
                                        var bitmap = DownloadAndCache(randomData.Data);

                                        RunOnUiThread(() =>
                                        {
                                            imageView.SetImageBitmap(bitmap);
                                            imageView.StartAnimation(GetAnimationForExistingViews());
                                            progressDialog.Hide();
                                            ChangeTitle(randomData.Title);
                                        });
                                    }).Start();
                                }
                                else
                                {
                                    imageView.SetBackgroundColor(GlobalMethods.GetRandomColor());
                                    imageView.StartAnimation(GetAnimationForExistingViews());
                                }
                            }
                            else
                            {
                                if (GlobalMethods.IsOnline(this))
                                {
                                    new Thread(() =>
                                    {
                                        var randomData = ReadXml.Instance.ReadHexColor();

                                        RunOnUiThread(() =>
                                        {
                                            var bgShape = (GradientDrawable) v.Background;
                                            bgShape.SetColor(Color.ParseColor(randomData.Data));
                                            v.StartAnimation(GetAnimationForExistingViews());

                                            ChangeTitle(randomData.Title);

                                        });
                                    }).Start();
                                }
                                else
                                {
                                    var bgShape = (GradientDrawable) v.Background;
                                    bgShape.SetColor(GlobalMethods.GetRandomColor());
                                    v.StartAnimation(GetAnimationForExistingViews());
                                }
                            }
                        }

                        _lastClickTime = clickTime;
                    }

                    break;

                // Called when user moves the view.
                case MotionEventActions.Move:

                    if (v.Id != Resource.Id.root_layout)
                    {
                        _childScrolling = true;

                        FrameLayout.LayoutParams rParams = new FrameLayout.LayoutParams(v.Width,
                            v.Height)
                        {
                            LeftMargin = (int) e.RawX - v.Width,
                            TopMargin = (int) e.RawY - v.Height
                        };

                        v.LayoutParameters = rParams;
                    }

                    break;
                // Called when user ended the moving the view.
                case MotionEventActions.Up:
                    if (v.Id != Resource.Id.root_layout && _childScrolling)
                    {

                        FrameLayout.LayoutParams rParams = new FrameLayout.LayoutParams(v.Width,
                            v.Height)
                        {
                            LeftMargin = (int) e.RawX - v.Width,
                            TopMargin = (int) e.RawY - v.Height
                        };

                        v.LayoutParameters = rParams;
                    }
                    _childScrolling = false;
                    break;

            }
            return true;
        }

        #region This callback methods are for detecting Device shaking.

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

        }

        /*
         * Method for detecting Shaking of the Device
         * 
         */

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                float x = e.Values[0];
                float y = e.Values[1];
                float z = e.Values[2];

                DateTime curTime = DateTime.Now;
                if (_hasUpdated == false)
                {
                    _hasUpdated = true;
                    _lastUpdate = curTime;
                    _lastX = x;
                    _lastY = y;
                    _lastZ = z;
                }
                else
                {
                    if ((curTime - _lastUpdate).TotalMilliseconds > ShakeDetectionTimeLapse)
                    {
                        float diffTime = (float) (curTime - _lastUpdate).TotalMilliseconds;
                        _lastUpdate = curTime;
                        float total = x + y + z - _lastX - _lastY - _lastZ;
                        float speed = Java.Lang.Math.Abs(total)/diffTime*10000;

                        if (speed > ShakeThreshold)
                        {
                            // **********  Device Shaking was Detected ******************
                            _rootLayout.RemoveAllViews();
                            ChangeTitle(Title);
                        }

                        _lastX = x;
                        _lastY = y;
                        _lastZ = z;
                    }
                }
            }

        }

        #endregion

        #region Callback methods for Animation.

        public void OnAnimationEnd(Animation animation)
        {
            animation.Duration = 5000;
            animation.SetAnimationListener(this);
            _title.StartAnimation(animation);
        }

        public void OnAnimationRepeat(Animation animation)
        {

        }

        public void OnAnimationStart(Animation animation)
        {

        }

        #endregion
    }
}


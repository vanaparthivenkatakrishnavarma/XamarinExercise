using Android.Graphics;
using Java.Net;
using System;
using Android.App;
using Android.Content;
using Android.Net;

namespace XamarinExercise.Droid
{
    /*
     * @author VenkataKrishna Vanparthi
     * This is a common class for entire application. Here we can have common methods for entire application.
     * 
     */

    internal class GlobalMethods
    {
        /*
         * 
         * This method was used to Download the image from server.
         * 
         */

        public static Bitmap DownLoadImageFromUrl(string imageUrl)
        {
            var newurl = new URL(imageUrl);
            var connection = newurl.OpenConnection();
            connection.UseCaches = true;
            return BitmapFactory.DecodeStream(connection.InputStream);
        }

        /*
         * 
         * This method was used to get the random color.
         * 
         */

        public static Color GetRandomColor()
        {
            var random = new Random();
            float r = random.Next(255);
            float g = random.Next(255);
            float b = random.Next(255);
            var color = Color.HSVToColor(
                new[]
                {
                    r,
                    g,
                    b,
                }
                );
            return color;
        }

        /*
         * 
         * This method was used to check the internet connectivity.
         * 
         */

        public static bool IsOnline(Context context)
        {
            var connectivityManager = (ConnectivityManager) context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            return activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting;
        }

        /*
         * 
         * This method was used to design the progressdialog nothing but loading bar.
         * 
         */

        public static ProgressDialog CreateProgressDialog(Context context, string message)
        {
            var progressDialog = new ProgressDialog(context);
            progressDialog.SetMessage(message);
            progressDialog.Show();

            return progressDialog;
        }
    }
}

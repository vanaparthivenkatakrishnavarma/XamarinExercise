using System;
using Android.Runtime;
using Android.Util;

namespace XamarinExercise.Droid
{

    /*
     * @author Venkatakrishna Vanparthi
     * LruCache limited by memory footprint in KB rather than number of items.
     * 
     */
    internal class MemoryLimitedLruCache : LruCache
    {
        public MemoryLimitedLruCache(int size) : base(size)
        {
        }

        protected override int SizeOf(Java.Lang.Object key, Java.Lang.Object value)
        {
            // android.graphics.Bitmap.getByteCount() method isn't currently implemented in Xamarin. Invoke Java method.
            IntPtr classRef = JNIEnv.FindClass("android/graphics/Bitmap");
            var getBytesMethodHandle = JNIEnv.GetMethodID(classRef, "getByteCount", "()I");
            var byteCount = JNIEnv.CallIntMethod(value.Handle, getBytesMethodHandle);

            return byteCount/1024;
        }
    }
}
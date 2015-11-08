using System;
using System.Windows.Threading;

namespace AnnoModificationManager4.Misc
{
    public static class DispatcherExtension
    {
        public static TResult Dispatch<TResult>(this DispatcherObject source, Func<TResult> func)
        {
            if (source.Dispatcher.CheckAccess())
                return func();
            return (TResult)source.Dispatcher.Invoke(func, new object[0]);
        }

        public static TResult Dispatch<T, TResult>(this T source, Func<T, TResult> func) where T : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source);
            return (TResult)source.Dispatcher.Invoke(func, new object[1] { source });
        }

        public static TResult Dispatch<TSource, T, TResult>(this TSource source, Func<TSource, T, TResult> func, T param1) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1);
            return (TResult)source.Dispatcher.Invoke(func, new object[2] { source, param1 });
        }

        public static TResult Dispatch<TSource, T1, T2, TResult>(this TSource source, Func<TSource, T1, T2, TResult> func, T1 param1, T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1, param2);
            return (TResult)source.Dispatcher.Invoke(func, new object[3] { source, param1, param2 });
        }

        public static TResult Dispatch<TSource, T1, T2, T3, TResult>(this TSource source, Func<TSource, T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1, param2, param3);
            return (TResult)source.Dispatcher.Invoke(func, new object[4] { source, param1, param2, param3 });
        }

        public static void Dispatch(this DispatcherObject source, Action func)
        {
            if (source.Dispatcher.CheckAccess())
                func();
            else
                source.Dispatcher.Invoke(func, new object[0]);
        }

        public static void Dispatch<TSource>(this TSource source, Action<TSource> func) where TSource : DispatcherObject
        {
            try
            {
                if (source.Dispatcher.CheckAccess())
                    func(source);
                else
                    source.Dispatcher.Invoke(func, new object[1] { source });
            }
            catch (Exception)
            {
            }
        }

        public static void Dispatch<TSource, T1>(this TSource source, Action<TSource, T1> func, T1 param1) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1);
            else
                source.Dispatcher.Invoke(func, new object[2] { source, param1 });
        }

        public static void Dispatch<TSource, T1, T2>(this TSource source, Action<TSource, T1, T2> func, T1 param1, T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2);
            else
                source.Dispatcher.Invoke(func, new object[3] { source, param1, param2 });
        }

        public static void Dispatch<TSource, T1, T2, T3>(this TSource source, Action<TSource, T1, T2, T3> func, T1 param1, T2 param2, T3 param3) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2, param3);
            else
                source.Dispatcher.Invoke(func, new object[4] { source, param1, param2, param3 });
        }
    }
}

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RxHOL.Extensions
{
    public static class RemoveTimestampExtension
    {
        public static IObservable<T> RemoveTimestamp<T>(this IObservable<Timestamped<T>> This)
        {
            return This.Select(x => x.Value);
        }
    }

    public static class LogTimestampedValuesExtension
    {
        public static IObservable<T> LogTimestampedValues<T>(this IObservable<T> source, Action<Timestamped<T>> onNext)
        {
            return source.Timestamp().Do(onNext).RemoveTimestamp();
        }

        public static IObservable<T> LogTimestampedValues<T>(this IObservable<T> source, Action<Timestamped<T>> onNext, IScheduler scheduler)
        {
            return source.Timestamp(scheduler).Do(onNext).RemoveTimestamp();
        }
    }
}

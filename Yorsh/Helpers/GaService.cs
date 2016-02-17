using Android.Gms.Analytics;
using Java.Lang;
using Yorsh.Data;
using Boolean = System.Boolean;
using Exception = System.Exception;
using String = System.String;

namespace Yorsh.Helpers
{
    public static class GaService
    {
        public static void TrackAppPage(String pageNameToTrack)
        {
            Rep.Instance.GaTracker.SetScreenName(pageNameToTrack);
            Rep.Instance.GaTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        public static void TrackAppEvent(String eventCategory, String eventToTrack)
        {
            var builder = new HitBuilders.EventBuilder();
            builder.SetCategory(eventCategory);
            builder.SetAction(eventToTrack);
            builder.SetLabel("AppEvent");

            Rep.Instance.GaTracker.Send(builder.Build());
        }

        public static void TrackAppException(Class activity, String method, Exception exception, Boolean isFatalException)
		{
            TrackAppException(activity.SimpleName, method, exception, isFatalException);
		}
        public static void TrackAppException(string activity, String method, Exception exception, Boolean isFatalException)
        {
            var builder = new HitBuilders.ExceptionBuilder();
            var exceptionMessageToTrack = string.Format("{0}, Method : {1}\nException type : {2}, Exception Message : {3}\nStack Trace : \n{4}", activity, method,
                exception.GetType(),exception.Message, exception.StackTrace);

            builder.SetDescription(exceptionMessageToTrack);
            builder.SetFatal(isFatalException);

            Rep.Instance.GaTracker.Send(builder.Build());
        }
        public static void TrackAppException(String activity, String method, string exceptionName, string message, Boolean isFatalException)
        {
            var builder = new HitBuilders.ExceptionBuilder();
            var exceptionMessageToTrack = string.Format("{0}, Method : {1}\nException Name : {2}\nMessage : \n{3}", activity, method,
                exceptionName, message);
            builder.SetDescription(exceptionMessageToTrack);
            builder.SetFatal(isFatalException);

            Rep.Instance.GaTracker.Send(builder.Build());
        }
    }
}


using Android.Content;
using Android.Views;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;

using OpenBacon;
using OpenBacon.Droid;

[assembly: ExportRenderer(typeof(ScrollableWebView), typeof(ScrollableWebViewRenderer))]
namespace OpenBacon.Droid
{
    class ScrollableWebViewRenderer : WebViewRenderer
    {
        public ScrollableWebViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            Parent.RequestDisallowInterceptTouchEvent(true);
            return base.DispatchTouchEvent(e);
        }
    }
}

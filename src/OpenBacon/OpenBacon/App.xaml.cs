using Reddit;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace OpenBacon
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = new NavigationPage(mainPage)
            {
                BarBackgroundColor = Color.FromHex("#0079D3"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

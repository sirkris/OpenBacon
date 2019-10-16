using Xamarin.Forms;

namespace OpenBacon.Grids
{
    public class BaconMenuItem
    {
        public Grid Grid { get; private set; }

        public BaconMenuItem(OpenBacon.BaconMenuItem baconMenuItem)
        {
            Grid = new Grid { Padding = 0, BackgroundColor = Color.FromHex("#0079D3") };

            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });

            Grid.Children.Add(
                new Image
                {
                    Source = baconMenuItem.ImageSrc + ".png",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Margin = 2
                }, 0, 1, 0, 2);

            Grid.Children.Add(
                new Label
                {
                    Text = baconMenuItem.Name, 
                    TextColor = Color.White,
                    FontAttributes = FontAttributes.Bold, 
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)), 
                    VerticalOptions = LayoutOptions.Start
                }, 1, 0);

            Grid.Children.Add(
                new Label
                {
                    Text = baconMenuItem.Detail, 
                    TextColor = Color.Silver,
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    VerticalOptions = LayoutOptions.Start
                }, 1, 1);
        }
    }
}

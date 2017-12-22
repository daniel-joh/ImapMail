using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppRoot : Page
    {
        public AppRoot()
        {
            this.InitializeComponent();

            this.Loaded += new RoutedEventHandler(AppRoot_Loaded);


        }

        void AppRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HelperUtils.AreSettingsAvailable())
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
                ContentFrame.Navigate(typeof(MainPage));


        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {

            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "mail")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
           
            CoreApplicationViewTitleBar titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
          
        }
 
        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitle.Margin = new Thickness(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset + 12, 8, 0, 0);
        }
        
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                switch (args.InvokedItem)
                {
                    case "Home":
                        ContentFrame.Navigate(typeof(MainPage));
                        break;

                    case "Mail":
                        OpenCreateMailPage();
                        break;

                    
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {

                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "home":
                        ContentFrame.Navigate(typeof(MainPage));
                        break;

                    case "Mail":
                        OpenCreateMailPage();
                        break;

                  
                }
            }
        }

        /// <summary>
        /// Opens CreateMailPage
        /// </summary>
        public async void OpenCreateMailPage()
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(CreateMailPage), null);
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

        }



    }
}

﻿using System;
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

namespace ImapMail
{
   
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
                        MailHandler.ReplyFlag = false;
                        MailHandler.ForwardFlag = false;
                        ContentFrame.Navigate(typeof(MainPage));
                        break;

                    case "Send Mail":                       
                        MailHandler.ReplyFlag = false;
                        MailHandler.ForwardFlag = false;
                        ContentFrame.Navigate(typeof(CreateMailPage));                    
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
                        MailHandler.ReplyFlag = false;
                        MailHandler.ForwardFlag = false;
                        ContentFrame.Navigate(typeof(MainPage));
                        break;

                    case "mail":
                        MailHandler.ReplyFlag = false;
                        MailHandler.ForwardFlag = false;
                        ContentFrame.Navigate(typeof(CreateMailPage));
                        break;
                }
            }
        } 
    }
}

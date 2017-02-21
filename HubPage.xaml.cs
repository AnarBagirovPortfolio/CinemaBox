using CinemaBox_for_WinRT.Common;
using CinemaBox_for_WinRT.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace CinemaBox_for_WinRT
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusbar.ForegroundColor = Windows.UI.Colors.Black;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            bool isDataDownloaded;
            try
            {
                var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
                this.DefaultViewModel["Groups"] = sampleDataGroups;
                isDataDownloaded = true;
            }
            catch
            {
                isDataDownloaded = false;
            }

            if (!isDataDownloaded)
            {
                await new MessageDialog("Ошибка подключения").ShowAsync();
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (((SampleDataItem)e.ClickedItem).URL != "")
            {
                var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
                App.GroupIndex = Convert.ToInt32(itemId.Substring(6, 1)) - 1;
                App.ItemIndex = Convert.ToInt32(itemId.Substring(15)) - 1;
                if (!Frame.Navigate(typeof(ItemPage_v2), itemId))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }            
        }

        private void SoonItemClick(object sender, ItemClickEventArgs e)
        {
            {
                var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
                App.GroupIndex = Convert.ToInt32(itemId.Substring(6, 1)) - 1;
                App.ItemIndex = Convert.ToInt32(itemId.Substring(15)) - 1;
                if (!Frame.Navigate(typeof(SoonItemPage), itemId))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://parkcinema.az/?lang=ru"));
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(PricePage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private async void OpenMainPageInWeb_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://parkcinema.az/?lang=ru"));
        }

        private void Reverse_Click(object sender, TappedRoutedEventArgs e)
        {
            Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI("0504242072", "Park Cinema");
        }

        private void OpenHallsPage(object sender, TappedRoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(HallsPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private async void AboutCinema_Click_from_HubPage(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.parkcinema.az/melumat?lang=ru"));
        }

        private void About_HubPage(object sender, TappedRoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(AboutPage)))
            {

            }
        }
    }
}

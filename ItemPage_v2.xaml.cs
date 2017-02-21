using CinemaBox_for_WinRT.Common;
using CinemaBox_for_WinRT.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CinemaBox_for_WinRT
{
    public sealed partial class ItemPage_v2 : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage_v2()
        {
            StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusbar.ForegroundColor = Windows.UI.Colors.Black;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        static string URL = "";

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
            var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
            this.InitializeComponent();
            URL = item.URL;

            if (!SampleDataSource.isItemDownloaded)
            {
                MessageDialog CurrentMessageDialog = new MessageDialog("Отсутсвует подключение", "Ошибка");

                CurrentMessageDialog.Commands.Add(new UICommand("Ok", new UICommandInvokedHandler(CurrentMessageDialogHandlers)));
                CurrentMessageDialog.Commands.Add(new UICommand("Обновить", new UICommandInvokedHandler(CurrentMessageDialogHandlers)));

                await CurrentMessageDialog.ShowAsync();
            }
        }

        private async void CurrentMessageDialogHandlers(IUICommand command)
        {
            var Actions = command.Label;

            switch (Actions)
            {
                case "Обновить":
                    var UpdatedInfo = await SampleDataSource.GetUpdatedItemAsync();
                    this.Image_Item_v2.Source = new BitmapImage(new Uri(UpdatedInfo.ImagePath));
                    this.Description_Item_v2.Text = UpdatedInfo.Description;
                    this.Subtitle_Item_v2.Text = UpdatedInfo.Subtitle;
                    this.Content_Item_v2.Text = UpdatedInfo.Content;
                    this.Schedule.Text = UpdatedInfo.ParkBulvarSchedule;
                    break;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
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
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void CallButton_ItemPage_v2_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI("0504242072", "Park Cinema");
        }

        private async void RefreshItemPage_v2_Click(object sender, RoutedEventArgs e)
        {
            var UpdatedInfo = await SampleDataSource.GetUpdatedItemAsync();
            this.Image_Item_v2.Source = new BitmapImage(new Uri(UpdatedInfo.ImagePath));
            this.Description_Item_v2.Text = UpdatedInfo.Description;
            this.Subtitle_Item_v2.Text = UpdatedInfo.Subtitle;
            this.Content_Item_v2.Text = UpdatedInfo.Content;
            this.Schedule.Text = UpdatedInfo.ParkBulvarSchedule;
        }

        private async void ViewWebPage_v2_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(URL));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(HallsPage)))
            {

            }
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(PricePage)))
            {

            }
        }

        private void About_ItemPage_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(AboutPage)))
            {

            }
        }

    }
}

using CinemaBox_for_WinRT.Common;
using CinemaBox_for_WinRT.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Windows.ApplicationModel.Appointments;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SoonItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        public SoonItemPage()
        {
            StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusbar.ForegroundColor = Windows.UI.Colors.Black;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        static string URL = "";
        static string Date = "";
        static string CinemaTitle = "";

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
            Date = item.Subtitle.ToLower();
            CinemaTitle = item.Title;

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
                    this.Image_SoonItem.Source = new BitmapImage(new Uri(UpdatedInfo.ImagePath));
                    this.Description_SoonItem.Text = UpdatedInfo.Description;
                    this.Subtitle_SoonItem.Text = UpdatedInfo.Subtitle;
                    this.Content_SoonItem.Text = UpdatedInfo.Content;
                    break;
            }
        }

        private async void RefreshSoonItemPage_Click(object sender, RoutedEventArgs e)
        {
            var UpdatedInfo = await SampleDataSource.GetUpdatedItemAsync();
            this.Image_SoonItem.Source = new BitmapImage(new Uri(UpdatedInfo.ImagePath));
            this.Description_SoonItem.Text = UpdatedInfo.Description;
            this.Subtitle_SoonItem.Text = UpdatedInfo.Subtitle;
            this.Content_SoonItem.Text = UpdatedInfo.Content;
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

        private async void ViewWebPage_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(URL));
        }

        private async void AddToCal_Click(object sender, RoutedEventArgs e)
        {
            DateTime myDateTime = ReturnDateTime(Date);
            var duration = new DateTimeOffset(myDateTime.AddDays(1)) - new DateTimeOffset(myDateTime);
            var appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
            var daysAppointments = await appointmentStore.FindAppointmentsAsync(myDateTime, duration);
            var daysAppointmentsSubjects = daysAppointments.Select(n => n.Subject);

            if (!daysAppointmentsSubjects.Contains("Премьера фильма \"" + CinemaTitle + "\""))
            {
                Appointment Premiere = new Appointment();
                Premiere.Subject = "Премьера фильма \"" + CinemaTitle + "\"";
                Premiere.StartTime = myDateTime;
                Premiere.AllDay = true;
                Premiere.Location = "Park Cinema";

                await appointmentStore.ShowAddAppointmentAsync(Premiere, new Rect());
            }
            else
            {
                await new MessageDialog("Напоминание ранее было добавлено в календарь").ShowAsync();
            }
        }

        static DateTime ReturnDateTime(string date)
        {
            int Day = Convert.ToInt32(Regex.Replace(date, @"[^\d]+", ""));
            int Month = 0;
            int Year = DateTime.Now.Year;
            if (date.Contains("январ")) Month = 1;
            else if (date.Contains("феврал")) Month = 2;
            else if (date.Contains("март")) Month = 3;
            else if (date.Contains("апрел")) Month = 4;
            else if (date.Contains("май") && date.Contains("мая")) Month = 5;
            else if (date.Contains("июн")) Month = 6;
            else if (date.Contains("июл")) Month = 7;
            else if (date.Contains("август")) Month = 8;
            else if (date.Contains("сентябр")) Month = 9;
            else if (date.Contains("октябр")) Month = 10;
            else if (date.Contains("ноябр")) Month = 11;
            else if (date.Contains("декабр")) Month = 12;
            if (new DateTime(Year, Month, Day) < DateTime.Now) Year = Year + 1;

            return new DateTime(Year, Month, Day);
        }

        private void About_SoonItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(AboutPage)))
            {

            }
        }
    }
}

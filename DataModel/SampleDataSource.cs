using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using HtmlAgilityPack;

namespace CinemaBox_for_WinRT.Data
{
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, String url, String parkbulvarschedule, String metroparkschedule)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.URL = url;
            this.ParkBulvarSchedule = parkbulvarschedule;
            this.MetroParkSchedule = metroparkschedule;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public string URL { get; private set; }
        public string ParkBulvarSchedule { get; private set; }
        public string MetroParkSchedule { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class Cinema
    {
        public string Name;
        public string Description;
        public string Image;
        public string URL;

        public Cinema(string WebPage)
        {
            Name = Regex.Match(WebPage, "(<a class=\"title\" href=\")(.*)(\">)(.*?)(</a>)").Groups[4].Value.Trim();
            Description = (App.MyLoader.GetString("Since") + " " + Regex.Match(WebPage, @"(?<=<b>)(.*)(?=</b>)").ToString().Trim()).Trim();
            Image = Regex.Match(WebPage, "(<img src=\")(.*?)(\" width)").Groups[2].Value.Trim();
            URL = Regex.Match(WebPage, "(data-href=\")(.*?)(\")").Groups[2].Value + App.MyLoader.GetString("SetLang");
        }

        public Cinema(string name, string description, string image, string url)
        {
            Name = name;
            Description = description;
            Image = image;
            URL = url;
        }
    }

    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static bool isMainPageLoaded = false;
        public static string MainPage = "";
        public static string MoviePage = "";

        public static string Remove(string str)
        {
            if (str.IndexOf("&") >= 0 && str.IndexOf(";") > 0)
            {
                switch (str.Substring(str.IndexOf("&"), str.IndexOf(";") - str.IndexOf("&") + 1))
                {
                    case "&nbsp;":
                        return Remove(str.Replace("&nbsp;", " "));
                    case "&laquo;":
                        return Remove(str.Replace("&laquo;", "\""));
                    case "&raquo;":
                        return Remove(str.Replace("&raquo;", "\""));
                    case "&mdash;":
                        return Remove(str.Replace("&mdash;", "-"));
                    case "&hellip;":
                        return Remove(str.Replace("&hellip;", "..."));
                    default:
                        return Remove(str.Replace(str.Substring(str.IndexOf("&"), str.IndexOf(";") - str.IndexOf("&") + 1), (Char.IsLetterOrDigit(str[str.IndexOf(";") + 1])) ? " " : ""));
                }
            }
            else
            {
                return str;
            }
        }

        static async Task LoadingMainPage()
        {
            if (isMainPageLoaded) return;
            try
            {
                var MyClient = new HttpClient();
                var Responce = await MyClient.GetAsync(App.MyLoader.GetString("URL"));
                MainPage = await Responce.Content.ReadAsStringAsync();
                isMainPageLoaded = true;
            }
            catch
            {

            }            
        }

        static async Task<string> LoadingMoviePage(string url)
        {
            try
            {
                var MyClient = new HttpClient();
                var Responce = await MyClient.GetAsync(url);
                return await Responce.Content.ReadAsStringAsync();
            }
            catch
            {
                return "";
            }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<List<string>> GetDescriptionContentAndTable(string URL)
        {
            var MyClient = new HttpClient();
            var Responce = await MyClient.GetAsync(URL);
            String WebPageSource = await Responce.Content.ReadAsStringAsync();

            List<string> Data = GetTable(WebPageSource);
            string Description = App.MyLoader.GetString("Country") + " " + Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexCountry")).ToString().Trim() + "\n" +
                App.MyLoader.GetString("Year") + " " + Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexYear")).ToString().Trim() + "\n" +
                App.MyLoader.GetString("Director") + " " + Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexDirector")).ToString().Trim() + "\n" +
                App.MyLoader.GetString("Genre") + " " + Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexGenre")).ToString().Trim() + "\n" +
                Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexDuration")).ToString().Trim() + "\n" +
                Regex.Match(WebPageSource, @App.MyLoader.GetString("RegexRating")).ToString().Trim();
            string Content = Regex.Replace(Remove(Regex.Match(WebPageSource, "(?<=<p class=\"description\">)(.*?)(?=</p>)", RegexOptions.Singleline).Value).Replace("\n", " ").Trim(), " +", " ");

            Data.Add(Description);
            Data.Add(Content);

            return Data;
        }

        public static bool isItemDownloaded = true;

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();

            if (_sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Description == "" && _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].URL != "")
            {
                StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusbar.ProgressIndicator.Text = App.MyLoader.GetString("Downloading");
                await statusbar.ProgressIndicator.ShowAsync();

                try
                {
                    string UniqueId = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].UniqueId;
                    string Title = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Title;
                    string Subtitle = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Subtitle;
                    string ImagePath = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].ImagePath;
                    string Description = "Some Description";
                    string Content = "Some Content";
                    string URL = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].URL;

                    List<string> SomeData = await GetDescriptionContentAndTable(URL);
                    string ParkBulvarSchedule = SomeData[0];
                    string MetroParkSchedule = SomeData[1];
                    Description = SomeData[2];
                    Content = SomeData[3];

                    _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex] = new SampleDataItem(UniqueId, Title, Subtitle, ImagePath, Description, Content, URL, ParkBulvarSchedule, MetroParkSchedule);

                    isItemDownloaded = true;
                }
                catch
                {
                    isItemDownloaded = false;
                }              
                
                await statusbar.ProgressIndicator.HideAsync();
            }

            return _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex];
        }

        public static async Task<SampleDataItem> GetUpdatedItemAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusbar.ProgressIndicator.Text = App.MyLoader.GetString("Downloading");
            await statusbar.ProgressIndicator.ShowAsync();

            string UniqueId = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].UniqueId;
            string Title = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Title;
            string Subtitle = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Subtitle;
            string ImagePath = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].ImagePath;
            string Description = "Some Description";
            string Content = "Some Content";
            string URL = _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].URL;

            try
            {
                List<string> SomeData = await GetDescriptionContentAndTable(URL);
                string ParkBulvarSchedule = SomeData[0];
                string MetroParkSchedule = SomeData[1];
                Description = SomeData[2];
                Content = SomeData[3];

                _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex] = new SampleDataItem(UniqueId, Title, Subtitle, ImagePath, Description, Content, URL, ParkBulvarSchedule, MetroParkSchedule);

                isItemDownloaded = true;
            }
            catch
            {

            }

            await statusbar.ProgressIndicator.HideAsync();
            return _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex];
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0) return;

            StatusBar statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusbar.ProgressIndicator.Text = App.MyLoader.GetString("Downloading");
            await statusbar.ProgressIndicator.ShowAsync();

            await LoadingMainPage();

            await statusbar.ProgressIndicator.HideAsync();

            if (isMainPageLoaded)
            {
                List<Cinema> TodayListItems = new List<Cinema>();
                List<Cinema> SoonListItems = new List<Cinema>();

                //Получение файла
                Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                string jsonText = await FileIO.ReadTextAsync(file);
                JsonObject jsonObject = JsonObject.Parse(jsonText);
                JsonArray jsonArray = jsonObject["Groups"].GetArray();

                string WebPageSourceForToday = MainPage.Substring(MainPage.LastIndexOf("today"), MainPage.LastIndexOf("soon") - MainPage.LastIndexOf("today"));
                string WebPageSourceForSoon = MainPage.Substring(MainPage.LastIndexOf("soon"), MainPage.Length - MainPage.LastIndexOf("soon"));
                Cinema SomeCinema;
                while (WebPageSourceForToday.IndexOf("<div class=\"preview-container\"") != -1)
                {
                    TodayListItems.Add(new Cinema(WebPageSourceForToday));
                    WebPageSourceForToday = WebPageSourceForToday.Substring(WebPageSourceForToday.IndexOf("data-show-faces") + 15);
                }
                while (WebPageSourceForSoon.IndexOf("<div class=\"preview-container\"") != -1)
                {
                    SomeCinema = new Cinema(WebPageSourceForSoon);
                    if (!TodayListItems.Contains(SomeCinema))
                    {
                        SoonListItems.Add(SomeCinema);
                    }
                    WebPageSourceForSoon = WebPageSourceForSoon.Substring(WebPageSourceForSoon.IndexOf("data-show-faces") + 15);
                }
                TodayListItems = TodayListItems.OrderBy(n => n.Name).ToList();

                //Обновление групп
                foreach (JsonValue groupValue in jsonArray)
                {
                    int Counter = 1;
                    JsonObject groupObject = groupValue.GetObject();
                    SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                                groupObject["Title"].GetString(),
                                                                groupObject["Subtitle"].GetString(),
                                                                groupObject["ImagePath"].GetString(),
                                                                groupObject["Description"].GetString());

                    if (group.UniqueId == "Group-1")
                    {
                        foreach (Cinema item in TodayListItems)
                        {
                            group.Items.Add(new SampleDataItem("Group-1 - Item-" + Counter.ToString(), item.Name,
                                item.Description, item.Image, "", "", item.URL, "", ""));
                            Counter = Counter + 1;
                        }
                        this.Groups.Add(group);
                    }
                    else if (group.UniqueId == "Group-2")
                    {
                        foreach (Cinema item in SoonListItems)
                        {
                            group.Items.Add(new SampleDataItem("Group-2 - Item-" + Counter.ToString(), item.Name,
                                item.Description, item.Image, "", "", item.URL, "", ""));
                            Counter = Counter + 1;
                        }
                        this.Groups.Add(group);
                        break;
                    }
                }
            }
            else
            {
                SampleDataGroup group = new SampleDataGroup("Group-1", "Group Title: 1", "Group subtitle: 1", "Assets/DarkGray.png", "");
                group.Items.Add(new SampleDataItem("Group-1-Item-1", App.MyLoader.GetString("ConnectionError"), "", "Assets/erroricon.png", "", "", "", "", ""));
                this.Groups.Add(group);
                group = new SampleDataGroup("Group-2", "Group Title: 2", "Group subtitle: 2", "Assets/LightGray.png", "");
                group.Items.Add(new SampleDataItem("Group-2-Item-1", App.MyLoader.GetString("ConnectionError"), "", "Assets/erroricon.png", "", "", "", "", ""));
                this.Groups.Add(group);
            }
        }

        static string GetCinemaName(string id)
        {
            if (id.IndexOf("cinema1") != -1) return "PARK BULVAR";
            if (id.IndexOf("cinema2") != -1) return "METRO PARK";
            if (id.IndexOf("cinema3") != -1) return "AMBURAN";
            if (id.IndexOf("cinema4") != -1) return "FLAME TOWERS";
            return id.Substring(0, id.IndexOf("d"));
        }

        static string GetTime(HtmlDocument document, string id)
        {
            return Regex.Replace(document.GetElementbyId(id).InnerText.Trim().Replace("\t", " ").Replace("\r\n", " ").Substring(3).Trim(), " +", " ");
        }

        static List<string> GetTable(string WebPageSource)
        {
            string MoviePageSource = Regex.Match(WebPageSource, "(<a name=\"schedule\">)(.*)(?=<div class=\"trailercon\">)", RegexOptions.Singleline).Value;
            string Schedule = null;
            HtmlDocument HTMLDocument = new HtmlDocument();
            HTMLDocument.LoadHtml(MoviePageSource);
            List<string> Days = Regex.Matches(MoviePageSource, "(?<=<span>)(.*)(?=</span>)").Cast<Match>().Select(n => n.Value).ToList();
            List<string> CinemasIDs = Regex.Matches(MoviePageSource, "(?<=<a href=\"#)(.*)(?=\" )").Cast<Match>().Select(n => n.Value).ToList();
            List<string> CinemasNames = CinemasIDs.Select(n => GetCinemaName(n)).ToList();
            List<string> Time = CinemasIDs.Select(n => GetTime(HTMLDocument, n)).ToList();

            //Приведение значений времен в нормальный вид и получение расписания
            for (int i = 0; i < Time.Count; i++)
            {
                foreach (var item in Regex.Matches(Time[i], @"[^0-9:]+[0-9: ^VIPvip]+").Cast<Match>().Select(n => Time[i].IndexOf(n.Value.Trim())))
                {
                    Time[i] = (item != 0) ? Time[i].Substring(0, item) + "\n\n" + Days.ElementAt(i) + " " + Time[i].Substring(item) : Days.ElementAt(i) + " " + Time[i];
                }

                Schedule += (i == 0 || CinemasNames[i] != CinemasNames[i - 1]) ? CinemasNames[i] + "\n\n" : null;
                Schedule += (i != Time.Count - 1) ? Time[i] + "\n\n" : Time[i] + "\n";
            }

            if (Schedule == null) Schedule = App.MyLoader.GetString("NoSessions");
            return new List<string>(new string[] { Schedule, null });
        }

        public void ResetGroups()
        {
            this._groups = new ObservableCollection<SampleDataGroup>();
        }
    }
}
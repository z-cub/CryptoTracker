﻿using CryptoTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using UWP.Core.Constants;
using UWP.Helpers;
using UWP.Services;
using UWP.Models;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace UWP.Views {
    public sealed partial class MainPage : Page {

        private int CurrentTabIndex = 0;
        readonly UISettings uiSettings = new UISettings();

        private string Redirect = "";
        private string taskName = "BackgroundTask";
        private string taskEntryPoint = "UWP.Background.Tasks";

        // ###############################################################################################
        public MainPage() {
            this.InitializeComponent();

            // Clear the current tile
            //TileUpdateManager.CreateTileUpdaterForApplication().Clear();            

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {
                var statusBar = StatusBar.GetForCurrentView();

                if (App._LocalSettings.Get<string>(UserSettings.Theme) == "Dark") {
                    statusBar.BackgroundColor = Color.FromArgb(255, 23, 23, 23); //31 31 31
                    statusBar.BackgroundOpacity = 1;
                    statusBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                } else {
                    statusBar.BackgroundColor = Color.FromArgb(255, 242, 242, 242); // 230
                    statusBar.BackgroundOpacity = 1;
                    statusBar.ForegroundColor = Color.FromArgb(255, 0, 0, 0);
                }

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }

            /// Subscribe to light/dark theme change event
            uiSettings.ColorValuesChanged += ColorValuesChanged;

            ExtendAcrylicIntoTitleBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.RegisterBackgroundTask();

            var param = e.Parameter.ToString();
            if (param.StartsWith("/tile-"))
                Redirect = param.Split("-")[1];

            switch (param) {
                case "/Portfolio":
                    NavView.SelectedItem = NavView.MenuItems[3];
                    break;
                case "/News":
                    NavView.SelectedItem = NavView.MenuItems[2];
                    break;
                default:
                    NavView.SelectedItem = NavView.MenuItems[0];
                    break;
            }
            ShowChangelog();
        }

        private async void RegisterBackgroundTask() {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy ||
                backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed) {
                foreach (var task in BackgroundTaskRegistration.AllTasks) {
                    if (task.Value.Name == taskName) {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = taskEntryPoint;
                taskBuilder.SetTrigger(new TimeTrigger(15, false));
                var registration = taskBuilder.Register();
            }
        }

        /// <summary>
        /// Show a notification with the changelog to new users.
        /// </summary>
        private async void ShowChangelog() {
            var v = Package.Current.Id.Version;
            var version = $"{v.Major}.{v.Minor}.{v.Build}";
            
            var lastVersion = App._LocalSettings.Get<string>(UserSettings.LastVersion);
            if (version == lastVersion)
                return;
            vm.InfoBarTitle = $"Welcome to CryptoTracker v{version}";
            vm.InfoBarMessage = "New in this version:\n";

            vm.InfoBarMessage = Changelogs.FormatChangelog(Changelogs.CurrentChangelog);

            vm.InfoBarOpened = true;
            App._LocalSettings.Set(UserSettings.LastVersion, version);
        }

        private void ColorValuesChanged(UISettings sender, object args) {
            if ((App._LocalSettings.Get<string>(UserSettings.Theme) == "Windows")) {
                var color = uiSettings.GetColorValue(UIColorType.Background);
                switch (color.ToString()) {
                    case "#FF000000":
                        ChangeTheme("Dark");
                        break;
                    case "#FFFFFFFF":
                        ChangeTheme("Light");
                        break;
                }
            }
        }

        private async void ChangeTheme(string theme) {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                ((Frame)Window.Current.Content).RequestedTheme = theme.Equals("Dark") ? ElementTheme.Dark : ElementTheme.Light;
            });
        }

        /// Extend acrylic into the title bar. 
        private void ExtendAcrylicIntoTitleBar() {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 150, 150, 150);
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Color.FromArgb(127, 150, 150, 150);
            Window.Current.SetTitleBar(CustomAppTitleBar);
        }

        // #########################################################################################
        internal async void UpdateButton_Click(object sender, RoutedEventArgs e) {
            SyncIcon.Visibility = Visibility.Collapsed;
            switch (ContentFrame.SourcePageType.Name) {
                case "CoinDetails":
                    var p0 = (CoinDetails)ContentFrame.Content;
                    p0.UpdatePage();
                    break;
                case "Home":
                    var p2 = (Home)ContentFrame.Content;
                    await p2.UpdateAllCards();
                    break;
                case "Portfolio":
                    var p1 = (Portfolio)ContentFrame.Content;
                    p1.UpdatePortfolio();
                    break;
                case "News":
                    var p3 = (News)ContentFrame.Content;
                    p3.UpdateNews();
                    break;
            }

            SyncIcon.Visibility = Visibility.Visible;
        }

        /// #########################################################################################
        ///  Navigation View
        private void NavView_Sync_Tapped(object sender, TappedRoutedEventArgs e) {
            UpdateButton_Click(sender, e);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            string source = "null";
            string selected = "null";

            if (args.IsSettingsSelected)
                return;

            if (((Frame)sender.Content).SourcePageType != null)
                source = (((Frame)sender.Content).SourcePageType).Name;
            
            selected = ((ContentControl)args.SelectedItem).Content.ToString();

            /// With ItemInvoked the navigation may have already been done
            /// so not to navigate twice, check the current page
            if (source != selected)
                PagesNavigation(selected);
        }
        
        private async void PagesNavigation(string toPage, bool samePage = false) {
            var dir = new SlideNavigationTransitionInfo();
            var page = typeof(Page);
            switch (toPage) {
                case "Home":
                    dir.Effect = SlideNavigationTransitionEffect.FromLeft;
                    page = typeof(Home);
                    CurrentTabIndex = 0;
                    break;
                case "Top 100":
                    dir.Effect = (CurrentTabIndex > 1) ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight;
                    page = typeof(Top100);
                    CurrentTabIndex = 1;
                    break;
                case "News":
                    dir.Effect = (CurrentTabIndex > 2) ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight;
                    page = typeof(News);
                    CurrentTabIndex = 2;
                    break;
                case "Portfolio":
                    dir.Effect = (CurrentTabIndex > 3) ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight;
                    page = typeof(Portfolio);
                    CurrentTabIndex = 3;
                    break;

                case "Settings":
                    dir.Effect = SlideNavigationTransitionEffect.FromRight;
                    page = typeof(Settings);
                    CurrentTabIndex = 4;
                    break;
            }

            ///if it's the same page, override the default animation for one from the Bottom
            if (samePage)
                dir.Effect = SlideNavigationTransitionEffect.FromBottom;

            /// Redirect to a coin's page from a Live Tile (and clear Redirect as it's a one-time)
            if (toPage == "Home" && Redirect != "") {
                ContentFrame.Navigate(typeof(CoinDetails), Redirect, dir);
                Redirect = "";
            } else if (toPage == "Settings") {
                var settings = new SettingsDialog();
                await settings.ShowAsync();
            } else
                ContentFrame.Navigate(page, null, dir);
        }

        /// Hide NavigationView if navigating to the Compact Overlay view
        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e) {
            var toPage = (e.SourcePageType).Name;
            App.CurrentPage = toPage;
            NavView.IsPaneVisible = (toPage == "CoinCompact") ? false : true;
            CustomAppTitleBar.Margin = (toPage == "CoinCompact") ? new Thickness(46, 0, 0, 0) : new Thickness(0);
        }

        /// #######################################################################################
        ///  AutoSuggest-Box
        private void AutoSuggestBox_TextChanged(AutoSuggestBox box, AutoSuggestBoxTextChangedEventArgs args) {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                box.ItemsSource = FilterCoins(box);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
            if (args.ChosenSuggestion != null)
                ContentFrame.Navigate(typeof(CoinDetails), ((SuggestionCoin)args.ChosenSuggestion).Symbol);
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e) {
            AutoSuggestBox box = sender as AutoSuggestBox;
            box.ItemsSource = FilterCoins(box);
        }

        private void AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e) {
            //CoinAutoSuggestBox.Text = "";
            if (((Frame)Window.Current.Content).ActualWidth < 720)
                CoinAutoSuggestBox.Visibility = Visibility.Collapsed;
        }

        /// #######################################################################################
        private List<SuggestionCoin> FilterCoins(AutoSuggestBox box) {
            var filtered = App.coinList.Where(x =>
                x.symbol.Contains(box.Text, StringComparison.InvariantCultureIgnoreCase) ||
                x.name.Contains(box.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            List<SuggestionCoin> list = new List<SuggestionCoin>();
            foreach (CoinBasicInfo coin in filtered) {
                list.Add(new SuggestionCoin {
                    Icon = IconsHelper.GetIcon(coin.symbol),
                    Name = coin.name,
                    Symbol = coin.symbol
                });
            }
            return list;
        }

        /// #######################################################################################
        ///  Search button
        private void NavView_Search_Tapped(object sender, TappedRoutedEventArgs e) {
            CoinAutoSuggestBox.Visibility = Visibility.Visible;
            CoinAutoSuggestBox.Focus(FocusState.Programmatic);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            string selected;
            string source = (((Frame)sender.Content).SourcePageType).Name;
            
            selected = ((ContentControl)sender.SelectedItem).Content?.ToString();
            
            if (selected == null)
                selected = (args.IsSettingsInvoked) ? "Settings" : "Null";

            // CoinDetails and the WebView are loaded on the current NavigationViewTab
            // so the user can go back by clicking the same tab itself
            if (source == "CoinDetails" || source == "WebVieww" || selected == "Settings")
                PagesNavigation(selected, true);
            
            
        }

    }
}
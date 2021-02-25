﻿using CryptoTracker.APIs;
using CryptoTracker.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CryptoTracker.Views {

	public sealed partial class Top100 : Page {

        public Top100() {
            this.InitializeComponent();
            InitPage();
        }

        // #########################################################################################
        private async void InitPage() {
            vm.GlobalStats = await CoinGecko.GetGlobalStats();

            var topCoins = await CryptoCompare.GetTop100();
            vm.Top100cards = topCoins;
			//topCoins.Sort((x, y) => y.MarketCapRaw.CompareTo(x.MarketCapRaw));
        }

        // #########################################################################################
        private void ListView_Click(object sender, ItemClickEventArgs e) {
            top100ListView.PrepareConnectedAnimation("toCoinDetails", e.ClickedItem, "listView_Element");

			var crypto = ((Top100card)e.ClickedItem).CoinInfo.Name;
			this.Frame.Navigate(typeof(CoinDetails), crypto);
		}

        private void Pin_click(object sender, RoutedEventArgs e) {
            //string c = ((Top100coin)((FrameworkElement)((FrameworkElement)sender).Parent).DataContext).Symbol;
            var card = ((Top100card)((FrameworkElement)sender).DataContext);
            var c = card.CoinInfo.Name;
            if (!App.pinnedCoins.Contains(c)) {
                App.pinnedCoins.Add(c);
                inAppNotification.Show(c + " pinned to home.", 2000);
            } else {
                App.pinnedCoins.Remove(c);
                inAppNotification.Show(c + " unpinned from home.", 2000);
            }
            card.CoinInfo.FavIcon = card.CoinInfo.FavIcon.Equals("\uEB51") ? "\uEB52" : "\uEB51";

            // Update pinnedCoin list
            App.UpdatePinnedCoins();
        }
    }
}

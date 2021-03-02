﻿using CryptoTracker.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;

namespace CryptoTracker.ViewModels {
    class SettingsViewModel : ObservableRecipient {
		private ObservableCollection<PurchaseModel> purchaseList = new ObservableCollection<PurchaseModel>();
		public ObservableCollection<PurchaseModel> PurchaseList {
			get => purchaseList;
			set => SetProperty(ref purchaseList, value);
		}

		public void InAppNotification(string title, string message = "", bool temporary = true) {
			var tuple = new Tuple<string, string, bool>(title, message, temporary);
			Messenger.Send(new NotificationMessage(tuple));
		}
	}
}

﻿using System.Collections.Generic;

namespace CryptoTracker.Constants {
	/// <summary>
	/// Class with key constants for user settings
	/// </summary>
	public class UserSettingsConstants {
		public const string Theme = "Theme";
		public const string Currency = "Currency";
		public const string PinnedCoins = "Pinned";
		public const string IsNewUser = "NewUser";
		public const string CoinListDate = "CoinListDate";

		/// <summary>
		/// Default settings
		/// </summary>
		public static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>(){
			{ Theme, "Windows" },
			{ Currency, "EUR" },
			{ PinnedCoins, "BTC|ETH|LTC|XRP" },
			{ CoinListDate,  0 },
			{ IsNewUser, true }
		};
	}
}

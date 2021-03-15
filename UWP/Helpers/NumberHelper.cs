﻿using System;

namespace UWP.Helpers {
    class NumberHelper {
        public static string AddUnitPrefix(double num) {
            if (num > 999999999) {
                return num.ToString("0,,,.##B", App.UserCulture);
            }
            else if (num > 999999) {
                return num.ToString("0,,.##M", App.UserCulture);
            }
            else if (num > 999) {
                num = Math.Round(num, 2);
                return num.ToString(App.UserCulture);
            }
            else {
                num = Math.Round(num, 3);
                return num.ToString(App.UserCulture);
            }
        }

        public static double Rounder(double price) {
            if (Math.Abs(price) > 99)
                return Math.Round(price, 2);
            else if (Math.Abs(price) > 10)
                return Math.Round(price, 3);
            else if (Math.Abs(price) > 1)
                return Math.Round(price, 4);
            else
                return Math.Round(price, 6);
		}
    }
}
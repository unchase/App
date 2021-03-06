﻿using System.Globalization;
using System.Threading;
using DotNetRu.Clients.Portable.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(DotNetRu.Droid.Localize))]

namespace DotNetRu.Droid
{
    public class Localize : ILocalize
    {
        public void SetLocale(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public CultureInfo GetCurrentCultureInfo()
        {           
            var androidLocale = Java.Util.Locale.Default;
            var language = androidLocale.ToString().ToLower().Contains("ru") ? "ru" : "en";
            return new CultureInfo(language);
        }
    }
}
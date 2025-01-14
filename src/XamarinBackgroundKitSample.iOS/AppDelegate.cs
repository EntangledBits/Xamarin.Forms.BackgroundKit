﻿using Foundation;
using Rg.Plugins.Popup;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinBackgroundKit.iOS;

namespace XamarinBackgroundKitSample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Popup.Init();
            BackgroundKit.Init();

            Forms.SetFlags("Visual_Experimental", "CollectionView_Experimental");
            Forms.Init();
            FormsMaterial.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}

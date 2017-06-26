using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Utilz.Controlz
{
    public static class ResourcesReader
    {
        public static readonly double AppBarButtonWidth;
        public static readonly double AppBarButtonEllipseDiametre;
        static ResourcesReader()
        {
            object bw;
            if (Application.Current.Resources.TryGetValue("AppBarButtonWidth", out bw) && bw.GetType() == typeof(double))
                AppBarButtonWidth = Convert.ToDouble(bw);
            else AppBarButtonWidth = 68.0;

            object ed;
            if (Application.Current.Resources.TryGetValue("AppBarButtonEllipseDiametre", out ed) && ed.GetType() == typeof(double))
                AppBarButtonEllipseDiametre = Convert.ToDouble(ed);
            else AppBarButtonEllipseDiametre = 40.0;
        }
    }
    public class AppBarButtonReallyCompact : AppBarButton
    {
        public AppBarButtonReallyCompact() : base()
        {
            Loading += OnLoading;
        }

        private void OnLoading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            Width = IsCompact ? ResourcesReader.AppBarButtonEllipseDiametre : ResourcesReader.AppBarButtonWidth;
        }
    }

    public class AppBarToggleButtonReallyCompact : AppBarToggleButton
    {
        public AppBarToggleButtonReallyCompact() : base()
        {
            Loading += OnLoading;
        }
        private void OnLoading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            Width = IsCompact ? ResourcesReader.AppBarButtonEllipseDiametre : ResourcesReader.AppBarButtonWidth;
        }
    }
}
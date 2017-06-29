using Windows.UI.Xaml;

namespace Utilz
{
    public static class ResourceGetter
    {
        public static object Get(FrameworkElement frameworkElement, string name)
        {
            object result;
            frameworkElement.Resources.TryGetValue(name, out result);
            return result;
        }
    }
}

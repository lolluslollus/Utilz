
namespace Utilz
{
    public sealed class LolloMath
    {
        public static void Swap(ref int a, ref int b)
        {
            int swap = a;
            a = b;
            b = swap;
        }
        public static void Swap(ref double a, ref double b)
        {
            double swap = a;
            a = b;
            b = swap;
        }
    }
}

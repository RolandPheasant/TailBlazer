using DynamicData.Kernel;

namespace System
{
    public static class ParseEx
    {
        public static Optional<bool> ParseBool(this string source)
        {
            bool result;
            if (bool.TryParse(source, out result))
                return result;

            return Optional.None<bool>();
        }

        public static Optional<decimal> ParseDecimal(this string source)
        {
            decimal result;
            if (decimal.TryParse(source, out result))
                return result;

            return Optional.None<decimal>();
        }

        public static Optional<double> ParseDouble(this string source)
        {
            double result;
            if (double.TryParse(source, out result))
                return result;

            return Optional.None<double>();
        }

        public static Optional<int> ParseInt(this string source)
        {
            int result;
            if (int.TryParse(source, out result))
                return result;

            return Optional.None<int>();
        }


        public static Optional<T> ParseEnum<T>(this string source)
            where T:struct
        {
            T result;
            if (Enum.TryParse(source, out result))
                return result;

            return Optional.None<T>();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoModificationManager4.Misc
{
    public static class StringExtension
    {
        public static string PutTogether(IEnumerable<string> input, char separator)
        {
            string str1 = "";
            foreach (string str2 in input)
                str1 = str1 + str2 + separator;
            return str1.Trim(separator);
        }

        public static string Short(this string str, int count)
        {
            if (str.Length > count)
                return str.Remove(count);
            return str;
        }

        public static string PutTogetherComma(IEnumerable<string> input)
        {
            string str1 = "";
            foreach (string str2 in input)
                str1 = str1 + str2 + ", ";
            return str1.Trim(',', ' ');
        }

        public static string PutTogetherReversed(IEnumerable<string> input, char separator)
        {
            string str1 = "";
            foreach (string str2 in Enumerable.Reverse(input))
                str1 = str1 + str2 + separator;
            return str1.Trim(separator);
        }

        public static string MakeUnique(string filename, string extension, Func<string, bool> condition)
        {
            int num = 0;
            string str;
            for (str = ""; condition(filename + str + extension); str = num.ToString())
                ++num;
            return filename + str + extension;
        }

        public static string Replace(this string str, string[] replacedstrings, string by)
        {
            foreach (string oldValue in replacedstrings)
                str = str.Replace(oldValue, by);
            return str;
        }

        public static string Replace(this string str, char[] replacedchars, string by)
        {
            foreach (char ch in replacedchars)
                str = str.Replace(ch.ToString(), by);
            return str;
        }

        public static string Extend(this string str, char by, int destinationlenght)
        {
            for (int length = str.Length; length < destinationlenght; ++length)
                str += by;
            return str;
        }
    }
}

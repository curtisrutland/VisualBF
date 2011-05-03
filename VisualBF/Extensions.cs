using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VisualBF {
    public static class Extensions {
        public static int FindBalancingBrace(this string s, int indexToBalance) {
            int matches = 1;
            for (int i = indexToBalance + 1; i < s.Length; i++) {
                if (s[i] == '[') ++matches;
                else if (s[i] == ']')
                    if (--matches <= 0)
                        return i;
            }
            return -1;
        }
    }
}

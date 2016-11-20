using System;
using System.Globalization;
using System.Linq;

class C
{
    static void Main()
    {
        Console.WriteLine("<lcids>");
        int prev_lcid = -1;
        var all = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(l => l.LCID);
        foreach (var c in all)
        {
            if (c.ThreeLetterWindowsLanguageName == "ZZZ")
                continue;

            if (c.LCID == CultureInfo.InvariantCulture.LCID)
                continue;

            if (prev_lcid == c.LCID)
            {
                switch (c.LCID)
                {
                    case 4:
                    case 0x7C04:
                        break;
                    default:
                        continue;
                }

            }

            prev_lcid = c.LCID;

            Console.WriteLine(
                $"  <lcid name=\"{ c.Name.Replace('-', '_') }\" id=\"{ "0x" + c.LCID.ToString("X4") }\" parent=\"{ "0x" + c.Parent.LCID.ToString("X4") }\"" +
                $" iso2=\"{ c.TwoLetterISOLanguageName }\" iso3=\"{ c.ThreeLetterISOLanguageName }\" win=\"{ c.ThreeLetterWindowsLanguageName }\" />");
        }

        Console.WriteLine("</lcids>");

        return;
    }
}

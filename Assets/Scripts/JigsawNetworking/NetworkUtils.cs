using System;
using System.IO;
using System.Net;
using UnityEngine;

public static class NetworkUtils
{
    public static string GetPublicIPAddress()
    {
        // https://stackoverflow.com/questions/3253701/get-public-external-ip-address/45242105

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");
        request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command
        request.Method = "GET";

        string publicIPAddress;
        using (WebResponse response = request.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                publicIPAddress = reader.ReadToEnd();
            }
        }
        return publicIPAddress.Replace("\n", "");
    }


    public static string IPv4toHex(string IPv4)
    {
        string[] pieces = IPv4.Split('.');
        string ret = "";
        for (int i = 0; i < pieces.Length; ++i)
        {
            ret += string.Format("{0:X2}", Convert.ToUInt16(pieces[i]));
            if ((i & 1) != 0 && (i != pieces.Length - 1))
            {
                ret += " ";
            }
        }
        return ret;
    }


    public static string HexToIPv4(string Hex)
    {
        string ret = "";
        for (int i = 0; i < Hex.Length; i += 2)
        {
            if (Hex[i] == ' ')
            {
                ++i;
            }
            int val = int.Parse(Hex.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            ret += val.ToString();
            if (i < Hex.Length - 2)
            {
                ret += ".";
            }
        }
        return ret;
    }


    public static bool IsValidHexAddr(string Hex)
    {
        if (Hex.Length != 8)
        {
            return false;
        }
        for (int i = 0; i < 8; ++i)
        {
            char c = Hex[i];
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
            {
                continue;
            }
            return false;
        }
        return true;
    }
}

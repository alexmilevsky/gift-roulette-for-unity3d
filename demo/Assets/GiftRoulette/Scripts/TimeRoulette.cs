using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Net.Sockets;

public class TimeRoulette : MonoBehaviour {
    
    DateTime savedTime;
    DateTime serverTime;

    static string[] NtpServers = { "time.windows.com", "pool.ntp.org", "time-a.nist.gov" };

    public bool IsCanContinue
    {
        get
        {
            if (PlayerPrefs.HasKey("RouletteSavedTime"))
            {
                string temp = PlayerPrefs.GetString("RouletteSavedTime");
                savedTime = DateTime.Parse(temp);
            }
            else
            {
                savedTime = DateTime.MinValue;
            }

            serverTime = GetNetworkTime();

            return savedTime < serverTime;
        }
    }

    public string TimeLeft()
    {
        if (savedTime > serverTime)
        {
            serverTime = serverTime.AddSeconds(Time.deltaTime);

            int h = (savedTime - serverTime).Hours;
            int m = (savedTime - serverTime).Minutes;
            int s = (savedTime - serverTime).Seconds;

            return String.Format("{0}:{1}:{2}", h, m, s);
        }
        else 
            return null;
    }  

    public void Save()
    {
        savedTime = serverTime.AddHours(2);
        PlayerPrefs.SetString("RouletteSavedTime", savedTime.ToString());
    }

    public static DateTime GetNetworkTime()
    {
        //**UTC** time
        //var 
        DateTime networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        // NTP message size - 16 bytes of the digest (RFC 2030)
        var ntpData = new byte[48];

        //Setting the Leap Indicator, Version Number and Mode values
        ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

        //Select time server
        string ntpServer = NtpServers[0];

        //Setting adresses list
        var addresses = Dns.GetHostEntry(ntpServer).AddressList;

        //The UDP port number assigned to NTP is 123
        var ipEndPoint = new IPEndPoint(addresses[0], 123);

        //NTP uses UDP
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            //Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;
            //Connecting, sending, recieving data
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
        }

        //Offset to get to the "Transmit Timestamp" field (time at which the reply 
        //departed the server for the client, in 64-bit timestamp format."
        const byte serverReplyTime = 40;

        //Get the seconds part
        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

        //Get the seconds fraction
        ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

        //Convert From big-endian to little-endian
        intPart = SwapEndianness(intPart);
        fractPart = SwapEndianness(fractPart);

        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        return networkDateTime = networkDateTime.AddMilliseconds((long)milliseconds);
    }

    // stackoverflow.com/a/3294698/162671
    static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                      ((x & 0x0000ff00) << 8) +
                      ((x & 0x00ff0000) >> 8) +
                      ((x & 0xff000000) >> 24));
    }

    public static DateTime GetNistTime()
    {
        DateTime dateTime = new DateTime();

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(response.GetResponseStream());
                string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                double milliseconds = Convert.ToInt64(time) / 1000.0;
                dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
            }
        }
        catch { }

        return dateTime;
    }

    public static DateTime GetNistTime1()
    {
        DateTime time = new DateTime();
        try
        {
            var client = new System.Net.Sockets.TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                var utcDateTimeString = response.Substring(7, 17);
                time = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
                return time;
            }
        }
        catch { return time; }
    }
}

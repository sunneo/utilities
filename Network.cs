using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using System.Collections.Specialized;
namespace Utilities
{
    public class Network
    {
        public const int TEST_CONNECTION_RESPONSE_TIMEOUT_PING_MS = 5000; // timeout in ms for 'ping'
        public const int TEST_CONNECTION_RESPONSE_TIMEOUT_MS = 10000; // timeout in ms for inquire
        public const int TEST_CONNECTION_PING_RETRY_COUNT = 5; // ping retry count
        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern UInt32 DnsFlushResolverCache();
        public static void FlushDnsCache()
        {
            UInt32 result = DnsFlushResolverCache();
        }
        private static bool IsExceptionGeneralNetworkFailure(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex.InnerException is Win32Exception)
                {
                    if (unchecked((uint)ex.InnerException.HResult) == 0x80004005)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool HandleExceptionForGeneralFailure(out String strResponse, Exception ex, String strCmd)
        {
            strResponse = "";
            return false;
        }
        public static Bitmap GetImageStream(String url)
        {
            Bitmap bmp = null;
            HttpWebRequest wreq;
            HttpWebResponse wresp;
            Stream mystream;

            mystream = null;
            wresp = null;
            try
            {
                for (int i = 0; i < 3; ++i)
                {
                    try
                    {
                        
                        wreq = (HttpWebRequest)WebRequest.Create(url);
                        wreq.AllowWriteStreamBuffering = true;


                        wresp = (HttpWebResponse)wreq.GetResponse();

                        if ((mystream = wresp.GetResponseStream()) != null)
                        {
                            bmp = (Bitmap)Bitmap.FromStream(mystream);
                            break;
                        }
                    }
                    catch (Exception ee)
                    {
                        if (mystream != null)
                            mystream.Close();

                        if (wresp != null)
                            wresp.Close();
                    }
                }
                return bmp;
            }
            finally
            {

            }
        }

        public static bool GetWebResponseWithStatus(out String strResponse, string strCmd, Int32 timeout = 30000, Encoding encoding=null, CookieContainer cookie=null,bool post=false)
        {
            strResponse = string.Empty;
            
            try
            {

                HttpWebRequest request = null;
                String url = strCmd;
                String query = "";
                if (post)
                {
                    int idxOfQuery=url.IndexOf('?');
                    if (idxOfQuery > -1)
                    {
                        url = url.Substring(0, idxOfQuery);
                        query =strCmd.Substring(idxOfQuery + 1);
                    }
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                }
                else
                {
                     request = (HttpWebRequest)WebRequest.Create(strCmd);
                }
                request.Timeout = timeout;
                if (cookie != null)
                {
                    request.CookieContainer = cookie;
                }
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate { return true; };
                if(strCmd.StartsWith("https:"))
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if(post)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Timeout = timeout;
                    NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(query);
                    byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(byteArray, 0, byteArray.Length);
                    }//end using
                }
               
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;
                    if (response.CharacterSet == null || response.CharacterSet == "")
                    {
                        if (encoding != null)
                        {
                            readStream = new StreamReader(receiveStream, encoding);
                        }
                        else
                        {
                            readStream = new StreamReader(receiveStream);
                        }
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    strResponse = readStream.ReadToEnd();
                    readStream.Close();
                }
                return true;
            }
            catch (WebException ex)
            {
                if (HandleExceptionForGeneralFailure(out strResponse, ex, strCmd))
                {
                    return !String.IsNullOrEmpty(strResponse);
                }
                strResponse = ex.Status.ToString() + " ";
                strResponse += ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                if (HandleExceptionForGeneralFailure(out strResponse, ex, strCmd))
                {
                    return !String.IsNullOrEmpty(strResponse);
                }
                strResponse = ex.Message;
                return false;
            }

            return true;
        }
        public static bool CanPing(String address, bool UseBusyWaiting = true, bool immediately = false, String fakeString = "")
        {
            DateTime dtNow = DateTime.Now;
            try
            {
                FlushDnsCache();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            int retryCount = TEST_CONNECTION_PING_RETRY_COUNT;
            int timeout0 = TEST_CONNECTION_RESPONSE_TIMEOUT_MS;
            if (immediately)
            {
                retryCount = 1;
                timeout0 = 1000;
            }
            for (int i = 0; i < retryCount; ++i)
            {
                if (DateTime.Now.Subtract(dtNow).TotalMilliseconds > timeout0)
                {
                    break;
                }
                try
                {
                    Ping pingSender = new Ping();
                    PingOptions options = new PingOptions();

                    // Use the default Ttl value which is 128,
                    // but change the fragmentation behavior.
                    options.DontFragment = true;
                    // Create a buffer of 32 bytes of data to be transmitted.
                    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = TEST_CONNECTION_RESPONSE_TIMEOUT_PING_MS;
                    if (immediately)
                    {
                        timeout = 1000;
                    }
                    PingReply reply = pingSender.Send(address, timeout, buffer, options);
                    bool ret = reply.Status == IPStatus.Success;
                    if (!ret)
                    {
                        Console.WriteLine("CanPing(): Ping Status:" + reply.Status.ToString());
                    }
                    if (ret)
                    {
                        if (!String.IsNullOrEmpty(fakeString))
                        {
                            Tracer.D("Ping " + fakeString + " successfully");
                        }
                        else
                        {
                            Tracer.D("Ping " + address + " successfully");
                        }
                        return ret;
                    }

                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
                if (UseBusyWaiting)
                {
                    for (int busyWait = 0; busyWait < 32; ++busyWait)
                    {
                        Thread.Sleep(32);
                        Application.DoEvents();
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            if (!String.IsNullOrEmpty(fakeString))
            {
                Tracer.D("Ping " + fakeString + " failed");
            }
            else
            {
                Tracer.D("Ping " + address + " failed");
            }
            return false;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        public static bool GetWebResponseWithStatus(out String strResponse, string strCmd, Int32 timeout = 30000, Encoding encoding=null)
        {
            strResponse = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strCmd);
                request.Timeout = timeout;
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate { return true; };
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

using System.Threading;
using System;
using System.Collections.Generic;
using RestSharp;
using HtmlAgilityPack;
using System.Net;
using System.Runtime.InteropServices;
using FluentScheduler;
using Newtonsoft.Json;

namespace TestUrl
{
    class Program
    {
        private static readonly AutoResetEvent _closingEvent = new AutoResetEvent(false);
        static string cookie = Extensions.GetEnvironmentVariable("cookie");
        static string email = Extensions.GetEnvironmentVariable("email");
        static string passwd = Extensions.GetEnvironmentVariable("passwd");
        static RestClient client = new RestClient();
        const string Version = "1.0";
        static Dictionary<string, string> dic = new Dictionary<string, string>(){
            {"content-type", "application/x-www-form-urlencoded; charset=UTF-8"},
            {"origin", "https://ggok.xyz"},
            {"referer", "https://ggok.xyz/auth/login"},
            {"user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36 Edg/99.0.1150.46"},
            {"authority", "ggok.xyz"}
        };
        #region 请求方法
        static void CheckIn()
        {
            ErgodicAction(() =>
            {
                Console.WriteLine("开始签到");
                RestRequest reques = new RestRequest("https://ggok.xyz/user/checkin", method: Method.Post);
                reques.AddHeaders(dic);
                var res = client.PostAsync<Result>(reques).Result;
                System.Console.WriteLine(res.msg);
                return true;
            }, 5);
        }
        static void CheckExpirationTimeAndAddSchedule()
        {
            ErgodicAction(() =>
            {
                RestRequest reques = new RestRequest("https://ggok.xyz/user");
                reques.AddHeaders(dic);
                var res = client.GetAsync(reques).Result.Content;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);
                var nodes = doc.DocumentNode.SelectNodes("//dd/i[@class='icon icon-md']");
                string dateTime = nodes[0].NextSibling.InnerText.Replace("&nbsp;", "");
                Console.WriteLine($"过期时间:{dateTime}");
                DateTime expireTime = Convert.ToDateTime(dateTime);
                expireTime = expireTime.AddSeconds(3);
                Console.WriteLine($"下次执行时间:{expireTime:yyyy-MM-dd HH:mm:ss}");
                JobManager.AddJob(Buy, s => s.ToRunOnceAt(expireTime));
                return true;
            });

        }
        static void Buy()
        {

            ErgodicAction(() =>
            {
                RestRequest reques = new RestRequest("https://ggok.xyz/user/buy", Method.Post);
                dic["referer"] = "https://ggok.xyz/user/shop";
                reques.AddHeaders(dic);
                reques.AddObject(new { coupon = "", shop = "8", autorenew = "1", disableothers = "1" });
                var res = client.PostAsync<Result>(reques).Result;
                Console.WriteLine(res.msg);
                CheckExpirationTimeAndAddSchedule();
                return true;
            });


        }
        static void Login(string email, string passwd)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Thread.Sleep(2000);
                    RestRequest reques = new RestRequest("https://ggok.xyz/auth/login", method: Method.Post);
                    reques.AddHeaders(dic);
                    reques.AddObject(new { email, passwd });
                    var res = client.PostAsync(reques).Result;
                    client.CookieContainer.Add(res?.Cookies);
                    var json = JsonConvert.DeserializeObject<Result>(res?.Content);
                    Console.WriteLine(json.msg);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
            }

        }
        #endregion
        static void Main(string[] args)
        {
            #region 初始化请求环境
            Console.WriteLine($"当前版本:{Version}");
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            if (email.IsEmpty() || passwd.IsEmpty())
            {
                dic.Add("Cookie", cookie);
            }
            #endregion

            #region 初始化任务调度
            JobManager.Initialize();
            ///每3天登录
            JobManager.AddJob(() =>
            {
                Login(email, passwd);
            }, s => s.ToRunEvery(3).Days().At(0, 1));
            //每一天签到
            JobManager.AddJob(CheckIn, s => s.ToRunEvery(1).Days().At(0, 1));
            #endregion

            #region 默认先进行一次登录和一次检查过期时间
            Login(email, passwd);
            CheckExpirationTimeAndAddSchedule();
            #endregion

            #region 控制台退出快捷键
            _closingEvent.WaitOne();
            Console.CancelKeyPress += ((s, a) =>
            {
                Console.WriteLine("程序已退出！");
                _closingEvent.Set();
            });
            #endregion


        }

        /// <summary>
        /// 循环执行方法 直到次数用完 或者方法返回true
        /// </summary>
        /// <param name="action"></param>
        /// <param name="ergodicNumber"></param>
        public static void ErgodicAction(Func<bool> action, int ergodicNumber = 5, int sleepSecond = 1)
        {
            for (int i = 0; i < ergodicNumber; i++)
            {
                Thread.Sleep(sleepSecond * 1000);
                bool isActionSuccess;
                try
                {
                    isActionSuccess = action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    isActionSuccess = false;
                }
                if (isActionSuccess)
                {
                    break;
                }
            }
        }

        ///获取版本号
        public static string GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string content = fvi.FileVersion;
            return content;
        }
    }

    public class Result
    {
        public int ret { get; set; }
        public string msg { get; set; }
    }
    public interface IMethod
    {
        void Excute();
    }
    public class MyMethod : IMethod
    {
        public void Excute()
        {
            Console.WriteLine("myhethod");
        }
    }

    public static class Extensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        public static string GetEnvironmentVariable(string key)
        {
            string str = Environment.GetEnvironmentVariable(key, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.Process);
            return str;

        }
        public static int? GetEnvironmentVariableInt(string key)
        {
            try
            {
                return int.Parse(GetEnvironmentVariable(key));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shadowsocks.Extension
{
    public class AutoServer
    {
        public string ServerAddress { get; set; }

        public int ServerPort { get; set; }

        public string Password { get; set; }

        public string Method { get; set; }

        public static AutoServer GetAutoServer(string html, List<string> allTexts, string key)
        {
            var serverAddress = string.Format("{0}服务器地址:", key);
            var index = allTexts.FindIndex(a => a.Contains(serverAddress));

            //服务器地址
            var pattern = string.Format(@"<h4>{0}(?<result>\w.+)</h4>", serverAddress);
            var regex = new Regex(pattern);
            var result = regex.Match(html).Result("${result}");

            //端口
            var txt = allTexts[index + 1];
            pattern = @"<h4>端口:(?<result>\w.+)</h4>";
            regex = new Regex(pattern);
            var port = regex.Match(txt).Result("${result}");

            //密码
            txt = allTexts[index + 2];
            pattern = string.Format(@"<h4>{0}密码:(?<result>\w.+)</h4>", key);
            regex = new Regex(pattern);
            var password = regex.Match(txt).Result("${result}");

            //加密方式
            txt = allTexts[index + 3];
            pattern = @"<h4>加密方式:(?<result>\w.+)</h4>";
            regex = new Regex(pattern);
            var method = regex.Match(txt).Result("${result}");

            return new AutoServer()
            {
                ServerAddress = result,
                ServerPort = Convert.ToInt32(port),
                Password = password,
                Method = method
            };
        }
    }
}
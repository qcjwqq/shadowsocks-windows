﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Net;
using Shadowsocks.Controller;
using Shadowsocks.Model;

namespace Shadowsocks.Extension
{
    public static class AutoPassword
    {
        private static readonly Timer _timer;
        private static ShadowsocksController _controller;

        static AutoPassword()
        {
            _timer = new Timer(PasswordCheck, null, 5000, 1000 * 60 * 30);
            Logging.Info("开启 ishadowsocks 监听");
        }

        public static void Register(ShadowsocksController controller)
        {
            _controller = controller;
            AutoUpdateConfig();
        }

        private static void PasswordCheck(object obj)
        {
            AutoUpdateConfig();
        }

        private static AutoAccountInfo CreateAutoAccountInfo()
        {
            var account = new AutoAccountInfo();
            var html = GetISHtml();
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var servers = new List<AutoServer>();
            var allTexts = html.Split('\n').ToList();
            var keys = new List<string> { "A", "B", "C" };
            keys.ForEach(a =>
            {
                servers.Add(AutoServer.GetAutoServer(html, allTexts, a));
            });

            account.AutoServers = servers;
            return account;
        }

        /// <summary>
        /// 获取ishadowsocks完整HTML内容
        /// </summary>
        /// <returns></returns>
        private static string GetISHtml()
        {
            try
            {
                //var request = WebRequest.Create("http://www.ishadowsocks.org/?timestamp=" + DateTime.Now.Ticks);
                var request = WebRequest.Create("https://www.ishadowsocks.xyz/?timestamp=" + DateTime.Now.Ticks);
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null)
                        {
                            return string.Empty;
                        }

                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("自动获取HTML文件失败" + ex);
                return string.Empty;
            }
        }

        private static void AutoUpdateConfig()
        {
            Logging.Info("开始自动设置账号密码");
            var hasException = false;
            var config = Configuration.Load();
            try
            {
                var autoServers = CreateAutoAccountInfo().AutoServers;
                config.configs.Clear();

                autoServers.ForEach(a =>
                {
                    config.configs.Add(new Server
                    {
                        server = a.ServerAddress,
                        server_port = a.ServerPort,
                        password = a.Password,
                        method = a.Method
                    });
                });

                Logging.Info("密码重新自动获取成功");
            }
            catch (Exception ex)
            {
                hasException = true;
                config.configs.Clear();
                Logging.Error(ex);
            }
            finally
            {
                _controller.Stop();
                Configuration.Save(config);
                _controller.Start();
                Logging.Info("完成自动设置账号密码");
                if (hasException)
                {
                    AutoUpdateConfig();
                }
            }
        }
    }
}

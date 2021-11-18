using log4net;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WxAPI.Helper
{
    /// <summary>
    /// log4net日志帮助类
    /// </summary>
    public class LogHelper
    {
        private static ILog logger;
        private static ILoggerRepository loggerRepository { get; set; }
        static LogHelper()
        {
            loggerRepository = log4net.LogManager.CreateRepository("NETCoreLog4netRepository");
            var file = new FileInfo("log4net.config");
            log4net.Config.XmlConfigurator.Configure(loggerRepository, file);
            logger = LogManager.GetLogger("NETCoreLog4netRepository", "loginfo");
        }
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Info(message);
            else
                logger.Info(message, exception);
        }

        /// <summary>
        /// 告警日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Warn(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Warn(message);
            else
                logger.Warn(message, exception);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Error(message);
            else
                logger.Error(message, exception);
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Debug(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Debug(message);
            else
                logger.Error(message, exception);
        }

        /// <summary>
        /// 毁灭性错误
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Fatal(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Fatal(message);
            else
                logger.Fatal(message, exception);
        }
    }
}

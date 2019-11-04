using System;
using System.Text;
using System.Web;
using Serilog;
using Serilog.Core;

namespace CommonWeb.AppCode
{
    public class LogHelper
    {

        private static Logger _logger;

        public static void Start(bool web = true)
        {
            string file= AppDomain.CurrentDomain.BaseDirectory + @"log\log.txt";
            var config = new LoggerConfiguration();
            //config.WriteTo.Console();
            config.WriteTo.File(file, rollingInterval: RollingInterval.Day);
            //config.MinimumLevel.Error();
            _logger = config.CreateLogger();
        }

        public static void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public static void Info(string msg)
        {
            _logger.Information(msg);
        }

        public static void Warning(string msg)
        {
            _logger.Warning(msg);
        }


        public static void Error(string msg)
        {
            _logger.Error(msg);
        }

        public static void Fatal(string msg)
        {
            _logger.Fatal(msg);
        }

        public static void LogHttpError()
        {
            var ex = HttpContext.Current.Server.GetLastError(); //实际出现的异常

            if (ex != null)
            {
                var iex = ex.InnerException; //实际发生的异常
                if (iex != null)
                {
                    ex = iex;
                }

                var httpError = ex as HttpException; //http异常


                //ASP.NET的400与404错误不记录日志，并都以自定义404页面响应
                if (httpError != null)
                {
                    var httpCode = httpError.GetHttpCode();
                    if (httpCode == 400 || httpCode == 404)
                    {

                        HttpContext.Current.Response.StatusCode = 404; //在IIS中配置自定义404页面
                        //Server.ClearError();
                        return;
                    }

                }

                //对于路径错误不记录日志，并都以自定义404页面响应
                if (ex.TargetSite.ReflectedType == typeof(System.IO.Path))
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    //Server.ClearError();
                    return;
                }



                var queryString = HttpContext.Current.Request.QueryString; //get参数
                var sbQueryString = new StringBuilder();
                sbQueryString.Append(queryString.Count + "个");
                foreach (var key in queryString.AllKeys)
                {
                    sbQueryString.Append("\r\n");
                    sbQueryString.AppendFormat("{0}:{1}", key, queryString[key]);
                }


                var form = HttpContext.Current.Request.Form; //post参数
                var sbForm = new StringBuilder();
                sbForm.Append(form.Count + "个");
                foreach (var key in form.AllKeys)
                {
                    sbForm.Append("\r\n");
                    sbForm.AppendFormat("{0}:{1}", key, form[key]);
                }


                var files = HttpContext.Current.Request.Files; //文件
                var sbFile = new StringBuilder();
                sbFile.Append(files.Count + "个");
                if (files.Count != 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFile file = files[i];
                        sbFile.Append("\r\n");
                        sbFile.AppendFormat("文件名：{0}", file.FileName);
                    }
                }


                var sb = new StringBuilder();
                sb.Append("\r\n错误消息：" + ex.Message);
                sb.Append("\r\n异常页面：" + HttpContext.Current.Request.RawUrl);
                sb.Append("\r\n请求方式：" + HttpContext.Current.Request.HttpMethod);
                sb.Append("\r\n");
                sb.Append("\r\nGET参数：" + sbQueryString.ToString());
                sb.Append("\r\n");
                sb.Append("\r\nPOST参数：" + sbForm.ToString());
                sb.Append("\r\n");
                sb.Append("\r\n上传文件：" + sbFile.ToString());
                sb.Append("\r\n");
                sb.Append("\r\n源错误：" + ex.Source);
                sb.Append("\r\n堆栈信息：" + ex.StackTrace);
                sb.Append("\r\n堆栈跟踪：" + ex.TargetSite);
                sb.Append("\r\n");
                sb.Append("\r\n\r\n===============================================");

                Error(sb.ToString());
            }
        }

        public static void Stop()
        {
            _logger.Dispose();
        }

    }

}





//protected void Application_Start()
//{
//    LogHelper.Start();
//    LogHelper.Info("系统启动");
//}

//protected void Application_Error(Object sender, EventArgs e)
//{
//    LogHelper.LogHttpError();
//}

//protected void Application_End(object sender, EventArgs e)
//{
//    LogHelper.Info("系统停止");
//    LogHelper.Stop();
//}
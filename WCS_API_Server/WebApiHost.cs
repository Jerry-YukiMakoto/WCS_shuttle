using System;
using Microsoft.Owin.Hosting;

namespace WCS_API_Server
{
    public class WebApiHost
    {
        //private string _baseAddress = "http://127.0.0.1:9000/";
        private string _baseAddress = "";
        private IDisposable _webService;

        public WebApiHost(Startup startup, string sIP) 
        {
            _baseAddress = $"http://{sIP}/";
            _webService = WebApp.Start(url: _baseAddress, startup: startup.Configuration);
        }

        ~WebApiHost()
        {
            _webService.Dispose();
        }
    }
}
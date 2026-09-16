using System;
using System.Configuration;
using HutkoSDK;

namespace HutkoSDKSamples
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Enforce a modern TLS floor. TLS 1.0/1.1 are deprecated (POODLE/BEAST)
            // and must not be enabled.
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            Config.MerchantId = Int32.Parse(ConfigurationManager.AppSettings["merchantID"]);
            // Prefer a secret from the environment; fall back to Web.config, which
            // holds only the public sandbox key for local demos.
            Config.SecretKey = Environment.GetEnvironmentVariable("HUTKO_SECRET_KEY")
                               ?? ConfigurationManager.AppSettings["secretKey"];
            Config.ContentType = ConfigurationManager.AppSettings["contentType"];
            Config.Protocol = ConfigurationManager.AppSettings["protocol"];
            Config.ApiHost = ConfigurationManager.AppSettings["ApiHost"];
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}
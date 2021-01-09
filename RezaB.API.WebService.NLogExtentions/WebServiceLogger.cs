using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RezaB.API.WebService.NLogExtentions
{
    public class WebServiceLogger
    {
        private Logger _logger;

        public WebServiceLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void LogInfo(string username, string subscriberNo, string message = "sent successful response.", params object[] formatProperties)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, string.Format(message, formatProperties));
            logEvent.Properties["Username"] = username;
            logEvent.Properties["SubscriberNo"] = subscriberNo;
            logEvent.Properties["RequestIP"] = GetRequestIP();
            _logger.Log(typeof(WebServiceLogger), logEvent);
        }

        public void LogInfo(string username, string message = "sent successful response.", params object[] formatProperties)
        {
            LogInfo(username, null, message, formatProperties);
        }

        public void LogException(string username, Exception exception)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, CultureInfo.InvariantCulture, "EXCEPTION occured: ", null, exception);
            logEvent.Properties["Username"] = username;
            logEvent.Properties["RequestIP"] = GetRequestIP();
            _logger.Log(typeof(WebServiceLogger), logEvent);
        }

        public void LogIncomingMessage(object request)
        {
            var messageString = string.Empty;
            XmlSerializer serializer = new XmlSerializer(request.GetType());
            using (var textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, request);
                messageString = textWriter.ToString();
            }

            _logger.Trace(messageString);
        }

        private string GetRequestIP()
        {
            var context = OperationContext.Current;
            var messageProps = context.IncomingMessageProperties;
            var endPoint = messageProps[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string ip = string.Empty;
            if (messageProps.Keys.Contains(HttpRequestMessageProperty.Name))
            {
                var endpointLoadBalancer = messageProps[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                if (endpointLoadBalancer != null && endpointLoadBalancer.Headers["X-Forwarded-For"] != null)
                    ip = endpointLoadBalancer.Headers["X-Forwarded-For"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = endPoint.Address;
                }
            }

            return ip;
        }
    }
}

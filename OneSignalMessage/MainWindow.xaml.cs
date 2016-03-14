using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace OneSignalMessage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List of avaible channels
        List<String> channels = new List<String>();
        List<CheckBox> checkBoxes = new List<CheckBox>();

        //Configuration info

        String RestKey = null;
        String AppID = null;
        String Language = null;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfigurations();
            LoadChannels();
        }

        /// <summary>
        /// Function to Load content of config.xml file
        /// </summary>
        private void LoadChannels()
        {
            XmlDocument doc = new XmlDocument();
            
            doc.PreserveWhitespace = true;
            try { 
                doc.Load(".\\configuration\\config.xml");
                XmlNodeList channelNodes = doc.DocumentElement.SelectNodes("/Configurations/Channels/Channel");
                foreach (XmlNode channelNode in channelNodes)
                {
                   channels.Add(channelNode.InnerText);
                   CheckBox cb = new CheckBox();
                   cb.Name = "cb"+channelNode.InnerText;
                   cb.Content = channelNode.InnerText;
                   cb.Margin = new Thickness(10,10,10,10);
                   stkChannelPanel.Children.Add(cb);
                   checkBoxes.Add(cb);
                 
                }
            }
            catch
            {

            }

        }

        private void LoadConfigurations()
        {
            XmlDocument doc = new XmlDocument();

            doc.PreserveWhitespace = true;
            try
            {
                doc.Load(".\\configuration\\config.xml");
                XmlNode restkey = doc.DocumentElement.SelectSingleNode("/Configurations/RestKey");
                XmlNode appid = doc.DocumentElement.SelectSingleNode("/Configurations/AppID");
                XmlNode language = doc.DocumentElement.SelectSingleNode("/Configurations/Language");

                this.RestKey = restkey.InnerText;
                this.AppID = appid.InnerText;
                this.Language = language.InnerText;
    
            }
            catch
            {

            }

        }
        /// <summary>
        /// Send Message 
        /// </summary>
        private void SendMessage()
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json";

            request.Headers.Add("authorization", "Basic "+RestKey);

            String yu = GetSelectedChannels();

            byte[] byteArray = Encoding.UTF8.GetBytes("{"
                                                    + "\"app_id\": \""+AppID+"\","
                                                    + "\"contents\": {\""+Language+"\": \""+txtMessage.Text+"\"},"
                                                    + "\"headings\": {\""+Language+"\":\""+txtTitle.Text+"\"},"
                                                    + "\"included_segments\": [" + GetSelectedChannels()+ "]}");

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }

        /// <summary>
        /// Trigger send message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private String GetSelectedChannels()
        {
            List<String> channels = new List<string>();
            foreach(CheckBox cb in checkBoxes)
            {
                if((bool)cb.IsChecked)
                {
                    channels.Add("\"" + cb.Content.ToString() + "\"");
                }
            }

            String result = String.Join(",",channels.ToArray());
            return result;
        }
    }
}

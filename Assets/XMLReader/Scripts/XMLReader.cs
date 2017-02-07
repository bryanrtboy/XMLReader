//Author: Bryan Leister - Feb. 2017
//An extremely simple XML reader to just read a feed, from various sources and display the XML
//Looping through the elements we can then print to screen the value of one element that we found
//This does not parse or store lists, just a basic read of the feed for simple feeds like
//the US Weather feed.
//
//Some feeds do not require that you send a header, but the us weather does, so it includes that
//function
using System.Collections;
using System.Net;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleXML
{
	public class XMLReader : MonoBehaviour
	{
		public Text m_display;
		public string m_feedURL = "http://w1.weather.gov/xml/current_obs/KFTG.xml";
		//other feeds to test -  http://feeds.foxnews.com/foxnews/national
		//http://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml";

		void Start ()
		{
			SendRequest ();
		}

		void SendRequest ()
		{
			XmlDocument doc = new XmlDocument ();

			string stationName = m_feedURL;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (stationName);

			request.Method = "GET";
			request.ContentType = "application/x-www-form-urlencoded";
			request.UserAgent = "MyApplication/v1.0 (http://foo.com; foo@gmail.com)";
			request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			Stream resStream = response.GetResponseStream ();

			doc.Load (resStream);

			try {
				XmlNodeList newList = doc.DocumentElement.ChildNodes;

				foreach (XmlNode n in newList) {
					Debug.Log (n.OuterXml);

					if (n.Name == "weather" && m_display != null)
						m_display.text = n.InnerText;
				}

			} catch (System.Exception e) {
				Debug.LogError ("Unknown exception: " + e.Message + " " + e.StackTrace);
			}

		}
	}
}

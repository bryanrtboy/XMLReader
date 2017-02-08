//Author: Bryan Leister - Feb. 2017
//An extremely simple XML reader to just read a feed, from various sources and display the XML
//Looping through the elements we can then print to screen the value of one element that we found
//This does not parse or store lists, just a basic read of the feed for simple feeds like
//the US Weather feed.
//
//Some feeds do not require that you send a header, but the us weather does, so it includes that
//function
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using UnityEngine;

namespace SimpleXML
{
	public class XMLReader : MonoBehaviour
	{

		public event Action<string,float> OnCurrentCondReceived;
		public event Action<string,float> OnForecastReceived;

		//other feeds to test -  http://feeds.foxnews.com/foxnews/national
		//string m_forecastURL = "http://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml";
		string m_forecastURL = "http://forecast.weather.gov/MapClick.php?lat=39.7392&lon=-104.9903&unit=0&lg=english&FcstType=dwml";
		string m_currentWeatherURL = "http://w1.weather.gov/xml/current_obs/KFTG.xml";
		string m_userAgent = "MyApplication/v1.0 (http://foo.com; foo@gmail.com)";

		Thread m_thread;
		XDocument xmlDoc;
		bool runThread = true;
		bool updateThread = false;
		bool haveFreshData = false;

		void Start ()
		{
			InvokeRepeating ("GetCurrentConditions", 0, 15);//this runs on the main thread, which is fine since this feed is fast
			InvokeRepeating ("GetForecast", 1, 36); //forecast only updates once per hour so use 3600 here. It will run on it's own thread as reading is slower...
		}

		void Update ()
		{
			if (haveFreshData) { //Don't do anything unless the thread has refreshed the data
				haveFreshData = false; //then we only run once

				List<string> tempList = Temperatures (xmlDoc);
				List<string> condList = Conditions (xmlDoc);

				if (tempList.Count > 0 && condList.Count > 0) {
					if (OnForecastReceived != null)
						OnForecastReceived (condList [0], Convert.ToSingle (tempList [0]));
				}
			}
		}

		void OnEnable ()
		{
			m_thread = new Thread (ThreadUpdate); //starting the thread that reads forecasts, since it is slow...
			m_thread.Start (); //Thread runs all the time, but only does anything when updateThread = true;
		}

		void OnDisable ()
		{
			EndThreads ();
		}

		void EndThreads ()
		{
			runThread = false;
			//you could use thread.abort() but that has issues on iOS
			while (m_thread.IsAlive) {
				//simply have main loop wait till thread ends
			}
		}

		#region Forecast reading using XDocument running on it's own thread

		void ThreadUpdate ()
		{
			while (runThread) {
				if (updateThread) {
					updateThread = false;
					FetchForecastXDocument (m_forecastURL);
				}
			}
		}

		void GetForecast ()
		{
			updateThread = true;
		}

		List<string> Temperatures (XDocument xml)
		{
			var maximums = from tempvalue in xml.Descendants ("temperature").Elements ("value")
			               where tempvalue.Parent.Attribute ("type").Value == "maximum"
			               select (string)tempvalue;

			List<string> returnme = maximums.ToList<string> ();
			return returnme;
		}

		List<string> Conditions (XDocument xml)
		{
			List<string> returnme = new List<string> ();

			foreach (XAttribute x in xml.Descendants ("weather").Elements("weather-conditions").Attributes("weather-summary"))
				returnme.Add (x.Value);
			return returnme;
			
		}

		void FetchForecastXDocument (string url)
		{
			xmlDoc = GetXDocument (url);
			haveFreshData = true;
		}

		XDocument  GetXDocument (string xmlURL)
		{
			XDocument doc = new XDocument ();
			float elapsedTime = 0.0f;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (xmlURL);

			request.Method = "GET";
			request.ContentType = "application/x-www-form-urlencoded";
			request.UserAgent = m_userAgent;
			request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			Stream resStream = response.GetResponseStream ();

			doc = XDocument.Load (resStream);

			return doc;
		}

		#endregion

		#region Current Conditions using XmlDocument

		void GetCurrentConditions ()
		{
			XmlDocument doc = GetXMLDocument (m_currentWeatherURL);

			float temp = -100f;
			string weather = "";

			try {

				XmlNodeList newList = doc.DocumentElement.ChildNodes;

				foreach (XmlNode n in newList) {
					Debug.Log (n.OuterXml);

					if (n.Name == "weather")
						weather = n.InnerText;

					if (n.Name == "temp_f")
						temp = Convert.ToSingle (n.InnerText);
					
				}

				if (OnCurrentCondReceived != null)
					OnCurrentCondReceived (weather, temp);

			} catch (System.Exception e) {
				Debug.LogError ("Unknown exception: " + e.Message + " " + e.StackTrace);
			}

		}

		XmlDocument GetXMLDocument (string xmlURL)
		{
			XmlDocument doc = new XmlDocument ();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (xmlURL);

			request.Method = "GET";
			request.ContentType = "application/x-www-form-urlencoded";
			request.UserAgent = m_userAgent;
			request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			Stream resStream = response.GetResponseStream ();

			doc.Load (resStream);

			return doc;
		}

		#endregion

	}
}

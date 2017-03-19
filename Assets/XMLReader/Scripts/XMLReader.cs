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

		public float updateForecastRate = 3600;
		public float updateCurrentConditionsRate = 120;

		//other feeds to test -  http://feeds.foxnews.com/foxnews/national
		//string m_forecastURL = "http://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml";
		string m_forecastURL = "http://forecast.weather.gov/MapClick.php?lat=39.7392&lon=-104.9903&unit=0&lg=english&FcstType=dwml";
		string m_currentWeatherURL = "http://w1.weather.gov/xml/current_obs/KFTG.xml";
		string m_userAgent = "MyApplication/v1.0 (http://foo.com; foo@gmail.com)";

		Thread m_thread;
		XDocument xDoc;
		XmlDocument xmlDoc;
		bool runThread = true;
		bool grabForecast = false;
		bool grabCurrentConditions = false;
		bool haveFreshForecast = false;
		bool haveFreshCurrent = false;

		float temp = -100f;
		string weather = "";

		void Start ()
		{
			Invoke ("StartThread", 3);
		}

		void Update ()
		{
//			if (haveFreshForecast) { //Don't do anything unless the thread has refreshed the data
//				haveFreshForecast = false; //then we only run once
//
//				List<string> tempList = Temperatures (xDoc);
//				List<string> condList = Conditions (xDoc);
//
//				if (tempList.Count > 0 && condList.Count > 0) {
//					if (OnForecastReceived != null)
//						OnForecastReceived (condList [0], Convert.ToSingle (tempList [0]));
//				}
//			}

			if (haveFreshCurrent) { //Don't do anything unless the thread has refreshed the data
				haveFreshCurrent = false; //then we only run once

				if (OnCurrentCondReceived != null)
					OnCurrentCondReceived (weather, temp);
				
			}
		}

		void StartThread ()
		{
			m_thread = new Thread (ThreadUpdate); //starting the thread that reads forecasts, since it is a lot of text and slow to read in...
			m_thread.Start (); //Thread runs all the time, but only does anything when updateThread = true;

			Invoke ("StartGettingInfo", 1);

		}

		void StartGettingInfo ()
		{
			if (!m_thread.IsAlive) {
				Debug.LogError ("No weather for you!");
				return;
			}
			InvokeRepeating ("GetCurrentConditions", 0, updateCurrentConditionsRate);//this runs on the main thread, which is fine since this feed is fast
			//InvokeRepeating ("GetForecast", 12, updateForecastRate); //forecast only updates once per hour so use 3600 here. It will run on it's own thread as reading is slower...

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
				if (grabForecast) {
					grabForecast = false;
					//FetchXMLFeed (m_forecastURL);

				}

				if (grabCurrentConditions) {
					grabCurrentConditions = false;
					//FetchXMLFeed (m_currentWeatherURL);
					GetTempAndConditions (m_forecastURL, "Daily Maximum Temperature", "Daily Maximum Temperature");
					//GetTempAndConditions (m_currentWeatherURL, "weather", "temp_f");

				}
			}
		}

		void GetForecast ()
		{
			grabForecast = true;
		}

		void GetCurrentConditions ()
		{
			grabCurrentConditions = true;
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

		void FetchXMLFeed (string url)
		{
			xDoc = GetXDocument (url);
			Debug.Log (xDoc.ToString ());
			haveFreshForecast = true;
		}



		XDocument  GetXDocument (string xmlURL)
		{
			XDocument doc = new XDocument ();
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

		XmlDocument GetTempAndConditions (string xmlURL, string weatherName, string temperatureName)
		{
			XmlDocument doc = GetXMLDocument (xmlURL);

			temp = -100f;
			weather = "";

			try {

				XmlNodeList newList = doc.DocumentElement.ChildNodes;

				Debug.Log ("Doc is " + doc.ChildNodes.Count);

				foreach (XmlNode n in newList) {

					if (n.HasChildNodes) {
						XmlNodeList childList = n.ChildNodes;

						foreach (XmlNode cn in childList) {
							Debug.Log (cn.ChildNodes.Count + " children of " + cn.Name);
							if (cn.Name == "parameters") {
								
								XmlNodeList parameterNodes = cn.ChildNodes;

								foreach (XmlNode pm in parameterNodes) {
									if (pm.Name == weatherName)
										weather = pm.InnerText;
									
									Debug.Log (pm.ChildNodes.Count + " children of " + pm.Name);
								}

							}
						}


					}




					//Debug.Log ("Got weather, it's child is " + n [weatherName].InnerText);


					if (n.Name == weatherName) {
						weather = n.InnerText;
						Debug.Log ("Got weather, it's " + n.InnerText);
					}

					if (n.Name == temperatureName) {
						temp = Convert.ToSingle (n.InnerText);
					}
					
				}
				Debug.Log ("Got current conditions, " + weather + ", temp is " + temp.ToString ());
				haveFreshCurrent = true;

			} catch (System.Exception e) {
				Debug.LogError ("Unknown exception: " + e.Message + " " + e.StackTrace);
			}

			return doc;

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

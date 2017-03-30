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
		bool gotNewForecastData = false;
		bool gotNewCurrentData = false;

		float temp = -100f;
		string weather = "";
		float forecastTemp = -100f;
		string weatherForecast = "";

		void Start ()
		{
			Invoke ("StartThread", 3);
		}

		void Update ()
		{
<<<<<<< Updated upstream
			if (gotNewForecastData) {
				gotNewForecastData = false;

				if (OnForecastReceived != null)
					OnForecastReceived (weatherForecast, forecastTemp);
			}

			if (gotNewCurrentData) { //Don't do anything unless the thread has refreshed the data
				gotNewCurrentData = false; //then we only run once

				if (OnCurrentCondReceived != null)
					OnCurrentCondReceived (weather, temp);
				
			}
		}

		//Runs every frame, but does nothing unless the flags are invoked by the InvokeRepeating in StartGettingInfo
		void ThreadUpdate ()
		{
			while (runThread) {
				if (grabForecast) {
					grabForecast = false;
					GetTempAndConditions (m_forecastURL, "weather-summary", "maximum");
				}
=======
			if (!InternetChecker.instance.m_hasInternet)
				return;

			if (haveFreshData) { //Don't do anything unless the thread has refreshed the data
				haveFreshData = false; //then we only run once
>>>>>>> Stashed changes

				if (grabCurrentConditions) {
					grabCurrentConditions = false;
					GetTempAndConditions (m_currentWeatherURL, "weather", "temp_f");

				}
			}
		}

		void StartThread ()
		{
			m_thread = new Thread (ThreadUpdate); //starting the thread that reads forecasts, since it is a lot of text and slow to read in...
			m_thread.Start (); //Thread runs all the time, but only does anything when updateThread = true;

			Invoke ("StartGettingInfo", 1);

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


		void StartGettingInfo ()
		{
<<<<<<< Updated upstream
			if (!m_thread.IsAlive) {
				Debug.LogError ("No weather for you!");
				return;
=======
			while (runThread && InternetChecker.instance.m_hasInternet) {
				if (updateThread) {
					updateThread = false;
					FetchForecastXDocument (m_forecastURL);
				}
>>>>>>> Stashed changes
			}

			InvokeRepeating ("GetCurrentConditions", 10, updateCurrentConditionsRate);//this runs on the main thread, which is fine since this feed is fast
			Invoke ("GetForecast", 4); //forecast only updates once.

		}

		void GetForecast ()
		{
			grabForecast = true; //Set flag so the thread runs the function
		}

		void GetCurrentConditions ()
		{
<<<<<<< Updated upstream
			grabCurrentConditions = true; //Set flag so the thread runs the function 
=======
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
>>>>>>> Stashed changes
		}

		XmlDocument GetTempAndConditions (string xmlURL, string weatherName, string temperatureName)
		{
			XmlDocument doc = GetXMLDocument (xmlURL);

			temp = -100f;
			weather = "";
			bool isCurrentData = false;

			try {

				XmlNodeList rootList = doc.DocumentElement.ChildNodes;

				foreach (XmlNode root in rootList) {

					if (root.HasChildNodes) {
						XmlNodeList rootChildren = root.ChildNodes;

						//Go through all of the nodes
						foreach (XmlNode child in rootChildren) {

							//Get the parameters node and then it's children
							if (child.Name == "parameters") {
								
								XmlNodeList parameterChildren = child.ChildNodes;

								//Go through all the paramter's children, looking for temperatur forecast and conditions
								foreach (XmlNode dataChild in parameterChildren) {
									
									//Get today's temperature
									foreach (XmlAttribute xattr in dataChild.Attributes) {
										if (xattr.Value == temperatureName) {
											XmlNodeList tempChildren = dataChild.ChildNodes;
											foreach (XmlNode temp in tempChildren) {
												//Debug.Log ("Temp max today is " + temp.InnerText);
											}
											forecastTemp = Convert.ToSingle (tempChildren [1].InnerText);
										} 										
									}

									//Get today's forecast
									if (dataChild.Name == "weather") {
										XmlNodeList weatherChild = dataChild.ChildNodes;

										foreach (XmlNode weath in weatherChild) {
											foreach (XmlAttribute weatAttr in weath.Attributes) {
												
												if (weatAttr.Name == weatherName) {
													{
														weatherForecast = weatherChild [1].Attributes [weatherName].Value;
														//Debug.Log ("Forecast is " + weather);
													}
												}
											}
										}
									}
								}
							}

							gotNewForecastData = true; //We seem to have parameters, we must have the forecast XML
						}


					}
						
					//If we are using the simple XML, just get the damn information directly
					if (root.Name == weatherName) {
						weather = root.InnerText;
						isCurrentData = true;
					}

					if (root.Name == temperatureName) {
						temp = Convert.ToSingle (root.InnerText);
						isCurrentData = true;
					}
					
				}

				//All is well, we have new data
				if (isCurrentData)
					gotNewCurrentData = true;

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

	}
}

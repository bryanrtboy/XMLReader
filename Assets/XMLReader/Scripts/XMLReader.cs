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
			if (InternetChecker.instance == null)
				return;

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
					
					if (InternetChecker.instance.m_hasInternet)
						GetTempAndConditions (m_forecastURL, "weather-summary", "maximum");
					else
						Debug.LogWarning ("No connection, not checking the forecast at " + System.DateTime.Now);

					grabForecast = false;
				} 

				if (grabCurrentConditions) {
					
					if (InternetChecker.instance.m_hasInternet)
						GetTempAndConditions (m_currentWeatherURL, "weather", "temp_f");
					else
						Debug.LogWarning ("No connection, not checking current conditions at " + System.DateTime.Now);

					grabCurrentConditions = false;
				} 
			}
		}

		void StartThread ()
		{

			if (InternetChecker.instance == null) {
				Debug.LogError ("There is no instance of an Internet checker in the scene, I don't trust that we have internet, not starting the thread. Please put a game object"
				+ " in the scene with the InternetCheck script attatched.");
				Destroy (this);
				return;
			}

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

			if (m_thread == null)
				return;

			//you could use thread.abort() but that has issues on iOS
			while (m_thread.IsAlive) {
				//simply have main loop wait till thread ends
			}
		}


		void StartGettingInfo ()
		{
			if (!m_thread.IsAlive) {
				Debug.LogError ("No weather for you!");
				return;
			}

			InvokeRepeating ("GetCurrentConditions", 10, updateCurrentConditionsRate);
			Invoke ("GetForecast", 4); //forecast only updates once.

		}

		void GetForecast ()
		{
			grabForecast = true; //Set flag so the thread runs the function
		}

		void GetCurrentConditions ()
		{
			grabCurrentConditions = true; //Set flag so the thread runs the function 
		}

		XmlDocument GetTempAndConditions (string xmlURL, string weatherName, string temperatureName)
		{

			XmlDocument doc = new XmlDocument ();

			if (InternetChecker.instance.m_hasInternet)
				doc = GetXMLDocument (xmlURL);
			else
				return doc;

			temp = -100f;
			weather = "";

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
						}
					} 
						
					//If we are using the simple XML, just get the damn information directly
					if (root.Name == weatherName) {
						weather = root.InnerText;
					}

					if (root.Name == temperatureName) {
						temp = Convert.ToSingle (root.InnerText);
					}

					if (grabForecast && forecastTemp > -99f)
						gotNewForecastData = true;

					if (grabCurrentConditions && temp > -99f)
						gotNewCurrentData = true;
					
				}

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
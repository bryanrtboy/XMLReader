using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleXML;

public class ShowUI : MonoBehaviour
{

	public Text m_current;
	public Text m_forecast;
	public XMLReader m_xmlReader;


	void OnEnable ()
	{
		m_xmlReader.OnForecastReceived += ReadForecast;
		m_xmlReader.OnCurrentCondReceived += ReadCurrent;
	}

	void OnDisable ()
	{
		m_xmlReader.OnForecastReceived -= ReadForecast;
		m_xmlReader.OnCurrentCondReceived -= ReadCurrent;
	}

	void ReadForecast (string s, float f)
	{
		if (m_forecast != null)
			m_forecast.text = "Forecast is " + s + ", with a temperature of " + f.ToString () + " at " + Time.time + " after start.";
	}

	void ReadCurrent (string s, float f)
	{
		if (m_current != null)
			m_current.text = "Currently it is " + s + ", with a temperature of " + f.ToString () + " at " + Time.time + " after start.";
	}
}

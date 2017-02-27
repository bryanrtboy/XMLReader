using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleXML;

public class ParticleController : MonoBehaviour
{

	public XMLReader m_xmlReader;

	public enum TypeOfWeather
	{
		PartlyCloudy,
		MostlyCloudy,
		Overcast,
		Fog,
		Snow,
		Flurries,
		Rain,
		Blizzard,
		Clear
	}

	public TypeOfWeather m_weather = TypeOfWeather.Clear;


	public bool m_testMode = false;
	public ParticleSystem m_snow;
	public ParticleSystem m_clouds;
	public ParticleSystem m_haze;
	public ParticleSystem m_rain;
	public ParticleSystem m_fog;

	public Color m_snowSkyColor = Color.grey;
	public Color m_clearSkyColor = Color.blue;


	Camera m_mainCamera;

	void Awake ()
	{
		if (m_snow)
			m_snow.Stop ();

		if (m_clouds)
			m_clouds.Stop ();

		m_mainCamera = Camera.main;
	}

	void OnEnable ()
	{
		if (m_testMode) {
			ChangeWeather ();
			return;
		}

		m_xmlReader.OnCurrentCondReceived += HandleCurrentConditions; //Set up handlers for the Events that occur when XMLReader gets a new forecast
		m_xmlReader.OnForecastReceived += HandleForecast;
	}

	void OnDisable ()
	{
		if (m_testMode)
			return;
		m_xmlReader.OnCurrentCondReceived -= HandleCurrentConditions; //Clean up and shut down Event Handlers 
		m_xmlReader.OnForecastReceived -= HandleForecast;
	}

	void HandleCurrentConditions (string s, float f)
	{
		Debug.Log ("Temp is " + f.ToString () + ", " + s + " at " + Time.time);

		s = s.ToLower ();
		if (s.Contains ("blizzard")) {
			m_weather = TypeOfWeather.Blizzard;
		} else if (s.Contains ("snow") || s.Contains ("snowy")) {
			m_weather = TypeOfWeather.Snow;
		} else if (s.Contains ("flurries")) {
			m_weather = TypeOfWeather.Flurries;
		} else if (s.Contains ("rain") || s.Contains ("showers")) {
			m_weather = TypeOfWeather.Rain;
		} else if (s.Contains ("partly cloudy")) {
			m_weather = TypeOfWeather.PartlyCloudy;
		} else if (s.Contains ("mostly cloudy") || s.Contains ("cloudy")) {
			m_weather = TypeOfWeather.MostlyCloudy;
		} else if (s.Contains ("fog")) {
			m_weather = TypeOfWeather.Fog;
		} else if (s.Contains ("overcast")) {
			m_weather = TypeOfWeather.Overcast;
		} else {
			m_weather = TypeOfWeather.Clear;
		}

		ChangeWeather ();

	}

	void ChangeWeather ()
	{
		switch (m_weather) {
		case TypeOfWeather.Blizzard:
			if (m_snow != null) {
				//Here we should increase the emmission value of m_snow
				m_snow.Play ();
				ParticleSystem ps = Instantiate (m_snow);
				m_mainCamera.backgroundColor = m_snowSkyColor;
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}
			break;
		case TypeOfWeather.Clear:
			m_mainCamera.backgroundColor = m_clearSkyColor;
			//Don't play any particles
			break;
		case TypeOfWeather.Flurries:
			if (m_snow != null) {
				//Change the snow emmissions to burst, or slower than snow
				m_snow.Play ();
				m_mainCamera.backgroundColor = Color.Lerp (m_snowSkyColor, m_clearSkyColor, .5f);
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}
			break;
		case TypeOfWeather.Fog:
			m_mainCamera.backgroundColor = m_snowSkyColor;
			if (m_fog != null) {
				m_fog.Play ();
			} else {
				Debug.Log ("If we had a fog effect, we'd play it.");
			}
			break;
		case TypeOfWeather.MostlyCloudy:
			m_mainCamera.backgroundColor = m_clearSkyColor;
			if (m_clouds != null) {
				//change the cloud count to be higher than when partly cloudy
				m_clouds.Play ();
			} else {
				Debug.Log ("If we had a cloud effect, we'd play it.");
			}
			break;
		case TypeOfWeather.Overcast:

			m_mainCamera.backgroundColor = m_snowSkyColor;

			if (m_haze != null) {
				m_haze.Play ();
			} else {
				Debug.Log ("If we had a overcast effect, we'd play it.");
			}
			break;
		case TypeOfWeather.PartlyCloudy:
			m_mainCamera.backgroundColor = m_clearSkyColor;
			if (m_clouds != null) {
				m_clouds.Play ();
			} else {
				Debug.Log ("If we had a cloud effect, we'd play it.");
			}
			break;
		case TypeOfWeather.Snow:

			m_mainCamera.backgroundColor = m_snowSkyColor;

			if (m_snow != null) {
				m_snow.Play ();
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}
			break;
		default :
		//Don't play any particles by default
			m_mainCamera.backgroundColor = m_clearSkyColor;
			break;
		}
	}

	void HandleForecast (string s, float f)
	{
		Debug.Log ("Forecast is " + f.ToString () + ", " + s + " at " + Time.time);
	}
}

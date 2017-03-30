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

	//	public DayNightCycleGradient m_dayNight;
	public bool m_testMode = false;
	public ParticleSystem m_snow;
	public ParticleSystem m_clouds;
	public ParticleSystem m_haze;
	public ParticleSystem m_rain;
	public ParticleSystem m_fog;

	List<ParticleSystem> m_systems;

	void Awake ()
	{
		m_systems = new List<ParticleSystem> ();

		if (m_snow)
			m_systems.Add (m_snow);
		if (m_clouds)
			m_systems.Add (m_clouds);
		if (m_haze)
			m_systems.Add (m_haze);
		if (m_rain)
			m_systems.Add (m_rain);
		if (m_fog)
			m_systems.Add (m_fog);

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
//		Debug.Log ("Current conditions - temp is " + f.ToString () + ", " + s + " at " + Time.time);

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

		foreach (ParticleSystem ps in m_systems)
			ps.Stop ();

//		if (m_dayNight) {
//			m_dayNight.m_isGrey = false;
//			m_dayNight.fogScale = .01f;
//		}

		switch (m_weather) {
		case TypeOfWeather.Blizzard:
			if (m_snow != null) {
				//Here we should increase the emmission value of m_snow
				m_snow.Play ();
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}

			Camera.main.backgroundColor = Color.gray;
//			if (m_dayNight) {
//				m_dayNight.m_isGrey = true;
//				m_dayNight.fogScale = .1f;
//			}

			break;
		case TypeOfWeather.Clear:
			//Don't play any particles
			break;
		case TypeOfWeather.Flurries:
			if (m_snow != null) {
				//Change the snow emmissions to burst, or slower than snow
				SetParticles (m_snow, 500f);
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}

			if (m_haze != null)
				SetParticles (m_haze, 5f);

			Camera.main.backgroundColor = Color.gray;

//			if (m_dayNight) {
//				m_dayNight.m_isGrey = true;
//				m_dayNight.fogScale = .1f;
//			}

			break;
		case TypeOfWeather.Fog:
			if (m_fog != null) {
				m_fog.Play ();
			} else {
				Debug.Log ("If we had a fog effect, we'd play it.");
			}

			if (m_haze != null)
				SetParticles (m_haze, 5f);

//			if (m_dayNight) {
//				m_dayNight.m_isGrey = true;
//				m_dayNight.fogScale = .1f;
//			}

			break;
		case TypeOfWeather.MostlyCloudy:
			if (m_clouds != null) {
				//change the cloud count to be higher than when partly cloudy
				Vector3 pos = new Vector3 (m_clouds.transform.position.x, 3, m_clouds.transform.position.z);
				Vector3 shape = new Vector3 (4, 10, 15);
				//setup and play the particles
				SetParticles (m_clouds, 10f, pos, shape);

			} else {
				Debug.Log ("If we had a cloud effect, we'd play it.");
			}

			if (m_haze != null)
				SetParticles (m_haze, 5f);

			break;
		case TypeOfWeather.Overcast:
			if (m_haze != null) {
				SetParticles (m_haze, 10f);
			} else {
				Debug.Log ("If we had a overcast effect, we'd play it.");
			}

//			if (m_dayNight) {
//				m_dayNight.m_isGrey = true;
//				m_dayNight.fogScale = .01f;
//			}
			break;
		case TypeOfWeather.PartlyCloudy:
			if (m_clouds != null) {
				Vector3 pos = new Vector3 (m_clouds.transform.position.x, -2.25f, m_clouds.transform.position.z);
				Vector3 shape = new Vector3 (15, 3, 15);
				SetParticles (m_clouds, 1f, pos, shape);
				m_clouds.Play ();
			} else {
				Debug.Log ("If we had a cloud effect, we'd play it.");
			}

			if (m_haze != null)
				SetParticles (m_haze, 5f);

			break;
		case TypeOfWeather.Snow:
			if (m_snow != null) {
				SetParticles (m_snow, 1000f);
			} else {
				Debug.Log ("If we had a snow effect, we'd play it.");
			}

			if (m_haze != null)
				SetParticles (m_haze, 5f);

			if (m_clouds != null) {
				//change the cloud count to be higher than when partly cloudy
				Vector3 pos = new Vector3 (m_clouds.transform.position.x, 3, m_clouds.transform.position.z);
				Vector3 shape = new Vector3 (4, 10, 15);
				//setup and play the particles
				SetParticles (m_clouds, 10f, pos, shape);

			}

			Camera.main.backgroundColor = Color.gray;
//			if (m_dayNight) {
//				m_dayNight.m_isGrey = true;
//				m_dayNight.fogScale = .1f;
//			}
//
			break;
		default :
		//Don't play any particles by default
			break;
		}
	}

	void SetParticles (ParticleSystem ps, float rateMultiplier, Vector3 _position = default( Vector3), Vector3 _shape = default(Vector3))
	{
		if (_position != Vector3.zero)
			ps.transform.position = _position;

		var em = ps.emission;
		em.rateOverTimeMultiplier = rateMultiplier;

		if (_shape != Vector3.zero) {
			var shape = ps.shape;
			shape.box = _shape;
		}
		ps.Play ();
	}

	void HandleForecast (string s, float f)
	{
		Debug.Log ("Particles got the Forecast at " + f.ToString () + ", " + s + " at " + Time.time);
	}
}

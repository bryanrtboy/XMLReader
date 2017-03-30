using UnityEngine;

public class InternetChecker : MonoBehaviour
{

	public static InternetChecker instance;

	[HideInInspector]
	public bool m_hasInternet = false;
	//poll this from other scripts before assuming you have a live connection

	public bool m_autoRecheck = false;
	public float m_autoRecheckRate = 10f;

	private const bool m_allowCarrierDataNetwork = false;
	private const string m_pingAddress = "8.8.8.8";
	// Google Public DNS server
	private const float m_waitingTime = 2.0f;

	private Ping m_ping;
	private float m_pingStartTime;


	void Awake ()
	{
		//Make sure this is in fact the only instance (Singleton pattern)
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);    
            
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad (gameObject);
	}

	void Start ()
	{
		StartInternetUpdate ();

		if (m_autoRecheck)
			InvokeRepeating ("StartInternetUpdate", m_autoRecheckRate * 2f, m_autoRecheckRate); //Start re-checking after a delay for the first check to run...
	}

	void Update ()
	{
		if (m_ping != null) {
			bool stopCheck = true;
			if (m_ping.isDone) {
				if (m_ping.time >= 0)
					InternetAvailable ();
				else
					InternetIsNotAvailable ();
			} else if (Time.time - m_pingStartTime < m_waitingTime)
				stopCheck = false;
			else
				InternetIsNotAvailable ();
			if (stopCheck)
				m_ping = null;
		}
	}

	public void StartInternetUpdate ()
	{
		bool internetPossiblyAvailable;
		switch (Application.internetReachability) {
		case NetworkReachability.ReachableViaLocalAreaNetwork:
			internetPossiblyAvailable = true;
			break;
		case NetworkReachability.ReachableViaCarrierDataNetwork:
			internetPossiblyAvailable = m_allowCarrierDataNetwork;
			break;
		default:
			internetPossiblyAvailable = false;
			break;
		}
		if (!internetPossiblyAvailable) {
			InternetIsNotAvailable ();
			return;
		}
		m_ping = new Ping (m_pingAddress);
		m_pingStartTime = Time.time;
	}

	private void InternetIsNotAvailable ()
	{
		Debug.Log ("No Internet :(");
		m_hasInternet = false;
	}

	private void InternetAvailable ()
	{
		Debug.Log ("Internet is available! ;)");
		m_hasInternet = true;
	}
}
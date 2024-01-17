using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using PimDeWitte.UnityMainThreadDispatcher;
using Vuforia;

public class VectorCalculation : MonoBehaviour {
	public GameObject smartwatch;
	private Animation smartwatchAnimation;

	public DartboardManager DartboardManager;
	private bool isPlaying = false;
	private bool canThrow = false;
	private List<GameObject> activeDarts = new List<GameObject>();
	public GameObject DartPrefab;
	private GameObject currentDart;
	public Transform DartOrigin;

    public DynamicTextElement TextElementScore;
    public GameObject ScoreText;
    public Transform ScoreTextOrigin;

    public GameObject WatchTarget;	//used to place darts
    private ObserverBehaviour watchTargetBehaviour;
    private Status watchTargetStatus;

    private UdpClient udpServer;
    private int port = 4455;
    private float[] orientationCalibration = { 0, 0, 0 };
    private float[] currentOrientation = { 0, 0, 0 };

    private void Start () {
		udpServer = new UdpClient(port);
		udpServer.BeginReceive(UdpMessageCallback, null);
		
		smartwatchAnimation = smartwatch.GetComponent<Animation>();
		smartwatch.SetActive(true);
		smartwatchAnimation.Play();
		
		if (DartboardManager != null) {
			DartboardManager.Subscribe(DartManagerCallback);
		}

		if (WatchTarget != null) {
			watchTargetBehaviour = WatchTarget.GetComponent<ObserverBehaviour>();
			watchTargetBehaviour.OnTargetStatusChanged += OnWatchTargetStatusChanged;
		}
	}

    private void OnWatchTargetStatusChanged(ObserverBehaviour observerBehaviour, TargetStatus status) {
	    watchTargetStatus = status.Status;
    }

    private void DartManagerCallback(DartGameEvents gameEvent) {
		switch (gameEvent) {
			case (DartGameEvents.GameStart):
				isPlaying = true;
				break;
			case (DartGameEvents.StartTurn):
				if(activeDarts.Count > 0) {
					foreach (GameObject dart in activeDarts) {
						Destroy(dart);
					}
					activeDarts.Clear();
				}
				break;
			case (DartGameEvents.StartThrow):
				canThrow = true;
				var newDart = Instantiate(DartPrefab);
				newDart.SetActive(true);
				newDart.transform.position = DartOrigin.position;
				newDart.transform.rotation = DartOrigin.rotation;
				newDart.GetComponent<Dart>().DartboardManager = DartboardManager;
				activeDarts.Add(newDart);
				currentDart = newDart;
				break;
			case (DartGameEvents.EndThrow):
				UpdateBoardScore();
				break;
			case (DartGameEvents.GameOver):
				isPlaying = false;
				break;
		}
	}

    private void Update () {
	    if (Input.GetKeyUp(KeyCode.S) && !isPlaying) {
		    DartboardManager.StartGame();
	    }
	    if (Input.GetKeyUp(KeyCode.E) && isPlaying) {
		    DartboardManager.EndTurn();
	    }
	    if (Input.GetKeyUp(KeyCode.Q) && isPlaying) {
		    DartboardManager.EndGame();
	    }
	    
	    if (canThrow && isPlaying && watchTargetStatus is Status.TRACKED or Status.EXTENDED_TRACKED) {
		    currentDart.transform.position = WatchTarget.transform.position;
	    } else if (!canThrow && isPlaying && watchTargetStatus is Status.TRACKED or Status.EXTENDED_TRACKED) {
		    DartboardManager.EndThrow();
	    }
    }

    private void OnDestroy() {
	    udpServer.Close();
    }
    
    private void UpdateBoardScore() {
	    var scoreTextOriginTransform = ScoreTextOrigin.transform;
	    ScoreText.transform.position = scoreTextOriginTransform.position;
	    ScoreText.transform.rotation = scoreTextOriginTransform.rotation;
	    ScoreText.transform.localScale = scoreTextOriginTransform.localScale;
	    
	    var lastThrow = DartboardManager.GetLastThrowInfo();
	    if (lastThrow.Type == DartSectionTypes.Miss) {
		    TextElementScore.AnimateTextChange(DartboardManager.GetPlayerScore().ToString());
		    DynamicTextElement.ShowTextEffect(ScoreText, "Miss");
	    } else {
		    TextElementScore.AnimateTextChange(DartboardManager.GetPlayerScore().ToString());
		    DynamicTextElement.ShowTextEffect(ScoreText, lastThrow.Score.ToString());
	    }
    }

    private void Throw(float[] acceleration) {
	    if (canThrow && isPlaying) {
		    currentDart.GetComponent<Dart>().Throw(acceleration[2]/4);
		    canThrow = false;
	    }
    }

    private void HandleMessage(string message) {
	    var messageSplit = message.Split('|');
	    var action = messageSplit[0];
	    var values = messageSplit[1];
	    
	    switch (action) {
		    case "ORIENTATION": {
			    currentOrientation = StringToFloatArray(values);
			    if (canThrow) {
				    currentDart.transform.rotation = Quaternion.Euler(
					    currentOrientation[0] - orientationCalibration[0],
					    currentOrientation[1] - orientationCalibration[1],
					    currentOrientation[2] - orientationCalibration[2]
				    );
			    }
			    break;
		    }
		    case "THROW":
			    Throw(StringToFloatArray(values));
			    break;
	    }
    }
    
    public void Calibrate() {
	    smartwatchAnimation.Stop();
	    smartwatch.SetActive(false);
	    orientationCalibration = currentOrientation;
	    orientationCalibration[1] += 90;
    }
    
    private static float[] StringToFloatArray(string str) {
	    var values = str.Split(',');
	    var floatArray = new float[values.Length];

	    for (var i = 0; i < values.Length; i++) {
		    floatArray[i] = float.Parse(values[i].Trim(), CultureInfo.InvariantCulture);
	    }

	    return floatArray;
    }

    private void UdpMessageCallback(IAsyncResult ar) {
	    var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
	    var data = udpServer.EndReceive(ar, ref clientEndPoint);
	    
	    var message = System.Text.Encoding.UTF8.GetString(data);

	    UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(message));

	    udpServer.BeginReceive(UdpMessageCallback, null);
    }
}
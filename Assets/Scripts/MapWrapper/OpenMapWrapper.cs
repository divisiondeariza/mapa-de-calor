﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;	
using UnityEngine;
using UnityEngine.UI;
using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;

public class OpenMapWrapper : MonoBehaviour
{
	//public TestMap testMap;
	public InputField DirectionInputField;
	public Texture	LocationTexture;
	public Texture	MarkerTexture;
	private GameObject go;
	protected Map		map;
	private Ray pulsacion;
	private RaycastHit colision;
	private bool isMarkerSet;
	public Dictionary<string, double> LastPutMarkerCoordinates = null;
	protected bool isOnMainWindow = true;
	protected InputReader inputReader;
	public GameObject ReportTrigger;
	protected double PrivateTriggerMovementManager;



	const float timeToLongPress = 1f;
	
	private float timeSincePress = 0f;
	private bool isLongPressing;

	public int MarkersCount {
		get {
			return 0;
		}
	}

	public bool HasTemporalMarker {
		get{
			return true;
		}
	}

	public void SetTemporalMarker ()
	{

	}

	public void AddTemporalMarker ()
	{
	}

	// Use this for initialization
	void Start ()
	{
		SetupMapInstance ();
		SetupVirtualEarthLayer ();
		DrawGPSUserLocation ();
		SetUpInputReader ();
		PrivateTriggerMovementManager = map.CenterWGS84 [0];
	
	}
	
	public void SetUpInputReader(){
		inputReader = new InputReader ();
		inputReader.SetGenerator (GetValidInputGenerator());
		inputReader.LongPressExecuted +=()=>{
			SetSingleMarkerOnMap();
			ReportTrigger.SetActive (true);
			PrivateTriggerMovementManager = map.CenterWGS84 [0];
		};
	}

	private InputGenerator GetValidInputGenerator()
	{
		#if UNITY_EDITOR
			return GetComponent<EditorInputGenerator>();
		#else
			return GetComponent<DeviceInputGenerator>();
		#endif
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(isOnMainWindow==true){
			inputReader.Update();
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		if(PrivateTriggerMovementManager!=map.CenterWGS84[0]&&ReportTrigger.activeInHierarchy==true){
			ReportTrigger.SetActive(false);
			RemoveLastPutMarker();
		}
	}

	public void SetSingleMarkerOnMap ()
	{
		if (LastPutMarkerCoordinates != null) {
			RemoveLastPutMarker ();
			
		}
		SetMarkerInMap ();
	}

	public void SetMarkerInMap ()
	{


		Dictionary<string, double> CursorCoordinates = GetCoordinatesOfCursor ();
		CreateAnnotation (CursorCoordinates ["latitude"], CursorCoordinates ["longitude"]);
		SetCoordinatesOnInputField (CursorCoordinates ["latitude"], CursorCoordinates ["longitude"]);
		LastPutMarkerCoordinates = CursorCoordinates;


	}

	public void RemoveLastPutMarker ()
	{
		RemoveMarker (LastPutMarkerCoordinates ["longitude"],LastPutMarkerCoordinates ["latitude"]);
	}

	public void RemoveMarker(double latitude, double longitude){
		Marker[] markers = map.GetComponentsInChildren<Marker> ();
		foreach (Marker marker in markers) {
			if (marker.CoordinatesWGS84 [0] == latitude && marker.CoordinatesWGS84 [1] == longitude) {
				map.RemoveMarker (marker);
				SetNullLastPutMarkerCoordinates ();
			}
		}
	}

	public void SetCoordinatesOnInputField (double latitude, double longitude)
	{
		DirectionInputField.text = latitude.ToString () + " , " + longitude.ToString ();
	}

	public void SetNullLastPutMarkerCoordinates ()
	{
		LastPutMarkerCoordinates = null;
	}
	
	public void SetupVirtualEarthLayer ()
	{

		
		// create a VirtualEarth tile layer
		VirtualEarthTileLayer virtualEarthLayer = map.CreateLayer<VirtualEarthTileLayer> ("VirtualEarth");
		virtualEarthLayer.Key = "Ag-ML2n_NjUqTCNOJyd9Yyr-GRfEWVmY_yboAe3A3aUL2JrE1d9er914Tfs9kgrp";
		virtualEarthLayer.gameObject.SetActive (true);
		

		
	}

	public void SetupOpenMapsLayer(){
		// create an OSM tile layer

		OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer>("OSM");
		osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";
		

	}
	
	public void SetupMapInstance ()
	{
		// create the map singleton
		map = Map.Instance;
		map.CurrentCamera = Camera.main;
		map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard; 
		map.CurrentZoom = 15.0f;
		map.CenterWGS84 = new double[2] { -74.084046, 4.638194 };
		map.UseLocation = true;
		map.InputsEnabled = true;
		map.ShowGUIControls = false;


		
	}
	
	public Dictionary<string, double> GetCoordinatesOfCursor ()
	{
		Dictionary<string, double> dictionary = new Dictionary<string, double> ();
		
		Vector3 wordPos = getCursorPosition ();
		double latitude = (0.0167 * wordPos [2]) + ((map.CenterWGS84) [1]);
		double longitude = (0.0167 * wordPos [0]) + ((map.CenterWGS84) [0]);
		dictionary.Add ("latitude", latitude);
		dictionary.Add ("longitude", longitude);
		return dictionary;
		
	}
	
	public Vector3 getCursorPosition ()
	{
		Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f);
		ReportTrigger.transform.position = mousePos;
		Ray ray = Camera.main.ScreenPointToRay (mousePos);
		RaycastHit hit;
		
		Vector3 wordPos;
		if (Physics.Raycast (ray, out hit, 1000f)) {
			
			wordPos = hit.point;
			
		} else {
			
			wordPos = Camera.main.ScreenToWorldPoint (Input.GetTouch (0).position);
		}
		
		return wordPos;
	}
	
	public void DrawGPSUserLocation ()
	{
		GameObject markerGO = CreateMarkerGameObject (Tile.AnchorPoint.MiddleCenter, LocationTexture, 4001, new Vector3 (1.0f, 1.0f, 1.0f) / 27);
		map.SetLocationMarker<LocationMarker> (markerGO);
		DestroyImmediate (go);
	}
	

	public void CreateAnnotation (double latitude, double longitude)
	{
		GameObject markerGO = CreateMarkerGameObject (Tile.AnchorPoint.BottomCenter, MarkerTexture, 4000, new Vector3 (1.0f, 1.0f, 1.0f) / 27);
		map.CreateMarker<Marker> ("Marker", new double[2] {
			longitude,
			latitude
		}, markerGO);
		DestroyImmediate (go);
		
	}
	
	public GameObject CreateMarkerGameObject (Tile.AnchorPoint anchorPoint, Texture mainTexture, int renderQueue, Vector3 localScale)
	{
		go = Tile.CreateTileTemplate (anchorPoint).gameObject;
		go.renderer.material.mainTexture = mainTexture;
		go.renderer.material.renderQueue = renderQueue;
		go.transform.localScale = localScale;
		GameObject markerGO = Instantiate (go) as GameObject;
		return markerGO;
		
	}

	public void ToggleisOnMainWindow(){
		isOnMainWindow = !isOnMainWindow;
	}

	public void SetToTrueIsOnMainWindow(){
		isOnMainWindow = true;
	}
}



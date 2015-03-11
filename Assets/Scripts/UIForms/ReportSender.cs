﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReportSender: MonoBehaviour {

	public InputField ubicationField;
	public InputField commentsField;
	public InputField dateField;
	public InputField hourField;
	public Button submitButton;

	public GameObject reportWindow;

	void Start(){
		submitButton.onClick.AddListener (delegate {

			if(!isEmpty(ubicationField.text) && isValidDate(dateField.text) && isValidHour(hourField.text)){
				resetColorValidation();
				HandleSubmitClicked();
				reportWindow.SetActive(false);
				ClearReportInput();
			}else{
				if(isEmpty(ubicationField.text)){
					ubicationField.image.color = Color.red;
				}
				if(!isValidDate(dateField.text)){
					dateField.image.color = Color.red;
				}
				if(!isValidHour(hourField.text)){
					hourField.image.color = Color.red;
				}
			}

		});

		ubicationField.onValueChange.AddListener (delegate {
			ubicationField.image.color = Color.white;
		});
		dateField.onValueChange.AddListener (delegate {
			dateField.image.color = Color.white;
		});
		hourField.onValueChange.AddListener (delegate {
			hourField.image.color = Color.white;
		});
	}

	void resetColorValidation ()
	{
		ubicationField.image.color = Color.white;
		commentsField.image.color = Color.white;
		dateField.image.color = Color.white;
		hourField.image.color = Color.white;
	}

	private void HandleSubmitClicked ()
	{
		FormData formdata = CreateFormData ();
		ReportSaver saver = new ReportSaver();
		saver.SetStorage (AppConfig.GetStorage());
		saver.Save (formdata);

	}

	private FormData CreateFormData ()
	{
		FormData data = new FormData ();
		data.annotation = GetAnnotation ();
		data.comments = commentsField.text;
		data.timestamp = GetTimeStamp ();
		return data;
	}

	int GetTimeStamp ()
	{
		String[] date = dateField.text.Split(new String[1]{"-"},StringSplitOptions.None);
		String[] hour = hourField.text.Split(new String[1]{":"},StringSplitOptions.None);
		Debug.Log (date.Length);
		return ConvertToUnixTimestamp(ConvertToDateTime(date, hour));
	}

	DateTime ConvertToDateTime (string[] date, string[] hour)
	{
		int year = 1990, month = 1, day = 1;

		year = int.Parse(date [2]);
		month = int.Parse(date [1]);
		day = int.Parse(date [0]);

		int hh = int.Parse(hour [0]);
		int minute = int.Parse(hour [1]);
		return new DateTime (year, month, day, hh, minute, 0);
	}


	private Vector2 GetAnnotation ()
	{
		string ubicationText = ubicationField.text;
		string[] coordinates = ubicationText.Split (new string[1]{" , "}, StringSplitOptions.None);
		return new Vector2 (float.Parse(coordinates[0]), float.Parse(coordinates[1]));
	}

	public static int ConvertToUnixTimestamp(DateTime date)
	{
		DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		TimeSpan diff = date.ToUniversalTime() - origin;
		Debug.Log ((int)diff.TotalSeconds);
		return (int)diff.TotalSeconds;
	}
	
	public void ClearReportInput(){
		ubicationField.text = "";
		commentsField.text = "";
		dateField.text = "";
		hourField.text = "";
	}

	public bool isEmpty (String text){
		if (text.Equals (""))
			return true;
		return false;
	}

	public bool isValidDate(String date){
		try {
			DateTime.Parse(date);
			return true;
		} catch {
			return false;
		}
	}

	public bool isValidHour(string hour) {
		try {
			string pattern = "^\\d{2}:\\d{2}$";
			if(System.Text.RegularExpressions.Regex.IsMatch(hour,pattern)) {
				TimeSpan span = TimeSpan.Parse(hour);
				return true;
			}
			return false;
		} catch {
			return false;
		}
	}
}
public class AppConfig{
	
	static public IDataStorage GetStorage(){
		return new PlayerPrefStorage();
	}
}

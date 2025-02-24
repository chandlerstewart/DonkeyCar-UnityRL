using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class MenuHandler : MonoBehaviour {

	public GameObject menuPanel;
	public GameObject stopPanel;
	public GameObject exitPanel;
    public GameObject carJSControl;

    public TrainingManager trainingManager;

    public void Awake()
    {
        //keep it processing even when not in focus.
        Application.runInBackground = true;

        //Set desired frame rate as high as possible.
        Application.targetFrameRate = 60;

		menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);
    }

	public void OnPidGenerateTrainingData()
	{
       

		if(carJSControl != null)
			carJSControl.SetActive(false);

	
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnManualGenerateTrainingData()
	{

		if(carJSControl != null)
			carJSControl.SetActive(true);
	
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnPidDrive()
	{

		if(carJSControl != null)
			carJSControl.SetActive(false);


		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnManualDrive()
	{

		if(carJSControl != null)
			carJSControl.SetActive(true);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

    public void OnNextTrack()
	{
		if(trainingManager != null)
			trainingManager.OnMenuNextTrack();
    }

    public void OnRegenTrack()
	{
		if(trainingManager != null)
			trainingManager.OnMenuRegenTrack();
    }

    public void OnStop()
    {

        if (carJSControl != null)
            carJSControl.SetActive(false);


        menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);
    }

}

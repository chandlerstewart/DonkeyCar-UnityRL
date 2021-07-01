using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using UnityStandardAssets.CrossPlatformInput;

public class Car : Agent, ICar {

	public WheelCollider[] wheelColliders;
	public Transform[] wheelMeshes;

	public float maxTorque = 50.0f;
	public float maxSpeed = 10.0f;

	public Transform centrOfMass;

	public float requestTorque = 0f;
	public float requestBrake = 0f;
	public float requestSteering = 0f;

	public Vector3 acceleration = Vector3.zero;
	public Vector3 prevVel = Vector3.zero;

	public Vector3 startPos;
	public Quaternion startRot;

	public float length = 1.7f;

	Rigidbody rb;

	//for logging
	public float lastSteer = 0.0f;
	public float lastAccel = 0.0f;

	//when the car is doing multiple things, we sometimes want to sort out parts of the training
	//use this label to pull partial training samples from a run 
	public string activity = "keep_lane";

    public float maxSteer = 25.0f;

	//name of the last object we hit.
	public string last_collision = "none";


	// Use this for initialization
	public override void Initialize() 
	{
		rb = GetComponent<Rigidbody>();
		

		if (rb && centrOfMass)
		{
			rb.centerOfMass = centrOfMass.localPosition;
			
		}

		requestTorque = 0f;
		requestSteering = 0f;

		SavePosRot();

        maxSteer = PlayerPrefs.GetFloat("max_steer", 25);
		InvokeRepeating("CheckReward", 1f, 1f);
	}

    public override void OnEpisodeBegin()
    {
		ResetCar();
    }


    public void SavePosRot()
	{
		startPos = transform.position;
		startRot = transform.rotation;
	}

	public void RestorePosRot()
	{
		Set(startPos, startRot);
	}

	public void RequestThrottle(float val)
	{
		requestTorque = val;
		requestBrake = 0f;
		//Debug.Log("request throttle: " + val);
	}

    public void SetMaxSteering(float val)
    {
        maxSteer = val;

        PlayerPrefs.SetFloat("max_steer", maxSteer);
        PlayerPrefs.Save();
    }

    public float GetMaxSteering()
    {
        return maxSteer;
    }

	public void RequestSteering(float val)
	{
		requestSteering = Mathf.Clamp(val, -maxSteer, maxSteer);
		//Debug.Log("request steering: " + val);
	}

	public void Set(Vector3 pos, Quaternion rot)
	{
		rb.position = pos;
		rb.rotation = rot;

		//just setting it once doesn't seem to work. Try setting it multiple times..
		StartCoroutine(KeepSetting(pos, rot, 10));
	}

	void CheckReward()
    {
		if (rb.velocity.magnitude >= maxSpeed / 3 && IsGrounded())
        {
			AddReward(0.01f);
        }
    }

	IEnumerator KeepSetting(Vector3 pos, Quaternion rot, int numIter)
	{
		while(numIter > 0)
		{
			rb.position = pos;
			rb.rotation = rot;
			transform.position = pos;
			transform.rotation = rot;

			numIter--;
			yield return new WaitForFixedUpdate();
		}
	}

	public float GetSteering()
	{
		return requestSteering;
	}

	public float GetThrottle()
	{
		return requestTorque;
	}

	public float GetFootBrake()
	{
		return requestBrake;
	}

	public float GetHandBrake()
	{
		return 0.0f;
	}

	public Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public Vector3 GetAccel()
	{
		return acceleration;
	}

	public float GetOrient ()
	{
		Vector3 dir = transform.forward;
		return Mathf.Atan2( dir.z, dir.x);
	}

	public Transform GetTransform()
	{
		return this.transform;
	}

	public bool IsStill()
	{
		return rb.IsSleeping();
	}

	public void RequestFootBrake(float val)
	{
		requestBrake = val;
	}

	public void RequestHandBrake(float val)
	{
		//todo
	}
	
	// Update is called once per frame
	void Update () {
	
		UpdateWheelPositions();
	}

	public string GetActivity()
	{
		return activity;
	}

	public void SetActivity(string act)
	{
		activity = act;
	}

	private void ResetCar()
    {
		RequestThrottle(0.0f);
		RequestHandBrake(1.0f);
		RequestFootBrake(1.0f);
		RequestSteering(0.0f);
		acceleration = Vector3.zero;
		prevVel = Vector3.zero;

		rb.angularVelocity.Set(0, 0, 0);
		rb.velocity.Set(0, 0, 0);
		rb.Sleep();


		RestorePosRot();
	}

	void FixedUpdate()
	{

		if (!IsGrounded())
        {
			AddReward(-.1f);

            EndEpisode();
		}


		lastSteer = requestSteering;
		lastAccel = requestTorque;

		float throttle = requestTorque * maxTorque;
		float steerAngle = requestSteering;
        float brake = requestBrake;


		//front two tires.
		wheelColliders[2].steerAngle = steerAngle;
		wheelColliders[3].steerAngle = steerAngle;

		//four wheel drive at the moment
		foreach(WheelCollider wc in wheelColliders)
		{
			if(rb.velocity.magnitude < maxSpeed)
			{
				wc.motorTorque = throttle;
			}
			else
			{
				wc.motorTorque = 0.0f;
			}

			wc.brakeTorque = 400f * brake;
		}

		acceleration = rb.velocity - prevVel;
	}

	void FlipUpright()
	{
		Quaternion rot = Quaternion.Euler(180f, 0f, 0f);
		this.transform.rotation = transform.rotation * rot;
		transform.position = transform.position + Vector3.up * 2;
	}

    public override void CollectObservations(VectorSensor sensor)
    {
		sensor.AddObservation(requestSteering);

		sensor.AddObservation(requestTorque);
    }

    public override void OnActionReceived(float[] vectorAction)
	{
		float h = vectorAction[0];
		float v = vectorAction[1];

		RequestSteering(h * maxSteer);
		RequestThrottle(v);
	}

	public override void Heuristic(float[] actionsOut)
	{
		if (Input.GetKeyDown(KeyCode.Space))
        {
			ScreenshotHandler.TakeScreenshot_Static(500, 500);
        }


		float h = CrossPlatformInputManager.GetAxis("Horizontal");
		float v = CrossPlatformInputManager.GetAxis("Vertical");

		actionsOut[0] = h;
		actionsOut[1] = v;
	}



	void UpdateWheelPositions()
	{
		Quaternion rot;
		Vector3 pos;

		for(int i = 0; i < wheelColliders.Length; i++)
		{
			WheelCollider wc = wheelColliders[i];
			Transform tm = wheelMeshes[i];

			wc.GetWorldPose(out pos, out rot);

			tm.position = pos;
			tm.rotation = rot;
		}
	}

	//get the name of the last object we collided with
	public string GetLastCollision()
	{
		return last_collision;
	}

	public void ClearLastCollision()
	{
		last_collision = "none";
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.transform.CompareTag("ground"))
        {
			Debug.Log("OnCollisionEnter");
        }

		last_collision = col.gameObject.name;
	}

	void OnCollisionExit(Collision other)
    {
		if (other.transform.CompareTag("ground"))
		{
			Debug.Log("OnCollisionExit");

		}
	}

	private bool IsGrounded() {


		bool[] grounded = new bool[5];
			
		grounded[0] = Physics.Raycast(rb.transform.position, -Vector3.up, 5);
		grounded[1] = Physics.Raycast(rb.transform.position - new Vector3(.5f, 0, 0), -Vector3.up, 5);
		grounded[2] = Physics.Raycast(rb.transform.position + new Vector3(.5f, 0, 0), -Vector3.up, 5);
		grounded[3] = Physics.Raycast(rb.transform.position - new Vector3(0, 0, 1), -Vector3.up, 5);
		grounded[4] = Physics.Raycast(rb.transform.position + new Vector3(0, 0, 1), -Vector3.up, 5);

		foreach (bool ground in grounded)
        {
			if (ground == false)
            {
				return false;
            }
        }
		
		return true;
	}
}

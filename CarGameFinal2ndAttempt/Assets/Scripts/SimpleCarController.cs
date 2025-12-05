using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCarController : MonoBehaviour {
	public ParticleSystem ps;
	public GameObject MainCamera;
	public GameObject MainCamera1;
	public GameObject ps1;

	public void GetInput()
	{
		m_horizontalInput = Input.GetAxis("Horizontal");
		m_verticalInput = Input.GetAxis("Vertical");
	}

	private void Steer()
	{
		m_steeringAngle = maxSteerAngle * m_horizontalInput;
		frontDriverW.steerAngle = m_steeringAngle;
		frontPassengerW.steerAngle = m_steeringAngle;
	}

	public  void Accelerate()
	{
		frontDriverW.motorTorque = m_verticalInput * motorForce;
		frontPassengerW.motorTorque = m_verticalInput * motorForce;

		Debug.Log(frontDriverW.motorTorque);
	}

	private void UpdateWheelPoses()
	{
		UpdateWheelPose(frontDriverW, frontDriverT);
		UpdateWheelPose(frontPassengerW, frontPassengerT);
		UpdateWheelPose(rearDriverW, rearDriverT);
		UpdateWheelPose(rearPassengerW, rearPassengerT);
	}

	private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
	{
		Vector3 _pos = _transform.position;
		Quaternion _quat = _transform.rotation;

		_collider.GetWorldPose(out _pos, out _quat);

		_transform.position = _pos;
		_transform.rotation = _quat;
	}

	private void FixedUpdate()
	{
		GetInput();
		Steer();
		Accelerate();
		UpdateWheelPoses();
		if (Input.GetKeyDown("space"))
		{
			MainCamera1.SetActive(true);
			MainCamera.SetActive(false);
			motorForce = motorForce + 100;
			ps1.SetActive(true);
			ps.Play();


		}
		else if(Input.GetKeyUp("space"))
		{
			gameObject.active = true;
			MainCamera.SetActive(true);
			MainCamera1.SetActive(false);
			MainCamera1.SetActive(false);
			ps1.SetActive(true);
			motorForce = motorForce - 100;
			

		}

	}


	private float m_horizontalInput;
	private float m_verticalInput;
	private float m_steeringAngle;

	public WheelCollider frontDriverW, frontPassengerW;
	public WheelCollider rearDriverW, rearPassengerW;
	public Transform frontDriverT, frontPassengerT;
	public Transform rearDriverT, rearPassengerT;
	public float maxSteerAngle = 30;
	public float motorForce = 50;
}

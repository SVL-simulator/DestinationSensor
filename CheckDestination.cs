/**
 * Copyright (c) 2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using Simulator.Api;
using Simulator.Sensors;

public class CheckDestination : MonoBehaviour
{
    private GameObject SensorGO;
    private GameObject AgentGO;
    private DestinationSensor Sensor;

    private void Start()
    {
        SensorGO = transform.parent.gameObject;
        AgentGO = SensorGO.transform.parent.gameObject;
        Sensor = SensorGO.GetComponent<DestinationSensor>();
    }

    private void OnTriggerStay(Collider destination)
    {
        if (Sensor.AngleDiff < Sensor.DestinationCheckAngle)
        {
            destination.gameObject.SetActive(false);
            Sensor.SuccessCount += 1;
            ApiManager.Instance?.AddDestinationReached(AgentGO);
        }
    }
}

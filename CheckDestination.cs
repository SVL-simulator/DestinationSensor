/**
 * Copyright (c) 2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using Simulator.Api;

public class CheckDestination : MonoBehaviour
{
    private void OnTriggerEnter(Collider destination)
    {
        destination.gameObject.SetActive(false);
        var sensorGO = transform.parent.gameObject;
        var agentGO = sensorGO.transform.parent.gameObject;
        ApiManager.Instance?.AddDestinationReached(agentGO);
    }
}

/**
 * Copyright (c) 2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using Simulator.Bridge;
using Simulator.Utilities;
using UnityEngine;
using Simulator.Sensors.UI;
using System.Collections.Generic;
using System;
using Simulator.Map;

namespace Simulator.Sensors
{
    [SensorType("Destination", new System.Type[] { typeof(Bridge.Data.Ros.PoseStamped), typeof(Bridge.Data.Ros.PoseWithCovarianceStamped) })]
    public class DestinationSensor : SensorBase
    {
        [SensorParameter]
        public string InitPoseTopic;

        [SensorParameter]
        public string InitPoseFrame;

        [SensorParameter]
        [Range(1.0f, 10.0f)]
        public float DestinationCheckRadius = 1.0f;

        private BridgeInstance Bridge;
        private Publisher<Bridge.Data.Ros.PoseStamped> Publish;
        private Publisher<Bridge.Data.Ros.PoseWithCovarianceStamped> PublishInitPose;
        private Bridge.Data.Ros.PoseStamped Destination;
        private Bridge.Data.Ros.PoseWithCovarianceStamped InitPose;
        private bool SendDestination = false;
        private bool SendInitPose = false;
        private NavOrigin NavOrigin;
        private GameObject DestinationGO;
        private Transform Marker;

        protected override void Initialize()
        {
            NavOrigin = NavOrigin.Find();
            Marker = transform.Find("Marker");
        }

        protected override void Deinitialize()
        {
            if (DestinationGO)
            {
                Destroy(DestinationGO);
            }
        }

        public override void OnBridgeSetup(BridgeInstance bridge)
        {
            if (bridge.Plugin.GetBridgeNameAttribute().Name != "ROS2")
            {
                Debug.LogWarning("Destination Sensor only works with ROS2 bridge.");
                return;
            }

            try
            {
                var sensorBridgePlugin = Activator.CreateInstance(GetDataBridgePlugin()) as ISensorBridgePlugin;
                sensorBridgePlugin.Register(bridge.Plugin);
            }
            catch (System.ArgumentException)
            {
                Debug.Log("Bridge plugin is already registered");
            }

            Bridge = bridge;
            Publish = Bridge.AddPublisher<Bridge.Data.Ros.PoseStamped>(Topic);
            PublishInitPose = Bridge.AddPublisher<Bridge.Data.Ros.PoseWithCovarianceStamped>(InitPoseTopic);
        }

        public override System.Type GetDataBridgePlugin()
        {
            return typeof(DestinationData);
        }

        public void Update()
        {
            if (Bridge != null && Bridge.Status == Status.Connected)
            {
                if (SendDestination)
                {
                    Publish(Destination);
                    SendDestination = false;
                }

                if (SendInitPose)
                {
                    PublishInitPose(InitPose);
                    SendInitPose = false;
                }
            }
            else
            {
                Debug.Log($"{Bridge != null} {Bridge?.Status}");
            }
        }

        private Bridge.Data.Ros.Time GetRosTime()
        {
            var time = SimulatorManager.Instance.CurrentTime;
            long nanosec = (long)(time * 1e9);

            var ros_time = new Bridge.Data.Ros.Time()
            {
                secs = (int)(nanosec / 1000000000),
                nsecs = (uint)(nanosec % 1000000000),
            };

            return ros_time;
        }

        public void SetInitialPose()
        {
            var nav_pose = NavOrigin.GetNavPose(transform);

            InitPose = new Bridge.Data.Ros.PoseWithCovarianceStamped
            {
                header = new Bridge.Data.Ros.Header()
                {
                    stamp = GetRosTime(),
                    frame_id = InitPoseFrame,
                },
                pose = new Bridge.Data.Ros.Pose
                {
                    position = new Bridge.Data.Ros.Point
                    {
                        x = nav_pose.position.x,
                        y = nav_pose.position.y,
                        z = 0.0,
                    },
                    orientation = new Bridge.Data.Ros.Quaternion
                    {
                        x = 0.0,
                        y = 0.0,
                        z = nav_pose.orientation.z,
                        w = nav_pose.orientation.w
                    }
                }
            };

            SendInitPose = true;
        }

        public void SetDestination(Vector3 position, Vector3 rotation, bool init=false)
        {
            if (init)
            {
                SetInitialPose();
            }

            if (DestinationGO)
            {
                Destroy(DestinationGO);
            }

            position.y = transform.position.y;
            DestinationGO = new GameObject("Destination");
            DestinationGO.transform.position = position;
            DestinationGO.transform.rotation = Quaternion.Euler(rotation);
            DestinationGO.transform.parent = NavOrigin.transform;
            DestinationGO.layer = LayerMask.NameToLayer("Destination");
            SphereCollider col = DestinationGO.AddComponent<SphereCollider>();
            col.radius = DestinationCheckRadius;

            var nav_pose = NavOrigin.GetNavPose(DestinationGO.transform);

            Destination = new Bridge.Data.Ros.PoseStamped
            {
                header = new Bridge.Data.Ros.Header()
                {
                    stamp = GetRosTime(),
                    frame_id = Frame,
                },
                pose = new Bridge.Data.Ros.Pose
                {
                    position = new Bridge.Data.Ros.Point
                    {
                        x = nav_pose.position.x,
                        y = nav_pose.position.y,
                        z = 0.0,
                    },
                    orientation = new Bridge.Data.Ros.Quaternion
                    {
                        x = 0.0,
                        y = 0.0,
                        z = nav_pose.orientation.z,
                        w = nav_pose.orientation.w
                    }
                }
            };

            SendDestination = true;
        }

        public override void OnVisualize(Visualizer visualizer) {}

        public override void OnVisualizeToggle(bool state)
        {
            if (Marker && DestinationGO)
            {
                Marker.position = DestinationGO.transform.position + new Vector3(0, 1, 0);
                Marker.GetComponent<MeshRenderer>().enabled = state;
            }
        }
    }
}

/**
 * Copyright (c) 2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using Simulator.Bridge;

namespace Simulator.Sensors
{
    public class DestinationData : ISensorBridgePlugin
    {
        public void Register(IBridgePlugin plugin)
        {
            if (plugin.GetBridgeNameAttribute().Name == "ROS2")
            {
                plugin.Factory.RegPublisher(plugin, (Bridge.Data.Ros.PoseStamped data) => data);
                plugin.Factory.RegPublisher(plugin, (Bridge.Data.Ros.PoseWithCovarianceStamped data) => data);
            }
        }
    }
}

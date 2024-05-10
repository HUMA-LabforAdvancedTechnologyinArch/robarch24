#!/usr/bin/env python2.7  
import roslib
roslib.load_manifest('tf_frames_republisher')

import rospy
import tf
from geometry_msgs.msg import PoseStamped

rospy.init_node('brick_tf_broadcaster')

rospy.loginfo("Publishing brick frame to camera frame in the tf tree.")

broadcaster = tf.TransformBroadcaster()
rate = rospy.Rate(10.0)

brick_pose = rospy.wait_for_message('brick_poses', PoseStamped)

if brick_pose:
    brick_id = brick_pose.header.frame_id
    x = brick_pose.pose.position.x
    y = brick_pose.pose.position.y
    z = brick_pose.pose.position.z
    qx = brick_pose.pose.orientation.x
    qy = brick_pose.pose.orientation.y
    qz = brick_pose.pose.orientation.z
    qw = brick_pose.pose.orientation.w

while not rospy.is_shutdown():
    broadcaster.sendTransform((x, y, z),
                        (qx, qy, qz, qw),
                        rospy.Time.now(),
                        brick_id,
                        "camera_color_optical_frame")
    rate.sleep()
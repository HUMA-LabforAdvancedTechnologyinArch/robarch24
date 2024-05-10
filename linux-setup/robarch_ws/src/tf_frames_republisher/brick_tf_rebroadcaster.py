#!/usr/bin/env python2.7 
import roslib
roslib.load_manifest('tf_frames_republisher')

import rospy
import tf
from geometry_msgs.msg import PoseStamped
from std_srvs.srv import SetBool, SetBoolResponse

class BrickBroadcaster:
    def __init__(self):
        self.bricks = {}
        self.parent_id = None

        self.listener = tf.TransformListener(True, rospy.Duration(10.0))
        self.broadcaster = tf.TransformBroadcaster()

        self.rate = rospy.Rate(10.0)

        rospy.Subscriber('brick_poses', PoseStamped, self.handle_pose)
        rospy.Service('/trigger_clean_tf', SetBool, self.clean_tf)

        self.main_loop()

    def handle_pose(self, data):
       self.bricks[data.header.frame_id] = data
       self.parent_id = "camera_color_optical_frame"
    
    # MAKE SPECIFIC TO BRICK
    def clean_tf(self, request):
        self.bricks = {}
        return SetBoolResponse(success = True, message = "Cleaned brick poses.")

    #transform doesn't work
    def transform_pose(self, data):
        brick_pose = data
        brick_id = data.header.frame_id
        brick_pose.header.frame_id = "camera_color_optical_frame"
        self.listener.waitForTransform("/base",
                                    "/camera_color_optical_frame",
                                    rospy.Time(),
                                    rospy.Duration(15))
        if self.listener.canTransform("/base",
                                    "/camera_color_optical_frame",
                                    rospy.Time()) and brick_pose != None:
            try:
                self.bricks[brick_id] = self.listener.transformPose("/base", brick_pose)
                self.parent_id = "base"
            except:
                self.bricks[brick_id] = brick_pose
                self.parent_id = "camera_color_optical_frame"
        else:
            self.bricks[brick_id] = brick_pose
            self.parent_id = "camera_color_optical_frame"

    def main_loop(self):
        while not rospy.is_shutdown():
            if len(self.bricks) > 0:
                for brick_id, bp in self.bricks.items():
                    x = bp.pose.position.x
                    y = bp.pose.position.y
                    z = bp.pose.position.z
                    qx = bp.pose.orientation.x
                    qy = bp.pose.orientation.y
                    qz = bp.pose.orientation.z
                    qw = bp.pose.orientation.w

                    self.broadcaster.sendTransform((x, y, z),
                                (qx, qy, qz, qw),
                                rospy.Time.now(),
                                brick_id,
                                self.parent_id)
                    rospy.loginfo('==> Published brick {}.'.format(brick_id))
                    self.rate.sleep()  

if __name__ == '__main__':
    rospy.init_node('brick_tf_rebroadcaster')
    broadcast_brick = BrickBroadcaster()
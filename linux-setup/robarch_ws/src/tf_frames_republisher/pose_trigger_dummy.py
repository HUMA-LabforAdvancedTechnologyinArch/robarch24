#!/usr/bin/env python  
import roslib
roslib.load_manifest('tf_frames_republisher')

import rospy
from std_srvs.srv import SetBool, SetBoolResponse

### This is client version for the estimator! ###

def trigger_response(request):
    if request.data:
        # result = estimate pose

        pose_result = True
        if pose_result:
            return SetBoolResponse(success = True, message = "Pose is estimated.")
        else:
            return SetBoolResponse(success = False, message = "Pose is not estimated.")
    else:
        return SetBoolResponse(success = False, message = "Pose is not estimated.")

if __name__ == '__main__':
    rospy.init_node('pose_trigger_service_provider')
    trigger_pose_service = rospy.Service('/trigger_pose', SetBool, trigger_response)
    print("Service advertised.")
    rospy.spin()

### end ###
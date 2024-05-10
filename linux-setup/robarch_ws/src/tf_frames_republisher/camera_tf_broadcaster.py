#!/usr/bin/env python2.7  
import roslib
roslib.load_manifest('tf_frames_republisher')

import rospy
import tf

rospy.init_node('fixed_tf_broadcaster')

rospy.loginfo("Publishing camera frame to the tf tree.")

broadcaster = tf.TransformBroadcaster()
rate = rospy.Rate(10.0)

# x =  -0.101561973284
# y =  0.0274660515163
# z =  0.0894864007819

# qx =  0.364688166443
# qy =  -0.353665207659
# qz =  -0.599499046041
# qw =  0.618485534143

# # horaud
# x = -0.0725448725539
# y = 0.0383998882541
# z = 0.0862198614343 
# qx = 0.372699566398
# qy = -0.345774758074
# qz = -0.58809944151
# qw = 0.629026149522

# From rhino
x = -0.0532
y = 0.0175
z = 0.0612
qx = 0.354
qy = -0.354
qz = -0.612
qw = 0.612

#park
# translation: 
#   x: -0.0667978516631
#   y: 0.0348005042085
#   z: 0.08272894401
# rotation: 
#   x: 0.364074793198
#   y: -0.354066432969
#   z: -0.599552526865
#   w: 0.618565496559

# tsai translation: 
#   x: -0.248269164979
#   y: 0.0984349767717
#   z: 0.130971100331
# rotation: 
#   x: 0.193900005313
#   y: -0.059279030649
#   z: -0.25281448353
#   w: 0.946030454786

### vacuum gripper + camera ###
# qw = 0.7079756826515786
# qx = 0.002145361267659802
# qy = 0.003971143667924615
# qz = 0.7062223872244712
# x = 0.04658969938353463
# y = -0.030007837202672336
# z = 0.09933015773296575

while not rospy.is_shutdown():
    broadcaster.sendTransform((x, y, z),
                        (qx, qy, qz, qw),
                        rospy.Time.now(),
                        "camera_color_optical_frame",
                        "tool0")
    
    # Old camera frame: (0.05696930822260923, -0.03202622099204151, 0.044210199878814506), (0.7100891446052339, -0.009442795309031226, 0.002199759032982751, 0.7040450279568455)
    rate.sleep()

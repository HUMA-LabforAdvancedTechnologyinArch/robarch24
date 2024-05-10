# Linux machine set-up

Refer to https://github.com/IFL-CAMP/easy_handeye 
Aruco generator here https://chev.me/arucogen/

In order to set up the marker tracking environment on your Linux computer follow the below instructions.

Copy the robarch_ws folder to your home.
    
Build the src files.

	pip install opencv-python opencv-contrib-python skbuild
	
	sudo -H pip2 install -U transforms3d==0.3.1
	
	catkin build

if the build fails with the following message "Could not find a package configuration file provided by ddynamic_reconfigure ..." then:

	sudo apt-get update
	sudo apt-get install ros-melodic-ddynamic-configure

if the build fails with the following message " Could not find a package configuration file provided by "realsense2" (requested version 2.50.0) with .." then:

	sudo apt-get install ros-$ROS_DISTRO-realsense2-camera

Install real sense rgbd launch. If an error raises, go to https://github.com/IntelRealSense/realsense-ros/tree/ros1-legacy.

	sudo apt install ros-melodic-rgbd-launch
	
If there is problem with UR_ROS_driver, go to 'Alternative: All-source build' in Alternative: All-source build. (https://github.com/UniversalRobots/Universal_Robots_ROS_Driver)


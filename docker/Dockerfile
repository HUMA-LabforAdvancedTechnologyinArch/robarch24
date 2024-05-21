# Build:
#  docker build --rm -f Dockerfile -t augmentedfabricationlab/ros-abb-fabtory-planner .
#
# Usage:
#  docker pull augmentedfabricationlab/ros-abb-fabtory-planner

FROM augmentedfabricationlab/ros-melodic-moveit
LABEL maintainer "Gido Dielemans <gido.dielemans@tum.de>"

SHELL ["/bin/bash","-c"]

ENV TERM xterm

ENV CATKIN_WS=/root/catkin_ws

RUN sudo apt install ros-melodic-moveit

# Add robot packages to local catkin workspace
RUN echo "Sourcing.." \
    && source /opt/ros/${ROS_DISTRO}/setup.bash \
    # Update apt-get because its cache is always cleared after installs to keep image size down
    && apt-get update && apt-get -y upgrade \
    && apt-get install -y python-pip \
    && apt-get install -y libserial-dev \
    && apt-get install -y python-opencv \
    && pip install transforms3d==0.3.1\
    && apt-get install -y ros-melodic-rgbd-launch \
    && apt-get install apt-transport-https \
    # && apt-get install -y librealsense2-dev librealsense2-dkms \
    # && apt-get install -y ros-melodic-descartes \
    # && apt-get update && apt-get -y upgrade \
    # && git clone https://github.com/UniversalRobots/Universal_Robots_Client_Library \
    # && git clone -b melodic-devel https://github.com/ros/xacro.git \
    && git clone https://github.com/marcoesposito1988/easy_aruco\
    && git clone https://github.com/IFL-CAMP/easy_handeye\
    && git clone -b ros1-legacy https://github.com/IntelRealSense/realsense-ros \
    # && git clone https://github.com/augmentedfabricationlab/tf_frames_republisher \
    # && git clone https://github.com/augmentedfabricationlab/robotic_setups_description.git 
    && git clone https://github.com/augmentedfabricationlab/workshop_robarch_2024_packages.git 

COPY my_robot_calibration.yaml /root/my_robot_calibration.yaml

RUN echo "Building tools..." \
    # Install dependencies
    && cd $CATKIN_WS \
    && rosdep install -y --from-paths . --ignore-src --rosdistro ${ROS_DISTRO} \
    # Build catkin workspace
    && source /opt/ros/${ROS_DISTRO}/setup.bash \
    && catkin_make \
    # Clear apt-cache to reduce image size
    && rm -rf /var/lib/apt/lists/*
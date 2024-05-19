from rtde_control import RTDEControlInterface as RTDEControl
from rtde_io import RTDEIOInterface
from rtde_receive import RTDEReceiveInterface as RTDEReceive
import time
import threading
from compas.geometry import Frame, Transformation, Translation, Vector, Point
from compas_fab.robots.robot import Configuration
from compas import json_load
from compas_fab.robots import to_degrees
import math
from compas_fab.robots import JointTrajectory


def get_config(ip="127.0.0.1"):
    ur_r = RTDEReceive(ip)
    robot_joints = ur_r.getActualQ()
    config = Configuration.from_revolute_values(robot_joints)
    return config

def get_tcp_offset(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    tcp = ur_c.getTCPOffset()
    return tcp

def set_tcp_offset(pose, ip = "127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.setTcp(pose)

def normalize_joint_values_to_pi(config):
    for i,v in enumerate(config.joint_values):
        if v>math.pi:
            v-=2*math.pi
        if v<-math.pi:
            v+=2*math.pi
        config.joint_values[i]=v
    return config


def move_to_joints(config, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool

    ur_c = RTDEControl(ip)
    ur_c.moveJ(config.joint_values, speed, accel, nowait)

def move_to_joints_urc(config, speed, accel, nowait, ur_c):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c.moveJ(config.joint_values, speed, accel, nowait)

def movel_to_joints(config, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c = RTDEControl(ip)
    ur_c.moveL_FK(config.joint_values, speed, accel, nowait)

def movel_to_joints_urc(config, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c = RTDEControl(ip)
    ur_c.moveL_FK(config.joint_values, speed, accel, nowait)

def move_to_target(frame, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    pose = frame.point.x, frame.point.y, frame.point.z, *frame.axis_angle_vector
    ur_c = RTDEControl(ip)
    ur_c.moveL(pose ,speed, accel, nowait)
    return pose

def move_in_z_until_contact(config, speed, accel, nowait, ip):
    ur_r = RTDEReceive(ip)
    ur_c = RTDEControl(ip)
    #tcp_force = ur_r.getActualTCPForce()
    # # ur_c.forceMode(([0, 0, 1, 0, 0, 0], [0.0, 0.0, max_force, 0.0, 0.0, 0.0], [0.01, 0.01, max_speed, 0.01, 0.01, 0.01]))
    # # ur_c.forceModeStop()

    move_to_joints(config, speed, accel, nowait, ur_c)
    ur_c.startContactDetection()
    contact_detected = ur_c.readContactDetection()
    if contact_detected:
        ur_c.stopContactDetection()

    return contact_detected

def pick_and_place_async(pick_frames, place_frames, speed, accel, ip, vaccum_io, safe_dist = 100):
    thread = threading.Thread(target=pick_and_place, args=(pick_frames, place_frames, speed, accel, ip, vaccum_io, safe_dist))
    thread.start()

def pick_and_place(pick_frames, place_frames, speed, accel, ip, vaccum_io, safe_dist = 100):
#move to pick safety plane
    if isinstance(pick_frames,Frame):
        pick_frames = [pick_frames]*len(place_frames)

    for pick, place in zip(pick_frames, place_frames):
        move_to_target(pick.transformed(Translation.from_vector(Vector(0,0,safe_dist))), speed, accel, False, ip = ip)
        #move to pick plane
        move_to_target(pick, speed, accel, False, ip = ip)
        #turn IO on
        set_digital_io(vaccum_io,True,ip=ip)
        #sleep on position to give some time to pick up
        time.sleep(0.5)
        #move to pick safety plane
        move_to_target(pick.transformed(Translation.from_vector(Vector(0,0,safe_dist))), speed, accel, False, ip = ip)
        #move to pre placement frame
        pre_place_frame = place.transformed(Translation.from_vector(Vector(0,0,safe_dist)))
        move_to_target(pre_place_frame, speed, accel, False, ip = ip)
        #move to placement frame
        move_to_target(place, speed, accel, False, ip = ip)
        #turn vaccuum off to place brick
        set_digital_io(vaccum_io,False,ip=ip)
        #sleep robot to make sure it is placed
        time.sleep(0.5)
        #move to post placement frame
        post_place_frame = place.transformed(Translation.from_vector(Vector(0,0,safe_dist)))
        move_to_target(post_place_frame, speed, accel, False, ip = ip)

# def moveJ_to_path(path, speed, speed, radius, ip = "127.0.0.1", ur_c = None):
#     # speed, accel, nowait bool
#     ur_c.movePath(path, True)
#     return path

    

def create_path(frames, speed, accel, radius):
    # speed rad/s, accel rad/s^2, nowait bool
    path = []
    for f in frames:
        pose = f.point.x/1000, f.point.y/1000, f.point.z/1000, *f.axis_angle_vector
        target = [*pose,speed,accel, radius]
        path.append(target)
    return path

def move_to_path(frames, speed, accel, radius, ip = "127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c = RTDEControl(ip)
    path = create_path(frames, speed, accel, radius)
    ur_c.moveL(path, True)
    return path

def stopL(accel, ip = "127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.stopL(accel)

def get_digital_io(signal, ip="127.0.0.1"):
    ur_r = RTDEReceive(ip)
    return ur_r.getDigitalOutState(signal)

def set_digital_io(signal, value, ip="127.0.0.1"):
    io = RTDEIOInterface(ip)
    io.setStandardDigitalOut(signal, value)

def set_tool_digital_io(signal, value, ip="127.0.0.1"):
    io = RTDEIOInterface(ip)
    io.setToolDigitalOut(signal, value)

def get_tcp_frame(ip="127.0.0.1"):
    ur_r = RTDEReceive(ip)
    tcp = ur_r.getActualTCPPose()
    frame = Frame.from_axis_angle_vector(tcp[3:], point=tcp[0:3])
    return frame

# def move_trajectory(configurations, speed, accel, blend, ur_c):
#     path = []
#     for config in configurations:
#         path.append(config.joint_values + [speed, accel, blend])
#     if len(path):
#         ur_c.moveJ(path)

def start_teach_mode(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.teachMode()

def stop_teach_mode(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.endTeachMode()

def measure_frame_from_3_points(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_r = RTDEReceive(ip)

    tcp = ur_c.getTCPOffset()
    print("Hello, your current TCP offset is:")
    print(tcp)

    print("The robot is in free drive mode now")
    print()

    print("1. Move the robot tip to the origin of the calibration frame and press Enter")
    ur_c.teachMode()
    input()
    ur_c.endTeachMode()

    frame_origin = ur_r.getActualTCPPose()
    print("Frame origin:")
    print(frame_origin)

    print("2. Move the robot tip to the X-axis of the calibration frame and press Enter")
    ur_c.teachMode()
    input()
    ur_c.endTeachMode()

    frame_point_on_xaxis = ur_r.getActualTCPPose()
    print("Frame on X-axis:")
    print(frame_point_on_xaxis)

    print("3. Move the robot tip to the Y-axis of the calibration frame and press Enter")
    ur_c.teachMode()
    input()
    ur_c.endTeachMode()

    frame_point_on_yaxis = ur_r.getActualTCPPose()
    print("Frame on Y-axis:")
    print(frame_point_on_yaxis)

    frame = Frame.from_points(
        point=frame_origin[0:3], point_xaxis=frame_point_on_xaxis[0:3], point_xyplane=frame_point_on_yaxis[0:3]
    )

    return frame

def send_trajectory_path_test(trajectory, speed, accel, radius, ip):
    ur_c = RTDEControl(ip)
    send_trajectory_path(trajectory, speed, accel, radius,ur_c)
    
def send_trajectory(trajectory_points, speed, accel, ip):

    #Convert points of trajectory to configurations
    for i in range(len(trajectory_points)):

        #Trajectory points
        point = trajectory_points[i]
        print (type(point))
        #Joint values
        # joint = [math.degrees(item) for item in point.values()]

        #move to configuration
        move_to_joints(point, speed, accel, 0 , ip)


def send_trajectory_path(configurations, speed, accel, radius, ur_c):

    print(f"Move trajectory of {len(configurations)} points with speed {speed}, accel {accel} and blend {radius}")
    
    path = []

   
    for config in configurations:
        path.append(config.joint_values + [speed, accel, radius])

    if len(path):
        ur_c.moveJ(path)
    
    # for i in range(len(configurations)):

    #     #Trajectory points
    #     point = list(configurations[i].values())
    #     print (point)
    #     #Joint values
    #     # joint = [math.degrees(item) for item in point.values()
    #     path.append(point+[speed,accel,radius])

    # ur_c.moveJ(path, False)



def pick_and_place_blocks_trajectories(move_to_pick_trajectory, pick_trajectory, move_trajectory, place_trajectory, speed, accel, radius, ip, vaccum_io):
    
    ur_c = RTDEControl(ip)
    #reverse pick configs list for safety movement
    pick_trajectory_reversed = list(reversed(pick_trajectory)) 
    place_trajectory_reversed = list(reversed(place_trajectory)) 
    nowait = True

    try:

        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,True,ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        #Send Exit Trajectory
        #send_trajectory_path(exit_trajectory, speed, accel, radius,ur_c)

        #Send Move to pick_trajectory
        send_trajectory_path(move_to_pick_trajectory, speed, accel, radius,ur_c)
        
        #Send to pick configs list
        send_trajectory_path([pick_trajectory[-2]], speed, accel, 0.0, ur_c)

        #Send to last pick config
        send_trajectory_path([pick_trajectory[-1]], speed/3., accel, 0.0, ur_c)
        
        #Turn off io to grasp new stick
        set_digital_io(vaccum_io, False, ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        # Reverse from pick location to the approach pick plane
        send_trajectory_path([pick_trajectory_reversed[-1]], speed, accel, radius, ur_c)
        
        # Send move trajectory
        send_trajectory_path(move_trajectory, speed*1.5, 0.1, radius*4, ur_c)

        send_trajectory_path([place_trajectory[0]], speed, accel, 0.0, ur_c)

        # Send Place Trajectory
        send_trajectory_path([place_trajectory[-2]], speed, accel, 0.0, ur_c)

        #move_to_joints(place_trajectory[-1], speed, accel, nowait, ip=ur_c)
        send_trajectory_path([place_trajectory[-1]], speed/3., 0.2, 0.0, ur_c)

        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,True,ip=ip)

        send_trajectory_path([place_trajectory[0]], speed, accel, 0.0, ur_c)

        send_trajectory_path(move_to_pick_trajectory, speed, accel, radius,ur_c)
    
    except Exception as e:
        print(e)
        raise

def release_pick_and_place_stick_trajectories(exit_trajectory, move_to_pick_trajectory, pick_trajectory, move_trajectory, place_trajectory, speed, accel, radius, ip, vaccum_io):
    
    ur_c = RTDEControl(ip)
    #reverse pick configs list for safety movement
    pick_trajectory_reversed = list(reversed(pick_trajectory))
    nowait = True

    try:

        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,True,ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        ur_c.setPayload(0.1,[0.0,0.0,0.1])

        # Reverse from place location to the approach place plane
        send_trajectory_path([exit_trajectory[-1]], speed/3., 0.2, 0.0, ur_c)

        #Send Move to pick_trajectory
        send_trajectory_path(move_to_pick_trajectory, speed, accel, radius,ur_c)
        
        #Send to pick configs list
        send_trajectory_path([pick_trajectory[-2]], speed, accel, 0.0, ur_c)

        #Send to last pick config
        send_trajectory_path([pick_trajectory[-1]], speed/3., accel, 0.0, ur_c)

        time.sleep(1.0)
        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,False,ip=ip)

        ur_c.setPayload(0.4,[0.0,0.0,0.1])
        #Send to reversed pick configs list
        send_trajectory_path([pick_trajectory_reversed[-1]], speed, accel, 0.0, ur_c)

        # Send move trajectory
        send_trajectory_path(move_trajectory, speed*1.5, accel, radius*1.5, ur_c)

        # Send Place Trajectory
        send_trajectory_path([place_trajectory[-2]], speed, accel, 0.0, ur_c)

        #move_to_joints(place_trajectory[-1], speed, accel, nowait, ip=ur_c)
        send_trajectory_path([place_trajectory[-1]], speed/3., 0.2, 0.0, ur_c)


    except Exception as e:
        print(e)
        raise
        

def pick_and_place_stick_trajectories(move_to_pick_trajectory, pick_trajectory, move_trajectory, place_trajectory, speed, accel, radius, ip, vaccum_io):
    
    ur_c = RTDEControl(ip)
    #reverse pick configs list for safety movement
    pick_trajectory_reversed = list(reversed(pick_trajectory)) 
    nowait = True

    try:

        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,True,ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        #Send Exit Trajectory
        #send_trajectory_path(exit_trajectory, speed, accel, radius,ur_c)

        #Send Move to pick_trajectory
        send_trajectory_path(move_to_pick_trajectory, speed, accel, radius,ur_c)

        #Turn on io to release stick that is being held
        set_digital_io(vaccum_io,False,ip=ip)
        
        #Send to pick configs list

        send_trajectory_path([pick_trajectory[-2]], speed, accel, 0.0, ur_c)

        #Send to last pick config
        send_trajectory_path([pick_trajectory[-1]], speed/3., accel, 0.0, ur_c)

        #Send to reversed pick configs list
        send_trajectory_path([pick_trajectory_reversed[-1]], speed, accel, 0.0, ur_c)

        # Send move trajectory
        send_trajectory_path(move_trajectory, speed, accel, radius*1.5, ur_c)

        # Send Place Trajectory
        send_trajectory_path([place_trajectory[-2]], speed, accel, 0.0, ur_c)

        #move_to_joints(place_trajectory[-1], speed, accel, nowait, ip=ur_c)
        send_trajectory_path([place_trajectory[-1]], speed/3., 0.2, 0.0, ur_c)


    except Exception as e:
        print(e)
        raise

def release_stick_trajectories(place_trajectory, speed, accel, radius, ip, vaccum_io):
    
    ur_c = RTDEControl(ip)
    #reverse pick configs list for safety movement

    place_trajectory_reversed = list(reversed(place_trajectory)) 
    move_trajectory_reversed = list(reversed(place_trajectory)) 
    nowait = True

    try:
        #Turn off io to open the gripper
        set_digital_io(vaccum_io, False, ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        # Reverse from place location to the approach place plane
        send_trajectory_path(place_trajectory_reversed, speed, accel, 0.0, ur_c)
        
        # Reverse move trajectory
        send_trajectory_path(move_trajectory_reversed, speed, accel, radius, ur_c)
        
   
    except Exception as e:
        print(e)
        raise

def pick_and_place_sticks_configs_trajectories(exit_safe_config, move_to_pick_trajectory, approach_pick_config, pick_config, move_trajectory, place_config, speed, accel, radius, ip, vaccum_io):
    
    ur_c = RTDEControl(ip)
    #reverse pick configs list for safety movement
    
    try:
        #Turn on io to release stick that is being held
        # set_digital_io(vaccum_io,True,ip=ip)
        #sleep on position to give some time for release
        # time.sleep(1.0)

        #move to exit safe config
        move_to_joints_urc(exit_safe_config, speed, accel, True, ur_c)

        #Send Move to pick_trajectory
        send_trajectory_path(move_to_pick_trajectory, speed, accel, radius, ur_c)

        #move to pick config
        move_to_joints_urc(pick_config, speed, accel, True, ur_c)

        #Turn off io to grasp new stick
        set_digital_io(vaccum_io, False,ip=ip)
        #sleep on position to give some time for release
        time.sleep(1.0)

        #move to approach pick config
        move_to_joints_urc(approach_pick_config, speed, accel, True, ur_c)

        # send move to place trajectory
        send_trajectory_path(move_trajectory, speed, accel, radius, ur_c)

        # move to place config
        move_to_joints_urc(place_config,  speed, accel, True, ur_c)
    
    except Exception as e:
        print(e)
        raise








# if __name__ == "__main__":
# #    print(get_config("192.168.10.12"))

#     exit_trajectory= JointTrajectory.from_json(r"X:\mas_t3_working\working_local\week_10\01_json_dump\1.json")
#     move_to_pick_trajectory=  JointTrajectory.from_json(r"X:\mas_t3_working\working_local\week_10\01_json_dump\2.json")
#     pick_trajectory=  JointTrajectory.from_json(r"X:\mas_t3_working\working_local\week_10\01_json_dump\3.json")
#     move_trajectory=  JointTrajectory.from_json(r"X:\mas_t3_working\working_local\week_10\01_json_dump\4.json")
#     place_trajectory=  JointTrajectory.from_json(r"X:\mas_t3_working\working_local\week_10\01_json_dump\5.json")
    
#     speed, accel, radius, ip, vaccum_io = 0.24,0.1, 0.001, "192.168.0.10", 0

#     pick_and_place_sticks(exit_trajectory, move_to_pick_trajectory, pick_trajectory, move_trajectory, place_trajectory, speed, accel, radius, ip, vaccum_io)

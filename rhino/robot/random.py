import time

from compas.geometry import Frame
from compas_fab.robots.robot import Configuration
from rtde_control import RTDEControlInterface as RTDEControl
from rtde_io import RTDEIOInterface
from rtde_receive import RTDEReceiveInterface as RTDEReceive


def get_config(ip="127.0.0.1"):
    # Create UR Client
    ur_r = RTDEReceive(ip)

    # Read value of joints
    robot_joints = ur_r.getActualQ()

    # Print received values
    config = Configuration.from_revolute_values(robot_joints)
    return config


def get_tcp_offset(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    tcp = ur_c.getTCPOffset()
    return tcp


def move_to_joints(config, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c = RTDEControl(ip)
    ur_c.moveJ(config.joint_values, speed, accel, nowait)


def movel_to_joints(config, speed, accel, nowait, ip="127.0.0.1"):
    # speed rad/s, accel rad/s^2, nowait bool
    ur_c = RTDEControl(ip)
    ur_c.moveL_FK(config.joint_values, speed, accel, nowait)


def get_digital_io(signal, ip="127.0.0.1"):
    ur_r = RTDEReceive(ip)
    return ur_r.getDigitalOutState(signal)


def set_digital_io(signal, value, ip="127.0.0.1"):
    io = RTDEIOInterface(ip)
    io.setStandardDigitalOut(signal, value)


def set_tool_digital_io(signal, value, ip="127.0.0.1"):
    io = RTDEIOInterface(ip)
    io.setToolDigitalOut(signal, value)


def get_tcp_pose(ip="127.0.0.1"):
    ur_r = RTDEReceive(ip)
    tcp = ur_r.getActualTCPPose()
    return tcp


def get_tcp_frame(ip="127.0.0.1"):
    tcp = get_tcp_pose(ip)
    frame = Frame.from_axis_angle_vector(tcp[3:], point=tcp[0:3])
    return frame


def move_trajectory(configurations, speed, accel, blend, ur_c):
    print(f"Move trajectory of {len(configurations)} points with speed {speed}, accel {accel} and blend {blend}")
    path = []
    for config in configurations:
        path.append(config.joint_values + [speed, accel, blend])

    if len(path):
        ur_c.moveJ(path)


def move_trajectory_ip(configurations, speed, accel, blend, ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    print(f"Move trajectory of {len(configurations)} points with speed {speed}, accel {accel} and blend {blend}")
    path = []
    for config in configurations:
        path.append(config.joint_values + [speed, accel, blend])

    if len(path):
        ur_c.moveJ(path)


def place_tile_from_approach_frame(
    approach_place_to_approach_place,
    approach_place,
    pre_place,
    place,
    choreography,
    speed,
    accel,
    signals,
    ip="127.0.0.1",
):
    ur_c = RTDEControl(ip)
    ur_io = RTDEIOInterface(ip)
    blend = 0.02

    move_trajectory(approach_place_to_approach_place, speed * 2, accel * 2, blend, ur_c)
    place_tile(approach_place, pre_place, place, choreography, speed, accel, signals, ur_c, ur_io)


def pick_and_place_tile(
    approach_pick,
    pick,
    approach_place_to_approach_place,
    approach_place,
    pre_place,
    place,
    choreography,
    speed,
    accel,
    signals,
    ip="127.0.0.1",
):
    ur_c = RTDEControl(ip)
    ur_io = RTDEIOInterface(ip)
    blend = 0.05

    try:
        pickup_tile(approach_pick, pick, speed, accel, signals, ur_c, ur_io)
        move_trajectory(approach_place_to_approach_place, speed * 2, accel * 2, blend, ur_c)
        place_tile(approach_place, pre_place, place, choreography, speed, accel, signals, ur_c, ur_io)
        move_trajectory(list(reversed(approach_place_to_approach_place)), speed * 2, accel * 2, blend, ur_c)
    except Exception as e:
        print(e)
        raise


def pick_and_place_tile_until_contact(
    approach_pick,
    pick,
    approach_place_to_approach_place,
    approach_place,
    pre_place,
    place,
    choreography,
    speed,
    accel,
    signals,
    ip="127.0.0.1",
):
    ur_c = RTDEControl(ip)
    ur_io = RTDEIOInterface(ip)
    blend = 0.05

    try:
        pickup_tile(approach_pick, pick, speed, accel, signals, ur_c, ur_io)
        move_trajectory(approach_place_to_approach_place, speed * 2, accel * 2, blend, ur_c)
        place_tile_until_contact(approach_place, pre_place, choreography, speed, accel, signals, ur_c, ur_io)
        move_trajectory(list(reversed(approach_place_to_approach_place)), speed * 2, accel * 2, blend, ur_c)
    except Exception as e:
        print(e)
        raise


def start_teach_mode(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.teachMode()


def stop_teach_mode(ip="127.0.0.1"):
    ur_c = RTDEControl(ip)
    ur_c.endTeachMode()


def pickup_tile(approach_pick, pick, speed, accel, signals, ur_c, ur_io):
    print("Approach pick: moveJ {}".format(approach_pick))
    ur_c.moveJ(approach_pick.joint_values, speed, accel, asynchronous=False)

    print("Pick: moveJ {}".format(pick))
    ur_c.moveJ(pick.joint_values, speed, accel, asynchronous=False)

    for signal in signals:
        ur_io.setStandardDigitalOut(signal, True)
    time.sleep(1)

    print("Approach pick: moveJ {}".format(approach_pick))
    ur_c.moveJ(approach_pick.joint_values, speed, accel, asynchronous=False)


def place_tile(approach_place, pre_place, place, choreography, speed, accel, signals, ur_c, ur_io):
    ur_c.moveJ(approach_place.joint_values, speed, accel, asynchronous=False)

    path = []
    for config in choreography:
        blend = 0.02
        print("Choreo point: moveJ {}".format(config))
        path.append(config.joint_values + [speed, accel, blend])

    if len(path):
        ur_c.moveJ(path)

    ur_c.moveL_FK(pre_place.joint_values, speed, accel, asynchronous=False)
    ur_c.moveJ(place.joint_values, speed / 8.0, accel / 8.0, asynchronous=False)
    time.sleep(0.3)

    for signal in signals:
        ur_io.setStandardDigitalOut(signal, False)
    time.sleep(1)

    print("Approach place: moveJ {}".format(approach_place))
    ur_c.moveJ(approach_place.joint_values, speed, accel, asynchronous=False)


def place_tile_until_contact(approach_place, pre_place, choreography, speed, accel, signals, ur_c, ur_io):
    print("here?")
    ur_c.moveJ(approach_place.joint_values, speed, accel, asynchronous=False)

    path = []
    for config in choreography:
        blend = 0.02
        print("Choreo point: moveJ {}".format(config))
        path.append(config.joint_values + [speed, accel, blend])

    if len(path):
        ur_c.moveJ(path)

    ur_c.moveJ(pre_place.joint_values, speed, accel, asynchronous=False)

    contacted = ur_c.moveUntilContact(
        xd=[0.0, 0.0, -0.03, 0.0, 0.0, 0.0], direction=[0.0, 0.0, 0.0, 0.0, 0.0, 0.0], acceleration=accel / 8.0
    )
    if not contacted:
        raise Exception("Not found contact!")

    # ur_c.moveJ(place.joint_values, speed / 8.0, accel / 8.0, asynchronous=False)
    time.sleep(0.3)

    for signal in signals:
        ur_io.setStandardDigitalOut(signal, False)
    time.sleep(1)

    print("Approach place: moveJ {}".format(approach_place))
    ur_c.moveJ(approach_place.joint_values, speed, accel, asynchronous=False)


if __name__ == "__main__":
    ip = "192.168.0.10"
    frame = get_tcp_pose(ip)
    print(frame)
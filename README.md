# How to localize robots (on linux machine)

```bash
cd robarch_ws
```
```bash
catkin build robarch_launcher
```
```bash
source devel/setup.bash
```
```bash
roslaunch robarch_launcher ur_marker_tracking.launch kinematics_config:=/home/YOUR_USERNAME/robarch_ws/my_robot_calibration.yaml
```

# How to plan robot motion (on linux machine)

```bash
cd robarch_ws
```
```bash
catkin build robarch_launcher
```
```bash
source devel/setup.bash
```
```bash
roslaunch robarch_launcher ur_robot_fabrication.launch
```

## Requirements

Install the following tools:

- [Anaconda](https://www.anaconda.com/products/individual)
- [Visual Studio Code](https://code.visualstudio.com/) and extensions: [python](https://marketplace.visualstudio.com/items?itemName=ms-python.python), [pylance](https://marketplace.visualstudio.com/items?itemName=ms-python.vscode-pylance) and [editorconfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig)
- [Github Desktop](https://desktop.github.com/)
- [Docker](https://docs.docker.com)

## Getting started

Create a environment using `Anaconda prompt`:

    (base) conda create -n robarch24 compas_fab compas_eve compas --yes

Activate the environment:

    (base) conda activate robarch24

Install RTDE (requires python 3.10):

    (robarch24) pip install --user ur_rtde

Check installation:

    (robarch24) pip show compas_fab
    
    Name: compas-fab
    Version: 0.22.0
    Summary: Robotic fabrication package for the COMPAS Framework
    ...

Install Rhino dependencies:

    (robarch24) python -m compas_rhino.install -v 7.0


RobArch Unity Application:

If you desire to run the unity file located in unity `robarch_unity`

It is required that you download and place the vuforia library from
the link below, and place the `.tgz` package in the path listed below.

Vuforia Package Download:
[Vuforia Package](https://developer.vuforia.com/downloads/sdk?_=1717102097)

Path for package:
`...\robarch24\robarch_unity\Packages\com.ptc.vuforia.engine-10.18.4.tgz`  

Note: If you do not have a Vuforia account you will be reqired to make a free developer acount.
Note: You may need to replace the license information within the unity file with your license information.
    
🚀 You're ready! 


## Additional ideas

A few additional things to try:

1. On Visual Studio Code, press `Ctrl+Shift+P`, `Select linter` and select `flake8`
1. To auto-format code, `right-click`, `Format document...`
1. (Windows-only) Change shell: press `Ctrl+Shift+P`, `Select Default Shell` and select `cmd.exe`
1. Try git integration: commit, pull & push are all easily available from Visual Studio Code.

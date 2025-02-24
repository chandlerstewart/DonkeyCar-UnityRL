# Facilitating a Smooth Learning Agent Transfer from Unity Simulation to Autonomous RC Car

This repository contains the code and documentation for our project focused on training a reinforcement learning (RL) agent within a Unity simulation environment and transferring the learned model to an autonomous RC car. Our goal is to bridge the gap between simulation and real-world autonomous driving using RL, realistic physics, and tailored image processing.

**Associated Paper**: [Here](./Research%20Project%20Paper.pdf)

## Overview
The project explores:

- Reinforcement Learning in Simulation: Training an RL agent using Unity’s ML-Agents library to navigate custom-designed tracks.

- Lane Image Processing: Utilizing OpenCV to extract lane edges and reduce background noise, thereby reducing training time and processing load.

- Model Transfer: Converting and transferring an ONNX model from the Unity simulation to the RC car, ensuring the model receives the same input structure during inference.

- Hardware Integration: Building and integrating an RC car using components such as a Jetson Nano, Pi Camera, and a custom 3D printed chassis.
While our simulated training showed promising results, transferring to the RC car introduced challenges in calibration and model optimization.


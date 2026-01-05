# CarGameFinal
This project uses Unity to create a high-performance vehicle simulation. It has an autonomous AI racing system and a specially coded "Realistic Car Controller" that manages intricate physics like weight distribution and drifting.

🛠 Tech Used: 
Engine: Unity 2022+

Language: C#

Physics: Unity WheelColliders, Rigidbody Dynamics, Custom Friction Curves(Majorly this but there are multiple small things too)

1. Advanced Vehicle Physics
Unlike simple arcade controllers, I have included the following real-world car systems:

Anti-Roll Bar System: Designed a script for suspension travel variation between wheels. This takes into consideration counter-forces that prevent the car from rolling when making high-speed turns.

Dynamic Downforce: Implemented a functionality that increases downforce as a function of velocity (code responsible is rb.velocity.magnitude * downforce), which helps to keep the vehicle on the ground when it moves at high speeds.

Center of Mass Optimization: Adjusted Rigidbody's COM by hand to a lower Y-offset value of -0.9f to better simulate a sports car chassis.

2. Custom Drift & Friction Logic
Created a proprietary drift system by manipulating WheelFrictionCurves in real-time:

Slip Angle Steering: Counter-steering logic based on the vehicle's slip angle (Mathf.Atan2(localVel.x, Mathf.Abs(localVel

Adaptive Grip: The script then decreases SidewaysStiffness and ForwardStiffness when the handbrake is engaged. The effect is to make the car slide.

3. Pathfinding & Autonomous AI
The IndependentAICar script handles the AI cars in the following way(to be honest it pretty simple but it works fine because the level is small not very big)

Waypoint Navigation: It is a linked list structure designed for navigation on tracks with a waypointThreshold.

Predictive Slowdown: It calculates the angleToTarget, and then a power function of maths (Mathf.Pow) is used to slow down before entering a corner.

Stuck Detection: This is a recovery system that dutifully watches for minDistanceMoved to auto-respawn the AI if it gets stuck.
(The project is a bit badly structured please forgive me for that)

Video of a few second of gameplay:


https://github.com/user-attachments/assets/d3df20c8-439a-4db2-8436-a58efd683181


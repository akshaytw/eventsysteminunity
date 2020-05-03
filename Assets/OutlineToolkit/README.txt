If you have any questions or problems, contact me at chaoliner@gmail.com.

-----------------
 OUTLINE TOOLKIT
-----------------

This toolkit provides 3 different ways to produce outlines on objects in your scene.

To Set Up Edge Detection:
Remove your current camera, and drag the Edge Detection Camera prefab into your scene. The Edge Detection Camera now replaces your main camera.
You can unparent the edge detection camera's child cameras, but be sure that your scripts move the child cameras as if they were still parented.
You can safely parent the edge detection camera to other objects, or add extra children to its transform.
The child cameras automatically update to match their parent's FOV and far clipping plane. If you change anything else about the (parent) edge detection camera (save for the clear flags and background color), you should also change it in its child cameras.
Do not enable any form of antialiasing or post-processing effects on the child cameras.
EdgeDetect.cs should probably execute after all other scripts (but before cameras render.) You can set its execution order in Edit > Project Settings > Script Execution Order.

To Use Edge Detection:
Increasing the Edge Detection's "Depth sensitivity" setting makes it more likely to place outlines onscreen on the outer edges of objects sitting in front of other objects. "Depth Sensitivity 2" does the same thing, but with a different method.
Increasing the Edge Detection's "Normals sensitivity" setting makes it more likely to place outlines onscreen where objects have sharp edges.

The Multi-Pass System:
To add an outline to any shader, add "OutlineToolkit/Outline/OUTLINE" as a UsePass. See StandardSurfaceOutlined.shader for an example.
If you don't want to modify your shader, you can add "Outline" as a second material. See the green sphere in Example 2 for an example.
This system works best with smooth, rounded surfaces.

The Mesh-Processing System:
Produces a mesh outlining the sharp corners of any mesh you provide to it. (Call MeshProcessor.processForAppearance.)
To accuarately display the mesh, use the "OutlineToolkit/ProcessedEdges" shader.

Demo Scenes:
Use WASD to move, and the mouse buttons to move up and down (unless you have changed the project's Input settings). Move the mouse to look around.
Example 1 demos the edge detection system.
Example 2 demos the multi-pass system.
Example 3 demos the mesh processing system.
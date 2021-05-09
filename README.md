# UnityIFMLib
### A iFacialMocap interface implementation library for Unity

This lib is designed to make the process of adding [iFacialMocap](https://www.ifacialmocap.com/) by Yasushi Emoto to a Unity project with ease with simplified configuration process through the Unity Inspector.

A Thread for listens in for UDP Packets which when processed are added to a Queue. Another takes from the Queue and distributes them over UnityEvents onto main thread. This removes the need to do UDP Listens on frame update.

Implements IFacialMocap communication [protocol](https://www.ifacialmocap.com/for-developer/) into a more usable package without the required IFacialMocap companion app.

Developed on Unity 2018.4.33f1
Tested in Unity 2020.3.0.

### Adding to Project

Import the Unitypackage in the releases to your project.

To add `IFMService` to an Empty GameObject in the scene, and bind a in-scene handler function which has an `IFMPacket` argument.

An example script with the handler would do the following
```
public class Example : MonoBehaviour
{
    public void OnMessage(IFMPacket packet)
    {
        //.. Do stuff ..
    }
}
```

To see an example of UnityIFMLib in action, in  `Example/Scene` open either scene `RiggedExample` or `StaticMeshExample`. Refer to the interactions between the Scene objects `IFMService`, `CharacterFaceController` and the Example Avatars used in the Scene.


### Connecting 

Instead of using to the [iFacialMocap Companion app](https://www.ifacialmocap.com/), iFacialMocap should connect to Unity application directly. 

You can do this by Opening the [iFacialMocap](https://www.ifacialmocap.com/) app, and pressing the gear on the to right of the screen. From here you can set the destination ip to the host where the app is being ran on the local network.

By default when `IFMService` **is enabled it will poll the IFM (default) port** `49983` every 5 seconds until first the packet is received. If such is received, there is a connection confirm packet sent to iFacialMocap, and a confirmation notice should appear on it. 

If a timeout occurs, the app will try again every 5 seconds and have the confirmation dialog appear again.

When **IFMService is turned off, the port listening is disabled.**


## Usable Classes:

### IFMPacket

The IFM Packet is a ready parsed object build from the UDP Messages

 - `Blendshapes` - Contains an Object Array of `BlendshapeData`. 
 - `Head`, `LeftEye`, `RightEye` which all are Objects of `IFMTransform`. 

### BlendshapeData

#### Members
- `Name` which is a string, using the [ARKit Blendshape](https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation) values. Note: protocol shorthands `_L` or `_R` are replaced with ARKit specific `Left` `Right` respectively for naming consistancy throughout all the blendshapes
- `Value` which is an int from 0 to 100.
#### Helpers
- `SetBlendshapeForRenderers(SkinnedMeshRenderer[])` - Loops through all Renderers provided and applies the blendshape where applicable

### IFMTransform
- `Position` - Vector3 position relative to camera. **Only head** has this set
- `EulerRotation` - Vector3 Euler Degree rotation of object relative to the camera.

### IFMUtility

Utility class with static helpers for quicker prototyping or implementation
- `ApplyBlendshapes(IFMPacket, SkinnedMeshRenderer[])` - Applies `IFMPacket.Blendshapes` to all the `SkinnedMeshRenderers` that have the same Blendshapes
- `GetBlendshapeIndex(SkinnedMeshRenderer, string)` - runs GetBlendShapeIndex on string, but also does some short hands for skipping the entire search if shared mesh is null or if blendshapecount is less than 1



# Model 
Sourced from iFacialMocap Companion app's example project


using UnityEngine;
using UnityIFM;

public class CharacterFaceControllerExample : MonoBehaviour
{
    // MainObject is the main asset where the entire model is at
    public GameObject mainObject;
   
    // HeadTarget is the Transform which will be manipulated to
    // translate & rotate the Head be it an IK rig or just the Head bone

    public Transform headTarget;

    // EyeTarget is the Transform which will be manipulated to rotate the eye
    public Transform leftEyeTarget;
    public Transform rightEyeTarget;

    private SkinnedMeshRenderer[] renders;

    void Start()
    {
        // Get all Skinned Mesh
        renders = mainObject.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    // Make sure to bind the even lister from the IFMManager to this.
    public void OnMessage(IFMPacket packet)
    {
        // Iterate through all blendshape data
        foreach(BlendshapeData data in packet.Blendshapes)
        {
            // Iterate through available skinned mesh
            foreach (SkinnedMeshRenderer renderer in renders)
            {
                Mesh mesh = renderer.sharedMesh;
                // Skip if no blendshapes
                if (mesh.blendShapeCount > 0)
                {
                    // See if we can find blendshape
                    int index = mesh.GetBlendShapeIndex(data.Name);
                    // Index is -1 if not found
                    if (index >= 0)
                    {
                        // Apply Blendshape value.
                        renderer.SetBlendShapeWeight(index, data.Value);
                    }
                }
            }
        }

        // Alternatively everything above can be written as
        // IFMUtility.ApplyBlendshapes(packet, renders);

        // But sometimes you may want to increase values for specific blendshapes.

        // Data can be slerped / lerped or otherwise interpolated here as well.
        headTarget.transform.position = packet.Head.Position;
        headTarget.transform.eulerAngles = packet.Head.EulerRotation;

        leftEyeTarget.transform.eulerAngles = packet.LeftEye.EulerRotation;
        rightEyeTarget.transform.eulerAngles = packet.RightEye.EulerRotation;
    }
}

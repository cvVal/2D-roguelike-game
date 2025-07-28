using UnityEngine;

/// <summary>
/// Controller to help BoardManager update Cinemachine camera lens size.
/// </summary>
public class CinemachineCameraController : MonoBehaviour
{

    public void UpdateLensSize(float newSize)
    {
        var cinemachineCamera = GetComponent("CinemachineCamera");
        var lensField = cinemachineCamera.GetType().GetField("Lens");
        var lensObject = lensField.GetValue(cinemachineCamera);
        var orthographicSizeField = lensObject.GetType().GetField("OrthographicSize");

        orthographicSizeField.SetValue(lensObject, newSize);
        lensField.SetValue(cinemachineCamera, lensObject);
    }
}

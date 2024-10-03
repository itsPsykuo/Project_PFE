using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GuardNavigatingStatment))]
public class GuardEditor : Editor
{
    private void OnSceneGUI()
    {
        GuardNavigatingStatment fov = (GuardNavigatingStatment)target;

        Color c = Color.green;

        if (fov.alertStage == AlertStage.Intrigued)
        {
            c = Color.Lerp(Color.green, Color.red, fov.alertLevel / 100f);
        }
        else if (fov.alertStage == AlertStage.Alerted)
        {
            c = Color.red;
        }

        Handles.color = new Color(c.r, c.g, c.b);
        Handles.DrawSolidArc(fov.transform.position, fov.transform.up,
            Quaternion.AngleAxis(-fov.fovAngle / 2f, fov.transform.up) * fov.transform.forward,
            fov.fovAngle, fov.fov);
        
        Handles.color = c;
        fov.fov = Handles.ScaleValueHandle(fov.fov, fov.transform.position, fov.transform.rotation, 3, Handles.SphereHandleCap, 1);
    }
}

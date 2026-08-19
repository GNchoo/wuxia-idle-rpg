using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT
{

    public class Talent_Size_Change : MonoBehaviour
    {

        public GameObject targetObject; // Assign in the inspector
        public float maxScale = 2.0f;   // Maximum scale limit
        public float minScale = 0.5f;   // Minimum scale limit

        public void ScaleUp()
        {
            // Calculate the new scale
            Vector3 newScale = targetObject.transform.localScale + new Vector3(0.25f, 0.25f, 0.25f);

            // Clamp the scale to the maximum value
            newScale = Vector3.Min(newScale, Vector3.one * maxScale);

            targetObject.transform.localScale = newScale;
        }

        public void ScaleDown()
        {
            // Calculate the new scale
            Vector3 newScale = targetObject.transform.localScale - new Vector3(0.25f, 0.25f, 0.25f);

            // Clamp the scale to the minimum value
            newScale = Vector3.Max(newScale, Vector3.one * minScale);

            targetObject.transform.localScale = newScale;
        }
    }

}
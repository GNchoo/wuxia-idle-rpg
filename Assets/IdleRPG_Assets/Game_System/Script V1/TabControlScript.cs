using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT.TabCollection.Manager
{
    public class TabControlScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ShowObject(GameObject gameObjectID)
        {
            gameObjectID.gameObject.SetActive(true);
        }

        public void HideObject(GameObject gameObjectID)
        {
            gameObjectID.gameObject.SetActive(false);
        }

        public void ShowCanvas(Canvas CanvasObjectId)
        {
            CanvasObjectId.enabled = true;
        }

        public void HideCanvas(Canvas CanvasObjectId)
        {
            CanvasObjectId.enabled = false;
        }
    }
}


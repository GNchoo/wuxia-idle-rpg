using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMPLETEXT
{

    public class TextManager : MonoBehaviour
    {
        public List<GameObject> textObjects; // List of text gameobjects
        private int currentIndex = 0; // Current index in the list

        void Start()
        {
            // Ensure only the first text object is active at the start
            SetActiveText(currentIndex);
        }

        public void ActivateNextText()
        {
            // Increment the index, looping back to the start if necessary
            currentIndex = (currentIndex + 1) % textObjects.Count;
            SetActiveText(currentIndex);
        }

        public void ActivatePreviousText()
        {
            // Decrement the index, looping back to the end if necessary
            currentIndex = (currentIndex - 1 + textObjects.Count) % textObjects.Count;
            SetActiveText(currentIndex);
        }

        private void SetActiveText(int index)
        {
            // Deactivate all text objects
            foreach (var textObject in textObjects)
            {
                textObject.SetActive(false);
            }

            // Activate the text object at the given index
            textObjects[index].SetActive(true);
        }
    }

}
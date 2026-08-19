using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SAMPLETEXT
{

    public class ShowBuildingDetails : MonoBehaviour
    {
        public int buildingIndex; // The index of the currently selected building
        public string[] buildingTitles; // Titles for each building
        public Sprite[] buildingImages; // Images for each building
        public string[] buildingDescriptions; // Descriptions for each building

        public TMP_Text titleText; // Reference to the text component for displaying the title
        public Image buildingImage; // Reference to the image component for displaying the building image
        public TMP_Text descriptionText; // Reference to the text component for displaying the description
        public GameObject buildingInfoPanel; // Reference to the panel containing the building info

        // Function to change the index and open the building info panel
        public void ChangeIndexAndOpenPanel(int newIndex)
        {
            // Set the new index
            buildingIndex = newIndex;

            // Show the building info panel
            buildingInfoPanel.SetActive(true);

            // Update the building info
            UpdateBuildingInfo();
        }

        // Function to update the building info based on the building index
        private void UpdateBuildingInfo()
        {
            // Check if the index is within the valid range
            if (buildingIndex >= 0 && buildingIndex < buildingTitles.Length)
            {
                // Set the title
                titleText.text = buildingTitles[buildingIndex];

                // Set the image
                if (buildingImages[buildingIndex] != null)
                {
                    buildingImage.sprite = buildingImages[buildingIndex];
                }

                // Set the description
                descriptionText.text = buildingDescriptions[buildingIndex];
            }
            else
            {
                Debug.LogError("Invalid building index: " + buildingIndex);
            }
        }
    }
}
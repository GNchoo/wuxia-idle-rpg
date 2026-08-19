using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SAMPLETEXT
{
    public class LoadingController : MonoBehaviour
    {
        public Slider loadingSlider;
        public Image loadingFillImage;
        public float loadingDuration = 3f;
        public string gameplaySceneName;
        public string secondLevelName = "GameScene V1";

        float timer;
        AsyncOperation secondLevelLoadOperation;
        bool activated;

        void Start()
        {
            if (string.IsNullOrEmpty(secondLevelName))
                secondLevelName = "GameScene V1";

            if (loadingDuration < 1f)
                loadingDuration = 3f;

            secondLevelLoadOperation = SceneManager.LoadSceneAsync(secondLevelName);
            if (secondLevelLoadOperation == null)
            {
                Debug.LogError("[Loading] Failed to start LoadSceneAsync for: " + secondLevelName);
                return;
            }

            secondLevelLoadOperation.allowSceneActivation = false;
        }

        void Update()
        {
            if (activated || secondLevelLoadOperation == null)
                return;

            timer += Time.unscaledDeltaTime;

            // Combine fake timer with real async progress (async caps at 0.9 until activation).
            float fake = Mathf.Clamp01(timer / loadingDuration);
            float real = Mathf.Clamp01(secondLevelLoadOperation.progress / 0.9f);
            float progress = Mathf.Max(fake, real * 0.9f);
            if (timer >= loadingDuration)
                progress = 1f;

            SetProgressUI(progress);

            if (timer >= loadingDuration && secondLevelLoadOperation.progress >= 0.9f)
            {
                activated = true;
                secondLevelLoadOperation.allowSceneActivation = true;
            }
        }

        void SetProgressUI(float progress)
        {
            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (loadingFillImage != null)
                loadingFillImage.fillAmount = progress;

            // Fallback: find a child Image named like loading bar
            if (loadingSlider == null && loadingFillImage == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img != null && img.type == Image.Type.Filled)
                    {
                        img.fillAmount = progress;
                        break;
                    }
                }
            }
        }
    }
}

using Asset.Script.ClientDefine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Asset.Script.Manager
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private Slider _loadingBar;

        [SerializeField]
        private float _miniumLoadingTime = 1.0f;

        private IEnumerator Start()
        {
            float startTime = Time.realtimeSinceStartup;

            AsyncOperation operation = SceneManager.LoadSceneAsync(SceneNames.Battle);

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f 
                || Time.realtimeSinceStartup - startTime < _miniumLoadingTime)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);

                if(_loadingBar != null)
                {
                    _loadingBar.value = progress;
                }

                yield return null;
            }

            if (_loadingBar != null)
            {
                _loadingBar.value = 1.0f;
            }

            operation.allowSceneActivation = true;
        }
    }

}

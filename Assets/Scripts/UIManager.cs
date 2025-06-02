using System.Collections;
using UnityEngine;

namespace Game
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private LevelCompletePanel _levelCompletePanel;
        [SerializeField] private GamePlayPanel _gamePlayPanel;
        [SerializeField] private LevelsPanel _levelsPanel;
        [SerializeField] private StorePanel _storePanel;

        //        public MenuPanel MenuPanel => _menuPanel;
        public StorePanel StorePanel => _storePanel;
        public LevelsPanel LevelsPanel => _levelsPanel;
        public GamePlayPanel GamePlayPanel => _gamePlayPanel;


        public static UIManager Instance { get; private set; }

        public static bool IsFirstTime
        {
            get => PrefManager.GetBool(nameof(IsFirstTime),true);
            private set => PrefManager.SetBool(nameof(IsFirstTime),value);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            LevelManager.LevelLoaded += LevelManagerOnLevelLoaded;
            LevelManager.LevelCompleted += LevelManagerOnLevelResulted;
        }


        private void OnDisable()
        {
            LevelManager.LevelLoaded -= LevelManagerOnLevelLoaded;
            LevelManager.LevelCompleted -= LevelManagerOnLevelResulted;
        }

        private void LevelManagerOnLevelResulted(ILevel level)
        {
            StartCoroutine(LevelResulted());
        }

        IEnumerator LevelResulted()
        {
            yield return new WaitForSeconds(0.3f);
            _levelCompletePanel.Show();

            yield return new WaitForSeconds(0.8f);

            AdsManager.ShowOrPassAdsIfCan();

            if(!ResourceManager.HasLevel(LevelManager.Instance.Level.LevelNo+1))
            {
                _levelCompletePanel.Hide();
                yield break;
            }

            yield return new WaitForSeconds(.8f);
            _levelCompletePanel.Hide();
            LevelManager.Instance.LoadLevel(LevelManager.Instance.Level.LevelNo+1);
        }



        private void LevelManagerOnLevelLoaded(ILevel level1)
        {
            if (!GamePlayPanel.Showing)
                GamePlayPanel.Show();

        }
    }
}
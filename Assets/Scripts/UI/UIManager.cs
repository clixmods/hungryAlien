using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Util;
    public static UIManager Instance;
    
    public GameObject InputHelperPrefab;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject menuPanel;
    [SerializeField] TextMeshProUGUI playButtonText;
    
    [SerializeField] bool isPaused;
    [SerializeField] bool isInMenu;


    public static void CreateInputHelper(string text, Transform transformToTarget , out UIInputHelper component, 
        float displayTime = -1)
    {
        component = null;
        if (transformToTarget == null) return;
        
        var inputHelperObject =
            Instantiate(Util.InputHelperPrefab, Vector3.zero, Quaternion.identity, Util.transform);

        component = inputHelperObject.GetComponentInChildren<UIInputHelper>();
            
        component.Setup(text, transformToTarget , displayTime);
        
    }
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Util = this;
        Time.timeScale = 0;
        isInMenu = true;
        menuPanel.SetActive(true);
        pausePanel.SetActive(false);
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void PlayGame()
    {
        menuPanel.SetActive(false);
        GameManager.State = GameGlobalState.Ingame;
        Time.timeScale = 1;
        isInMenu = false;
    }
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        GameManager.State = GameGlobalState.Ingame;
        Time.timeScale = 1;
        isPaused = false;
    }
    public void RestartGame()
    {
        GameManager.Instance.RestartMap();
    }
    public void PauseGame()
    {
        if(!isInMenu)
        {
            if (!isPaused)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0;
                isPaused = true;
                GameManager.State = GameGlobalState.Paused;
            }
            else
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1;
                isPaused = false;
                GameManager.State = GameGlobalState.Ingame;
            }
        }
        
    }
}

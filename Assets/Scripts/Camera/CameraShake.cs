using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    CinemachineVirtualCamera _virtualCam;
    CinemachineBasicMultiChannelPerlin noise;


    [SerializeField] float _noiseValue;
    [SerializeField] float DecreaseMultiplier = 1;


    public static void SetNoisier(float amount, float multiplier = 1)
    {
        Instance._noiseValue = amount;
        Instance.DecreaseMultiplier = multiplier;
    }

    public void Noise(float amplitudeGain, float frequencyGain)
    {
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        _virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
        if (_virtualCam != null)
        {
            noise = _virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if(noise == null)
                noise = _virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_virtualCam != null && noise != null)
        {
            if (_noiseValue > 0)
            {
                _noiseValue -= Time.deltaTime * DecreaseMultiplier;
            }
            else
            {
                _noiseValue = 0;
            }

            noise.m_AmplitudeGain = _noiseValue;
        }
        else
        {
            Start();
            Debug.LogWarning("CameraShake ne trouve pas de virtual camera");
        }
    }
}
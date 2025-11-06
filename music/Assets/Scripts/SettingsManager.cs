using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    // 绑定UI组件
    public GameObject settingsPanel; // 设置面板
    public GameObject backgroundOverlay; // 背景遮罩层
    public Slider volumeSlider;      // 音量滑块
    public AudioMixer audioMixer;    // 音频混合器（用于调整音量）
    public Button saveButton;        // 保存按钮

    private const string VolumeKey = "volume"; // 用于保存音量的键

    void Start()
    {
        // 如果有保存的音量设置，加载并设置滑块值
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.5f); // 默认音量为 0.5
        volumeSlider.value = savedVolume;

        // 监听滑块变化，实时调整音量
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // 监听保存按钮点击事件
        saveButton.onClick.AddListener(SaveSettings);

        // 初始时隐藏面板和遮罩
        settingsPanel.SetActive(false);
        backgroundOverlay.SetActive(false);
    }

    // 显示设置面板和背景遮罩
    public void ShowSettingsPanel()
    {
        settingsPanel.SetActive(true);
        backgroundOverlay.SetActive(true);
    }

    // 隐藏设置面板和背景遮罩
    private void HideSettingsPanel()
    {
        settingsPanel.SetActive(false);
        backgroundOverlay.SetActive(false);
    }

    // 调整音量
    private void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // 转换滑块值到分贝
    }

    // 保存音量设置并关闭面板
    private void SaveSettings()
    {
        // 保存当前音量值
        PlayerPrefs.SetFloat(VolumeKey, volumeSlider.value);
        PlayerPrefs.Save();

        // 隐藏设置面板和背景遮罩
        HideSettingsPanel();
    }
}

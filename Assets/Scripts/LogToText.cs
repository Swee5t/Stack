using UnityEngine;
using UnityEngine.UI;

public class LogToText : MonoBehaviour
{
    private Text _logText;
    private float _deltaTime;

    private void Start()
    {
        _logText = GetComponent<Text>();
        if (_logText == null) return;

        var os = SystemInfo.operatingSystem;
        var cpu = SystemInfo.processorType;
        var coreCount = SystemInfo.processorCount;
        var memory = (SystemInfo.systemMemorySize / 1024f).ToString("F2") + " GB";
        var gpu = SystemInfo.graphicsDeviceName;
        var gpuVendor = SystemInfo.graphicsDeviceVendor;
        var gpuVersion = SystemInfo.graphicsDeviceVersion;
        var resolution = Screen.currentResolution;
        var refreshRate = resolution.refreshRateRatio.value.ToString("F2");
        var deviceModel = SystemInfo.deviceModel;
        var deviceType = SystemInfo.deviceType;

        var info = "";
        info += "FPS: calculating...\n";
        info += $"OS: {os}\n";
        info += $"CPU: {cpu} ({coreCount}C)\n";
        info += $"MEMORY: {memory}\n";
        info += $"GPU: {gpu} ({gpuVendor})\n";
        info += $"GPU Version: {gpuVersion}\n";
        info += $"RESOLUTION: {resolution.width}x{resolution.height} @{refreshRate}Hz\n";
        info += $"DEVICE: {deviceModel} ({deviceType})\n";

        _logText.text = info;
    }

    private void Update()
    {
        if (ReferenceEquals(_logText, null)) return;

        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        var fps = 1.0f / _deltaTime;

        var current = _logText.text;
        var lines = current.Split('\n');
        lines[0] = $"FPS: {fps:F1}";

        _logText.text = string.Join("\n", lines);
    }
}
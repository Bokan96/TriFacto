using UnityEngine;

public class SelectArea : MonoBehaviour
{
    public UIManager uiManager;
    public int areaIndex;
    private AudioSource audioSource;
    public void OnImageClick()
    {
        uiManager.SelectArea(areaIndex);
        audioSource = GameObject.Find("TerrainSound").GetComponent<AudioSource>();
        audioSource.Play();
    }
}
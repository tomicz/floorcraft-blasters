using UnityEngine;
using UnityEngine.UI;

namespace Matterless.Floorcraft
{
    /// <summary>
    /// View component for displaying an NFT image in the UI
    /// </summary>
    public class NFTContainerView : MonoBehaviour
    {
        [SerializeField] private Image m_Image;
        
        private void Awake()
        {
            if (m_Image != null)
            {
                m_Image.gameObject.SetActive(false);
            }
        }
        
        public void SetImage(Sprite sprite)
        {
            if (m_Image != null && sprite != null)
            {
                m_Image.sprite = sprite;
                m_Image.gameObject.SetActive(true);
            }
        }
    }
}


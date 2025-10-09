using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ShopKeeper : MonoBehaviour
{
    public static ShopKeeper currentShopKeeper;
    public Animator anim;
    public ShopManager shopManager;
    public CanvasGroup shopCanvasGroup;
    public static event Action<ShopManager, bool> OnShopStateChanged;

    [SerializeField] private List<ShopItems> shopItems;
    [SerializeField] private List<ShopItems> shopWeapons;
    [SerializeField] private List<ShopItems> shopArmour;
    [SerializeField] private Camera shopkeeperCam;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -1);

    [Header("Portrait Settings")]
    // Remove this line since we don't need it for the fix
    // [SerializeField] private Image portraitImage;

    // Cache references that might become invalid
    private Camera shopCamera;
    private CanvasGroup canvasGroup;

    private bool isShopOpen;
    private bool PlayerInRange;

    void Start()
    {
        RefreshReferences();
    }

    void Update()
    {
        // Refresh references if they become null
        if (shopCamera == null || canvasGroup == null || shopManager == null)
        {
            RefreshReferences();
        }

        if (PlayerInRange)
        {
            if (Input.GetButtonDown("Interact"))
            {
                if (!isShopOpen)
                {
                    OpenShop();
                }
                else
                {
                    CloseShop();
                }
            }
        }
    }

    private void RefreshReferences()
    {
        // Try to get references from GameManager first
        if (GameManager.Instance != null)
        {
            shopCamera = GameManager.Instance.shopCamera;
            canvasGroup = GameManager.Instance.canvasGroup;

            // IMPORTANT: Also set the shopkeeperCam to the persistent one
            if (shopkeeperCam == null && GameManager.Instance.shopCamera != null)
            {
                shopkeeperCam = GameManager.Instance.shopCamera;
            }

            // Get ShopManager reference
            if (GameManager.Instance.shopManager != null)
            {
                shopManager = GameManager.Instance.shopManager;
            }
            else
            {
                // Fallback: find in scene if not set in GameManager
                shopManager = FindObjectOfType<ShopManager>();
            }
        }
        else
        {
            // Fallback: find references in scene
            shopCamera = FindObjectOfType<Camera>();

            // Find CanvasGroup - you might need to adjust this based on your setup
            CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>();
            foreach (var cg in canvasGroups)
            {
                if (cg.name.Contains("Shop") || cg.CompareTag("ShopUI"))
                {
                    canvasGroup = cg;
                    break;
                }
            }

            shopManager = FindObjectOfType<ShopManager>();
        }

        // Use local reference if global one is not available
        if (canvasGroup == null && shopCanvasGroup != null)
        {
            canvasGroup = shopCanvasGroup;
        }

        // Debug to see what we found
        Debug.Log($"ShopKeeper RefreshReferences: shopkeeperCam = {(shopkeeperCam != null ? "Found" : "NULL")}");
        Debug.Log($"ShopKeeper RefreshReferences: canvasGroup = {(canvasGroup != null ? "Found" : "NULL")}");
    }

    private void RefreshPortraitRenderTexture()
    {
        if (shopkeeperCam != null && shopkeeperCam.targetTexture != null)
        {
            // Store the render texture reference
            RenderTexture renderTex = shopkeeperCam.targetTexture;

            // Clear and reassign to refresh the connection
            shopkeeperCam.targetTexture = null;
            shopkeeperCam.targetTexture = renderTex;

            // Force a render to make sure it's working
            shopkeeperCam.Render();

            Debug.Log("Portrait render texture refreshed!");
        }
    }

    public void OpenShop()
    {
        // Force refresh references before opening
        RefreshReferences();

        if (shopManager == null || canvasGroup == null)
        {
            Debug.LogWarning("ShopKeeper: Missing references. Cannot open shop.");
            Debug.LogWarning($"shopManager: {shopManager}, canvasGroup: {canvasGroup}, shopkeeperCam: {shopkeeperCam}");
            return;
        }

        Time.timeScale = 0;
        currentShopKeeper = this;
        isShopOpen = true;
        OnShopStateChanged?.Invoke(shopManager, true);

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (shopkeeperCam != null)
        {
            shopkeeperCam.transform.position = transform.position + cameraOffset;
            shopkeeperCam.gameObject.SetActive(true);

            Debug.Log("ShopKeeper: Activating camera and refreshing portrait...");
            // IMPORTANT: Refresh the render texture after activating the camera
            StartCoroutine(RefreshPortraitDelayed());
        }
        else
        {
            Debug.LogError("ShopKeeper: shopkeeperCam is NULL! Cannot show portrait.");
        }

        OpenItemShop();
    }

    private IEnumerator RefreshPortraitDelayed()
    {
        // Wait a frame for the camera to fully activate
        yield return null;
        RefreshPortraitRenderTexture();
    }

    public void CloseShop()
    {
        Time.timeScale = 1;
        currentShopKeeper = null;
        isShopOpen = false;

        if (shopManager != null)
        {
            OnShopStateChanged?.Invoke(shopManager, false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (shopkeeperCam != null)
        {
            shopkeeperCam.gameObject.SetActive(false);
        }
    }

    public void OpenItemShop()
    {
        if (shopManager != null)
        {
            shopManager.PopulateShopItems(shopItems);
        }
    }

    public void OpenWeaponShop()
    {
        if (shopManager != null)
        {
            shopManager.PopulateShopItems(shopWeapons);
        }
    }

    public void OpenArmourShop()
    {
        if (shopManager != null)
        {
            shopManager.PopulateShopItems(shopArmour);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (anim != null)
            {
                anim.SetBool("PlayerInRange", true);
            }
            PlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (anim != null)
            {
                anim.SetBool("PlayerInRange", false);
            }
            PlayerInRange = false;

            // Close shop if player leaves while it's open
            if (isShopOpen)
            {
                CloseShop();
            }
        }
    }
}
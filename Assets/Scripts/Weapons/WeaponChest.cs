using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponChest : MonoBehaviour
{
    private enum ChestState
    {
        Closed,
        Opening,
        RewardReady
    }

    [Header("Reward Objects")]
    [SerializeField] private WeaponData pistolData;
    [SerializeField] private WeaponData daggerData;
    [SerializeField] private WeaponData minigunData;
    [SerializeField] private WeaponData scytheData;

    [SerializeField] private GameObject pistolRewardObject;
    [SerializeField] private GameObject daggerRewardObject;
    [SerializeField] private GameObject minigunRewardObject;
    [SerializeField] private GameObject scytheRewardObject;

    [Header("Price")]
    [SerializeField] private int cost = 1000;

    [Header("Available Weapons")]
    [SerializeField] private WeaponData[] possibleWeapons;

    [Header("Interaction UI")]
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private GameObject interactionPanel;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    [Header("Auto Close")]
    [SerializeField] private float autoCloseDelay = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private PlayerCombat playerCombat;
    private PlayerInputReader inputReader;
    private Transform player;

    private bool playerInRange;
    private bool subscribedToInput;

    private ChestState state = ChestState.Closed;
    private WeaponData currentReward;
    private float rewardSpawnTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        HideAllRewards();
        HideText();
    }

    private void Update()
    {
        if (state == ChestState.RewardReady &&
            Time.time > rewardSpawnTime + autoCloseDelay)
        {
            CloseChest();
        }
    }

    private void OnDestroy()
    {
        if (inputReader != null)
            inputReader.OnInteractPressed -= HandleInteractPressed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        player = other.transform;
        playerCombat = other.GetComponent<PlayerCombat>();
        inputReader = other.GetComponent<PlayerInputReader>();

        if (inputReader != null && !subscribedToInput)
        {
            inputReader.OnInteractPressed += HandleInteractPressed;
            subscribedToInput = true;

          //  if (showDebug)
            //    Debug.Log("Chest connected to player input.");
        }

        UpdateInteractionText();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        HideText();
    }

    private void HandleInteractPressed()
    {
        if (!playerInRange)
            return;

        if (state == ChestState.Closed)
        {
            TryOpenChest();
            return;
        }

        if (state == ChestState.Opening)
        {
            if (showDebug)
                Debug.Log("Chest is opening...");
            return;
        }

        if (state == ChestState.RewardReady)
        {
            TryCollectReward();
        }
    }

    private void TryOpenChest()
    {
        if (SoulManager.Instance == null)
        {
            Debug.LogWarning("SoulManager.Instance does not exist.");
            return;
        }

        List<WeaponData> notOwnedWeapons = GetNotOwnedWeapons();

        if (notOwnedWeapons.Count == 0)
        {
            SetText("You already own all available weapons.");
            return;
        }

        if (!SoulManager.Instance.TrySpendSouls(cost))
        {
            SetText("Not enough souls. Need " + cost + " souls.");
            return;
        }

        currentReward = notOwnedWeapons[Random.Range(0, notOwnedWeapons.Count)];

        state = ChestState.Opening;
        HideText();

        if (animator != null)
            animator.SetTrigger(openTrigger);

        //if (showDebug)
           // Debug.Log("Opening chest...");
    }

    public void OnChestOpenAnimationFinished()
    {
        if (state != ChestState.Opening)
            return;

        ShowReward(currentReward);

        rewardSpawnTime = Time.time;
        state = ChestState.RewardReady;

        UpdateInteractionText();

       // if (showDebug && currentReward != null)
         //   Debug.Log("Weapon ready to pick up: " + currentReward.weaponName);
    }

    private void TryCollectReward()
    {
        if (currentReward == null)
            return;

        bool added = playerCombat.AddWeapon(currentReward);

        if (!added)
        {
            SetText("Could not pick up this weapon.");
            return;
        }

       // if (showDebug)
         //   Debug.Log("Picked up: " + currentReward.weaponName);

        CloseChest();
    }

    private void CloseChest()
    {
        HideAllRewards();

        if (animator != null)
            animator.SetTrigger(closeTrigger);

        currentReward = null;
        state = ChestState.Closed;

        UpdateInteractionText();

      //  if (showDebug)
        //    Debug.Log("Chest closed.");
    }

    private List<WeaponData> GetNotOwnedWeapons()
    {
        List<WeaponData> result = new List<WeaponData>();

        foreach (WeaponData weapon in possibleWeapons)
        {
            if (weapon == null)
                continue;

            if (!playerCombat.HasWeapon(weapon))
                result.Add(weapon);
        }

        return result;
    }

    private void ShowReward(WeaponData reward)
    {
        HideAllRewards();

        if (reward == pistolData && pistolRewardObject != null)
            pistolRewardObject.SetActive(true);

        if (reward == daggerData && daggerRewardObject != null)
            daggerRewardObject.SetActive(true);

        if (reward == minigunData && minigunRewardObject != null)
            minigunRewardObject.SetActive(true);

        if (reward == scytheData && scytheRewardObject != null)
            scytheRewardObject.SetActive(true);
    }

    private void HideAllRewards()
    {
        if (pistolRewardObject != null)
            pistolRewardObject.SetActive(false);

        if (daggerRewardObject != null)
            daggerRewardObject.SetActive(false);

        if (minigunRewardObject != null)
            minigunRewardObject.SetActive(false);

        if (scytheRewardObject != null)
            scytheRewardObject.SetActive(false);
    }

    private void UpdateInteractionText()
    {
        if (!playerInRange || state == ChestState.Opening)
        {
            HideText();
            return;
        }

        if (state == ChestState.Closed)
        {
            SetText("Press E / Square to open chest - " + cost + " souls for a weapon");
            return;
        }

        if (state == ChestState.RewardReady)
        {
            SetText("Press E / Square to pick up weapon");
        }
    }

    private void SetText(string message)
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        if (interactionText == null)
            return;

        interactionText.gameObject.SetActive(true);
        interactionText.text = message;
    }

    private void HideText()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

    }
}
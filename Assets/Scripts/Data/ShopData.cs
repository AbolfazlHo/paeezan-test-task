using System;
using System.Collections;
using Enums;
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;

namespace Data
{
    public class ShopData : MonoBehaviour
    {
        public static ShopData Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _currencyText;

//        [SerializeField] private IntEvent OnShopDataLoading;

        [SerializeField] private NullEvent OnShopDataLoading;
        [SerializeField] private NullEvent OnShopDataLoaded;
        
        
        [SerializeField] private IntEvent OnShopPanelSelected;



        private bool _isLoadning = false;

        private Coroutine _loadShopItemsCoroutine = null;
        private Coroutine _placeShopItemsCoroutine = null;
        
        
        
        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
//            throw new NotImplementedException();

            OnShopPanelSelected.OnEventRaised += CheckForFirstShow;

            OnShopDataLoading.OnEventRaised += CheckForLoad;

        }

        private void OnDisable()
        {
            OnShopPanelSelected.OnEventRaised -= CheckForFirstShow;

            OnShopDataLoading.OnEventRaised -= CheckForLoad;
        }

        private void CheckForFirstShow(int tabType)
        {
            if (tabType != (int)TabType.Shop) return;
//            OnShopDataLoading.Raise(tabType);
            OnShopDataLoading.Raise();
        }
        
//        private void CheckForLoad(int tabType)
        private void CheckForLoad()
        {
//            if (tabType != (int)TabType.Shop) return;


            if (_isLoadning) return;

            _isLoadning = true;
            LoadCurrency();

            if (_loadShopItemsCoroutine != null) StopCoroutine(_loadShopItemsCoroutine);
            if (_placeShopItemsCoroutine != null) StopCoroutine(_placeShopItemsCoroutine);

            _loadShopItemsCoroutine = StartCoroutine(nameof(LoadShopItems));


        }

        private void LoadCurrency()
        {
            var totalCurrency = PlayerPrefsSaveService.Main.LoadInt("Currency");
            _currencyText.text = $"Currency : {totalCurrency}";
                
        }

        private IEnumerator LoadShopItems()
        {
            // ToDo . . . 

//            yield return null;



            // صرفا برای دیدن پنل لودیگ در حالت کنونی
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.1f);
            }


            yield return _placeShopItemsCoroutine = StartCoroutine(nameof(PlaceShopItems));
        }

        private IEnumerator PlaceShopItems()
        {
            // ToDo . . . 

            _isLoadning = false;
            OnShopDataLoaded.Raise();
            
            yield return null;
        }
        
        

        
    }
}

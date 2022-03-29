using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace ZFramework
{
    public class CardManager : SingletonCrossScene<CardManager>
    {
        private const int MinCardsInHand = 4;
        private const int MaxCardsInHand = 6;

        private CardsDB _db;
        /// <summary>
        /// В инвентаре лежат non shared данные, чтобы пожно было их модифицировать без изменения карт в базе данных.
        /// Например апгрейд карт в мете, если нам данная фича не нужна, то можно заменить на List<int> с айдишниками.
        /// </summary>
        private List<CardInv> _inventory = new List<CardInv>();

        private Dictionary<int, int> _mapIDs = new Dictionary<int, int>();
        private Dictionary<string, UIControllers> _pageControllers = new Dictionary<string, UIControllers>();

        public List<CardInv> CardsInArm = new List<CardInv>();
        /// <summary>
        /// Функция предварительной инициализации
        /// </summary>
        public void PreInit()
        {
            RegisterStaticObject();

            App.OnBeforeStart += LoadDB;
            App.OnBeforeStart += RegisterpageControllers;
            App.OnStart += Init;
        }

        /// <summary>
        /// Регистрирует (создает и инициализирует) глобальную статик переменную, чтобы у нас был доступ к стору из любого участка кода.
        /// Реализуется паттерн синглтона (потокобезопасный).
        /// </summary>
        public void RegisterStaticObject()
        {
            App.CardManager = CardManager.Instance();
        }

        private void LoadDB()
        {
            StartCoroutine(LoadAsyncDB());
        }

        private void RegisterpageControllers()
        {
            AddControllerToPage("Index", new UICardHandController());
            App.UI.OnRenderPage += RunRenderPageControllers;
        }

        private void AddControllerToPage(string link, IUIController controller)
        {
            if(!_pageControllers.ContainsKey(link))
            {
                _pageControllers.Add(link, new UIControllers());
            }

            _pageControllers[link] += controller;
        }

        private void RunRenderPageControllers(string link, VisualElement page)
        {
            foreach(IUIController controller in _pageControllers[link].Controllers)
            {
                controller.Render(page);
            }
        }

        private IEnumerator LoadAsyncDB()
        {
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<CardsDB> dbHandle = Addressables.LoadAssetAsync<CardsDB>("Assets/App/Content/DB/CardsBD.asset");

            yield return dbHandle;

            if (dbHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                _db = dbHandle.Result;
            }
        }

        private void Init()
        {
            GenerateMapIDs();
            GenerateInventory();
            GenerateCardsInArm();
        }

        private void GenerateMapIDs()
        {
            // Генерируем соотношение ID карт c идентификатором в массиве бд с картами
            // На случай, если в массиве айдишники идут не по порядку
            for (int i = 0; i < _db.Cards.Length; i++)
            {
                _mapIDs.Add(_db.Cards[i].ID, i);
            }
        }
        
        private void GenerateInventory()
        {
            //generate inventory
            for (int i = 0; i < 30; i++)
            {
                int randomCardID = Random.Range(0, _db.Cards.Length);
                _inventory.Add(new CardInv(_db.Cards[randomCardID].ID, _db.Cards[randomCardID].Effects));
            }
        }

        private void GenerateCardsInArm()
        {
            int randCardsInArm = Random.Range(MinCardsInHand, MaxCardsInHand);

            for (int i = 0; i < randCardsInArm; i++)
            {
                int randomCardID = Random.Range(0, _inventory.Count);
                CardsInArm.Add(new CardInv(_inventory[randomCardID].ID, _inventory[randomCardID].Effects));
            }
        }

        public CardsDB.CardInfo GetCardInfoByID(int id)
        {
            return _db.Cards[_mapIDs[id]];
        }
    }
}
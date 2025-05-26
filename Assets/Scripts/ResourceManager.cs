using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ResourceManager : Singleton<ResourceManager>
{

#if IN_APP
    public static event Action<string> ProductPurchased;
    public static event Action<bool> ProductRestored;
#endif

    public static string NoAdsProductID => GameSettings.Default.InAppSetting.removeAdsId;

    public static bool EnableAds
    {
        get => PrefManager.GetBool(nameof(EnableAds), true);
        set => PrefManager.SetBool(nameof(EnableAds), value);
    }

    public static int Hints
    {
        get => PrefManager.GetInt(nameof(Hints), 3);
        set => PrefManager.SetInt(nameof(Hints), value);
    }

//    public static int Stars
//    {
//        get => PrefManager.GetInt(nameof(Stars), 0);
//        set => PrefManager.SetInt(nameof(Stars),value);
//    }

    public static bool UnlimitedHints
    {
        get => PrefManager.GetBool(nameof(UnlimitedHints), false);
        set => PrefManager.SetBool(nameof(UnlimitedHints), value);
    }

    public static int Coins
    {
        get => PrefManager.GetInt(nameof(Coins));
        set => PrefManager.SetInt(nameof(Coins), value);
    }

    public static Store Store => Store.Default;



    //    public static bool AbleToRestore => EnableAds;

    public Purchaser Purchaser { get; private set; }

    protected override void OnInit()
    {
        base.OnInit();
        Purchaser = new Purchaser(
            Store.Hints.Where(product => product.priceDetails.type == PriceDetails.Type.InAppConsumable).Select(product => product.priceDetails.productId),
            Store.Hints.Where(product => product.priceDetails.type == PriceDetails.Type.InAppNonConsumable).Select(product => product.priceDetails.productId).Concat(new[] { NoAdsProductID }));
        Purchaser.RestorePurchased += PurchaserOnRestorePurchased;
    }

    private void PurchaserOnRestorePurchased(bool restored)
    {
        var unlimitedProductId = Store.Hints.FirstOrDefault(product => product.isUnlimited).priceDetails
            .productId;
        if (!UnlimitedHints &&
            Purchaser.ItemAlreadyPurchased(unlimitedProductId))
        {
            UnlimitedHints = true;
        }

        if (EnableAds && Purchaser.ItemAlreadyPurchased(NoAdsProductID))
        {
            EnableAds = false;
        }
        Debug.Log($"InAppPurchase Restored Unlimited:{Purchaser.ItemAlreadyPurchased(unlimitedProductId)}");
        ProductRestored?.Invoke(restored);
    }


    public static void RestorePurchase()
    {
        Debug.Log("Restore InAppPurchase");
        Instance.Purchaser.Restore();
    }

    private static void PurchaseInApp(string productId, Action<bool> completed = null)
    {
        Instance.Purchaser.BuyProduct(productId, success =>
        {
            completed?.Invoke(success);
            if (success)
                ProductPurchased?.Invoke(productId);
        });
    }

    public static string GetPrice(string productId) => Instance?.Purchaser?.GetPrice(productId);

    public static HintProduct GetHint(string id) => Store.Hints.FirstOrDefault(product => product.id == id);

    public static void PurchaseHint(string hintProductId, Action<bool> completed = null)
    {
        var product = GetHint(hintProductId);
        if (product.priceDetails.type == PriceDetails.Type.InAppConsumable || product.priceDetails.type == PriceDetails.Type.InAppNonConsumable)
        {
            PurchaseInApp(product.priceDetails.productId, success =>
            {
                if (success)
                {
                    if (product.isUnlimited)
                        UnlimitedHints = true;
                    else
                        Hints += product.value;
                }
                completed?.Invoke(success);
            });
        }
    }

    public static void PurchaseNoAds(Action<bool> completed)
    {
        PurchaseInApp(NoAdsProductID, (success) =>
        {
            if (success)
                EnableAds = false;

            completed?.Invoke(success);
        });
    }
}


//Level Related
public partial class ResourceManager
{

    public static IEnumerable<ILevel> Levels => Resources.Load<Levels>(global::Levels.DEFAULT_NAME).Select(level => new LevelDecorator(level));
    public static Levels LevelsScriptable => Resources.Load<Levels>(global::Levels.DEFAULT_NAME);
    public static bool AbleToRestore => !UnlimitedHints;

    public static ILevel GetLevel(int lvl) => Levels.FirstOrDefault(level => level.LevelNo == lvl);
    public static bool HasLevel(int lvl) => GetLevel(lvl) != null;

    public static int TargetLevel => Levels.ToList().FindLast(lvl => !lvl.Locked).LevelNo;

    public static void CompleteLevel(int lvl)
    {
        if (HasLevel(lvl + 1))
        {
            PrefManager.SetBool(GetKeyForLocked(lvl + 1), false);
        }

    }


    private class LevelDecorator : ILevel
    {
        private readonly ILevel _lvl;
        public IEnumerable<ShapeData> Shapes => _lvl.Shapes;
        public BoardData Board => _lvl.Board;


        public int LevelNo => _lvl.LevelNo;
        public bool Locked => PrefManager.GetBool(GetKeyForLocked(LevelNo), LevelNo != 1);

        public LevelDecorator(ILevel lvl)
        {
            _lvl = lvl;
        }
    }

    private static string GetKeyForLocked(int lvl) => $"Lock_{lvl}";
}
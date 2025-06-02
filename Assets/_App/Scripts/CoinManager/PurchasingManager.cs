using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchasingManager : MonoBehaviour
{
   public void OnPressDown(int i)
   {
      switch (i)
      {
         case 1:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(10);
            };
             IAPManager.Instance.BuyProductID(IAPKey.PACK1);
            break;
         case 2:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(20);
            };
            IAPManager.Instance.BuyProductID(IAPKey.PACK2);
            break;
         case 3:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(40);
            };
            IAPManager.Instance.BuyProductID(IAPKey.PACK3);
            break;
         case 4:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(60);
            };
            IAPManager.Instance.BuyProductID(IAPKey.PACK4);
            break;
         case 5:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(100);
            };
            IAPManager.Instance.BuyProductID(IAPKey.PACK5);
            break;
         case 6:
            IAPManager.OnPurchaseSuccess = () =>
            {
               GameDataManager.Instance.playerData.AddDiamond(200);
            };
            IAPManager.Instance.BuyProductID(IAPKey.PACK6);
            break;
      }
   }

   public void Sub(int i)
   {
      GameDataManager.Instance.playerData.SubDiamond(i);
   }
}

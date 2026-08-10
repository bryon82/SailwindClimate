using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Climate.Climate_Plugin;

namespace Climate
{
    internal class AddShopItems
    {
        internal static void SceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (scene.name == "island 1 A Gold Rock")
                GoldRockCity();
            if (scene.name == "island 15 M (Fort)")
                FortAestrin();
            if (scene.name == "island 9 E Dragon Cliffs")
                DragonCliffs();
        }

        internal static void GoldRockCity()
        {
            var scenery = GameObject.Find("island 1 A (gold rock) scenery");
            if (scenery == null)
            {
                LogError("Gold Rock City scenery not found.");
                return;
            }

            var shopkeeper = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "rad shopkeeper");
            if (shopkeeper == null)
            {
                // resize shop (10) local scale to fit new stall
                var shop10 = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "shop (10)");
                if (shop10 != null)
                {
                    shop10.localScale = new Vector3(21.78927f, 13.92925f, 12.39353f);
                    shop10.localPosition = new Vector3(1558.77f, 8.4f, -361.33f);
                }

                var shopPos = new Vector3(1545f, 7.21f, -361.5f);
                var shopRot = new Vector3(270f, 238f, 0f);
                var shopkeeperPos = new Vector3(1544f, 5.06f, -360f);
                var shopkeeperRot = new Vector3(0f, 140f, 0f);
                AddShopStall(scenery, "market_stall (10)", "shop (11)", shopPos, shopRot, "shopkeeper (11)", shopkeeperPos, shopkeeperRot);
            }

            MakeShopItem("shop item 320", scenery.transform, new Vector3(1543.7f, 7.036f, -362.12f), new Vector3(78.5f, 325f, 0f), Items.Barometer);
            MakeShopItem("shop item 321", scenery.transform, new Vector3(1543.8f, 6.806f, -363.2f), new Vector3(78.5f, 325f, 0f), Items.Thermometer);
            MakeShopItem("shop item 322", scenery.transform, new Vector3(1544.7f, 6.806f, -362.6f), new Vector3(78.5f, 325f, 0f), Items.Hygrometer);
        }

        internal static void FortAestrin()
        {
            var scenery = GameObject.Find("island 15 M (Fort) scenery");
            if (scenery == null)
            {
                LogError("Fort Aestrin scenery not found.");
                return;
            }

            var shopkeeper = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "rad shopkeeper");
            if (shopkeeper == null)
            {
                var shopPos = new Vector3(-47.74f, 2.26f, 44.77f);
                var shopRot = new Vector3(270f, 359.7961f, 0f);
                var shopkeeperPos = new Vector3(-47.74f, 2.1f, 43.5f);
                var shopkeeperRot = new Vector3(0f, 359.7961f, 0f);
                AddShopStall(scenery, "market stall medi 2 (2)", "shop area (13)", shopPos, shopRot, "shopkeeper (3)", shopkeeperPos, shopkeeperRot);
                var shop = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "rad shop");
                shop.localScale = new Vector3(6f, 6f, 6f);

                // ft. aestrin shops have banners
                var bannerPost = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "banner M post");
                if (bannerPost != null)
                {
                    var bp = GameObject.Instantiate(bannerPost.gameObject, scenery.transform);
                    bp.name = "rad banner";
                    bp.transform.localPosition = new Vector3(-44.5009f, 2.43f, 46.2771f);
                    bp.GetComponent<MeshRenderer>().enabled = true;
                    bp.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                }
            }

            MakeShopItem("shop item (320)", scenery.transform, new Vector3(-48.447f, 2.95f, 44.35f), new Vector3(77f, 180f, 0f), Items.Barometer);
            MakeShopItem("shop item (321)", scenery.transform, new Vector3(-48.166f, 2.85f, 44.82f), new Vector3(77f, 180f, 0f), Items.Thermometer);
            MakeShopItem("shop item (322)", scenery.transform, new Vector3(-48.716f, 2.85f, 44.82f), new Vector3(77f, 180f, 0f), Items.Hygrometer);
        }

        internal static void DragonCliffs()
        {
            var scenery = GameObject.Find("island 9 E (dragon cliffs) scenery");
            if (scenery == null)
            {
                LogError("Dragon Cliffs scenery not found.");
                return;
            }

            var shopkeeper = scenery.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "rad shopkeeper");
            if (shopkeeper == null)
            {
                var shopPos = new Vector3(-73.134f, 4.68f, -552.089f);
                var shopRot = new Vector3(270f, 45f, 0f);
                var shopkeeperPos = new Vector3(-72.574f, 3.603f, -552.519f);
                var shopkeeperRot = new Vector3(0f, 313.5019f, 0f);
                AddShopStall(scenery, "market_stall", "shop area (8)", shopPos, shopRot, "shopkeeper (3)", shopkeeperPos, shopkeeperRot);
            }

            MakeShopItem("shop item spawner (320)", scenery.transform, new Vector3(-73.474f, 4.6f, -552.5f), new Vector3(76f, 140f, 0f), Items.Barometer);
            MakeShopItem("shop item spawner (321)", scenery.transform, new Vector3(-73.574f, 4.502f, -552f), new Vector3(76f, 140f, 0f), Items.Thermometer);
            MakeShopItem("shop item spawner (322)", scenery.transform, new Vector3(-73.974f, 4.502f, -552.4f), new Vector3(76f, 140f, 0f), Items.Hygrometer);
        }

        private static void MakeShopItem(string name, Transform parent, Vector3 position, Vector3 rotation, GameObject go)
        {
            var shopitem = new GameObject(name);
            shopitem.transform.parent = parent;
            shopitem.transform.localPosition = position;
            shopitem.transform.localRotation = Quaternion.Euler(rotation);
            var filter = shopitem.AddComponent<MeshFilter>();
            filter.mesh = go.GetComponent<MeshFilter>().mesh;
            shopitem.AddComponent<MeshRenderer>();
            var itemSpawner = shopitem.AddComponent<ShopItemSpawner>();
            itemSpawner.itemPrefab = go;
        }

        private static void AddShopStall(GameObject scenery, string templateStallName, string templateShop, Vector3 pos, Vector3 rot, string templateShopkeeper, Vector3 shopkeeperPos, Vector3 shopkeeperRot) 
        {
            var stallTemplate = scenery.GetComponentsInChildren<Transform>()?.FirstOrDefault(t => t.name == templateStallName);
            var stall = GameObject.Instantiate(stallTemplate.gameObject, scenery.transform);
            stall.name = "rad market stall";
            stall.transform.localPosition = pos;
            stall.transform.localRotation = Quaternion.Euler(rot);
            stall.GetComponent<MeshRenderer>().enabled = true;

            var shopTemplate = scenery.GetComponentsInChildren<Transform>()?.FirstOrDefault(t => t.name == templateShop);
            var shop = GameObject.Instantiate(shopTemplate.gameObject, scenery.transform);
            shop.transform.localPosition = pos;
            shop.transform.localRotation = Quaternion.Euler(rot);
            shop.name = "rad shop";
            var shopArea = shop.GetComponent<ShopArea>();
            shopArea.itemsForSale.Clear();
            
            var shopkeeperTemplate = scenery.GetComponentsInChildren<Transform>()?.FirstOrDefault(t => t.name == templateShopkeeper);
            var shopkeeper = GameObject.Instantiate(shopkeeperTemplate.gameObject, scenery.transform);
            shopkeeper.transform.localPosition = shopkeeperPos;
            shopkeeper.transform.localRotation = Quaternion.Euler(shopkeeperRot);
            shopkeeper.name = "rad shopkeeper";            
            shopkeeper.SetPrivateField("shopLocalPos", pos);
            shopkeeper.SetPrivateField("shopRotation", Quaternion.Euler(rot));
            shopArea.SetPrivateField("shopkeeper", shopkeeper.GetComponent<Shopkeeper>());
            shopkeeper.SetPrivateField("shop", shopArea);
        }
    }
}

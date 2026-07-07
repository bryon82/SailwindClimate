using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Climate.Cli_Plugin;

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

            MoveShopItem("shop item (221)", scenery.transform, new Vector3(1524.367f, 7.13f, -385.224f), new Vector3(0f, 129.6f, 0f));

            MakeShopItem("shop item 320", scenery.transform, new Vector3(1525.5f, 7.006f, -385.2f), new Vector3(90f, 241f, 0f), Items.Barometer);
            MakeShopItem("shop item 321", scenery.transform, new Vector3(1526f, 7.006f, -386.05f), new Vector3(90f, 245f, 0f), Items.Thermometer);
            MakeShopItem("shop item 322", scenery.transform, new Vector3(1526.7f, 7.006f, -386.76f), new Vector3(90f, 237f, 0f), Items.Hygrometer);
        }

        internal static void FortAestrin()
        {
            var scenery = GameObject.Find("island 15 M (Fort) scenery");
            if (scenery == null)
            {
                LogError("Fort Aestrin scenery not found.");
                return;
            }

            MakeShopItem("shop item (320)", scenery.transform, new Vector3(-74.816f, 2.7f, 44.2995f), new Vector3(0f, 180f, 0f), Items.Barometer);
            MakeShopItem("shop item (321)", scenery.transform, new Vector3(-75.316f, 2.7f, 44.2995f), new Vector3(0f, 180f, 0f), Items.Thermometer);
            MakeShopItem("shop item (322)", scenery.transform, new Vector3(-75.816f, 2.7f, 44.2995f), new Vector3(0f, 180f, 0f), Items.Hygrometer);
        }

        internal static void DragonCliffs()
        {
            var scenery = GameObject.Find("island 9 E (dragon cliffs) scenery");
            if (scenery == null)
            {
                LogError("Dragon Cliffs scenery not found.");
                return;
            }

            MoveShopItem("shop item spawner (101)", scenery.transform, new Vector3(-89.108f, 4.507f, -543.922f));
            MoveShopItem("shop item spawner (165)", scenery.transform, new Vector3(-88.906f, 4.5156f, -544.329f));
            MoveShopItem("shop item spawner (168)", scenery.transform, new Vector3(-90.455f, 4.489f, -542.922f));
            MoveShopItem("shop item spawner (60)", scenery.transform, new Vector3(-91.1984f, 4.453f, -542.0734f));
            MoveShopItem("shop item spawner (163)", scenery.transform, new Vector3(-91.05f, 4.513f, -542.25f));

            var itemToRemove = scenery.GetComponentsInChildren<Transform>()?.FirstOrDefault(t => t.name == "shop item spawner (71)");
            if (itemToRemove != null)
                itemToRemove.gameObject.SetActive(false);

            MakeShopItem("shop item spawner (320)", scenery.transform, new Vector3(-89.268f, 4.477f, -544.36f), new Vector3(80f, 230f, 0f), Items.Barometer);
            MakeShopItem("shop item spawner (321)", scenery.transform, new Vector3(-90.7f, 4.455f, -542.58f), new Vector3(65f, 230f, 0f), Items.Thermometer);
            MakeShopItem("shop item spawner (322)", scenery.transform, new Vector3(-90.95f, 4.505f, -542.78f), new Vector3(70f, 230f, 0f), Items.Hygrometer);
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

        private static void MoveShopItem(string name, Transform parent, Vector3 position, Vector3? rotation = null)
        {
            var itemToMove = parent.GetComponentsInChildren<Transform>()?.FirstOrDefault(t => t.name == name);
            if (itemToMove != null)
            {
                itemToMove.localPosition = position;
                if (rotation.HasValue)
                    itemToMove.localRotation = Quaternion.Euler(rotation.Value);
            }
        }
    }
}

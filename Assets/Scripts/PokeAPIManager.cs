using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

public class PokeAPIManager : MonoBehaviour
{
    public string baseUrl = "https://pokeapi.co/api/v2/pokemon?limit=100000&offset=0"; // URL for fetching Pokémon
    public List<PokemonData> allPokemonList = new List<PokemonData>(); // Store all Pokémon data
    public ObjectPool PrefabPool;  // Parent object in the ScrollView for buttons

    private void Start()
    {
       StartCoroutine(GetPokemonList());

    }

    IEnumerator GetPokemonList()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
                JArray pokemonArray = (JArray)jsonResponse["results"];

                // Store all Pokémon data in a list
                for (int i = 0; i < 20; i++)
                {
                    string pokemonName = pokemonArray[i]["name"].ToString();
                    string pokemonUrl = pokemonArray[i]["url"].ToString();
                    int serialNumber = i + 1;  // Serial number starts at 1
                    // Fetch Pokémon details (including image and stats)
                    yield return StartCoroutine(GetPokemonDetails(pokemonName, pokemonUrl, serialNumber));
                }
            }
        }
    }

    IEnumerator GetPokemonDetails(string name, string url, int serialNumber)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                JObject pokemonDetails = JObject.Parse(request.downloadHandler.text);

                // Extract image URL
                string imageUrl = pokemonDetails["sprites"]["front_default"].ToString();

                // Extract stats
                // Display Pokémon stats
                string stats = "Stats:\n";
                foreach (var stat in pokemonDetails["stats"])
                {
                    stats += stat["stat"]["name"].ToString() + ": " + stat["base_stat"].ToString() + "\n";
                }

                // Download the image and convert it to a Sprite
                Sprite imageSprite = null;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    yield return StartCoroutine(DownloadImage(imageUrl, (sprite) =>
                    {
                        imageSprite = sprite;
                    }));
                }

                // Add to list
                PokemonData pokemonData = new PokemonData(serialNumber, name, url, imageSprite, stats);
                allPokemonList.Add(pokemonData);

                GameObject prefab = PrefabPool.GetObject();

                // Get the Image component
                Transform childTransform = prefab.transform.GetChild(0).transform.GetChild(1);
                GameObject nestedChild = childTransform.gameObject;

                if(nestedChild)
                { 
                nestedChild.GetComponent<Image>().sprite = imageSprite; // Set the downloaded sprite
                }
                else
                {
                    Debug.LogError("Image component not found in prefab or imageSprite is null.");
                }

                // Get all TextMeshProUGUI components
                TextMeshProUGUI[] textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>();

                // Set the name text (first TextMeshProUGUI)
                if (textComponents.Length > 0)
                {
                    textComponents[0].text = name; // Set the name to the first TextMeshProUGUI
                }
                else
                {
                    Debug.LogError("No TextMeshProUGUI components found in prefab.");
                }

                // Set the stats text (second TextMeshProUGUI)
                if (textComponents.Length > 1)
                {
                    textComponents[1].text = stats; // Set the stats to the second TextMeshProUGUI
                }
                else
                {
                    Debug.LogError("Second TextMeshProUGUI component not found in prefab.");
                }

                Debug.Log($"Added: {name} with stats: {stats} and image loaded: {imageSprite != null}");
            }
        }

    }

    IEnumerator DownloadImage(string url, System.Action<Sprite> callback)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    callback(null);
                }
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    callback(sprite);
                }
            }
        }


    [System.Serializable]
    public class PokemonData
    {
        public string name;
        public string url;
        public Sprite imageSprite;  // Store the image as a Sprite
        public string stats;
        public int serialNumber;  // New field to store the serial number

        public PokemonData(int serialNumber, string name, string url, Sprite imageSprite, string stats)
        {
            this.serialNumber = serialNumber;
            this.name = name;
            this.url = url;
            this.imageSprite = imageSprite;
            this.stats = stats;
        }
    }

}



# Pokémon Card Collection 📇

![Pokemon Card Collection](https://github.com/user-attachments/assets/7a55ebd9-e571-4818-afcf-6f02ae15db42)

## 📌 Introduction
**Pokémon Card Collection** is a Unity-based mobile app that allows users to explore and collect data on various Pokémon from the Pokémon API. With a smooth and interactive interface, users can swipe through cards to view Pokémon details such as images, stats, and more. This app features a custom object pool system to efficiently manage UI elements and provides a scrolling system to browse through the Pokémon collection.

## 🔥 Features
- 🎮 **Swipe Navigation**: Smooth swipe interactions to navigate through Pokémon cards.
- 🐾 **Pokémon Details**: Fetches Pokémon data including name, stats, and images from the Pokémon API.
- 📱 **Object Pooling**: Efficient memory management with object pooling for card elements.
- 📊 **Dynamic Content**: Fetches data dynamically from the Pokémon API to populate the cards.
- 🖼️ **Image Downloading**: Downloads Pokémon images and displays them as sprites on the cards.
- 🖱️ **Smooth UI**: Interactive UI elements for a responsive user experience.

---

## 🏗️ How It Works
The app is structured with several scripts that handle different aspects of the Pokémon card display, from data fetching to UI updates.

### 📌 **ObjectPool Script**
Manages the pooling of card UI elements to ensure smooth performance when cards are created or recycled.

```csharp
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 20;
    public Transform scrollContent;  
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    public swipe swipe;

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, scrollContent);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
        swipe.enabled = true;
    }

    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, scrollContent);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
```

### 📌 **PokeAPIManager Script**
Handles fetching Pokémon data from the Pokémon API and updating the UI with the Pokémon’s details.

```csharp
public class PokeAPIManager : MonoBehaviour
{
    public string baseUrl = "https://pokeapi.co/api/v2/pokemon?limit=100000&offset=0"; // URL for fetching Pokémon
    public List<PokemonData> allPokemonList = new List<PokemonData>();

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

                for (int i = 0; i < 20; i++)
                {
                    string pokemonName = pokemonArray[i]["name"].ToString();
                    string pokemonUrl = pokemonArray[i]["url"].ToString();
                    int serialNumber = i + 1;
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

                string imageUrl = pokemonDetails["sprites"]["front_default"].ToString();
                string stats = "Stats:\n";
                foreach (var stat in pokemonDetails["stats"])
                {
                    stats += stat["stat"]["name"].ToString() + ": " + stat["base_stat"].ToString() + "\n";
                }

                Sprite imageSprite = null;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    yield return StartCoroutine(DownloadImage(imageUrl, (sprite) =>
                    {
                        imageSprite = sprite;
                    }));
                }

                PokemonData pokemonData = new PokemonData(serialNumber, name, url, imageSprite, stats);
                allPokemonList.Add(pokemonData);

                GameObject prefab = PrefabPool.GetObject();
                Transform childTransform = prefab.transform.GetChild(0).transform.GetChild(1);
                GameObject nestedChild = childTransform.gameObject;

                if (nestedChild)
                { 
                    nestedChild.GetComponent<Image>().sprite = imageSprite;
                }

                TextMeshProUGUI[] textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>();
                if (textComponents.Length > 0)
                {
                    textComponents[0].text = name;
                }
                if (textComponents.Length > 1)
                {
                    textComponents[1].text = stats;
                }
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
        public Sprite imageSprite;
        public string stats;
        public int serialNumber;

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
```

### 📌 **Swipe Script**
Handles swipe actions and smooth scrolling for the Pokémon cards in the UI.

```csharp
public class swipe : MonoBehaviour
{
    public GameObject scrollbar;
    private float scroll_pos = 0;
    float[] pos;
    private bool runIt = false;
    private float time;
    private Button takeTheBtn;
    int btnNumber;

    void Update()
    {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);

        if (runIt)
        {
            HandleTransition(distance, pos, takeTheBtn);
            time += Time.deltaTime;

            if (time > 1f)
            {
                time = 0;
                runIt = false;
            }
        }

        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                for (int j = 0; j < pos.Length; j++)
                {
                    if (j != i)
                    {
                        transform.GetChild(j).localScale = Vector2.Lerp(transform.GetChild(j).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    private void HandleTransition(float distance, float[] pos, Button btn)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[btnNumber], 1f * Time.deltaTime);
            }
        }

        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            btn.transform.name = ".";
        }
    }

    public void WhichBtnClicked(Button btn)
    {
        btn.transform.name = "clicked";

        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            if (btn.transform.parent.transform.GetChild(i).transform.name == "clicked")
            {
                btnNumber = i;
                takeTheBtn = btn;
                time = 0;
                scroll_pos = pos[btnNumber];
                runIt = true;
            }
        }
    }
}
```

---

## 🎯 Conclusion
The **Pokémon Card Collection** app offers an engaging experience for Pokémon enthusiasts to explore their favorite Pokémon. By integrating smooth swipe functionality, dynamic content loading from the Pokémon API, and efficient memory management through object pooling, it provides an interactive and responsive user interface. The use of images, stats, and detailed Pokémon data makes this app a fun and informative experience. 🌟

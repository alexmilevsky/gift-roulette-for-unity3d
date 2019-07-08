using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Roulette : MonoBehaviour {

    [System.Serializable]
    public class ItemSet
    {
        public GameObject prefab;
        public Chance chance;
    }

    // chance type for item
    public enum Chance { Default, Rare }

    public static event System.Action WinEvent;
    public static event System.Action StartedEvent;   
    public static event System.Action RecycleItemsEvent; 

    // current speed of roulette
    public static float speed;
    
    [Header("Set max speed of roulette:")]
    public int maxSpeed;
    [Header("Set minimum time when the roulette ends:")]
    public int minTime;
    
    [Space(20)]
    [Header("Set your prefabs and chances(1-5) of items:")]
    public ItemSet[] items;
    
    [Space(20)]
    [Header("Set start position:")]
    // границаmj
    public Transform pointOfStart;
    // объект который будет спавнится
    private GameObject objectToSpawn;
    // state of roulette (playing or not)
    private bool isPlay;
    // it increases speed of roulette
    private bool isSpeedUp;
    // last item from collider
    private string selectedItem;

    [Space(20)]
    [Header("Events:")]
    public UnityEngine.Events.UnityEvent Win;
    public UnityEngine.Events.UnityEvent Started;
    
    private void Start()
    {
        StartPoint.ItemExit += GenerateNewItem;

        speed = 0;
        isPlay = false;
        isSpeedUp = false;
        selectedItem = "";
    }

    private void OnDestroy()
    {
        StartPoint.ItemExit += GenerateNewItem;
    }

    private void Update()
    {        
        if (isPlay && isSpeedUp)
        {
            SpeedUp();
        }
        else if (isPlay && !isSpeedUp)
        {
            SpeedDown();
        }
    }

    /// <summary>
    /// It increases the speed of the roulette
    /// </summary>
    private void SpeedDown()
    {
        if (speed > 0)
        {
            speed -= Time.deltaTime * 2;
        }
        else
        {
            speed = 0;
        }
    }

    /// <summary>
    /// It decreases the speed of the roulette
    /// </summary>
    private void SpeedUp()
    {
        if (speed < maxSpeed)
        {
            speed += Time.deltaTime * 2;
        }
        else
        {
            speed = maxSpeed;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Play()
    {
        if (!isPlay)
        {
            StartCoroutine(StartRoulette());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator StartRoulette()
    {
        GenerateNewItem();
        // start event
        Started.Invoke();
        
        if (StartedEvent != null) 
            StartedEvent();
        
        // set variables to start
        isPlay = true;
        // speed-up
        isSpeedUp = true;
        
        yield return new WaitForSeconds(Random.Range(minTime, minTime + 3));
        // speed-down
        isSpeedUp = false;

        // when all stop
        while (isPlay && speed != 0)
        {
            yield return new WaitForSeconds(1);
        }
       
        StartCoroutine(FinishRoulette());
    }

    /// <summary>
    /// Spawn winner item
    /// </summary>
    public IEnumerator FinishRoulette()
    {
        // reset play
        isPlay = false;
        
        RecycleAllItems();
        
        yield return new WaitForSeconds(0.5f);
        
        ShowWinnerItem();

        yield return new WaitForSeconds(1.5f);
       
        RecycleAllItems();
        
        // win event
        Win.Invoke();
        
        if (WinEvent != null) 
            WinEvent();
    }

    /// <summary>
    /// Spawn winner item
    /// </summary>
    public void ShowWinnerItem()
    {
        selectedItem = GetWinnerItemName();
        
        foreach (var item in items)
        {
            if (item.prefab.gameObject.name == selectedItem)
            {
                item.prefab.Spawn(this.gameObject.transform);
                break;
            }
        }
    }
    
    /// <summary>
    ///  Get name of winner item
    /// </summary>
    public string GetWinnerItemName()
    {        
        int indexOfCloneName = selectedItem.IndexOf("(Clone)");
        
        // if item contains "(Clone)"
        if (indexOfCloneName != -1)
        {
            return selectedItem.Remove(indexOfCloneName);
        }
        else
        {
            return selectedItem;
        }
    }

    /// <summary>
    /// Recycle All items to ObjectPool
    /// </summary>
    public void RecycleAllItems()
    {
        if (RecycleItemsEvent != null) 
            RecycleItemsEvent();
        
        ObjectPool.RecycleAll();        
    }

    /// <summary>
    /// Generate new item for spawn
    /// </summary>
    public void GenerateNewItem()
    {
        int indexOfItem = Random.Range(0, items.Length);
        
        // if you get rare items when trying chance
        switch (items[indexOfItem].chance)
        {
            case Chance.Rare:
                TryChance(Chance.Rare, indexOfItem);
                break;
            default:
                objectToSpawn = items[indexOfItem].prefab;
                break;
        }
        objectToSpawn.Spawn(pointOfStart);
    }

    /// <summary>
    /// try to get a chance
    /// </summary>
    private void TryChance(Chance chance, int currentIndexOfItem)
    {
        bool isChance = false;
        
        if (chance == Chance.Rare)
        {
            // random chance for rare item
            if (Random.Range(0, 25) == 1)
            {
                isChance = true;
            }
        }
        
        if (!isChance)
        {
            int newIndex;
            
            do
            {
                newIndex = Random.Range(0, items.Length);
            }
            while (currentIndexOfItem == newIndex);
            
            objectToSpawn = items[newIndex].prefab;
        }
        // item getChance
        else
        {
            objectToSpawn = items[currentIndexOfItem].prefab;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        selectedItem = collision.gameObject.name;
    }
}
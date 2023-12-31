using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }


    public Transform map;

    public Material outline;

    [Space(20f, order = 0)]

    public GameObject unitCounter;

    [Space(20f, order = 0)]

    public GameObject spell;

    [Space(20f, order = 0)]

    public Sprite chest;

    [Space(1f, order = 0)]

    public Sprite empty;
    public Sprite locked;
    public Sprite redTexture;
    public Sprite brownTexture;
    public Sprite blueTexture;
    public Sprite blackTexture;

    [Space(20f, order = 0)]

    public Transform unitCounterContentUI;
    public Transform recruitUnitContentUI;
    public Transform buildingsContentUI;

    [Space(20f, order = 0)]

    public Transform moveUnitContentUI1;
    public Transform moveUnitContentUI2;

    [Space(20f, order = 0)]
    
    public Transform AttackUnitContentUI1;
    public Transform AttackUnitContentUI2;

    [Space(20f, order = 0)]

    public GameObject unitSlotUI;
    public GameObject buildingSlotUI;
    public GameObject unitCounterSlotUI;
    public GameObject researchUI;

   
    [Space(40f, order = 0)]

    public GameObject BattleConter;
    public Transform BattleUnits
        ;
    public Transform battleYourBar;
    public Transform battleEnemyBar;

    public Transform battleInfo;

    public Sprite noBuilding;

    public BuildingStats[] buildingsStats { private set; get; }
    public UnitStats[] unitStats { private set; get; }

    public Spell[] spells { private set; get; }

    public Research[,] research { private set; get; }

    public Transform pause;

    public Sprite battleIcon;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(this);
        }   
    }

    private void Start()
    {
       if (SceneManager.GetActiveScene().buildIndex != 2) SetUp();
    }

    public void SetUp()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
            map = GameObject.FindGameObjectWithTag("GameMap").transform;

        LoadResearch("Development/2EconomicDevelopment", 2);
        LoadResearch("Development/3ManagementDevelopment", 3);
        LoadResearch("Development/0MilitaryDevelopment", 0);
        LoadResearch("Development/1ScientificDevelopment", 1);

        buildingsStats = Resources.LoadAll<BuildingStats>("Buildings");
        unitStats = Resources.LoadAll<UnitStats>("Units");
        spells = Resources.LoadAll<Spell>("Spells");
    }    

    private void LoadResearch(string path,int index)
    {
        Research[] list = Resources.LoadAll<Research>(path);
        if (research == null) research = new Research[4,list.Length];
        

        int id = 0;
        foreach (Research research in list)
        {
            this.research[index, id] = research;
            id++;
        }
    }

}

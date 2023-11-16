
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
	const string mapsPath = "Assets/Resources/Maps/";
	string currentMap = "World";
	public string saveName;

	public bool ready;
	private bool readyToNextTurn;

	public static GameManager Instance;
	public CameraController cameraController;

	public ProvinceStats[] provinces;
	public int numberOfProvinces;
	public List<Player> botsList;
	private Transform players;

	public Player humanPlayer;


	public Transform map;
	public Transform buildings;
	public SelectingProvinces selectingProvinces;
	public PathFinding pathFinding;

	private int yourProvinceIndex;
	private int enemyProvinceIndex;
	public bool youAttack { private set; get; }

	public int turn { private set; get; } = 0;

	public bool isPlaying = true;
	public bool toLoad;

	public Action load;
	private void Awake()
	{
		if(SceneManager.GetActiveScene().buildIndex == 2)LoadMap(currentMap);
		if (Instance == null)
		{
			Instance = this;
			isPlaying = true;
            DontDestroyOnLoad(this);
        }
        else
		{
			Destroy(this.gameObject);
		}
	}
	private void Start()
	{
		if (!toLoad && isPlaying && SceneManager.GetActiveScene().buildIndex == 2)
		{
			isPlaying = false;
			Debug.Log("xs");
			SetUp();
		}
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.S))
		{
			Save();
		}

		if (UnityEngine.Input.GetKeyDown(KeyCode.L))
		{
			Load(saveName);
		}

	}
	private void OnLevelWasLoaded(int level)
	{
		if (level == 2)
		{        	
			if (isPlaying)
			{
                isPlaying = false;
				Debug.Log("si");
                SetUp();
                if(toLoad)
                {
                    Debug.Log("Git!!!!!!");
                    load();
                    toLoad = false;
                }
            }
			else
			{
				UpdateMap();
			}		
		}else if(level ==0)
		{
			isPlaying = true;
		}
	}



	
	private void LoadMap(string name)
	{
		if (Directory.Exists(mapsPath + name))
		{
			GameObject obj = Resources.Load("Maps/" + name + "/Map") as GameObject;
			Texture2D[] sprites = Resources.LoadAll<Texture2D>("Maps/" + name + "/Sprites");
			obj = Instantiate(obj);
			map = obj.transform;
			foreach (Texture2D sprite in sprites)
			{
				SpriteRenderer spriteRenderer = obj.transform.GetChild(int.Parse(sprite.name)).GetComponent<SpriteRenderer>();
				spriteRenderer.sprite = Sprite.Create(sprite, new Rect(0, 0, sprite.width, sprite.height), new Vector2(0.5f, 0.5f));
			}
		}
		else
		{
			Debug.Log("Map does not exist");
		}

	}
	private void SetUp()
	{
		//Debug.Log("setp");
		GameAssets.Instance.SetUp();
		players = GameObject.FindGameObjectWithTag("Players").transform;
		DontDestroyOnLoad(players);
        map = GameObject.FindGameObjectWithTag("GameMap").transform;
		buildings = GameObject.FindGameObjectWithTag("Buildings").transform;
		selectingProvinces = FindObjectOfType<SelectingProvinces>();
		CreateHumanPlayer();
		LoadBots();
		ProvinceStats[] array = Resources.Load<MapStats>("Maps/World/MapStats").provinces;
		numberOfProvinces = Resources.Load<MapStats>("Maps/World/MapStats").numberOfProvinces;


		provinces = new ProvinceStats[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			provinces[i] = new ProvinceStats();
			provinces[i].CopyData(array[i]);
			ProvinceStats provinceStats = provinces[i];
			if (provinceStats.provinceOwnerIndex != -1)
			{
				selectingProvinces.ChangeProvinceColor(map.GetChild(i).GetComponent<SpriteRenderer>(), GetPlayerColor(provinceStats.provinceOwnerIndex));
			}
		}
		UpdateBotProvinces();
		pathFinding = new PathFinding(numberOfProvinces);

		ready = true;
		readyToNextTurn = true;
		cameraController = Camera.main.GetComponent<CameraController>();
		for (int i = 0; i < provinces.Length; i++)
		{
			ProvinceStats provinceStats = provinces[i];
			if (provinceStats.buildingIndex != -1)
			{
				BonusManager.SetBonus(provinceStats, provinceStats.buildingIndex);
				Transform province = map.GetChild(provinceStats.index).transform;
				Transform transform = new GameObject(province.name, typeof(SpriteRenderer)).transform;
				transform.position = province.position + new Vector3(0, 0.08f, 0);
				transform.parent = buildings;
				transform.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.buildingsStats[provinceStats.buildingIndex].icon;
				transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
			}
		}
		for (int i = 0; i < botsList.Count; i++)
		{
			BonusManager.UpdateLimits(botsList[i].index);
		}
		Time.timeScale = 1.0f;

		UIManager.Instance.SetUp();
		humanPlayer.stats.movementPoints.Set(humanPlayer.stats.movementPoints.limit);
		UIManager.Instance.UpdateCounters();
	}
	private void LoadBots()
	{
		AddBot("Player", true, Color.yellow, 1000, 1 + botsList.Count);
		AddBot("xd", true, Color.cyan, 1000, 1 + botsList.Count);
		AddBot("green", true, Color.red, 1000, 1 + botsList.Count);
	}

	private void AddBot(string name, bool isComputer, Color color, int startCoins, int index)
	{
		Player player = new GameObject(name, typeof(Player)).GetComponent<Player>();
		player.transform.parent = players;
		player.SetUp(name, isComputer, color, startCoins, index);
		botsList.Add(player);
	}

	private void CreateHumanPlayer()
	{
		Player player = new GameObject("Human", typeof(Player)).GetComponent<Player>();
		player.transform.parent = players;
		player.SetUp("Player", false, Color.blue, 10000, 0);
		humanPlayer = player;
	}
	private void UpdateBotProvinces()
	{
		foreach (Player item in botsList)
		{
			item.UpdateProvinces();
		}
	}

	public void UpdateUnitCounter(int index)
	{
	 if(map != null)	selectingProvinces.UpdateUnitNumber(map.GetChild(index).transform);
	}
	public void Battle(int yourProvinceIndex, int enemyProvinceIndex, bool youAttack)
	{
		this.youAttack = youAttack;
		this.yourProvinceIndex = yourProvinceIndex;
		this.enemyProvinceIndex = enemyProvinceIndex;
		SceneManager.LoadScene(1);
	}
	public void GetUnits(out Dictionary<int, int> yourUnits, out Dictionary<int, int> enemyUnits)
	{
		yourUnits = provinces[yourProvinceIndex].units;
		enemyUnits = provinces[enemyProvinceIndex].units;
	}
	public void GetProvinceHP(out int yourProvinceHP, out int enemyProvinceHP)
	{
		if (youAttack)
		{
			yourProvinceHP = 10;
			enemyProvinceHP = (int)provinces[enemyProvinceIndex].lifePoints.value;
		}
		else
		{
			yourProvinceHP = (int)provinces[yourProvinceIndex].lifePoints.value;
			enemyProvinceHP = 10;
		}

	}
	public void SetUnitsConters(int your, int enemy)
	{
		GameManager.Instance.humanPlayer.stats.warriors.Subtract(provinces[yourProvinceIndex].unitsCounter - your);
		ProvinceStats province = provinces[enemyProvinceIndex];
		if (province.provinceOwnerIndex > 0)
		{
			botsList[province.provinceOwnerIndex - 1].stats.warriors.Subtract(provinces[enemyProvinceIndex].unitsCounter - enemy);
		}

		provinces[yourProvinceIndex].unitsCounter = your;
		provinces[enemyProvinceIndex].unitsCounter = enemy;
	}
	public void SetBattleResult(bool isWin)
	{
		if (isWin)
		{
			ProvinceStats provinceStats = provinces[enemyProvinceIndex];
			provinceStats.SetNewOwner(0);
			provinceStats.units = new Dictionary<int, int>();
			provinceStats.unitsCounter = 0;
		}
		else
		{

		}
	}
	public void UpdateMap()
	{
		cameraController = Camera.main.GetComponent<CameraController>();
		players = GameObject.FindGameObjectWithTag("Players").transform;
		humanPlayer = players.GetChild(0).GetComponent<Player>();
		for (int i = 0; i < botsList.Count; i++)
		{
			botsList[i] = players.GetChild(i + 1).GetComponent<Player>();
		}


		map = GameObject.FindGameObjectWithTag("GameMap").transform;
		buildings = GameObject.FindGameObjectWithTag("Buildings").transform;
		selectingProvinces = FindObjectOfType<SelectingProvinces>();
		humanPlayer.stats.movementPoints.limit = 30;

		humanPlayer.stats.movementPoints.UpdateLimit();
		humanPlayer.stats.warriors.UpdateLimit();

		for (int i = 0; i < provinces.Length; i++)
		{
			ProvinceStats provinceStats = provinces[i];
			if (provinceStats.provinceOwnerIndex != -1)
			{
				selectingProvinces.ChangeProvinceColor(map.GetChild(i).GetComponent<SpriteRenderer>(), GetPlayerColor(provinceStats.provinceOwnerIndex));
			}
			else
			{
				selectingProvinces.ChangeProvinceColor(map.GetChild(i).GetComponent<SpriteRenderer>(), new Color32(48, 48, 48, 255));

			}

			if (provinceStats.buildingIndex != -1)
			{
				Transform province = map.GetChild(provinceStats.index).transform;
				Transform transform = new GameObject(province.name, typeof(SpriteRenderer)).transform;
				transform.position = province.position + new Vector3(0, 0.08f, 0);
				transform.parent = buildings;
				transform.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.buildingsStats[provinceStats.buildingIndex].icon;
				transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
			}
			UpdateUnitCounter(i);
		}
	}
	public void NextTurn(TextMeshProUGUI text)
	{
		if (readyToNextTurn)
		{
			turn++;
			StartCoroutine(BotsNextTurn());

			humanPlayer.stats.movementPoints.Set(humanPlayer.stats.movementPoints.limit);

			float startDevelopmentPoints = (float)Math.Round(humanPlayer.stats.developmentPoints.value, 2);
			float developmentPointsIncome = (float)Math.Round(humanPlayer.stats.developmentPoints.NextTurn(), 2);

			float startCoins = (int)humanPlayer.stats.coins.value;
			float coinsIncome = humanPlayer.stats.coins.NextTurn();

			float startPopulation = humanPlayer.stats.GetPopulation();
			float populationIncome = 0;


			text.text = "Turn:" + turn;
			UIManager.Instance.CloseUIWindow("ProvinceStats");


			for (int i = 0; i < provinces.Length; i++)
			{
				float value = provinces[i].population.NextTurn();
				if (provinces[i].provinceOwnerIndex == 0) populationIncome += value;
			}
			/*
			string stats = startCoins + " <sprite index=21/>   ";
			if (coinsIncome >= 0) stats += "<color=green>+"+ coinsIncome +"</color>";
			else stats += "<color=red>" + coinsIncome + "</color>";

			stats += "\n";

			stats +=  startPopulation + " <sprite index=1/>   ";
			if (populationIncome >= 0) stats += "<color=green>+" + populationIncome + "</color>";
			else stats += "<color=red>" + populationIncome + "</color>";

			stats += "\n";

			stats += startDevelopmentPoints + " <sprite index=22/>   ";
			if (developmentPointsIncome >= 0) stats += "<color=green>+" + developmentPointsIncome + "</color>";
			else stats += "<color=red>" + developmentPointsIncome + "</color>";

			UIManager.Instance.OpenTurnDetails(stats);
			*/
		}
	}

	IEnumerator BotsNextTurn()
	{
		readyToNextTurn = false;
		foreach (Player bot in botsList)
		{
			yield return new WaitUntil(() => ready);
			ready = false;
			PlayerStats playerStats = bot.stats;
			playerStats.movementPoints.Set(playerStats.movementPoints.limit);
			playerStats.developmentPoints.NextTurn();
			playerStats.coins.NextTurn();
			bot.RunEnemyManager();
		}
		readyToNextTurn = true;
		UpdateBotDebuger();
		yield return 0;
	}
	public void UpdateBotDebuger()
	{
		string debugtext = "";
		foreach (Player bot in botsList)
		{
			PlayerStats playerStats = bot.stats;

			debugtext += "<color=#" + GetPlayerColor(bot.index).ToHexString().Remove(6) + ">Player " + bot.index + " : <color=white>" + Icons.GetIcon("Coin") + playerStats.coins.ToString() + "   " + Icons.GetIcon("DevelopmentPoint") + playerStats.developmentPoints.ToString() +
			Icons.GetIcon("Warrior") + playerStats.warriors.ToString() + "\n";
		}
		UIManager.Instance.debugText.text = debugtext;
	}
	/// 0,1,2,3,4
	public void GetValuesByTaxesIndex(int index, out float coinsIncome, out float peopleIncome)
	{
		coinsIncome = 0;
		peopleIncome = 0;
		switch (index)
		{
			case 0:
				coinsIncome = -0.05f;
				peopleIncome = 0.02f;
				break;
			case 1:
				coinsIncome = 0.00f;
				peopleIncome = 0.015f;
				break;
			case 2:
				coinsIncome = 0.05f;
				peopleIncome = 0.01f;
				break;
			case 3:
				coinsIncome = 0.1f;
				peopleIncome = 0.00f;
				break;
			case 4:
				coinsIncome = 0.15f;
				peopleIncome = -0.01f;
				break;
		}
	}
	public void GetValuesByResearchIndex(int index, out float coinsIncome, out float developmentIncome)
	{
		coinsIncome = 0;
		developmentIncome = 0;
		switch (index)
		{
			case 0:
				coinsIncome = 0f;
				developmentIncome = 0f;
				break;
			case 1:
				coinsIncome = -0.0005f;
				developmentIncome = 0.75f;
				break;
			case 2:
				coinsIncome = -0.001f;
				developmentIncome = 0.5f;
				break;
			case 3:
				coinsIncome = -0.015f;
				developmentIncome = 0;
				break;
			case 4:
				coinsIncome = -0.002f;
				developmentIncome = -0.25f;
				break;
		}
	}
	public Color GetPlayerColor(int playerIndex)
	{
		if (playerIndex == 0)
		{
			return humanPlayer.playerColor;
		}
		else if (playerIndex - 1 <= botsList.Count && playerIndex > 0)
		{
			return botsList[playerIndex - 1].playerColor;
		}
		return Color.gray;
	}
	public int GetEnemyIndex()
	{
		return provinces[enemyProvinceIndex].provinceOwnerIndex;
	}

	public void OpenMenu()
	{
		SceneManager.LoadScene(2);
	}



	private void Save()
	{
		Player[] players = new Player[botsList.Count + 1];
		players[0] = humanPlayer;
		for (int i = 0; i < botsList.Count; i++)
		{
			players[i + 1] = botsList[i];
		}

		GameData gameData = new GameData(provinces, players);
		SavesManager.Save(gameData);
		Debug.Log("Saved");
	}

	public void Load(string name)
	{
		GameData gameData = SavesManager.Load(name);
		provinces = gameData.LoadProvinces();
		UpdateMap();
		PlayerData[] players = gameData.GetPlayers();

		if (humanPlayer != null) { Destroy(humanPlayer.gameObject); }
		Player player = new GameObject("Human", typeof(Player)).GetComponent<Player>();
		player.transform.parent = this.players;
		player.index = 0;
		player.name = players[0].playerName;
		player.isComputer = false;
		player.playerColor = GetColor(players[0].playerColor);
		player.stats = players[0].stats.ToPlayerStats();
		humanPlayer = player;

		for (int i = 0; i < botsList.Count; i++)
		{
			Debug.Log("de");
			if (botsList[i] != null) Destroy(botsList[i].gameObject);
		}
		botsList.Clear();

		for (int i = 1; i < players.Length; i++)
		{
			player = new GameObject(name, typeof(Player)).GetComponent<Player>();
			player.transform.parent = this.players;
			player.index = players[i].index;
			player.name = players[i].playerName + "1" ;
			player.isComputer = true;
			player.playerColor = GetColor(players[i].playerColor);
			player.stats = players[i].stats.ToPlayerStats();
			botsList.Add(player);
		}

		UpdateBotDebuger();
		UIManager.Instance.UpdateCounters();
		Debug.Log("Loaded");
	}

	private Color GetColor(float[] rgba)
	{
		return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
	}

	public string GetName()
	{
		return currentMap + " Turn " + turn.ToString() + "  " + DateTime.Now.Ticks.ToString();
	}
}




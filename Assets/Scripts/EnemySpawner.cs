using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    enum State
    {
        JustBeforeWave,
        DuringWave,
        InBetweenWaves
    }
    private float timeRemaining = 0;
    private State state = State.JustBeforeWave;
    public Enemy enemyPrefab;
    private int waveNumber = 0;
    private Castle _castleScript;
    private Castle Castle
    {
        get
        {
            if (!_castleScript)
            {
                _castleScript = FindObjectOfType<Castle>();
                if (_castleScript)
                {
                    _castleScript.upgradesPanel.OnSkipButtonPressed += OnSkipButtonPressedEventHandler;
                }
            }
            return _castleScript;
        }
    }
    private UpgradesSystem _upgradesSystem;
    private UpgradesSystem UpgradesSystem
    {
        get
        {
            if (!_upgradesSystem)
            {
                _upgradesSystem = Castle?.upgradesPanel.GetComponent<UpgradesSystem>();
            }
            return _upgradesSystem;
        }
    }

    void Start()
    {
        var placementScript = GetComponent<Placement>();
        placementScript.OnCastleSpawn += OnCastleSpawnEventHandler;
    }

    private void SpawnEnemies()
    {
        StartCoroutine(SpawnEnemiesEnumerator());
    }
    private IEnumerator SpawnEnemiesEnumerator()
    {
        if (state == State.DuringWave)
            yield return new WaitForSeconds(0.0f);
        yield return new WaitForSeconds(3.0f);
        var castleLocation = Castle?.transform.position ?? new Vector3();
        for (int i = 0; i < Random.Range(waveNumber + (int)(waveNumber * 1.6), waveNumber + 1 + (int)(waveNumber * 1.8)); i++)
        {
            Vector3 newLocation = new Vector3(
                castleLocation.x - Random.Range(4.0f, 5.0f) * (Random.Range(0, 2) * 2 - 1),
                castleLocation.y,
                castleLocation.z - Random.Range(4.0f, 5.0f) * (Random.Range(0, 2) * 2 - 1));
            var enemy = Instantiate(enemyPrefab, newLocation, new Quaternion());
            enemy.CastlePosition = castleLocation;
            enemy.OnDie += OnEnemyDieEventHandler;
        }
        state = State.DuringWave;
    }

    void Update()
    {
        if (state == State.InBetweenWaves)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                float minutes = Mathf.FloorToInt(timeRemaining / 60);
                float seconds = Mathf.FloorToInt(timeRemaining % 60);
                var text = string.Format("{0:00}:{1:00}", minutes, seconds);
                UpgradesSystem.SetTimerText(text);
            }
            else
            {
                CloseShopsAndStartWave();
            }

        }
    }

    private void OnEnemyDieEventHandler(object? sender, Enemy.EnemyDieEventArgs e)
    {
        Castle?.AddMoney(e.Money);
        StartCoroutine(CheckInBetweenWaves(e));
    }

    private void OnCastleSpawnEventHandler(object? sender, System.EventArgs e)
    {
        if (state != State.InBetweenWaves) //TODO zrobić z tym porządek
            CloseShopsAndStartWave();
    }

    private IEnumerator CheckInBetweenWaves(Enemy.EnemyDieEventArgs e)

    {
        yield return new WaitForSeconds(1.0f);
        if (state == State.DuringWave)
        {
            var allEnemies = FindObjectsOfType<Enemy>();
            if (allEnemies.Length == 0 && state == State.DuringWave) //double check 
            {
                StartShops();
                timeRemaining = 60;
            }
        }
    }
    private void StartShops()
    {
        state = State.InBetweenWaves;
        Castle?.ShowUpgrades();
        foreach (var turret in FindObjectsOfType<UpgradableTurret>())
        {
            turret.ShowUpgrades();
        }
    }

    private void CloseShopsAndStartWave()
    {
        timeRemaining = 0;
        waveNumber++;
        state = State.JustBeforeWave;
        Castle?.HideUpgrades();
        foreach (var turret in FindObjectsOfType<UpgradableTurret>())
        {
            turret.HideUpgrades();
        }
        SpawnEnemies();
    }
    private void OnSkipButtonPressedEventHandler(object? sender, System.EventArgs e)
    {
        CloseShopsAndStartWave();
    }

}

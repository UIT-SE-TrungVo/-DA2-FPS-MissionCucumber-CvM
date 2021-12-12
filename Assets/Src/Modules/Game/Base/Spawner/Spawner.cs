using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject _prefabCat;
    [SerializeField] GameObject _prefabMouse;

    [SerializeField] GameObject _prefabAICat;
    [SerializeField] GameObject _prefabAIMouse;

    [SerializeField] List<GameObject> _mouseSpawns;
    [SerializeField] List<GameObject> _catSpawns;

    [SerializeField] GameObject _prefabCucumber;

    [SerializeField] List<GameObject> _cucumberPoints;
    public List<GameObject> CucumberPoints => _cucumberPoints;

    [SerializeField] GameObject _mouseRetreatPoint;
    public GameObject MouseRetreatPoint => _mouseRetreatPoint;

    [SerializeField] GameObject _catRetreatPoint;
    public GameObject CatRetreatPoint => _catRetreatPoint;

    static Spawner _ins;
    public static Spawner Ins => _ins;

    void Awake()
    {
        _ins = this;
    }

    public void DoSpawn(CharacterSide startSide)
    {
        switch (startSide)
        {
            case CharacterSide.MICE:
                Debug.Log("Spawn character mouse");
                GameVar.Ins.Player = SpawnCharacter(_prefabMouse, _mouseSpawns);
                break;
            case CharacterSide.CATS:
                Debug.Log("Spawn character cat");
                GameVar.Ins.Player = SpawnCharacter(_prefabCat, _catSpawns);
                break;
        }
        this.enabled = false;
    }

    GameObject SpawnCharacter(GameObject character, List<GameObject> spawnPoints)
    {
        GameObject spawnPoint = Utils.PickFromList(spawnPoints, true);

        Vector3 spawnPosition = spawnPoint.transform.position;
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        Debug.Log("Spawn character", character.name, spawnPosition, randomRotation);
        return PhotonNetwork.Instantiate(character.name, spawnPosition, randomRotation);
    }

    public void SpawnCucumbers(List<int> cucumberIndexes)
    {
        cucumberIndexes.ForEach(index =>
        {
            Vector3 position = this._cucumberPoints[index].transform.position;
            GameObject cucumber = PhotonNetwork.Instantiate(_prefabCucumber.name, position, Quaternion.identity);
            ObjectiveTracker.Ins.AddObjective(cucumber);
        });
    }

    public void SpawnBots()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int numPMouse = 0;
            int numPCat = 0;

            foreach (var item in GameVar.Players)
            {
                switch ((CharacterSide)item.Value)
                {
                    case CharacterSide.CATS: numPCat++; break;
                    case CharacterSide.MICE: numPMouse++; break;
                }
            }

            int numBotMouse = NetworkGame.NUM_MICE_SLOT - numPMouse;
            int numBotCat = NetworkGame.NUM_CATS_SLOT - numPCat;

            Debug.Log("Cats team", numPCat, numBotCat);
            Debug.Log("Mice team", numPMouse, numBotMouse);

            for (var i = 0; i < numBotMouse; i++) this.SpawnCharacter(_prefabAIMouse, _mouseSpawns);
            for (var i = 0; i < numBotCat; i++) this.SpawnCharacter(_prefabAICat, _catSpawns);
        }
    }
}

using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class SquadsRegister : IRegisterWithSubscription
{
    [SerializeField]
    private SerializableDictionary<int, Squad> _squads = new SerializableDictionary<int, Squad>();

    public SerializableDictionary<int, Squad> Squads => _squads;

    public Squad PlayerSquad => GetSquad(PlayerSquadIndex);

    public const int PlayerSquadIndex = 1;
    private bool _debug => LowLevelSettings.instance.debugSquads;

    public SquadsRegister()
    {
        _squads = new SerializableDictionary<int, Squad>();
    }

    public void OnEnable()
    {
        LocationEnterManager.OnExitPrevLocationCompleted += ListenLocationExit;
    }

    public void OnDisable()
    {
        LocationEnterManager.OnExitPrevLocationCompleted -= ListenLocationExit;
    }

    private void ListenLocationExit(Location location)
    {
        DisbandPlayersSquad();
    }

    public int RegisterNewSquad()
    {
        int index = 0;
        do
        {
            index = UnityEngine.Random.Range(2, int.MaxValue);
        }
        while (_squads.ContainsKey(index));

        _squads.Add(index, new Squad());
        return index;
    }

    public void RegisterNewSquad(int squadIndex)
    {
        _squads.Add(squadIndex, new Squad(new List<Entity>()));
    }

    public Squad MergeSquads(int parentSquadIndex, int childSquadIndex)
    {
        var parentSquad = GetSquad(parentSquadIndex);
        var childSquad = GetSquad(childSquadIndex);

        if (parentSquad == null)
        {
            throw new System.NullReferenceException("No squad with index " + parentSquadIndex);
        }
        if (childSquad == null)
        {
            throw new System.NullReferenceException("No squad with index " + childSquadIndex);
        }

        foreach (var item in childSquad.members)
        {
            MoveToSquad(parentSquadIndex, item);
        }

        DisbandSquad(childSquadIndex);
        return parentSquad;
    }

    public void DisbandSquad(int squadIndex)
    {
        if (IsSquadExists(squadIndex))
        {
            _squads.Remove(squadIndex);
            if (_debug) Debug.Log("Squad " + squadIndex + " disbanded");
        }
    }

    public void MoveToSquad(int squadIndex, Entity squadmate)
    {
        if (squadmate != Entity.Null)
        {
            var prevSquadIndex = squadmate.GetComponentData<SquadMemberComponent>().squadIndex;
            if (!IsSquadExists(squadIndex) || !GetSquad(squadIndex).members.Contains(squadmate))
            {
                if (IsSquadExists(prevSquadIndex))
                {
                    var prevSquad = GetSquad(prevSquadIndex);
                    prevSquad.Remove(squadmate);
                    if (prevSquad.Count == 0)
                    {
                        DisbandSquad(prevSquadIndex);
                    }
                }

                squadmate.SetComponentData(new SquadMemberComponent(squadIndex));

                if (!IsSquadExists(squadIndex))
                {
                    RegisterNewSquad(squadIndex);
                }

                _squads[squadIndex].Add(squadmate);

                if (_debug) Debug.Log(squadmate.GetName() + " added to squad " + squadIndex);
            }
        }
        else
        {
            throw new System.NullReferenceException("Trying to add Null teammate");
        }
    }

    public void SplitfromSquad(int squadIndex, Entity squadmate)
    {
        if (squadIndex == 1)
        {
            var walkData = squadmate.GetComponentData<WalkabilityDataComponent>();
            walkData.isPlayersSquadmate = false;
            squadmate.SetComponentData(walkData);
        }
        _squads[squadIndex].members.Remove(squadmate);
        var newSquadIndex = RegisterNewSquad();
        MoveToSquad(newSquadIndex, squadmate);
    }

    public void RemoveFromAnySquads(Entity squadmate)
    {
        if (squadmate.HasComponent<SquadMemberComponent>())
        {
            var index = squadmate.GetComponentData<SquadMemberComponent>().squadIndex;
            if (IsSquadExists(index))
            {
                _squads[index].members.Remove(squadmate);
                if (index == 1)
                {
                    var walkData = squadmate.GetComponentData<WalkabilityDataComponent>();
                    walkData.isPlayersSquadmate = false;
                    squadmate.SetComponentData(walkData);
                }
                if (_squads[index].members.Count == 0)
                {
                    DisbandSquad(index);
                }
            }
        }
    }

    public void AddEnemyIndex(int squadIndex, int enemySquadIndex)
    {
        if (!IsSquadExists(squadIndex))
        {
            throw new System.NullReferenceException("No squad with index " + squadIndex);
        }
        if (!IsSquadExists(enemySquadIndex))
        {
            throw new System.NullReferenceException("No squad with index " + enemySquadIndex);
        }
        if (squadIndex != enemySquadIndex)
        {
            var squad = GetSquad(squadIndex);
            if (!squad.enemySquadIndexes.Contains(enemySquadIndex))
            {
                if (_debug) Debug.Log("Squad " + enemySquadIndex + " is now enemy of squad " + squadIndex);
                squad.enemySquadIndexes.Add(enemySquadIndex);
            }
        }
    }

    public bool AreSquadsEnemies(int squadIndex1, int squadIndex2)
    {
        var squad1 = GetSquad(squadIndex1);
        var squad2 = GetSquad(squadIndex2);

        if (squad1 == null)
        {
            throw new System.NullReferenceException("No squad with index " + squadIndex1);
        }
        if (squad2 == null)
        {
            throw new System.NullReferenceException("No squad with index " + squadIndex2);
        }

        return squad1.enemySquadIndexes.Contains(squadIndex2) || squad2.enemySquadIndexes.Contains(squadIndex1);
    }

    public bool IsSquadExists(int squadIndex)
    {
        return _squads.ContainsKey(squadIndex);
    }

    public void AddToPlayersSquad(Entity entity)
    {
        if (GetPlayersSquad().members.Count <= 5)
        {
            var walkData = entity.GetComponentData<WalkabilityDataComponent>();
            walkData.isPlayersSquadmate = true;
            entity.SetComponentData(walkData);
            MoveToSquad(PlayerSquadIndex, entity);
            Announcer.Announce(DescriberUtility.GetName(entity) + LocalizationManager.GetString("Added to players squad message"));
        }
        else
        {
            if (_debug) Debug.Log("PlayersSquad is full");
        }
    }

    public Entity GetSquadLeader(int squadIndex)
    {
        var squad = GetSquad(squadIndex);
        if (squad != null)
        {
            return squad.Leader;
        }
        foreach (var item in _squads.Keys)
        {
            Debug.Log(item);
        }
        throw new System.NullReferenceException("no such squad " + squadIndex);
    }

    public List<Entity> GetSquadmates(int squadIndex)
    {
        var squad = GetSquad(squadIndex);
        if (squad != null)
        {
            return squad.members;
        }
        return new List<Entity>();
    }

    public List<Entity> GetSquadmates(Entity entity)
    {
        if (entity.HasComponent<SquadMemberComponent>())
        {
            return GetSquadmates(entity.GetComponentData<SquadMemberComponent>().squadIndex);
        }
        return new List<Entity>();
    }

    public Squad GetSquad(int squadIndex)
    {
        if (IsSquadExists(squadIndex))
        {
            return _squads[squadIndex];
        }
        else
        {
            return null;
        }
    }

    public bool AreSquadmates(Entity first, Entity second)
    {
        if (first.HasComponent<SquadMemberComponent>() && second.HasComponent<SquadMemberComponent>())
        {
            if (first.GetComponentData<SquadMemberComponent>().squadIndex == second.GetComponentData<SquadMemberComponent>().squadIndex)
            {
                return true;
            }
        }
        return false;
    }

    public int GetSquadIndex(Entity entity)
    {
        return entity.GetComponentData<SquadMemberComponent>().squadIndex;
    }

    public Squad GetPlayersSquad()
    {
        return _squads[1];
    }

    private void DisbandPlayersSquad()
    {
        Debug.Log("Disband player");

        DisbandSquad(1);
    }
}
using System;
using System.Reflection;

public class Registers : Singleton<Registers>
{
    private RegistersData _registersData;
    public static GodsRegister GodsRegister => instance._registersData.GodsRegister;
    public static SquadsRegister SquadsRegister => instance._registersData.SquadsRegister;
    public static LocksRegister LocksRegister => instance._registersData.LocksRegister;
    public static UniqueObjectsRegister UniqueObjectsRegister => instance._registersData.UniqueObjectsRegister;
    public static StatsRegister StatsRegister => instance._registersData.StatsRegister;
    public static GlobalMapRegister GlobalMapRegister => instance._registersData.GlobalMapRegister;

    public static event Action OnLoaded;

    public static bool IsInit => instance._registersData != null;

    public static void SetDefaultValues()
    {
        if (instance._registersData != null)
        {
            instance._registersData.OnDisable();
        }
        instance._registersData = new RegistersData(
            new GodsRegister(GodsDatabase.GetAllGods()),
            new SquadsRegister(),
            new LocksRegister(),
            new UniqueObjectsRegister(),
            GlobalMapGenerator.instance.Generate(),
            new StatsRegister()
            );
        instance._registersData.OnEnable();
        OnLoaded?.Invoke();
    }

    public static void Save()
    {
        DataSaveLoader.SaveSaveData(instance._registersData);
    }

    public static void Load()
    {
        if (instance._registersData != null)
        {
            instance._registersData.OnDisable();
        }
        instance._registersData = DataSaveLoader.LoadData<RegistersData>();
        instance._registersData.OnEnable();
        OnLoaded?.Invoke();
    }
}

[System.Serializable]
public class RegistersData : IRegisterWithSubscription
{
    public readonly GodsRegister GodsRegister;
    public readonly SquadsRegister SquadsRegister;
    public readonly LocksRegister LocksRegister;
    public readonly UniqueObjectsRegister UniqueObjectsRegister;
    public readonly GlobalMapRegister GlobalMapRegister;
    public readonly StatsRegister StatsRegister;

    public RegistersData(GodsRegister godsRegister, SquadsRegister squadsRegister, LocksRegister locksRegister, UniqueObjectsRegister uniqueObjectsRegister, GlobalMapRegister globalMapRegister, StatsRegister statsRegister)
    {
        GodsRegister = godsRegister;
        SquadsRegister = squadsRegister;
        LocksRegister = locksRegister;
        UniqueObjectsRegister = uniqueObjectsRegister;
        GlobalMapRegister = globalMapRegister;
        StatsRegister = statsRegister;
    }

    public void OnEnable()
    {
        foreach (var item in GetType().GetFields())
        {
            if (typeof(IRegisterWithSubscription).IsAssignableFrom(item.FieldType))
            {
                (item.GetValue(this) as IRegisterWithSubscription).OnEnable();
            }
        }
    }

    public void OnDisable()
    {
        foreach (var item in GetType().GetFields())
        {
            if (typeof(IRegisterWithSubscription).IsAssignableFrom(item.FieldType))
            {
                (item.GetValue(this) as IRegisterWithSubscription).OnDisable();
            }
        }
    }
}

public interface IRegisterWithSubscription
{
    public abstract void OnEnable();

    public abstract void OnDisable();
}
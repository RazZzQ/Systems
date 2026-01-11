using UnityEngine;

public interface IKeyring
{
    bool HasKey(string keyId);
    bool TryAddKey(string keyId);
    bool TryConsumeKey(string keyId); //Llaves de un solo uso
}

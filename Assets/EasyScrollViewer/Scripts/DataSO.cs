using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "Test/Data")]
public class DataSO : ScriptableObject
{
    public List<int> nums;
    public List<string> messages;
}

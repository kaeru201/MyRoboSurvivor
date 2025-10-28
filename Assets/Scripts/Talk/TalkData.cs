using UnityEngine;

[System.Serializable]
public class Talk
{
    [TextArea(3, 10)]//�ŏ�3�s�A�ő�10�s��string��\��
    public string talk;
}

[CreateAssetMenu(fileName = "TalkData", menuName = "TalkData")]
public class TalkData : ScriptableObject
{
    public Talk[] talkDatas;
}
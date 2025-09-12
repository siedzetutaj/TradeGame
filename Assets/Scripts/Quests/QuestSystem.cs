using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public QuestPanelReferences QuestPanelRefs;
    

    //TODO:
    /*
     * Najpierw zrobiæ fabularne:
     * SO w którym wybieram w jakim miescie jest quest
     * Co trzeba dostarczyc (z i do jakiego miasta)
     * Potem na podstawie tego s¹ tworzone kolejne rzeczy
     * 
     */


    public QuestReferences GenerateQuestRef()
    {
        GameObject questObj = Instantiate(QuestPanelRefs.QuestPrefab, QuestPanelRefs.QuestHolder.transform);
        return questObj.GetComponent<QuestReferences>();
    }
    public QuestItem GenerateItemToGet(ItemSO item, int amount, QuestReferences questRef)
    {
        GameObject itemObj = Instantiate(questRef.ItemToGetPrefab, questRef.ItemsToGetHolder.transform);
        QuestItem questItemToGet = itemObj.GetComponent<QuestItem>();
        questItemToGet.Image.sprite = item.sprite;
        questItemToGet.AmountText.text = $"{amount}";
        return questItemToGet;
    }
    public QuestItem GenerateReward(ItemSO item, int amount, QuestReferences questRef)
    {
        GameObject rewardObj = Instantiate(questRef.RewardPrefab, questRef.RewardHolder.transform);
        QuestItem questReward = rewardObj.GetComponent<QuestItem>();
        questReward.Image.sprite = item.sprite;
        questReward.AmountText.text = $"{amount}";
        return questReward;
    }
}

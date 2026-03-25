using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogDatabaseSO", menuName = "Dialog System/DialogDatabaseSO")]
public class DialogDatabaseSO : ScriptableObject
{
    public List<DialogSO> dialogs = new List<DialogSO>();

    private Dictionary<int, DialogSO> dialogsByld;

    public void Initailize()
    {
        dialogsByld = new Dictionary<int, DialogSO>();

        foreach (var dialog in dialogs)
        {
            if (dialog != null)
            {
                dialogsByld[dialog.id] = dialog;
            }
        }
    }

    public DialogSO GetDialongsByld(int id)
    {
        if (dialogsByld == null)
            Initailize();

        if(dialogsByld.TryGetValue(id, out DialogSO dialogs))
        {
            return dialogs;
        }

        return null;
    }
}

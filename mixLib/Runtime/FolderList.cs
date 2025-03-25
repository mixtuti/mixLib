using System.Collections.Generic;
using UnityEngine;

namespace mixLib
{
    [CreateAssetMenu(fileName = "FolderList", menuName = "mixLib/Folder List")]
    public class FolderList : ScriptableObject
    {
        [Tooltip("作成するフォルダのパスを入力")]
        public List<string> folders = new List<string>();
    }
}

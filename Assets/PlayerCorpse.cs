using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Game
{
    public class PlayerCorpse : NetworkBehaviour
    {
        public MeshRenderer corpseRenderer;
        [SyncVar(hook = nameof(CorpseCol))]
        public Color32 corpseCol;

        public void ServerSetCorpse(Color32 corpseColor)
        {
            corpseCol = corpseColor;
        }
        void CorpseCol(Color32 _, Color32 corpsColor)
        {
            corpseRenderer.materials[0].color = corpsColor;
        }
    }
}

using UnityEngine;
using System.Collections;

namespace Assets.Scripts {
    interface ITileAnimation {
        void UpdateAnimation();
        bool IsCompleted();
        void ReverseAnimation();
    }
}

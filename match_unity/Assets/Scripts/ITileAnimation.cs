using UnityEngine;
using System.Collections;

namespace Assets.Scripts {
    public interface ITileAnimation {
        void UpdateAnimation();
        bool IsCompleted();
        void ReverseAnimation();
    }
}

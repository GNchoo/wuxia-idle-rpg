using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAMPLETEXT
{


    public class FixScrollActiveTeam : MonoBehaviour
    {
        private void OnEnable()
        {
            this.GetComponent<Scrollbar>().value = 1;
        }
    }

}
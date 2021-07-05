using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class TabNavigation : MonoBehaviour
    {
		public List<InputField> inputs;
		public int inputIndex;

		private void Start()
		{
			if (inputs != null)
			{
				inputs[0].Select();
				inputIndex = 0;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab) && inputs.Count > 1)
			{
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					if (inputIndex <= 0)
					{
						inputIndex = inputs.Count;
					}

					inputIndex--;

					inputs[inputIndex].Select();
				}
				else
				{
					if (inputs.Count <= inputIndex + 1)

					{
						inputIndex = -1;
					}

					inputIndex++;

					inputs[inputIndex].Select();
				}
			}
		}
	}
}

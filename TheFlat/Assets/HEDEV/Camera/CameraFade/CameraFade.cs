using UnityEngine;

namespace Proto.CameraSystem
{
	public class CameraFade : MonoBehaviour
	{
		public void OnPostRender()
		{
			CameraFadeSystem.OnPostRenderUpdate();
		}
	}
}